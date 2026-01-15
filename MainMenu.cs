using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        PlayerPrefs.DeleteAll();

        Debug.Log("Saves deleted. Starting a new game...");

        SceneManager.LoadScene(1);
    }

    public void PlayGame()
    {
        if (PlayerPrefs.HasKey("SaveScene"))
        {
            string levelToLoad = PlayerPrefs.GetString("SaveScene");
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void QuitGame()
    {
        Debug.Log("The game is over!");
        Application.Quit();
    }
}