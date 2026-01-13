using UnityEngine;
using UnityEngine.SceneManagement; // Нужно для работы со сценами

public class Coin : MonoBehaviour
{
    [Header("Настройки")]
    public int value = 1;
    public AudioClip pickupSound;

    [Tooltip("Если галочка стоит — монетка исчезнет навсегда после подбора (как тайник). Если нет — будет появляться снова (как лут с врага).")]
    public bool isUnique = true;

    private string uniqueID;

    void Start()
    {
        if (isUnique)
        {
            // ID for coin
            uniqueID = SceneManager.GetActiveScene().name + "_" + gameObject.name;

            // 1 = was taken, 0 = not taken
            if (PlayerPrefs.GetInt(uniqueID) == 1)
            {
                Destroy(gameObject); // if already taken, destroy self
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                player.AddCoins(value);

                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                if (isUnique)
                {
                    PlayerPrefs.SetInt(uniqueID, 1); 
                    PlayerPrefs.Save(); 
                }

                Destroy(gameObject);
            }
        }
    }
}