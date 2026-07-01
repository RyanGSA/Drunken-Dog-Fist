using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projetil : MonoBehaviour
{
    [SerializeField] private float velocidade = 8f;
    [SerializeField] private float tempoDeVida = 4f;

    private float dano;
    private float empurrao;
    private LayerMask alvos;
    private GameObject dono;
    private Vector2 direcao = Vector2.right;

    public void configurar(Vector2 direcao, float dano, float empurrao, LayerMask alvos, GameObject dono)
    {
        this.direcao = direcao.normalized;
        this.dano = dano;
        this.empurrao = empurrao;
        this.alvos = alvos;
        this.dono = dono;
    }

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        transform.Translate(direcao * (velocidade * Time.deltaTime), Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (dono != null && other.transform.IsChildOf(dono.transform)) return;
        if ((alvos.value & (1 << other.gameObject.layer)) == 0) return; 

        IDanificavel d = other.GetComponentInParent<IDanificavel>();
        if (d != null && !d.estaMorto)
            d.receberDano(new InfoDano(dano, transform.position, dono, empurrao));

        Destroy(gameObject);
    }
}
