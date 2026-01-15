using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMove : MonoBehaviour
{
    [Header("Which scene to load?")]
    public string sceneToLoad; 

    [Header("Which exit?")]
    public int connectionID; 

    public static int nextConnectionID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            nextConnectionID = connectionID;
            PlayerMovement.isLevelTransition = true;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}