using UnityEngine;

// so pra testar sem depender do jogador: clique da dano no inimigo sob o mouse, E spawna inimigo.
// tirar quando a caixa de dano do jogador estiver pronta.
public class CombateDebug : MonoBehaviour
{
    [SerializeField] private float dano = 10f;
    [SerializeField] private float empurrao = 3f;
    [SerializeField] private LayerMask camadas = ~0;
    [SerializeField] private GameObject prefabInimigo;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Collider2D c = Physics2D.OverlapPoint(mouse, camadas);
            if (c != null)
            {
                IDanificavel alvo = c.GetComponentInParent<IDanificavel>();
                if (alvo != null && !alvo.estaMorto)
                    alvo.receberDano(new InfoDano(dano, mouse, gameObject, empurrao));
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && prefabInimigo != null)
            Instantiate(prefabInimigo, new Vector3(mouse.x, mouse.y, 0f), Quaternion.identity);
    }
}
