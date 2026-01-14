using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Здоровье")]
    public int maxHealth = 3;
    public int currentHealth = 3;

    [Header("Настройки Нокбэка")]
    public float knockbackForceX = 7f;
    public float knockbackForceY = 6f; 
    public float knockbackDuration = 0.3f; 

    [Header("Неуязвимость")]
    public float invulnerabilityTime = 1.5f;
    public float flashSpeed = 0.1f;

    [Header("UI")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerMovement movement;
    private SpriteRenderer[] sprites;
    private PlayerCombat combatScript;

    public bool isInvulnerable { get; private set; } = false;
    private bool isDead = false;

    void Start()
    {

        maxHealth = PlayerPrefs.GetInt("MaxHealth", 3);
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        sprites = GetComponents<SpriteRenderer>();
        if (sprites.Length == 0) sprites = GetComponentsInChildren<SpriteRenderer>();

        movement = GetComponent<PlayerMovement>();
        combatScript = GetComponent<PlayerCombat>();

        currentHealth = maxHealth;
        UpdateHearts();
    }

    void Update()
    {
        UpdateHearts();
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (isInvulnerable || isDead) return;

        if (combatScript != null) combatScript.CancelAttack();

        currentHealth -= damage;
        UpdateHearts();

        if (anim != null) anim.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageRoutine(attacker));
        }
    }

    private IEnumerator DamageRoutine(Transform attacker)
    {
        isInvulnerable = true;

        float dir = Mathf.Sign(transform.position.x - attacker.position.x);

        Vector2 force = new Vector2(dir * knockbackForceX, knockbackForceY);

        if (movement != null)
        {
            movement.ApplyKnockback(force, knockbackDuration);
        }

        yield return new WaitForSeconds(knockbackDuration);

        float timer = 0f;
        while (timer < invulnerabilityTime)
        {
            SetSpriteAlpha(0.5f);
            yield return new WaitForSeconds(flashSpeed);
            SetSpriteAlpha(1f);
            yield return new WaitForSeconds(flashSpeed);
            timer += flashSpeed * 2;
        }

        SetSpriteAlpha(1f);
        isInvulnerable = false;
    }

    private void SetSpriteAlpha(float alpha)
    {
        foreach (var s in sprites)
        {
            if (s != null)
            {
                Color c = s.color;
                c.a = alpha;
                s.color = c;
            }
        }
    }

    public void SetRollInvulnerability(bool isActive)
    {
        if (isDead) return;
        isInvulnerable = isActive;
        SetSpriteAlpha(isActive ? 0.5f : 1f);
    }

    private void UpdateHearts()
    {
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth) hearts[i].sprite = fullHeart;
            else hearts[i].sprite = emptyHeart;
            hearts[i].enabled = (i < maxHealth);
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        if (movement != null) movement.BlockInput(true);
        if (anim != null) anim.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        StartCoroutine(RespawnRoutine());
    }

    public void HealFull()
    {
        currentHealth = maxHealth;
    }

    public void TakeAbyssDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(AbyssRespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator AbyssRespawnRoutine()
    {
        isInvulnerable = true; 

        if (movement != null)
        {
            movement.BlockInput(true);
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0; 
        }

        if (anim != null) anim.SetTrigger("Hurt"); 

        yield return new WaitForSeconds(0.2f);

        if (movement != null)
        {
            movement.TeleportToSafePos();
        }

        yield return new WaitForSeconds(0.3f);

        if (movement != null)
        {
            movement.BlockInput(false);
            GetComponent<Rigidbody2D>().gravityScale = 3f; 
        }

        StartCoroutine(FlashInvulnerability());
    }

    private IEnumerator FlashInvulnerability()
    {
        float timer = 0f;
        while (timer < invulnerabilityTime)
        {
            SetSpriteAlpha(0.5f);
            yield return new WaitForSeconds(flashSpeed);
            SetSpriteAlpha(1f);
            yield return new WaitForSeconds(flashSpeed);
            timer += flashSpeed * 2;
        }
        SetSpriteAlpha(1f);
        isInvulnerable = false;
    }
}