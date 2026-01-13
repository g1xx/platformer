using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Настройки")]
    public AudioSource source;

    [Header("Шаги")]
    public AudioClip[] footsteps;

    [Header("Движение")]
    public AudioClip jumpSound;      
    public AudioClip landSound;     
    public AudioClip rollSound;      

    [Header("Бой")]
    public AudioClip[] swordSwings;
    public AudioClip[] swordHits;

    void Start()
    {
        if (source == null) source = GetComponent<AudioSource>();
    }

    public void PlayRoll()
    {
        if (rollSound != null)
        {
            source.pitch = Random.Range(0.9f, 1.1f);
            source.volume = Random.Range(0.9f, 1.0f);
            source.PlayOneShot(rollSound);
        }
    }

    public void PlayFootstep()
    {
        PlayRandomClip(footsteps, 0.9f, 1.1f);
    }

    public void PlayJump()
    {
        if (jumpSound != null)
        {
            source.pitch = Random.Range(0.95f, 1.05f);
            source.PlayOneShot(jumpSound);
        }
    }

    public void PlayLand()
    {
        if (landSound != null)
        {
            source.pitch = Random.Range(0.9f, 1.1f);
            source.PlayOneShot(landSound);
        }
    }

    public void PlayAttackSwing()
    {
        PlayRandomClip(swordSwings, 0.9f, 1.1f);
    }

    public void PlayAttackHit()
    {
        PlayRandomClip(swordHits, 0.8f, 1.0f);
    }

    private void PlayRandomClip(AudioClip[] clips, float minPitch, float maxPitch)
    {
        if (clips.Length == 0) return;
        int index = Random.Range(0, clips.Length);
        source.pitch = Random.Range(minPitch, maxPitch);
        source.volume = Random.Range(0.85f, 1.0f);
        source.PlayOneShot(clips[index]);
    }
}