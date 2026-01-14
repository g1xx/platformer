using UnityEngine;

public class LootBag : MonoBehaviour
{
    [Header("Что выпадает")]
    public GameObject droppedItemPrefab;

    [Header("Настройки выпадения")]
    public int minDrops = 1; 
    public int maxDrops = 3; 

    public float dropForce = 5f; 

    public void InstantiateLoot(Vector3 spawnPosition)
    {
        if (droppedItemPrefab != null)
        {
            int dropCount = Random.Range(minDrops, maxDrops + 1);

            for (int i = 0; i < dropCount; i++)
            {
                GameObject loot = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);

                Rigidbody2D rb = loot.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float randomX = Random.Range(-1f, 1f);
                    float randomY = Random.Range(1f, 2f); 

                    Vector2 forceDirection = new Vector2(randomX, randomY).normalized;

                    rb.AddForce(forceDirection * dropForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}