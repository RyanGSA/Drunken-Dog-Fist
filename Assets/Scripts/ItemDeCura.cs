using UnityEngine;

// item que fica na cena e cura o player ao encostar. some depois de usado.
[RequireComponent(typeof(Collider2D))]
public class ItemDeCura : MonoBehaviour
{
    [SerializeField] private float cura = 25f;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        // ordena junto com os personagens (quem esta mais embaixo aparece na frente)
        var sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) sprite.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (!outro.CompareTag("player")) return;

        var vida = outro.GetComponentInParent<Vida>();
        if (vida == null || vida.estaMorto) return;
        if (vida.vidaAtual >= vida.vidaTotal) return; // vida cheia: nao gasta o item

        vida.curar(cura);
        Destroy(gameObject);
    }
}
