using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public float velocidade = 3f;
    public float danoSocoFraco = 5f;
    public float danoSocoForte = 8f;
    private PLAYER_ACTIONS controls;
    private Animator anim;
    private Rigidbody2D corpo;
    private Vector2 movement;
    public GameObject hitbox;
    private CaixaDeDano caixa;

    private void Awake()
    {
        controls = new PLAYER_ACTIONS();
        anim = GetComponent<Animator>();
        corpo = GetComponent<Rigidbody2D>();
        caixa = hitbox.GetComponent<CaixaDeDano>();

        // dynamic pra colidir com o inimigo, sem gravidade e sem girar.
        // NeverSleep e essencial: corpo dormindo (parado) nao dispara OnTriggerStay2D -> nao tomaria dano parado.
        corpo.bodyType = RigidbodyType2D.Dynamic;
        corpo.gravityScale = 0f;
        corpo.constraints = RigidbodyConstraints2D.FreezeRotation;
        corpo.sleepMode = RigidbodySleepMode2D.NeverSleep;
        corpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // o collider do corpo tem que ser solido (nao-trigger) pra bloquear o inimigo.
        // a caixa de ataque fica no filho "hitbox", entao esse aqui e so o corpo.
        var meuColisor = GetComponent<Collider2D>();
        if (meuColisor != null) meuColisor.isTrigger = false;

        controls.Player.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => movement = Vector2.zero;

        controls.Player.Light_Atk.started += _ =>
        {
            movement = Vector2.zero;
            controls.Player.Move.Disable();
            caixa.valorDano = danoSocoFraco;
            anim.SetTrigger("punch1");
        };

        controls.Player.Heavy_Atk.started += _ =>
        {
            movement = Vector2.zero;
            controls.Player.Move.Disable();
            caixa.valorDano = danoSocoForte;
            anim.SetTrigger("punch2");
        };
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    public void EnableHitbox()
    {
        hitbox.SetActive(true);
        caixa.ativar();
    }

    public void DisableHitbox()
    {
        caixa.desativar();
        hitbox.SetActive(false);
    }

    private void Update()
    {
        anim.SetBool("walking", movement != Vector2.zero);

        if (movement.x < 0)
            transform.localScale = new Vector3(-6, 6, 6);
        else if (movement.x > 0)
            transform.localScale = new Vector3(6, 6, 6);
    }

    // movimento por MovePosition (corpo dynamic): faz varredura e para no contato com o inimigo,
    // em vez de forcar velocidade pra dentro dele (que atravessava)
    private void FixedUpdate()
    {
        corpo.linearVelocity = Vector2.zero;
        corpo.MovePosition(corpo.position + movement * velocidade * Time.fixedDeltaTime);
    }

    public void EndAttack()
    {
        controls.Player.Move.Enable();
        movement = controls.Player.Move.ReadValue<Vector2>();
    }
}