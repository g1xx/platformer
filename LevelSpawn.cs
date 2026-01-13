using UnityEngine;

public class LevelSpawn : MonoBehaviour
{
    [Header("ID этой точки")]
    public int connectionID; 

    void Start()
    {
        if (connectionID == LevelMove.nextConnectionID)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                player.transform.position = transform.position;

            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}