using UnityEngine;

public class Abyss : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                // call TakeAbyssDamage instead of TakeDamage
                health.TakeAbyssDamage(damage);
            }
        }
    }
}