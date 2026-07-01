using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OndaSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Grupo
    {
        public GameObject prefab;
        public int quantidade = 1;
        public Transform ponto;   // define explicitamente onde vai nascer. se nulo, vai rodando pelos PontosDeSpawn do OndaSpawner
    }

    [System.Serializable]
    public class Onda
    {
        public Grupo[] grupos;
    }

    [SerializeField] private Onda[] ondas;
    [SerializeField] private Transform[] pontosDeSpawn;
    [SerializeField] private float intervaloEntreOndas = 1.5f;
    [SerializeField] private float espalhamento = 0.5f;   // raio aleatorio pra nao nascer tudo empilhado
    [SerializeField] private bool iniciarNoStart = true;

    [Tooltip("Disparado quando todas as ondas forem limpas (chame o boss/checkpoint aqui).")]
    public UnityEvent aoLimparSala;

    private readonly List<GameObject> ativos = new List<GameObject>();
    private bool rodando;

    void Start()
    {
        if (iniciarNoStart) iniciar();
    }

    // chamar isso quando o player entra na sala
    public void iniciar()
    {
        if (rodando) return;
        rodando = true;
        StartCoroutine(rotinaOndas());
    }

    IEnumerator rotinaOndas()
    {
        for (int i = 0; i < ondas.Length; i++)
        {
            spawnarOnda(ondas[i]);

            while (!ondaLimpa())
                yield return null;

            if (i < ondas.Length - 1)
                yield return new WaitForSeconds(intervaloEntreOndas);
        }

        rodando = false;
        aoLimparSala?.Invoke();
    }

    void spawnarOnda(Onda onda)
    {
        ativos.Clear();
        if (onda == null || onda.grupos == null) return;

        int idx = 0;
        foreach (var g in onda.grupos)
        {
            if (g == null || g.prefab == null) continue;
            for (int n = 0; n < g.quantidade; n++)
            {
                Vector3 origem = g.ponto != null ? g.ponto.position : posicaoDeSpawn(idx++);
                Vector3 pos = origem + (Vector3)(Random.insideUnitCircle * espalhamento);
                ativos.Add(Instantiate(g.prefab, pos, Quaternion.identity));
            }
        }
    }

    bool ondaLimpa()
    {
        foreach (var go in ativos)
        {
            if (go == null) continue;
            var v = go.GetComponent<Vida>();
            if (v != null && !v.estaMorto) return false;
        }
        return true;
    }

    Vector3 posicaoDeSpawn(int i)
    {
        if (pontosDeSpawn == null || pontosDeSpawn.Length == 0)
            return transform.position;
        var p = pontosDeSpawn[i % pontosDeSpawn.Length];
        return p != null ? p.position : transform.position;
    }

    void OnDrawGizmosSelected()
    {
        if (pontosDeSpawn == null) return;
        Gizmos.color = Color.cyan;
        foreach (var p in pontosDeSpawn)
            if (p != null) Gizmos.DrawWireSphere(p.position, 0.3f);
    }
}
