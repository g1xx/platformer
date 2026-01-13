using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMove : MonoBehaviour
{
    [Header("Куда идем?")]
    public string sceneToLoad; 

    [Header("К какому выходу?")]
    public int connectionID; 

    public static int nextConnectionID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            nextConnectionID = connectionID;

            SceneManager.LoadScene(sceneToLoad);
        }
    }
}