#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// ferramenta de editor (menu "Drunken Dog Fist"). fatia os strips 128x128, cria as animacoes,
// monta os controllers e os prefabs de todos os inimigos (comuns, atiradores e bosses),
// alem do Projetil.prefab. tudo orientado pela TABELA de config abaixo.
public static class GeradorInimigos
{
    const int celula = 128;
    const int fps = 12;
    const float ppu = 100f;
    const string pastaPrefabs = "Assets/Inimigos/Prefabs";

    static readonly HashSet<string> emLoop = new HashSet<string> { "Idle", "Idle_2", "Walk", "Run" };

    class ConfigInimigo
    {
        public string pasta, nome, tipo;
        public float vida, alcanceTiro;
        public int combo;
        public bool projetil, chefe;
        public ConfigInimigo(string pasta, string nome, string tipo, float vida, int combo, bool projetil, float alcanceTiro, bool chefe)
        {
            this.pasta = pasta; this.nome = nome; this.tipo = tipo;
            this.vida = vida; this.combo = combo; this.projetil = projetil; this.alcanceTiro = alcanceTiro;
            this.chefe = chefe;
        }
    }

    // pasta, nome, tipo (Comum/Atirador/Chefe), vida, golpesNoCombo, usaProjetil, alcanceTiro, mostraBarraDeChefe
    static readonly ConfigInimigo[] TABELA =
    {
        new ConfigInimigo("Assets/Inimigos/Comuns/CapangaFraco",       "CapangaFraco",  "Comum",     20f, 2, false, 0f, false),
        new ConfigInimigo("Assets/Inimigos/Comuns/CapangaMedio",       "CapangaMedio",  "Comum",     30f, 1, false, 0f, false),
        new ConfigInimigo("Assets/Inimigos/Comuns/CapangaCombo",       "CapangaCombo",  "Comum",     30f, 3, false, 0f, false),
        new ConfigInimigo("Assets/Inimigos/MiniBosses/AtiradorMedio",  "AtiradorMedio", "Atirador",  60f, 2, true,  5f, true),
        new ConfigInimigo("Assets/Inimigos/MiniBosses/AtiradorLongo",  "AtiradorLongo", "Atirador",  60f, 1, true,  8f, true),
        new ConfigInimigo("Assets/Inimigos/MiniBosses/Brigao",         "Brigao",        "Comum",    120f, 3, false, 0f, true),
        new ConfigInimigo("Assets/Inimigos/Bosses/BossGerente",        "BossGerente",   "Chefe",    200f, 3, false, 0f, true),
        new ConfigInimigo("Assets/Inimigos/Bosses/BossCassino",        "BossCassino",   "Chefe",    250f, 1, true,  6f, true),
        new ConfigInimigo("Assets/Inimigos/Bosses/BossChefao",         "BossChefao",    "Chefe",    300f, 1, true,  7f, true),
    };

    // ---------------------------------------------------------------- Menu

    [MenuItem("Drunken Dog Fist/Inimigos/Gerar TODOS os inimigos")]
    static void gerarTodos()
    {
        garantirPasta("Assets", "Inimigos");
        garantirPasta("Assets/Inimigos", "Prefabs");

        montarProjetil();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GameObject projetil = AssetDatabase.LoadAssetAtPath<GameObject>(pastaPrefabs + "/Projetil.prefab");

        foreach (var cfg in TABELA)
            gerarInimigo(cfg, projetil);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Inimigos gerados: " + TABELA.Length + " (+ Projetil.prefab)");
    }

    // ------------------------------------------------------- Por inimigo

    static void gerarInimigo(ConfigInimigo cfg, GameObject projetil)
    {
        if (!AssetDatabase.IsValidFolder(cfg.pasta))
        {
            Debug.LogError("Pasta nao encontrada: " + cfg.pasta);
            return;
        }

        string saida = garantirPasta(cfg.pasta, "Animacoes");
        var clipes = new Dictionary<string, AnimationClip>();

        var pngs = Directory.GetFiles(cfg.pasta, "*.png", SearchOption.TopDirectoryOnly)
            .Select(p => p.Replace('\\', '/')).OrderBy(p => p);
        foreach (string png in pngs)
        {
            fatiar(png);
            var clipe = criarClipe(png, saida);
            if (clipe != null) clipes[Path.GetFileNameWithoutExtension(png)] = clipe;
        }

        var controller = criarController(saida, cfg.nome, clipes);
        montarPrefab(cfg, controller, projetil);
        Debug.Log(cfg.nome + " ok (" + clipes.Count + " clipes)");
    }

    // -------------------------------------------------------------- Slice

