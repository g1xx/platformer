using UnityEngine;
using TMPro;

public class BonfireMenu : MonoBehaviour
{
    [Header("Price settings (Health)")]
    public int healthBaseCost = 50;      
    public int healthCostIncrease = 50;
    public int healthIncreaseAmount = 1;

    [Header("Price settings (Damage)")]
    public int damageBaseCost = 100;
    public int damageCostIncrease = 100;
    public int damageIncreaseAmount = 5;

    [Header("UI Links")]
    public GameObject menuPanel;
    public TextMeshProUGUI coinsText;

    public TextMeshProUGUI healthButtonText;
    public TextMeshProUGUI damageButtonText;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerCombat playerCombat;

    private int currentHealthCost;
    private int currentDamageCost;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            playerCombat = playerObj.GetComponent<PlayerCombat>();
        }

        if (menuPanel != null) menuPanel.SetActive(false);

        UpdateCosts();
    }

    void Update()
    {
        if (menuPanel.activeSelf && playerMovement != null && coinsText != null)
        {
            coinsText.text = "Гео: " + playerMovement.coins;
        }
    }

    void UpdateCosts()
    {
        int healthLevel = PlayerPrefs.GetInt("HealthLevel", 0);
        int damageLevel = PlayerPrefs.GetInt("DamageLevel", 0);

        currentHealthCost = healthBaseCost + (healthLevel * healthCostIncrease);
        currentDamageCost = damageBaseCost + (damageLevel * damageCostIncrease);

        if (healthButtonText != null)
            healthButtonText.text = $"+HP ({currentHealthCost}g)";

        if (damageButtonText != null)
            damageButtonText.text = $"+DMG ({currentDamageCost}g)";
    }

    public void BuyHealthUpgrade()
    {
        if (playerHealth != null && playerHealth.hearts != null && playerHealth.maxHealth >= playerHealth.hearts.Length)
        {
            Debug.Log("Maximum health!");
            if (healthButtonText != null) healthButtonText.text = "MAXED";
            return;
        }

        if (playerMovement.coins >= currentHealthCost)
        {
            playerMovement.AddCoins(-currentHealthCost);

            playerHealth.maxHealth += healthIncreaseAmount;
            playerHealth.HealFull();

            PlayerPrefs.SetInt("MaxHealth", playerHealth.maxHealth);

            int currentLevel = PlayerPrefs.GetInt("HealthLevel", 0);
            PlayerPrefs.SetInt("HealthLevel", currentLevel + 1);

            PlayerPrefs.Save();

            UpdateCosts();

            Debug.Log("Health improved! New price: " + currentHealthCost);
        }
        else
        {
            Debug.Log("Not enough money! Needed: " + currentHealthCost);
        }
    }

    public void BuyDamageUpgrade()
    {
        if (playerMovement.coins >= currentDamageCost)
        {
            playerMovement.AddCoins(-currentDamageCost);

            playerCombat.damage += damageIncreaseAmount;

            PlayerPrefs.SetInt("Damage", playerCombat.damage);

            int currentLevel = PlayerPrefs.GetInt("DamageLevel", 0);
            PlayerPrefs.SetInt("DamageLevel", currentLevel + 1);

            PlayerPrefs.Save();

            UpdateCosts();

            Debug.Log("Damage improved! New price: " + currentDamageCost);
        }
        else
        {
            Debug.Log("Not enough money! Needed: " + currentDamageCost);
        }
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
        if (playerMovement != null) playerMovement.StopResting();
    }

    public void OpenMenu()
    {
        UpdateCosts();
        menuPanel.SetActive(true);
    }
}