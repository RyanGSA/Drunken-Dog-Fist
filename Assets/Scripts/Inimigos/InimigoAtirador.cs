using System.Collections;
using UnityEngine;

public class InimigoAtirador : InimigoBase
{
    [Header("Tiro")]
    [SerializeField] protected GameObject prefabProjetil;
    [SerializeField] protected Transform pontoDeTiro;
    [SerializeField] protected float alcanceTiro = 6f;
    [SerializeField] protected float danoTiro = 5f;
    [SerializeField] protected float empurraoTiro = 2f;
    [SerializeField] protected float delayTiro = 0.3f;
    [SerializeField] protected float cadenciaTiro = 1.5f;
    [SerializeField] protected int tirosAntesDeRecarregar = 3;
    [SerializeField] protected float tempoRecarregar = 1.4f;
    [SerializeField] protected LayerMask alvoDoTiro;

    private int tirosDados;

    protected override float distanciaDeAtaque() =>
        prefabProjetil != null ? alcanceTiro : alcanceAtaque;

    protected override void iniciarAtaque()
    {
        if (estaAtacando) return;

        estado = Estado.Atacando;
        definirAndando(false);
        virarParaAlvo();
        StartCoroutine(rotinaTiro());
    }

    protected virtual IEnumerator rotinaTiro()
    {
        estaAtacando = true;

        if (animador != null) animador.SetTrigger("atirar");
        yield return new WaitForSeconds(delayTiro);
        if (!estaMorto) dispararProjetil();
        tirosDados++;

        yield return new WaitForSeconds(cadenciaTiro);

        if (tirosDados >= tirosAntesDeRecarregar)
        {
            tirosDados = 0;
            if (animador != null) animador.SetTrigger("recarregar");
            yield return new WaitForSeconds(tempoRecarregar);
        }

        estaAtacando = false;
        if (!estaMorto)
            estado = (alvo != null && distanciaAlvo() <= alcanceVisao) ? Estado.Perseguindo : Estado.Parado;
    }

    protected void dispararProjetil()
    {
        if (prefabProjetil == null) return;

        Vector3 origem = pontoDeTiro != null ? pontoDeTiro.position : transform.position;
        // mira no player (ou reto pra frente se nao tiver alvo)
        Vector2 dir = alvo != null
            ? ((Vector2)alvo.position - (Vector2)origem).normalized
            : new Vector2(direcaoOlhando(), 0f);

        GameObject go = Instantiate(prefabProjetil, origem, Quaternion.identity);
        Projetil p = go.GetComponent<Projetil>();
        if (p != null) p.configurar(dir, danoTiro, empurraoTiro, alvoDoTiro, gameObject);
    }
}