    static void fatiar(string png)
    {
        TextureImporter imp = AssetImporter.GetAtPath(png) as TextureImporter;
        if (imp == null) return;

        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = ppu;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;

        int largura, altura;
        imp.GetSourceTextureWidthAndHeight(out largura, out altura);
        int quadros = Mathf.Max(1, largura / celula);
        string nomeBase = Path.GetFileNameWithoutExtension(png);

        // spritesheet e obsoleto no unity 6 mas resolve o grid simples sem complicar
#pragma warning disable CS0618
        var metas = new SpriteMetaData[quadros];
        for (int i = 0; i < quadros; i++)
        {
            metas[i] = new SpriteMetaData
            {
                name = nomeBase + "_" + i,
                rect = new Rect(i * celula, 0, celula, altura),
                alignment = (int)SpriteAlignment.BottomCenter,
                pivot = new Vector2(0.5f, 0f)
            };
        }
        imp.spritesheet = metas;
#pragma warning restore CS0618

        EditorUtility.SetDirty(imp);
        imp.SaveAndReimport();
    }

    // ---------------------------------------------------------------- Clips

    static AnimationClip criarClipe(string png, string saida)
    {
        Sprite[] sprites = carregarSprites(png);
        if (sprites.Length == 0) return null;

        string estado = Path.GetFileNameWithoutExtension(png);
        var clipe = new AnimationClip { frameRate = fps };

        var bind = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
        var chaves = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            chaves[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = sprites[i] };
        AnimationUtility.SetObjectReferenceCurve(clipe, bind, chaves);

        if (emLoop.Contains(estado))
        {
            var cfg = AnimationUtility.GetAnimationClipSettings(clipe);
            cfg.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clipe, cfg);
        }

