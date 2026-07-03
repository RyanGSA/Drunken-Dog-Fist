using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Vida))]
public abstract class InimigoBase : MonoBehaviour, IDanificavel
{
    protected enum Estado { Parado, Perseguindo, Atacando, Atordoado, Morto }

    [Header("Atributos")]
    [SerializeField] protected float velocidade = 5f;
    [SerializeField] protected float alcanceVisao = 6f;
    [SerializeField] protected float alcanceAtaque = 0.8f;
    [SerializeField] protected float tempoAtordoado = 0.25f;
    [SerializeField] protected bool ehChefe;

    [Header("Ataque melee")]
    [SerializeField] protected CaixaDeDano caixaDeDano;
    [SerializeField] protected float danoAtaque = 6f;
    [SerializeField] protected float recarga = 1.2f;
    [SerializeField, Range(1, 3)] protected int golpesNoCombo = 1;
    [SerializeField] protected float inicioGolpe = 0.25f;
    [SerializeField] protected float fimGolpe = 0.45f;
    [SerializeField] protected float duracaoGolpe = 0.6f;
    [SerializeField] protected float tempoParaSumir = 1.2f;

    protected Animator animador;
    protected SpriteRenderer sprite;
    protected Rigidbody2D corpo;
    protected Collider2D colisor;
    protected Vida vida;
    protected Transform alvo;
    private SpriteRenderer spriteAlvo;

    protected Estado estado = Estado.Parado;
    protected bool estaAtacando;
    private float tempoDano;
    private float tempoRecarga;
    private float escalaBaseX = 1f;

    public bool estaMorto => vida.estaMorto;

    protected virtual void Awake()
    {
        corpo = GetComponent<Rigidbody2D>();
        vida = GetComponent<Vida>();
        colisor = GetComponent<Collider2D>();
        animador = GetComponent<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        escalaBaseX = Mathf.Abs(transform.localScale.x);

        // kinematic = parede imovel. o player (dynamic + MovePosition) bate e nao atravessa.
        corpo.bodyType = RigidbodyType2D.Kinematic;
        corpo.gravityScale = 0f;
        corpo.useFullKinematicContacts = true;

        caixaDeDano.valorDano = danoAtaque;
        caixaDeDano.desativar();
    }

    protected virtual void OnEnable()
    {
        vida.aoReceberDano += quandoLevaDano;
        vida.aoMorrer += quandoMorre;
    }

    protected virtual void OnDisable()
    {
        vida.aoReceberDano -= quandoLevaDano;
        vida.aoMorrer -= quandoMorre;
    }

    protected virtual void Start()
    {
        GameObject jogador = GameObject.FindWithTag("player");
        if (jogador != null)
        {
            alvo = jogador.transform;
            spriteAlvo = jogador.GetComponentInChildren<SpriteRenderer>();
        }

        if (ehChefe)
        {
            BarraFlutuante.criar(vida);
            GerenteMusica.tocarChefe();
        }
    }

    protected virtual void Update()
    {
        if (tempoRecarga > 0f) tempoRecarga -= Time.deltaTime;

        switch (estado)
        {
            case Estado.Parado: parado(); break;
            case Estado.Perseguindo: perseguir(); break;
            case Estado.Atordoado: atordoado(); break;
        }

        ordenarProfundidade();
    }

    void parado()
    {
        definirAndando(false);
        virarParaAlvo();
        if (alvo != null && distanciaAlvo() <= alcanceVisao)
            estado = Estado.Perseguindo;
    }

    void perseguir()
    {
        if (alvo == null) { estado = Estado.Parado; return; }

        float dist = distanciaAlvo();
        if (dist > alcanceVisao) { estado = Estado.Parado; return; }
        if (dist <= distanciaDeAtaque()) { iniciarAtaque(); return; }

        virarParaAlvo();
        definirAndando(true);
        corpo.MovePosition(Vector2.MoveTowards(corpo.position, pontoAlvo(), velocidade * Time.deltaTime));
    }

    // atiradores aumentam pra atacar de mais longe
    protected virtual float distanciaDeAtaque() => alcanceAtaque;

    protected virtual void iniciarAtaque()
    {
        if (estaAtacando) return;
        if (tempoRecarga > 0f) { definirAndando(false); return; }

        estado = Estado.Atacando;
        definirAndando(false);
        virarParaAlvo();
        StartCoroutine(rotinaMelee(golpesNoCombo));
    }

    protected IEnumerator rotinaMelee(int golpes)
    {
        estaAtacando = true;

        for (int i = 1; i <= golpes; i++)
        {
            animador.SetTrigger("atacar" + i);
            yield return new WaitForSeconds(inicioGolpe);
            caixaDeDano.ativar();
            yield return new WaitForSeconds(fimGolpe - inicioGolpe);
            caixaDeDano.desativar();
            yield return new WaitForSeconds(duracaoGolpe - fimGolpe);
        }

        terminarAtaque();
    }

    protected void terminarAtaque()
    {
        estaAtacando = false;
        tempoRecarga = recarga;
        caixaDeDano.desativar();
        if (!estaMorto) voltarADecidir();
    }

    void atordoado()
    {
        definirAndando(false);
        tempoDano -= Time.deltaTime;
        if (tempoDano <= 0f) voltarADecidir();
    }

    protected void voltarADecidir() =>
        estado = (alvo != null && distanciaAlvo() <= alcanceVisao) ? Estado.Perseguindo : Estado.Parado;

    public virtual void receberDano(InfoDano dano)
    {
        if (estaMorto) return;
        vida.aplicar(dano);
    }

    protected virtual void quandoLevaDano(InfoDano dano)
    {
        if (estaMorto) return;

        if (estaAtacando)
        {
            StopAllCoroutines();
            estaAtacando = false;
            caixaDeDano.desativar();
        }

        estado = Estado.Atordoado;
        tempoDano = tempoAtordoado;
        animador.SetTrigger("dano");
    }

    void quandoMorre()
    {
        estado = Estado.Morto;
        estaAtacando = false;
        definirAndando(false);
        animador.SetBool("morrendo", true);
        colisor.enabled = false;
        caixaDeDano.desativar();
        finalizarMorte();
    }

    protected virtual void finalizarMorte()
    {
        StopAllCoroutines();
        StartCoroutine(sumir());
    }

    protected IEnumerator sumir()
    {
        yield return new WaitForSeconds(tempoParaSumir);
        Destroy(gameObject);
    }

    protected float distanciaAlvo() =>
        alvo == null ? Mathf.Infinity : Vector2.Distance(corpo.position, pontoAlvo());

    // mira na base (pes) do player, nao no centro do sprite, pra ficar na mesma linha do chao
    protected Vector2 pontoAlvo()
    {
        if (alvo == null) return corpo.position;
        float y = spriteAlvo != null ? spriteAlvo.bounds.min.y : alvo.position.y;
        return new Vector2(alvo.position.x, y);
    }

    // vira o objeto inteiro (sprite + caixa de dano) no eixo x
    protected void virarParaAlvo()
    {
        if (alvo == null) return;
        Vector3 s = transform.localScale;
        s.x = escalaBaseX * (alvo.position.x > transform.position.x ? 1f : -1f);
        transform.localScale = s;
    }

    protected float direcaoOlhando() => transform.localScale.x < 0f ? -1f : 1f;

    protected void definirAndando(bool andando) => animador.SetBool("andando", andando);

    // quem esta mais embaixo na tela aparece na frente
    void ordenarProfundidade() => sprite.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
}
