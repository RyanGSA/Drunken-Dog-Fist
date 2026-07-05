using System.Collections;
using UnityEngine;
public class Chefe : InimigoAtirador
{
    [Header("Chefe")]
    [SerializeField] private float tempoTelegrafo = 0.5f; 

    [Header("Cachorros pos-morte")]
    [SerializeField] private GameObject prefabCachorro;  
    [SerializeField] private int cachorrosMin = 2;
    [SerializeField] private int cachorrosMax = 7;


    protected override void quandoLevaDano(InfoDano dano)
    {
        return;
    }
    protected override void iniciarAtaque()
    {
        if (estaAtacando) return;
        StartCoroutine(rotinaTelegrafo());
    }

    IEnumerator rotinaTelegrafo()
    {
        estaAtacando = true;
        estado = Estado.Atacando;
        definirAndando(false);
        virarParaAlvo();

        yield return new WaitForSeconds(tempoTelegrafo);

        estaAtacando = false;
        if (prefabProjetil != null)
            base.iniciarAtaque();                       // boss atirador
        else
            StartCoroutine(rotinaMelee(golpesNoCombo)); // boss corpo a corpo
    }

    protected override void finalizarMorte()
    {
        spawnCachorros();
        base.finalizarMorte();
    }

    void spawnCachorros()
    {
        if (prefabCachorro == null) return;
        int qtd = Random.Range(cachorrosMin, cachorrosMax + 1);
        for (int i = 0; i < qtd; i++)
        {
            Vector2 pos = corpo.position + new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1f, 1f));
            Instantiate(prefabCachorro, pos, Quaternion.identity);
        }
    }
}
