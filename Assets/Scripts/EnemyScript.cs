using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int health = 20;

    public void TakeDamage(int damage)
    {
        health -= damage;

        Debug.Log("Enemy HP: " + health);

        if (health <= 0)
            Destroy(gameObject);
    }
}