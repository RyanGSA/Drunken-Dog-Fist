using System.Collections;
using UnityEngine;

// liga o player no sistema de dano dos inimigos (IDanificavel).
// o PlayerScript continua cuidando so de input/movimento/animacao.
[RequireComponent(typeof(Vida))]
public class VidaDoJogador : MonoBehaviour, IDanificavel
{
    [SerializeField] private float tempoInvencivel = 0.6f;
    [SerializeField] private TelaGameOver telaGameOver;

    private Vida vida;
    private SpriteRenderer sprite;
    private PlayerScript controle;
    private float invencivelAte;

    public bool estaMorto => vida.estaMorto;

    void Awake()
    {
        vida = GetComponent<Vida>();
        sprite = GetComponent<SpriteRenderer>();
        controle = GetComponent<PlayerScript>();
    }

    void OnEnable()
    {
        vida.aoReceberDano += quandoLevaDano;
        vida.aoMorrer += quandoMorre;
    }

    void OnDisable()
    {
        vida.aoReceberDano -= quandoLevaDano;
        vida.aoMorrer -= quandoMorre;
    }

    public void receberDano(InfoDano dano)
    {
        // janela de invencibilidade pra nao derreter quando varios inimigos cercam
        if (Time.time < invencivelAte) return;
        invencivelAte = Time.time + tempoInvencivel;
        vida.aplicar(dano);
    }

    void quandoLevaDano(InfoDano dano)
    {
        if (estaMorto) return;
        StartCoroutine(piscarVermelho());
    }

    // o player nao tem animacao de dano, entao so pisca o sprite
    IEnumerator piscarVermelho()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
    }

    void quandoMorre()
    {
        controle.enabled = false; // o OnDisable do PlayerScript ja desliga o input
        StopAllCoroutines();
        StartCoroutine(morrer());
    }

    IEnumerator morrer()
    {
        // sem animacao de morte: apaga o sprite aos poucos
        sprite.color = Color.white;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            Color c = sprite.color;
            c.a = 1f - t / 0.5f;
            sprite.color = c;
            yield return null;
        }

        if (telaGameOver != null) telaGameOver.mostrar();
    }
}
