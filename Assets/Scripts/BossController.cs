using UnityEngine;
using System.Collections;

public enum BossState
{
    Idle,
    Move,
    Attack,
    Hurt,
    Disappear
}

public class BossController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float hurtDuration = 0.5f;
    [SerializeField] private float invulnerabilityDuration = 1f;
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameObject player;
    
    private BossState currentState = BossState.Idle;
    private float currentHealth;
    private float lastAttackTime = -1f;
    private float lastDamageTime = -1f;
    private bool isAttacking = false;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0; // Disable gravity for top-down movement
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.freezeRotation = true; // Prevent rotation
        rb.bodyType = RigidbodyType2D.Dynamic; // Ensure it's dynamic for collisions
        
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store original color for flash effect
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        // Initialize health
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Don't process if in Disappear state
        if (currentState == BossState.Disappear)
        {
            return;
        }
        
        // Don't process if in Hurt state (let coroutine handle it)
        if (currentState == BossState.Hurt)
        {
            UpdateAnimation();
            return;
        }
        
        // Don't move or attack if currently attacking
        if (isAttacking)
        {
            movement = Vector2.zero;
            UpdateAnimation();
            return;
        }
        
        if (player == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        
        // State machine logic
        switch (currentState)
        {
            case BossState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(BossState.Move);
                }
                break;
                
            case BossState.Move:
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(BossState.Attack);
                }
                else if (distanceToPlayer > detectionRange)
                {
                    ChangeState(BossState.Idle);
                }
                else
                {
                    // Chase player
                    Vector2 directionToPlayer = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                    movement = directionToPlayer;
                }
                break;
                
            case BossState.Attack:
                // Stop moving and attack
                movement = Vector2.zero;
                TryAttack();
                break;
        }
        
        UpdateAnimation();
    }
    
    void TryAttack()
    {
        // Check if enough time has passed since last attack
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        // Perform attack
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
                StartCoroutine(AttackEffect());
            }
        }
    }
    
    IEnumerator AttackEffect()
    {
        isAttacking = true;
        
        if (spriteRenderer != null)
        {
            // Flash red when attacking
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }
        
        isAttacking = false;
        
        // After attack, return to Move or Idle based on distance
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= detectionRange)
            {
                ChangeState(BossState.Move);
            }
            else
            {
                ChangeState(BossState.Idle);
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        // Don't take damage if in Disappear state
        if (currentState == BossState.Disappear) return;
        
        // Check invulnerability
        if (Time.time - lastDamageTime < invulnerabilityDuration) return;
        
        // Apply damage
        currentHealth -= damage;
        lastDamageTime = Time.time;
        
        Debug.Log($"Boss took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Check if boss should die
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            ChangeState(BossState.Disappear);
            return;
        }
        
        // Enter hurt state
        ChangeState(BossState.Hurt);
        StartCoroutine(HurtEffect());
    }
    
    IEnumerator HurtEffect()
    {
        if (spriteRenderer != null)
        {
            // Flash effect during hurt
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f); // Light red
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            yield return new WaitForSeconds(hurtDuration);
        }
        
        // After hurt state, return to appropriate state
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= attackRange)
            {
                ChangeState(BossState.Attack);
            }
            else if (distanceToPlayer <= detectionRange)
            {
                ChangeState(BossState.Move);
            }
            else
            {
                ChangeState(BossState.Idle);
            }
        }
        else
        {
            ChangeState(BossState.Idle);
        }
    }
    
    void ChangeState(BossState newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        
        // Handle state-specific initialization
        switch (newState)
        {
            case BossState.Disappear:
                movement = Vector2.zero;
                StartCoroutine(DisappearEffect());
                break;
        }
    }
    
    IEnumerator DisappearEffect()
    {
        // Stop all movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Play disappear animation
        UpdateAnimation();
        
        // Wait for animation to play (adjust duration as needed)
        yield return new WaitForSeconds(1f);
        
        // Disable the boss GameObject
        gameObject.SetActive(false);
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        switch (currentState)
        {
            case BossState.Idle:
                animator.Play("Idle");
                break;
                
            case BossState.Move:
                animator.Play("Move");
                break;
                
            case BossState.Attack:
                animator.Play("Attack");
                break;
                
            case BossState.Hurt:
                animator.Play("Hurt");
                break;
                
            case BossState.Disappear:
                animator.Play("Disappear");
                break;
        }
    }

    void FixedUpdate()
    {
        // Don't move if in Disappear or Hurt state, or if attacking
        if (currentState == BossState.Disappear || 
            currentState == BossState.Hurt || 
            isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Apply movement
        if (movement.magnitude > 0.1f)
        {
            Vector2 newVelocity = movement * moveSpeed;
            rb.linearVelocity = newVelocity;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
