#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// fatia o spritesheet do Stone UI (464x352) nos pedacos que o jogo usa:
// painel 9-slice, barras de vida (moldura + tira de preenchimento), coracao, botoes e seta.
// os rects foram medidos no png (origem do y fica EMBAIXO no importer).
public static class FatiadorStoneUI
{
    const string caminho = "Assets/Stone UI/Stone_UI_Sprite_Sheet.png";

    [MenuItem("Drunken Dog Fist/UI/Fatiar Stone UI")]
    static void fatiar()
    {
        TextureImporter imp = AssetImporter.GetAtPath(caminho) as TextureImporter;
        if (imp == null)
        {
            Debug.LogError("Nao achei " + caminho);
            return;
        }

        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = 32f;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;

        // spritesheet e obsoleto no unity 6 mas resolve o corte simples sem complicar
#pragma warning disable CS0618
        imp.spritesheet = new[]
        {
            fatia("painel",                  131, 258, 42, 43, new Vector4(8, 8, 8, 8)), // 9-slice
            fatia("barraMoldura",            214, 295, 47, 19),  // vermelha, coracao + calha vazia
            fatia("barraPreenchimento",      336, 284, 16,  4),  // tira laranja (Image Filled)
            fatia("barraChefeMoldura",       343, 295, 46, 19),  // azul, pro chefe
            fatia("barraChefePreenchimento", 336, 268, 16,  4),  // tira azul
            fatia("coracao",                 214, 263, 20, 19),
            fatia("botao",                   101, 102, 22, 22),
            fatia("botaoApertado",            66, 102, 28, 16),
            fatia("seta",                    402, 208, 12, 15),
        };
#pragma warning restore CS0618

        EditorUtility.SetDirty(imp);
        imp.SaveAndReimport();
        Debug.Log("Stone UI fatiado: 9 sprites");
    }

    static SpriteMetaData fatia(string nome, int x, int y, int w, int h, Vector4 borda = default)
    {
        return new SpriteMetaData
        {
            name = nome,
            rect = new Rect(x, y, w, h),
            border = borda,
            alignment = (int)SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f)
        };
    }
}
#endif
