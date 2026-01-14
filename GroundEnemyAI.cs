using UnityEngine;

[RequireComponent(typeof(Enemy))] 
public class GroundEnemyAI : MonoBehaviour
{
    private Enemy enemyStats; 
    private Rigidbody2D rb;
    private SpriteRenderer spr;
    private Animator anim;

    [Header("Patrol")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 2f;

    [Header("Atack")]
    public float agroDistance = 5f;
    public float attackRange = 1.2f;

    public float timeBetweenHits = 0.8f;
    public float restTime = 3.0f;
    private int attackStep = 0;
    private float attackTimer;

    [Header("Fight settings")]
    public Transform attackPoint;
    public float weaponHitRadius = 0.4f;
    public int weaponDamage = 20;
    public LayerMask playerLayer;

    [Header("Окружение")]
    public LayerMask obstacleLayer;
    public float ledgeCheckX = 0.7f;
    public float ledgeCheckY = 1.2f;

    private Transform currentPoint;
    private float waitCounter;
    private bool isWaiting;
    private Transform player;

    void Start()
    {
        enemyStats = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (pointA != null) pointA.parent = null;
        if (pointB != null) pointB.parent = null;
        currentPoint = pointB;
        waitCounter = waitTime;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (enemyStats.isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (enemyStats.isHurt) return;

        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool canSee = CheckLineOfSight(dist);

        if (canSee)
        {
            if (dist > attackRange) ChasePlayer();
            else PerformComboAttack();
        }
        else
        {
            attackStep = 0;
            Patrol();
        }
    }

    bool CheckLineOfSight(float dist)
    {
        if (dist > agroDistance) return false;
        Vector2 start = (Vector2)transform.position + Vector2.up * 0.5f;
        Vector2 end = (Vector2)player.position + Vector2.up * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(start, end - start, dist, obstacleLayer);
        return hit.collider == null;
    }

    void Patrol()
    {
        if (isWaiting)
        {
            StopMoving();
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0) { isWaiting = false; waitCounter = waitTime; FlipPoint(); }
        }
        else
        {
            if (anim != null) anim.SetBool("IsWalking", true);
            Vector2 dir = (currentPoint.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
            FaceTarget(currentPoint.position);
            if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f) isWaiting = true;
        }
    }

    void ChasePlayer()
    {
        waitCounter = waitTime;
        isWaiting = false;

        float dirSign = (player.position.x > transform.position.x) ? 1 : -1;

        Vector2 checkPos = new Vector2(transform.position.x + (ledgeCheckX * dirSign), transform.position.y);
        if (Physics2D.Raycast(checkPos, Vector2.down, ledgeCheckY, obstacleLayer).collider == null)
        {
            StopMoving();
        }
        else
        {
            if (anim != null) anim.SetBool("IsWalking", true);
            rb.linearVelocity = new Vector2(dirSign * speed, rb.linearVelocity.y);
        }
        FaceTarget(player.position);
    }

    void PerformComboAttack()
    {
        StopMoving();
        FaceTarget(player.position);

        if (attackStep == 0 && Time.time > attackTimer)
        {
            if (anim != null) anim.SetTrigger("Attack1");
            attackTimer = Time.time + timeBetweenHits;
            attackStep = 1;
        }
        else if (attackStep == 1 && Time.time > attackTimer)
        {
            if (anim != null) anim.SetTrigger("Attack2");
            attackTimer = Time.time + restTime;
            attackStep = 2;
        }
        else if (attackStep == 2 && Time.time > attackTimer)
        {
            attackStep = 0;
        }
    }

    public void AttackHit()
    {
        if (enemyStats.isDead) return;

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, weaponHitRadius, playerLayer);
        if (hit != null)
        {
            hit.GetComponent<PlayerHealth>()?.TakeDamage(weaponDamage, transform);
        }
    }

    void StopMoving() { rb.linearVelocity = Vector2.zero; if (anim) anim.SetBool("IsWalking", false); }
    void FaceTarget(Vector3 target) { if (spr) spr.flipX = (target.x < transform.position.x); }
    void FlipPoint() { currentPoint = (currentPoint == pointB) ? pointA : pointB; }
}