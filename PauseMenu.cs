using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;
    public static bool GameIsPaused = false;

    [Header("Sounds")]
    public AudioSource audioSource; 
    public AudioClip openSound;     
    public AudioClip closeSound;    

    void Start()
    {
        if (audioSource == null)
            audioSource = FindFirstObjectByType<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        if (audioSource != null && closeSound != null)
            audioSource.PlayOneShot(closeSound);
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}