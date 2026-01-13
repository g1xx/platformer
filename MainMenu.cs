using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        PlayerPrefs.DeleteAll();

        Debug.Log("Сохранения удалены. Начинаем новую игру...");

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
        Debug.Log("Игра закрылась!");
        Application.Quit();
    }
}