using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public int health = 30;
    public float velocidade = 3f;
    private PLAYER_ACTIONS controls;
    private Animator anim;
    private Vector2 movement;
    public GameObject hitbox;

    private void Awake()
    {
        controls = new PLAYER_ACTIONS();
        anim = GetComponent<Animator>();

        controls.Player.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => movement = Vector2.zero;

        controls.Player.Light_Atk.started += _ =>
        {
            movement = Vector2.zero;          
            controls.Player.Move.Disable();
            anim.SetTrigger("punch1");
        };

        controls.Player.Heavy_Atk.started += _ =>
        {
            movement = Vector2.zero;
            controls.Player.Move.Disable();
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
    }

    public void DisableHitbox()
    {
        hitbox.SetActive(false);
    }

    private void Update()
    {
        transform.Translate(movement * velocidade * Time.deltaTime);

        anim.SetBool("walking", movement != Vector2.zero);

        if (movement.x < 0)
            transform.localScale = new Vector3(-6, 6, 6);
        else if (movement.x > 0)
            transform.localScale = new Vector3(6, 6, 6);
    }

    public void EndAttack()
    {
        controls.Player.Move.Enable();
        movement = controls.Player.Move.ReadValue<Vector2>();
    }
}