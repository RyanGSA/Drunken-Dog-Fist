#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// monta o HUD inteiro na cena aberta: barra de vida do player, barra grande de chefe,
// painel de pause e tela de game over, ja com os scripts e referencias ligados.
// tambem termina o setup do player (Vida, VidaDoJogador, CaixaDeDano, layer, freeze).
// obs: rode "Fatiar Stone UI" antes, pra existir os sprites.
public static class MontadorHUD
{
    const string sheet = "Assets/Stone UI/Stone_UI_Sprite_Sheet.png";

    // posicao da calha (onde entra o preenchimento) dentro do sprite da barra, em fracao 0..1
    static readonly Vector2 calhaMin = new Vector2(0.447f, 0.368f);
    static readonly Vector2 calhaMax = new Vector2(0.915f, 0.526f);

    static TMP_FontAsset fonte;

    [MenuItem("Drunken Dog Fist/UI/Montar HUD na cena")]
    static void montar()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().ToDictionary(s => s.name);
        if (!sprites.ContainsKey("barraMoldura"))
        {
            Debug.LogError("Rode 'Drunken Dog Fist/UI/Fatiar Stone UI' antes de montar o HUD.");
            return;
        }

        GameObject player = GameObject.FindWithTag("player");
        if (player == null)
        {
            Debug.LogError("Nao achei nenhum objeto com a tag 'player' na cena. Coloque o player primeiro.");
            return;
        }

        fonte = TMP_Settings.defaultFontAsset;
        prepararPlayer(player, sprites);
        Vida vidaPlayer = player.GetComponent<Vida>();

        // recria o HUD do zero se ja existir
        var antigo = GameObject.Find("HUD");
        if (antigo != null) Object.DestroyImmediate(antigo);

        var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(640, 360);
        scaler.matchWidthOrHeight = 1f;

        garantirEventSystem();

        // --- barra do player (canto superior esquerdo)
        var barraJog = novoUI("BarraJogador", canvasGo.transform);
        rect(barraJog, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(141, 57));
        barraJog.AddComponent<Image>().sprite = sprites["barraMoldura"];
        Image fillJog = criarFill(barraJog.transform, sprites["barraPreenchimento"]);
        wire(barraJog.AddComponent<BarraDeVida>(), ("vida", vidaPlayer), ("preenchimento", fillJog));

        // a barra dos chefes/mini-bosses agora flutua acima deles (BarraFlutuante), nao fica no HUD.

        // --- tela de game over (centro, desligada). script fica no canvas (sempre ativo).
        var painelGO = criarPainel("PainelGameOver", canvasGo.transform, sprites["painel"], new Vector2(360, 200));
        titulo(painelGO.transform, "GAME OVER", 46, new Vector2(0, 60));
        var telaGO = canvasGo.AddComponent<TelaGameOver>();
        wire(telaGO, ("painel", painelGO));
        criarBotao(painelGO.transform, "Tentar de novo", -10, telaGO.tentarDeNovo, sprites["painel"]);
        criarBotao(painelGO.transform, "Menu", -66, telaGO.voltarMenu, sprites["painel"]);
        painelGO.SetActive(false);

        // --- painel de pause (centro, desligado)
        var painelPause = criarPainel("PainelPause", canvasGo.transform, sprites["painel"], new Vector2(320, 190));
        titulo(painelPause.transform, "PAUSA", 40, new Vector2(0, 55));
        var menuPause = canvasGo.AddComponent<MenuPause>();
        wire(menuPause, ("painel", painelPause), ("painelGameOver", painelGO));
        criarBotao(painelPause.transform, "Continuar", -5, menuPause.continuar, sprites["painel"]);
        criarBotao(painelPause.transform, "Voltar ao Menu", -61, menuPause.voltarMenu, sprites["painel"]);
        painelPause.SetActive(false);

        // liga o game over na vida do player
        wire(player.GetComponent<VidaDoJogador>(), ("telaGameOver", telaGO));

