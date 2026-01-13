using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    private AudioSource source;

    [Header("Движение (Idle)")]
    public AudioClip[] idleSounds;
    [Range(0f, 1f)] public float idleVolume = 0.5f; 

    [Header("Атака")]
    public AudioClip attackSound;
    [Range(0f, 1f)] public float attackVolume = 1f; 

    [Header("Получение урона")]
    public AudioClip hurtSound;
    [Range(0f, 1f)] public float hurtVolume = 1f; 

    [Header("Смерть")]
    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1f; 

    void Start()
    {
        source = GetComponent<AudioSource>();

        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = 15f;
    }


    public void PlayIdleSound()
    {
        if (idleSounds.Length > 0)
        {
            AudioClip clip = idleSounds[Random.Range(0, idleSounds.Length)];
            PlaySound(clip, idleVolume, 0.9f, 1.1f);
        }
    }

    public void PlayAttackSound()
    {
        PlaySound(attackSound, attackVolume, 0.9f, 1.1f);
    }

    public void PlayHurtSound()
    {
        PlaySound(hurtSound, hurtVolume, 1.1f, 1.3f);
    }

    public void PlayDeathSound()
    {
        PlaySound(deathSound, deathVolume, 0.8f, 1.0f);
    }

    private void PlaySound(AudioClip clip, float volume, float minPitch, float maxPitch)
    {
        if (clip != null)
        {
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(clip, volume);
        }
    }
}