using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Характеристики")]
    public int maxHealth = 100;
    protected int currentHealth;

    [Header("Отбрасывание")]
    public float knockbackForceX = 5f; 
    public float knockbackForceY = 2f;
    public float stunDuration = 0.3f;

    public bool fallsOnDeath = false;

    protected Animator anim;
    protected Rigidbody2D rb;
    protected EnemyAudio audioScript;
    protected LootBag lootBag;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private RigidbodyType2D startBodyType;

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isHurt = false;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioScript = GetComponent<EnemyAudio>();
        lootBag = GetComponent<LootBag>();

        startPosition = transform.position;
        startRotation = transform.rotation;
        if (rb != null) startBodyType = rb.bodyType;
    }

    public virtual void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (anim != null) anim.SetTrigger("Hurt");

        if (audioScript != null) audioScript.PlayHurtSound();

        // ЛОГИКА ОТБРАСЫВАНИЯ
        if (attacker != null && rb != null && !isDead)
        {
            StopCoroutine("ResetHurt"); 
            isHurt = true;

            rb.linearVelocity = Vector2.zero;

            float direction = Mathf.Sign(transform.position.x - attacker.position.x);
            Vector2 knockback = new Vector2(direction * knockbackForceX, knockbackForceY);
            rb.AddForce(knockback, ForceMode2D.Impulse);

          
            StartCoroutine(ResetHurt());
        }

        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;

        if (lootBag != null) lootBag.InstantiateLoot(transform.position);

        isDead = true;
        if (audioScript != null) audioScript.PlayDeathSound();

        if (anim != null)
        {
            anim.ResetTrigger("Hurt");
            anim.SetBool("IsDead", true);
            anim.SetBool("IsWalking", false);
        }

        if (fallsOnDeath)
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 3f;
                rb.freezeRotation = false;
            }
        }
        else
        {
            Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D c in allColliders) c.enabled = false;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
            }
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != this && s is not EnemyAudio) s.enabled = false;
        }
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        transform.position = startPosition;
        transform.rotation = startRotation;

        if (rb != null)
        {
            rb.bodyType = startBodyType;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = (startBodyType == RigidbodyType2D.Dynamic) ? 1f : 0f;
            rb.freezeRotation = true;
            if (fallsOnDeath) rb.gravityScale = 0f;
        }

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D c in allColliders) c.enabled = true;

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != this) s.enabled = true;
        }
    }

    IEnumerator ResetHurt()
    {
        yield return new WaitForSeconds(stunDuration);
        isHurt = false;
    }
}