        AssetDatabase.CreateAsset(clipe, saida + "/" + estado + ".anim");
        return clipe;
    }

    // ----------------------------------------------------------- Controller

    static AnimatorController criarController(string saida, string nome, Dictionary<string, AnimationClip> clipes)
    {
        string caminho = saida + "/" + nome + ".controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(caminho);

        controller.AddParameter("andando", AnimatorControllerParameterType.Bool);
        controller.AddParameter("atacar1", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("atacar2", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("atacar3", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("atirar", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("recarregar", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("dano", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("morrendo", AnimatorControllerParameterType.Bool);

        var sm = controller.layers[0].stateMachine;

        var parado = sm.AddState("Parado"); parado.motion = escolher(clipes, "Idle", "Idle_2");
        var andar = sm.AddState("Andar"); andar.motion = escolher(clipes, "Walk", "Run", "Idle");
        var levarDano = sm.AddState("LevarDano"); levarDano.motion = escolher(clipes, "Hurt");
        var morrer = sm.AddState("Morrer"); morrer.motion = escolher(clipes, "Dead");
        sm.defaultState = parado;

        transicao(parado, andar, "andando", true);
        transicao(andar, parado, "andando", false);

        // acoes: so cria o estado se o sprite existir
        acao(sm, parado, andar, clipes, "Atacar1", "atacar1", "Attack_1", "Attack");
        acao(sm, parado, andar, clipes, "Atacar2", "atacar2", "Attack_2");
        acao(sm, parado, andar, clipes, "Atacar3", "atacar3", "Attack_3");
        acao(sm, parado, andar, clipes, "Atirar", "atirar", "Shot", "Shot_1");
        acao(sm, parado, andar, clipes, "Recarregar", "recarregar", "Recharge");

        transicaoQualquer(sm, levarDano, "dano");
        transicaoPorTempo(levarDano, parado, 0.9f);
        transicaoQualquer(sm, morrer, "morrendo");

        EditorUtility.SetDirty(controller);
        return controller;
    }

    // cria um estado de acao (ataque/tiro/pulo) e liga por trigger a partir de Parado e Andar
    static void acao(AnimatorStateMachine sm, AnimatorState parado, AnimatorState andar,
        Dictionary<string, AnimationClip> clipes, string nomeEstado, string trigger, params string[] sprites)
    {
        Motion m = escolher(clipes, sprites);
        if (m == null) return;

        var st = sm.AddState(nomeEstado);
        st.motion = m;
        transicao(parado, st, trigger, true);
        transicao(andar, st, trigger, true);
        transicaoPorTempo(st, parado, 0.9f);
    }

    // ---------------------------------------------------------------- Prefab

    static void montarPrefab(ConfigInimigo cfg, AnimatorController controller, GameObject projetil)
    {
        Sprite idle = carregarSprites(cfg.pasta + "/Idle.png").FirstOrDefault();

        GameObject root = new GameObject(cfg.nome);
        definirCamada(root, "Enemy");
        root.transform.localScale = new Vector3(-1f, 1f, 1f); // nasce virado pra esquerda

        var sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = idle;

        var anim = root.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        var rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var capsula = root.AddComponent<CapsuleCollider2D>();
        capsula.size = new Vector2(0.3f, 0.9f);
        capsula.offset = new Vector2(0f, 0.45f);

        var vidaComp = root.AddComponent<Vida>();

        // filho: caixa de dano do golpe melee
        GameObject filhoCaixa = new GameObject("CaixaDeDano");
        filhoCaixa.transform.SetParent(root.transform, false);
        filhoCaixa.transform.localPosition = new Vector3(0.3f, 0.6f, 0f);
        definirCamada(filhoCaixa, "EnemyHitbox");
        var box = filhoCaixa.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.2f, 0.9f); // cobre do centro ate a frente (pega colado tambem)
        var caixa = filhoCaixa.AddComponent<CaixaDeDano>();

        // ponto de tiro (so quando usa projetil)
        Transform ponto = null;
        if (cfg.projetil)
        {
            GameObject go = new GameObject("PontoDeTiro");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = new Vector3(0.6f, 0.6f, 0f);
            ponto = go.transform;
        }

        // script por tipo
        System.Type tipo = cfg.tipo == "Atirador" ? typeof(InimigoAtirador)
                         : cfg.tipo == "Chefe" ? typeof(Chefe)
                         : typeof(Capanga);
        var inimigo = root.AddComponent(tipo);

        // wiring: caixa de dano
        var soCaixa = new SerializedObject(caixa);
        definirMascara(soCaixa, "alvos", "Player");
        setObj(soCaixa, "dono", root);
        soCaixa.ApplyModifiedPropertiesWithoutUndo();

        // wiring: vida
        var soVida = new SerializedObject(vidaComp);
        setFloat(soVida, "vidaMaxima", cfg.vida);
        soVida.ApplyModifiedPropertiesWithoutUndo();

        // wiring: script do inimigo
        var so = new SerializedObject(inimigo);
        setObj(so, "caixaDeDano", caixa);
        setInt(so, "golpesNoCombo", cfg.combo);
        setBool(so, "ehChefe", cfg.chefe);
        if (cfg.projetil)
        {
            setObj(so, "prefabProjetil", projetil);
            setObj(so, "pontoDeTiro", ponto);
            setFloat(so, "alcanceTiro", cfg.alcanceTiro);
            setFloat(so, "alcanceVisao", cfg.alcanceTiro + 6f); // ve de longe -> persegue -> atira
            definirMascara(so, "alvoDoTiro", "Player");
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, pastaPrefabs + "/" + cfg.nome + ".prefab");
        Object.DestroyImmediate(root);
    }

    static void montarProjetil()
    {
        GameObject go = new GameObject("Projetil");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); // placeholder
        sr.color = new Color(1f, 0.85f, 0.3f);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        go.AddComponent<Projetil>();

        PrefabUtility.SaveAsPrefabAsset(go, pastaPrefabs + "/Projetil.prefab");
        Object.DestroyImmediate(go);
    }

    // ---------------------------------------------------- Transicoes / utils

    static Motion escolher(Dictionary<string, AnimationClip> clipes, params string[] nomes)
    {
        foreach (string n in nomes)
            if (clipes.ContainsKey(n)) return clipes[n];
        return null;
    }

    static void transicao(AnimatorState de, AnimatorState para, string parametro, bool ligado)
    {
        var t = de.AddTransition(para);
        t.duration = 0f;
        t.hasExitTime = false;
        t.AddCondition(ligado ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parametro);
    }

    static void transicaoPorTempo(AnimatorState de, AnimatorState para, float tempo)
    {
        var t = de.AddTransition(para);
        t.duration = 0f;
        t.hasExitTime = true;
        t.exitTime = tempo;
    }

    static void transicaoQualquer(AnimatorStateMachine sm, AnimatorState para, string parametro)
    {
        var t = sm.AddAnyStateTransition(para);
        t.duration = 0f;
        t.hasExitTime = false;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0f, parametro);
    }

    static Sprite[] carregarSprites(string png) =>
        AssetDatabase.LoadAllAssetsAtPath(png).OfType<Sprite>().OrderBy(s => indice(s.name)).ToArray();

    static int indice(string nome)
    {
        int u = nome.LastIndexOf('_');
        int r;
        if (u >= 0 && int.TryParse(nome.Substring(u + 1), out r)) return r;
        return 0;
    }

    static string garantirPasta(string pai, string filho)
    {
        string completo = pai + "/" + filho;
        if (!AssetDatabase.IsValidFolder(completo))
            AssetDatabase.CreateFolder(pai, filho);
        return completo;
    }

    static void definirCamada(GameObject go, string nome)
    {
        int camada = LayerMask.NameToLayer(nome);
        if (camada < 0)
        {
            Debug.LogWarning("Camada '" + nome + "' nao existe. Crie em Project Settings > Tags and Layers.");
            return;
        }
        go.layer = camada;
    }

    static void definirMascara(SerializedObject so, string prop, string nomeCamada)
    {
        var p = so.FindProperty(prop);
        if (p == null) return;
        int camada = LayerMask.NameToLayer(nomeCamada);
        p.intValue = camada < 0 ? 0 : (1 << camada);
    }

    static void setObj(SerializedObject so, string prop, Object val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = val;
    }

    static void setInt(SerializedObject so, string prop, int val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.intValue = val;
    }

    static void setBool(SerializedObject so, string prop, bool val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.boolValue = val;
    }

    static void setFloat(SerializedObject so, string prop, float val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.floatValue = val;
    }
}
#endif
