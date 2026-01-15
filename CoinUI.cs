using UnityEngine;
using TMPro; 

public class CoinUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI coinText; 
    private PlayerMovement player;   

    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>();

        if (player == null)
        {
            Debug.LogError("Player not found!");
        }
    }

    void Update()
    {
        if (player != null && coinText != null)
        {
            coinText.text = player.coins.ToString();
        }
    }
}