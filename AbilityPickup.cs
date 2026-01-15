using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    [Header("What ability do we give?")]
    public bool unlockDoubleJump = true;

    [Header("Effects")]
    public GameObject pickupEffect; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                if (unlockDoubleJump)
                {
                    player.hasDoubleJump = true;
                    Debug.Log("Double jump taken!");
                }

                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}