#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
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

// cria a cena do menu principal (titulo + botoes Jogar/Sair ligados no MenuPrincipal)
// e ja poe no Build Settings: menu no indice 0, a fase atual no indice 1.
public static class MontadorMenu
{
    const string sheet = "Assets/Stone UI/Stone_UI_Sprite_Sheet.png";
    const string caminhoMenu = "Assets/Scenes/MenuPrincipal.unity";

    static TMP_FontAsset fonte;

    [MenuItem("Drunken Dog Fist/UI/Montar Menu Principal")]
    static void montar()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().ToDictionary(s => s.name);
        Sprite painel = sprites.ContainsKey("painel") ? sprites["painel"] : null;

        // salva a cena da fase (aberta agora) e guarda o caminho dela pro build settings
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string caminhoJogo = EditorSceneManager.GetActiveScene().path;

        fonte = TMP_Settings.defaultFontAsset;

        var cena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(640, 360);
        scaler.matchWidthOrHeight = 1f;

        if (Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        // fundo escuro
        var fundo = novoUI("Fundo", canvasGo.transform);
        esticar(fundo);
        fundo.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.20f, 1f);

        // titulo
        var tit = novoUI("Titulo", canvasGo.transform);
        rectCentro(tit, new Vector2(0, 95), new Vector2(580, 110));
        var tmp = tit.AddComponent<TextMeshProUGUI>();
        tmp.text = "DRUNKEN DOG FIST";
        tmp.fontSize = 54;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        if (fonte != null) tmp.font = fonte;

        // script do menu, apontando pra cena da fase
        var menu = canvasGo.AddComponent<MenuPrincipal>();
        var so = new SerializedObject(menu);
        var p = so.FindProperty("cenaDoJogo");
        if (p != null && !string.IsNullOrEmpty(caminhoJogo))
            p.stringValue = Path.GetFileNameWithoutExtension(caminhoJogo);
        so.ApplyModifiedPropertiesWithoutUndo();

        criarBotao(canvasGo.transform, "Jogar", -20, menu.jogar, painel);
        criarBotao(canvasGo.transform, "Sair", -90, menu.sair, painel);

        EditorSceneManager.SaveScene(cena, caminhoMenu);

        // build settings: menu = indice 0, fase = indice 1
        var lista = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(caminhoMenu, true) };
        if (!string.IsNullOrEmpty(caminhoJogo))
            lista.Add(new EditorBuildSettingsScene(caminhoJogo, true));
        EditorBuildSettings.scenes = lista.ToArray();

        Debug.Log("Menu Principal criado em " + caminhoMenu + " (indice 0 no Build Settings). Fase: "
            + (string.IsNullOrEmpty(caminhoJogo) ? "salve a fase e adicione no Build Settings" : caminhoJogo));
    }

    // ------------------------------------------------------------ helpers

    static GameObject novoUI(string nome, Transform pai)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        return go;
    }

    static void esticar(GameObject go)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void rectCentro(GameObject go, Vector2 pos, Vector2 tam)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
    }

    static void criarBotao(Transform pai, string texto, float posY, UnityAction aoClicar, Sprite bg)
    {
        var go = novoUI(texto, pai);
        rectCentro(go, new Vector2(0, posY), new Vector2(260, 56));
        var img = go.AddComponent<Image>();
        if (bg != null) { img.sprite = bg; img.type = Image.Type.Sliced; }
        else img.color = new Color(0.25f, 0.28f, 0.35f, 1f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var t = novoUI("Texto", go.transform);
        esticar(t);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        if (fonte != null) tmp.font = fonte;

        UnityEventTools.AddPersistentListener(btn.onClick, aoClicar);
    }
}
#endif
