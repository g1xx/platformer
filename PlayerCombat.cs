using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    [Header("Настройки боя")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int damage = 20;

    private Animator anim;
    private PlayerMovement movement;
    private PlayerAudio audioScript;

    private bool isAttacking = false;
    private bool canCombo = false;
    private int comboStep = 0;

    void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        audioScript = GetComponent<PlayerAudio>();
    }

    void Update()
    {
        damage = PlayerPrefs.GetInt("Damage", 20);
        if (Time.timeScale == 0) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (movement != null)
        {
            if (movement.isRolling || movement.isResting) return;

            if (movement.isInputBlocked) return;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            AttemptAttack();
        }
    }

    void AttemptAttack()
    {
        if (!isAttacking)
        {
            StartAttack(1);
        }
        else if (isAttacking && canCombo && comboStep == 1)
        {
            StartAttack(2);
        }
    }

    void StartAttack(int step)
    {
        isAttacking = true;
        canCombo = false;
        comboStep = step;


        anim.SetInteger("ComboStep", step);
        anim.SetTrigger("Attack");

        if (audioScript != null) audioScript.PlayAttackSwing();

        DealDamage();
    }

    public void OpenComboWindow() { canCombo = true; }

    public void FinishAttack()
    {
        isAttacking = false;
        canCombo = false;
        comboStep = 0;

    }

    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        bool hitAnyone = false;

        foreach (Collider2D e in hitEnemies)
        {
            Enemy enemy = e.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform);
                hitAnyone = true;
            }
        }

        if (hitAnyone && audioScript != null)
        {
            audioScript.PlayAttackHit();
        }
    }

    public void CancelAttack()
    {
        isAttacking = false;
        canCombo = false;
        comboStep = 0;
        if (anim != null) { anim.SetInteger("ComboStep", 0); anim.ResetTrigger("Attack"); }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}