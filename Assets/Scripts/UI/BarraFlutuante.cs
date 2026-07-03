using UnityEngine;

public class BarraFlutuante : MonoBehaviour
{
    private Vida vida;
    private Transform alvo;
    private SpriteRenderer spriteAlvo;
    private Transform preenchimento;
    private float largura;
    private float altura;

    private static Sprite quadrado;

    public static void criar(Vida vida)
    {
        var sr = vida.GetComponentInChildren<SpriteRenderer>();
        float larg = sr != null ? sr.bounds.size.x * 0.3f : 0.3f;
        float acima = sr != null ? (sr.bounds.max.y - vida.transform.position.y) + -1.4f : -1.4f;

        var go = new GameObject("BarraFlutuante");
        var b = go.AddComponent<BarraFlutuante>();
        b.vida = vida;
        b.alvo = vida.transform;
        b.spriteAlvo = sr;
        b.largura = larg;
        b.altura = acima;

        var fundo = criarParte(go.transform, new Color(0.1f, 0.1f, 0.1f, 0.85f), 30000);
        fundo.localScale = new Vector3(larg + 0.06f, 0.18f, 1f);

        var fill = criarParte(go.transform, new Color(0.85f, 0.2f, 0.2f, 1f), 30001);
        fill.localScale = new Vector3(larg, 0.12f, 1f);
        b.preenchimento = fill;

        vida.aoMorrer += b.sumir;
    }

    static Transform criarParte(Transform pai, Color cor, int ordem)
    {
        var go = new GameObject("parte");
        go.transform.SetParent(pai, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite();
        sr.color = cor;
        sr.sortingOrder = ordem;
        return go.transform;
    }

    static Sprite sprite()
    {
        if (quadrado == null)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            quadrado = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        return quadrado;
    }

    void LateUpdate()
    {
        if (alvo == null) { Destroy(gameObject); return; }

        float cx = spriteAlvo != null ? spriteAlvo.bounds.center.x : alvo.position.x;
        transform.position = new Vector3(cx, alvo.position.y + altura, 0f);

        float frac = Mathf.Clamp01(vida.vidaAtual / vida.vidaTotal);
        float w = largura * frac;
        preenchimento.localScale = new Vector3(w, 0.12f, 1f);
        preenchimento.localPosition = new Vector3(-largura / 2f + w / 2f, 0f, 0f);
    }

    void sumir() => Destroy(gameObject);
}
