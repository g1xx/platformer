using UnityEngine;
using UnityEngine.EventSystems; 

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("References")]
    public AudioSource audioSource; 

    [Header("Sounds")]
    public AudioClip hoverSound; 
    public AudioClip clickSound; 

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = FindFirstObjectByType<AudioSource>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound); 
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}