        Selection.activeGameObject = canvasGo;
        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("HUD montado. Ajuste as posicoes/tamanhos no gosto se quiser.");
    }

    // ------------------------------------------------------------ player

    static void prepararPlayer(GameObject player, System.Collections.Generic.Dictionary<string, Sprite> sprites)
    {
        int camadaPlayer = LayerMask.NameToLayer("Player");
        if (camadaPlayer >= 0) player.layer = camadaPlayer;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        // afina o collider do corpo (o primeiro BoxCollider2D do root) pra o inimigo chegar perto.
        // estava ~0.32 de largura x scale 6 = quase 2 unidades -> parava longe demais.
        var corpoBox = player.GetComponent<BoxCollider2D>();
        if (corpoBox != null)
        {
            corpoBox.size = new Vector2(0.12f, 0.32f);
            corpoBox.offset = Vector2.zero;
        }

        if (player.GetComponent<Vida>() == null)
        {
            var v = player.AddComponent<Vida>();
            var so = new SerializedObject(v);
            var p = so.FindProperty("vidaMaxima");
            if (p != null) p.floatValue = 30f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        if (player.GetComponent<VidaDoJogador>() == null)
            player.AddComponent<VidaDoJogador>();

        Transform hitbox = player.transform.Find("hitbox");
        if (hitbox == null)
        {
            Debug.LogWarning("Player sem filho 'hitbox'. Crie a CaixaDeDano manualmente no filho de ataque.");
            return;
        }

        var caixa = hitbox.GetComponent<CaixaDeDano>();
        if (caixa == null) caixa = hitbox.gameObject.AddComponent<CaixaDeDano>();

        // caixa perto do centro pra acertar mesmo colado (o player tem scale 6, entao os valores sao pequenos)
        hitbox.localPosition = new Vector3(0.12f, 0f, 0f);
        var boxHit = hitbox.GetComponent<BoxCollider2D>();
        if (boxHit != null)
        {
            boxHit.offset = Vector2.zero;
            boxHit.size = new Vector2(0.35f, 0.4f);
        }

        var soCaixa = new SerializedObject(caixa);
        int camadaEnemy = LayerMask.NameToLayer("Enemy");
        var alvos = soCaixa.FindProperty("alvos");
        if (alvos != null) alvos.intValue = camadaEnemy < 0 ? 0 : (1 << camadaEnemy);
        var dono = soCaixa.FindProperty("dono");
        if (dono != null) dono.objectReferenceValue = player;
        soCaixa.ApplyModifiedPropertiesWithoutUndo();

        var sr = hitbox.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false; // era so o quadrado vermelho de debug
    }

    // ------------------------------------------------------------ helpers de UI

    static GameObject novoUI(string nome, Transform pai)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        return go;
    }

    static void rect(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 piv, Vector2 pos, Vector2 tam)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = piv;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
    }

    static void esticar(GameObject go, Vector2 aMin, Vector2 aMax)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Image criarFill(Transform pai, Sprite sprite)
    {
        var go = novoUI("preenchimento", pai);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        img.fillAmount = 1f;
        esticar(go, calhaMin, calhaMax);
        return img;
    }

    static GameObject criarPainel(string nome, Transform pai, Sprite fundo, Vector2 tam)
    {
        var go = novoUI(nome, pai);
        var img = go.AddComponent<Image>();
        img.sprite = fundo;
        img.type = Image.Type.Sliced;
        rect(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, tam);
        return go;
    }

    static void titulo(Transform pai, string texto, float tamanho, Vector2 pos)
    {
        var go = novoUI("Titulo", pai);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tamanho;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        if (fonte != null) tmp.font = fonte;
        rect(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(320, 60));
    }

    static void criarBotao(Transform pai, string texto, float posY, UnityAction aoClicar, Sprite fundo)
    {
        var go = novoUI(texto, pai);
        var img = go.AddComponent<Image>();
        img.sprite = fundo;
        img.type = Image.Type.Sliced;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        rect(go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, posY), new Vector2(240, 46));

        var t = novoUI("Texto", go.transform);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        if (fonte != null) tmp.font = fonte;
        esticar(t, Vector2.zero, Vector2.one);

        UnityEventTools.AddPersistentListener(btn.onClick, aoClicar);
    }

    static void garantirEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    static void wire(Component c, params (string nome, Object valor)[] campos)
    {
        var so = new SerializedObject(c);
        foreach (var campo in campos)
        {
            var p = so.FindProperty(campo.nome);
            if (p != null) p.objectReferenceValue = campo.valor;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
