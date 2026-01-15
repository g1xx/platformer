using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemyAI : MonoBehaviour
{
    [Header("Flight settings")]
    public float speed = 3f;
    public float chaseDistance = 10f;
    public float stopDistance = 0.5f;
    public bool returnToStart = true;

    [Header("Attack Settings)]
    public float attackRange = 1.5f; 
    public float attackCooldown = 1f; 
    private float lastAttackTime;

    private Vector3 startPosition;
    private Transform player;
    private Enemy enemyStats;
    private Rigidbody2D rb;
    private SpriteRenderer spr;
    private Animator anim; 

    void Start()
    {
        enemyStats = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        startPosition = transform.position;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (enemyStats.isDead)
        {
            rb.gravityScale = 3f;
            rb.freezeRotation = false; 
            return;
        }

        if (enemyStats.isHurt) return;

        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer < chaseDistance)
        {
            ChaseTarget(player.position, distToPlayer);

            
            if (distToPlayer <= attackRange)
            {
                if (Time.time > lastAttackTime + attackCooldown)
                {
                    if (anim != null) anim.SetTrigger("Attack");
                    lastAttackTime = Time.time;
                }
            }
        }
        else if (returnToStart)
        {
            float distToHome = Vector2.Distance(transform.position, startPosition);
            if (distToHome > 0.1f)
            {
                ChaseTarget(startPosition, distToHome);
            }
        }
    }

    void ChaseTarget(Vector3 target, float distance)
    {
        if (distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 newPos = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        rb.MovePosition(newPos);

        FlipSprite(target.x);
    }

    void FlipSprite(float targetX)
    {
        if (spr != null)
        {
            spr.flipX = targetX < transform.position.x;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}