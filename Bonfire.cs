using UnityEngine;
using System.Collections; 

public class Bonfire : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject uiPrompt;
    public ParticleSystem fireParticles;

    [Header("Ёффект отдыха")]
    public CanvasGroup restScreen; 
    public float fadeDuration = 1.0f;

    [Header("Bonfire Menu")]
    public BonfireMenu bonfireMenu;

    private bool playerInRange = false;
    private PlayerMovement playerScript;
    private PlayerHealth healthScript;

    void Start()
    {
        if (uiPrompt != null) uiPrompt.SetActive(false);

        if (restScreen != null)
        {
            restScreen.alpha = 0;
            restScreen.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (playerScript != null)
            {
                if (playerScript.isResting)
                {
                    playerScript.StopResting();
                }
                else
                {
                    StartCoroutine(RestRoutine());
                }
            }
        }
    }

    IEnumerator RestRoutine()
    {
        playerScript.StartResting(transform.position);

        if (restScreen != null) 
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                restScreen.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                yield return null;
            }
            restScreen.alpha = 1;
        }

        yield return new WaitForSeconds(0.5f);

        RespawnAllEnemies();
        if (healthScript != null) healthScript.HealFull();
        if (fireParticles != null) fireParticles.Play();

        yield return new WaitForSeconds(0.5f);

        if (bonfireMenu != null)
        {
            bonfireMenu.OpenMenu();

            while (bonfireMenu.menuPanel.activeSelf)
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
            playerScript.StopResting();
        }

        if (restScreen != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                restScreen.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
                yield return null;
            }
            restScreen.alpha = 0;
        }
    }

    void RespawnAllEnemies()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in allEnemies)
        {
            enemy.Respawn();
        }

        Debug.Log("¬раги возрождены: " + allEnemies.Length);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            playerScript = collision.GetComponent<PlayerMovement>();
            healthScript = collision.GetComponent<PlayerHealth>();
            if (uiPrompt != null) uiPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            playerScript = null;
            if (uiPrompt != null) uiPrompt.SetActive(false);
        }
    }
}