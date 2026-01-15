using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public static Vector2 lastCheckPointPos;
    public static bool checkpointLoaded = false;

    public static bool isLevelTransition = false;

    [Header("Abilities")]
    public bool hasDoubleJump = true;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;
    public float doubleJumpForce = 14f;
    public float fallMultiplier = 3f;
    public float jumpCutMultiplier = 0.5f;
    public float coyoteTime = 0.1f;

    [Header("Wall Mechanics")]
    public float wallSlideSpeed = 2f; 
    public Vector2 wallJumpForce = new Vector2(8f, 16f); 
    public float wallJumpStopInputTime = 0.2f; 
    [Header("Safe Ground System")]
    public Vector2 currentSafePosition;
    private float safeGroundTimer;
    public float safeCheckOffset = 0.3f;

    [Header("Slope Settings")]
    public float slopeCheckDistance = 0.5f;
    public float maxSlopeAngle = 60f;

    [Header("Roll/Dash Settings")]
    public float rollSpeed = 15f;
    public float rollDuration = 0.4f;
    public float rollCooldown = 0.6f;

    [Header("Inventory")]
    public int coins = 0; 

    private PlayerHealth healthScript;

    public bool isKnockedBack { get; private set; } = false;
    public bool isInputBlocked = false;
    public bool isRolling { get; private set; } = false;
    private bool canRoll = true;
    public bool isResting { get; private set; } = false;

    private bool isTurning = false;
    private int facing = 1;

    private Rigidbody2D rb;
    private CapsuleCollider2D cc;
    private Animator anim;
    private float moveInput;

    [Header("Detection")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);

    public Transform wallCheck;     
    public LayerMask Walls;   
    public float wallCheckRadius = 0.2f;


    private bool isGrounded;
    private bool isTouchingWall;    
    private bool isWallSliding;     

    private int jumpsRemaining;
    private float coyoteTimeCounter;

    private float defaultGravity;
    private bool isJumping;
    private float lastGroundedTime;

    private Vector2 slopeNormalPerp;
    private bool isOnSlope;

    private PlayerAudio audioScript;
    private float airTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<PlayerHealth>();
        facing = (transform.localScale.x > 0) ? 1 : -1;
        defaultGravity = rb.gravityScale;
        currentSafePosition = transform.position;
        audioScript = GetComponent<PlayerAudio>();

        coins = PlayerPrefs.GetInt("SaveCoins", 0);

        if (isLevelTransition)
        {
            isLevelTransition = false; 
            return; 
        }

        if (checkpointLoaded)
        {
            transform.position = lastCheckPointPos;
            currentSafePosition = lastCheckPointPos;
        }
        else if (PlayerPrefs.HasKey("SaveX"))
        {
            LoadGame();
        }
    }

    void Update()
    {

        // --- Cheat for tests ---
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    AddCoins(500);
        //}
        // ----------------------------------

        if (isResting)
        {
            if (Input.GetButtonDown("Jump") || Input.GetAxisRaw("Horizontal") != 0) StopResting();
            return;
        }

        if (!isGrounded)
        {
            airTime += Time.deltaTime;
        }

        if (isKnockedBack || isRolling) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (!isInputBlocked)
        {
            CheckDirection(moveInput);
        }

        if (isInputBlocked) return;

        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && canRoll)
        {
            StartCoroutine(RollRoutine());
            return;
        }

        if (isGrounded && rb.linearVelocity.y <= 0.1f && !isOnSlope && IsPositionSafe())
        {
            safeGroundTimer += Time.deltaTime;
            if (safeGroundTimer > 0.2f) currentSafePosition = transform.position;
        }
        else safeGroundTimer = 0f;

        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, Walls);

        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && moveInput != 0)
        {
            isWallSliding = true;

            jumpsRemaining = hasDoubleJump ? 2 : 1;
        }
        else
        {
            isWallSliding = false;
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpsRemaining = hasDoubleJump ? 2 : 1;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (!isWallSliding)
            {
                if (coyoteTimeCounter < 0 && jumpsRemaining == 2) jumpsRemaining = 1;
                else if (coyoteTimeCounter < 0 && jumpsRemaining == 1 && !hasDoubleJump) jumpsRemaining = 0;
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isWallSliding)
            {
                WallJump();
            }
            else if (coyoteTimeCounter > 0 || jumpsRemaining > 0)
            {
                PerformJump();
            }
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            coyoteTimeCounter = 0;
        }

        HandleAnimations();
    }

    void WallJump()
    {
        isWallSliding = false;
        rb.linearVelocity = Vector2.zero;
        float jumpDir = -facing;
        Vector2 force = new Vector2(jumpDir * wallJumpForce.x, wallJumpForce.y);
        rb.linearVelocity = force;
        FlipInstant((int)jumpDir);
        jumpsRemaining--;

        if (anim != null) anim.SetTrigger("JumpStart");

        if (audioScript != null) audioScript.PlayJump();

        StartCoroutine(WallJumpCooldownRoutine());
    }

    IEnumerator WallJumpCooldownRoutine()
    {
        isInputBlocked = true;
        yield return new WaitForSeconds(wallJumpStopInputTime);
        isInputBlocked = false;
    }

    void PerformJump()
    {
        bool isDoubleJump = (jumpsRemaining == 1 && hasDoubleJump && coyoteTimeCounter < 0);

        if (isDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
            if (anim != null) anim.SetTrigger("DoubleJump");

            if (audioScript != null) audioScript.PlayJump();
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim != null) anim.SetTrigger("JumpStart");

            if (audioScript != null) audioScript.PlayJump();
        }

        lastGroundedTime = -10f;
        jumpsRemaining--;
        coyoteTimeCounter = 0;
        isJumping = true;
        isOnSlope = false;
    }

    void HandleAnimations()
    {
        if (anim != null)
        {
            bool isRunning = Mathf.Abs(moveInput) > 0;
            anim.SetBool("IsRunning", isRunning);

            bool isGroundedForAnim = (Time.time - lastGroundedTime) < 0.2f;
            anim.SetBool("IsGrounded", isGroundedForAnim);

            anim.SetBool("IsWallSliding", isWallSliding);

            if (isGroundedForAnim && rb.linearVelocity.y <= 0.1f)
            {
                anim.SetFloat("VerticalSpeed", 0f);
            }
            else
            {
                anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
            }
        }
    }

    void FixedUpdate()
    {
        CheckGround();
        SlopeCheck();

        if (isKnockedBack) return;

        if (isWallSliding)
        {
            float speedY = Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, speedY);
        }

        if (isInputBlocked && !isRolling && !isWallSliding) 
        {
          
        }
        else if (isInputBlocked) 
        {
            
        }
        else if (isRolling)
        {
            rb.linearVelocity = new Vector2(facing * rollSpeed, rb.linearVelocity.y);
            if (isGrounded) rb.gravityScale = 0f;
            else rb.gravityScale = defaultGravity * fallMultiplier;
            return;
        }
        else if (isGrounded && !isJumping)
        {
            if (moveInput == 0)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
            else
            {
                if (isOnSlope)
                {
                    Vector2 targetVelocity = slopeNormalPerp * moveSpeed * -moveInput;
                    rb.linearVelocity = targetVelocity;
                    rb.gravityScale = 0f;
                }
                else
                {
                    rb.linearVelocity = new Vector2(moveInput * moveSpeed, 0f);
                    rb.gravityScale = 0f;
                }
            }
        }
        else
        {
            if (!isWallSliding) 
            {
                rb.gravityScale = defaultGravity * fallMultiplier;
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -20f, jumpForce * 2));
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    bool IsPositionSafe()
    {
        Vector2 pos = transform.position;
        Vector2 leftOrigin = new Vector2(pos.x - safeCheckOffset, pos.y);
        Vector2 rightOrigin = new Vector2(pos.x + safeCheckOffset, pos.y);
        float rayLength = 1.0f;

        bool leftHit = Physics2D.Raycast(leftOrigin, Vector2.down, rayLength, groundLayer);
        bool rightHit = Physics2D.Raycast(rightOrigin, Vector2.down, rayLength, groundLayer);
        return leftHit && rightHit;
    }

    public void TeleportToSafePos()
    {
        transform.position = currentSafePosition;
        rb.linearVelocity = Vector2.zero;
        transform.position += Vector3.up * 0.2f;
    }

    private void CheckGround()
    {
        bool wasGrounded = isGrounded;

        RaycastHit2D hit = Physics2D.BoxCast(groundCheck.position, groundCheckSize, 0f, Vector2.down, 0.1f, groundLayer);

        if (hit.collider != null && rb.linearVelocity.y <= 0.5f)
        {
            isGrounded = true;
            lastGroundedTime = Time.time;
            if (rb.linearVelocity.y <= 0.1f) isJumping = false;

            if (!wasGrounded && audioScript != null && airTime > 0.2f)
            {
                audioScript.PlayLand();
            }

            airTime = 0f;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void SlopeCheck()
    {
        Vector2 checkPos = transform.position;
        if (cc != null) checkPos = new Vector2(transform.position.x, transform.position.y - cc.size.y / 2 + 0.2f);
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            isOnSlope = (slopeAngle != 0 && slopeAngle < maxSlopeAngle);
        }
        else isOnSlope = false;
    }

    public void ApplyKnockback(Vector2 force, float duration) { StartCoroutine(KnockbackRoutine(force, duration)); }

    public void StartResting(Vector2 bonfirePos)
    {
        isResting = true;
        isInputBlocked = true;

        if (bonfirePos.x > transform.position.x && facing == -1) FlipInstant(1);
        else if (bonfirePos.x < transform.position.x && facing == 1) FlipInstant(-1);

        rb.linearVelocity = Vector2.zero;
        if (anim != null) { anim.SetBool("IsRunning", false); anim.SetBool("IsCrouch", true); }

        lastCheckPointPos = transform.position;
        checkpointLoaded = true;
        currentSafePosition = transform.position;

        SaveGame();
    }
    public void StopResting() { isResting = false; isInputBlocked = false; if (anim != null) anim.SetBool("IsCrouch", false); }

    IEnumerator KnockbackRoutine(Vector2 force, float duration)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = defaultGravity;
        rb.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }

    private void CheckDirection(float direction)
    {
        if (direction == 0) return;
        int inputDir = direction > 0 ? 1 : -1;
        if (inputDir != facing)
        {
            if (!isGrounded || Mathf.Abs(rb.linearVelocity.x) > 2f || isInputBlocked) FlipInstant(inputDir);
            else if (!isTurning) { isTurning = true; if (anim != null) anim.SetTrigger("Turn"); }
        }
    }

    void FlipInstant(int dir)
    {
        facing = dir;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        transform.localScale = s;
        isTurning = false;
    }

    IEnumerator RollRoutine()
    {
        isRolling = true;
        canRoll = false;

        if (healthScript != null) healthScript.SetRollInvulnerability(true);
        if (anim != null) anim.SetTrigger("Roll");

        if (audioScript != null) audioScript.PlayRoll();

        yield return new WaitForSeconds(rollDuration);

        isRolling = false;
        rb.linearVelocity = Vector2.zero;

        if (healthScript != null) healthScript.SetRollInvulnerability(false);

        yield return new WaitForSeconds(rollCooldown - rollDuration);
        canRoll = true;
    }

    public void BlockInput(bool block) { isInputBlocked = block; if (block) rb.linearVelocity = Vector2.zero; }
    public void TurnFlip()
    {
        if (isTurning)
        {
            FlipInstant(-facing);
        }
    }
    public void TurnEnd() { isTurning = false; }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(groundCheck.position - Vector3.up * 0.1f, new Vector3(groundCheckSize.x, groundCheckSize.y, 1));
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("SaveCoins", coins);
        PlayerPrefs.Save();

        Debug.Log("Coins " + coins);
    }

    public void SaveGame()
    {
        PlayerPrefs.SetFloat("SaveX", transform.position.x);
        PlayerPrefs.SetFloat("SaveY", transform.position.y);
        PlayerPrefs.SetString("SaveScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("SaveCoins", coins);

        PlayerPrefs.Save(); 
        Debug.Log("Game saved!");
    }

    public void LoadGame()
    {
        float x = PlayerPrefs.GetFloat("SaveX");
        float y = PlayerPrefs.GetFloat("SaveY");

        Vector2 loadPos = new Vector2(x, y);

        transform.position = loadPos;

        lastCheckPointPos = loadPos;
        currentSafePosition = loadPos;
        checkpointLoaded = true;
        coins = PlayerPrefs.GetInt("SaveCoins", 0);

        Debug.Log("Game loaded you have " + coins + " coins");
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey("SaveX");
        PlayerPrefs.DeleteKey("SaveY");
        PlayerPrefs.DeleteKey("SaveScene");
    }
}