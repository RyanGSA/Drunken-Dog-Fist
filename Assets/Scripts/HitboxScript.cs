using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int damage = 5;
    private bool hasHit = false;

    private void OnTriggerEnter2D(Collider2D other)
    {   
        if (other.CompareTag("enemy") && !hasHit)
        {
            EnemyScript enemy = other.GetComponent<EnemyScript>();

            if (enemy != null)
                enemy.TakeDamage(damage);
            hasHit = true;
        }
    }
}