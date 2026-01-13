using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Урон от касания тела")]
    public int contactDamage = 10;

    private Enemy parentEnemy; 

    void Start()
    {
        parentEnemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (parentEnemy != null && parentEnemy.isDead) return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health == null) health = collision.GetComponentInParent<PlayerHealth>();

            if (health != null)
            {
                Transform pushFrom = parentEnemy != null ? parentEnemy.transform : transform;
                health.TakeDamage(contactDamage, pushFrom);
            }
        }
    }
}