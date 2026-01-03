using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement, combat, and animations.
/// Handles both melee and ranged attacks, health management, and input processing.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackRange = 1.5f; // Distance to attack enemies
    [SerializeField] private float attackCooldown = 1.0f; // Time between attacks
    [SerializeField] private float rangedAttackCooldown = 0.5f; // Time between ranged attacks
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string walkAnimationName = "Walk";
    [SerializeField] private string attack01AnimationName = "Attack01";
    [SerializeField] private string attack03AnimationName = "Attack03"; // Ranged attack animation
    [SerializeField] private string hurtAnimationName = "Hurt";
    [SerializeField] private float footstepCooldown = 0.3f; // Time between footstep sounds
    [SerializeField] private Sprite arrowSprite; // Arrow sprite to use for projectiles
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Animator animator;
    private InputAction moveAction;
    private InputAction attackAction;
    private PlayerHealth playerHealth;
    private AudioSource audioSource;
    private AudioClip[] footstepClips;
    private string currentAnimationState = "";
    private float lastAttackTime = 0f;
    private float lastRangedAttackTime = 0f;
    private float lastFootstepTime = 0f;
    private bool isAttacking = false;
    private bool isHurt = false;
    private float previousHealth = 0f;
    private float hurtAnimationStartTime = 0f;
    private bool hurtAnimationExists = true; // Cache whether hurt animation exists
    private bool hurtAnimationWarningShown = false; // Track if we've already warned about missing animation
    private Vector2 lastMovementDirection = Vector2.right; // Track last movement direction for arrow firing

    public CoinManager coinManager;

    void Start()
    {
        // Get or add Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure Rigidbody2D for top-down movement
        rb.gravityScale = 0; // No gravity for top-down movement
        rb.freezeRotation = true; // Prevent rotation
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        // Keep as Dynamic to use velocity-based movement
        // We'll prevent enemy pushing via Physics2D.IgnoreCollision

        // Get SpriteRenderer component for flipping
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on Player. Sprite flipping will not work.");
        }

        // Get Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on Player. Animation will not work.");
        }
        else
        {
            // Check if hurt animation exists in the Animator Controller
            CheckHurtAnimationExists();
        }
        
        // Get PlayerHealth component
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth not found on Player. Attack damage will not work.");
        }
        else
        {
            // Subscribe to health events
            previousHealth = playerHealth.CurrentHealth;
            playerHealth.OnHealthChanged += OnHealthChanged;
        }

        // Get or add AudioSource component for footstep sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Load footstep sound clips
        #if UNITY_EDITOR
        footstepClips = new AudioClip[]
        {
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Assets/Sound/16_human_walk_stone_1.wav"),
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Assets/Sound/16_human_walk_stone_2.wav"),
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Assets/Sound/16_human_walk_stone_3.wav")
        };
        #else
        // At runtime, load from Resources folder (files need to be moved to Resources/Assets/Sound/ for this to work)
        // Alternatively, assign clips in inspector via SerializeField
        footstepClips = new AudioClip[]
        {
            Resources.Load<AudioClip>("Assets/Sound/16_human_walk_stone_1"),
            Resources.Load<AudioClip>("Assets/Sound/16_human_walk_stone_2"),
            Resources.Load<AudioClip>("Assets/Sound/16_human_walk_stone_3")
        };
        #endif

        // Get or add BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // Prevent enemies from pushing player by ignoring collision with enemy colliders
        // This allows enemies to detect player (via OverlapCircle) but not push them
        IgnoreEnemyCollisions();

        // Setup Input System - use project-wide actions if available
        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.Enable();
            }
            else
            {
                Debug.LogWarning("Move action not found in InputSystem.actions. Make sure project-wide actions are configured.");
            }
        }
        else
        {
            Debug.LogWarning("InputSystem.actions is null. Please configure project-wide actions in Project Settings > Input System Package > Input Actions.");
        }
        
        // Setup attack input (J key)
        // Use Keyboard.current for direct key input if InputSystem.actions doesn't have attack action
        if (Keyboard.current != null)
        {
            // Attack will be handled in Update() using Keyboard.current.jKey
        }
    }

    void Update()
    {
        // Handle melee attack input (J key)
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
        {
            // Check if bow is equipped - if so, use ranged attack instead
            if (IsBowEquipped())
            {
                FireArrow();
            }
            else
            {
                AttackNearbyEnemies();
            }
        }
        
        // Handle ranged attack input (R key) - alternative input for ranged attack
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (IsBowEquipped())
            {
                FireArrow();
            }
        }
        
        // Handle hurt animation completion
        if (isHurt)
        {
            if (animator != null)
            {
                // Wait a small amount of time for animation to start (at least 0.05 seconds)
                float timeSinceHurtStart = Time.time - hurtAnimationStartTime;
                if (timeSinceHurtStart < 0.05f)
                {
                    // Animation just started, wait a bit before checking
                    return;
                }
                
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                // Check if hurt animation exists and has finished, or if animation doesn't exist
                bool hurtAnimExists = !string.IsNullOrEmpty(hurtAnimationName);
                bool hurtAnimPlaying = hurtAnimExists && stateInfo.IsName(hurtAnimationName);
                bool hurtAnimFinished = hurtAnimPlaying && stateInfo.normalizedTime >= 1.0f;
                
                if (hurtAnimFinished)
                {
                    // Hurt animation finished
                    isHurt = false;
                    currentAnimationState = ""; // Reset to allow animation change
                    Debug.Log($"[Player] Hurt animation finished");
                }
                else if (hurtAnimPlaying)
                {
                    // Still playing hurt animation, don't process other logic
                    return;
                }
                else if (hurtAnimExists)
                {
                    // Animation name exists but not playing - might be an issue, but wait a bit more
                    if (timeSinceHurtStart > 0.2f)
                    {
                        // Been waiting too long, animation might not exist in animator
                        if (!hurtAnimationWarningShown)
                        {
                            Debug.LogWarning($"[Player] Hurt animation '{hurtAnimationName}' not found in Animator Controller. Hurt animation will be skipped.");
                            hurtAnimationWarningShown = true;
                            hurtAnimationExists = false; // Cache that it doesn't exist
                        }
                        isHurt = false;
                        currentAnimationState = ""; // Reset to allow animation change
                    }
                    else
                    {
                        // Still waiting for animation to start
                        return;
                    }
                }
                else
                {
                    // Hurt animation doesn't exist, just clear the hurt state
                    isHurt = false;
                }
            }
            else
            {
                // No animator, just clear hurt state
                isHurt = false;
            }
        }
        
        if (moveAction == null) return;
        
        // Read movement input
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        
        // Update last movement direction for arrow firing
        if (moveInput.magnitude > 0.1f)
        {
            lastMovementDirection = moveInput.normalized;
        }
        
        // Check if player is moving
        bool isMoving = moveInput.magnitude > 0.1f;
        
        // Play footstep sounds when moving
        if (isMoving && !isHurt && !isAttacking && audioSource != null && footstepClips != null && footstepClips.Length > 0)
        {
            if (Time.time - lastFootstepTime >= footstepCooldown)
            {
                // Pick a random footstep sound
                AudioClip randomFootstep = footstepClips[Random.Range(0, footstepClips.Length)];
                if (randomFootstep != null)
                {
                    audioSource.PlayOneShot(randomFootstep);
                    lastFootstepTime = Time.time;
                }
            }
        }
        
        // Play appropriate animation and ensure it loops (even if Animation Clip doesn't have loop enabled)
        if (animator != null)
        {
            // Don't change animation if currently hurt
            if (isHurt)
            {
                return;
            }
            
            // Don't change animation if currently attacking
            if (isAttacking)
            {
                // Check if attack animation has finished
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(attack01AnimationName) || stateInfo.IsName(attack03AnimationName))
                {
                    if (stateInfo.normalizedTime >= 1.0f)
                    {
                        // Attack animation finished, return to normal animations
                        isAttacking = false;
                        currentAnimationState = ""; // Reset to allow animation change
                    }
                }
            }
            else
            {
                string targetAnimation = isMoving ? walkAnimationName : idleAnimationName;
                
                // Check if we need to switch animations
                if (currentAnimationState != targetAnimation)
                {
                    animator.Play(targetAnimation, 0, 0f);
                    currentAnimationState = targetAnimation;
                }
                // Check if current animation has finished and restart it to create loop effect
                else if (currentAnimationState != "")
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    // If animation has finished (normalizedTime >= 1.0) and it's still the same state, restart it
                    if (stateInfo.IsName(currentAnimationState) && stateInfo.normalizedTime >= 1.0f)
                    {
                        animator.Play(currentAnimationState, 0, 0f); // Restart from beginning to create loop
                    }
                }
            }
        }
        
        // Apply movement (but stop movement during hurt animation)
        Vector2 movement = isHurt ? Vector2.zero : moveInput * moveSpeed;
        rb.linearVelocity = movement;
        
        // Flip sprite when moving left
        if (spriteRenderer != null)
        {
            if (moveInput.x < 0)
            {
                // Moving left - flip sprite
                spriteRenderer.flipX = true;
            }
            else if (moveInput.x > 0)
            {
                // Moving right - unflip sprite
                spriteRenderer.flipX = false;
            }
        }
    }

    void OnDisable()
    {
        // Disable input action when object is disabled
        if (moveAction != null)
        {
            moveAction.Disable();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from health events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for coin collection
        if (other.CompareTag("Coin"))
        {
            // Notify CoinManager to collect the coin
            if (coinManager != null)
            {
                coinManager.CollectCoin();
            }
            else
            {
                Debug.LogWarning("CoinManager reference is not set in PlayerController.");
            }

            // Destroy the collected coin
            Destroy(other.gameObject);
        }
    }
    
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        // Play hurt animation when taking damage (but not if dead)
        if (currentHealth > 0 && animator != null)
        {
            // Check if health actually decreased (damage was taken)
            if (currentHealth < previousHealth && !isHurt && !isAttacking)
            {
                PlayHurtAnimation();
            }
        }
        
        // Update previous health for next comparison
        previousHealth = currentHealth;
    }
    
    private void PlayHurtAnimation()
    {
        if (animator == null) return;
        
        // If we know the animation doesn't exist, skip it
        if (!hurtAnimationExists)
        {
            return;
        }
        
        isHurt = true;
        hurtAnimationStartTime = Time.time;
        
        if (animator != null)
        {
            animator.Play(hurtAnimationName, 0, 0f);
            currentAnimationState = hurtAnimationName;
            Debug.Log($"[Player] Playing hurt animation: {hurtAnimationName}");
        }
    }
    
    /// <summary>
    /// Checks if the hurt animation exists in the Animator Controller.
    /// Since we can't directly check state names, we'll verify when trying to play.
    /// </summary>
    private void CheckHurtAnimationExists()
    {
        if (animator == null || string.IsNullOrEmpty(hurtAnimationName))
        {
            hurtAnimationExists = false;
            return;
        }
        
        // We can't reliably check if a state exists without playing it
        // So we'll assume it exists and verify when actually playing
        // The Update() method will detect if it doesn't exist and cache the result
        hurtAnimationExists = true;
    }
    
    /// <summary>
    /// Prevents enemies from pushing the player by ignoring collisions between player and enemy colliders.
    /// Enemies can still detect the player via OverlapCircle for attack range.
    /// </summary>
    private void IgnoreEnemyCollisions()
    {
        if (boxCollider == null) return;
        
        // Find all enemies and ignore collision with their colliders
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null)
            {
                Physics2D.IgnoreCollision(boxCollider, enemyCollider, true);
            }
        }
        
        // Also set up to ignore collisions with enemies spawned later
        // This will be handled by enemies when they spawn (they can call a method on player)
    }
    
    /// <summary>
    /// Called by enemies when they spawn to ignore collision with player.
    /// </summary>
    public void IgnoreCollisionWithEnemy(Collider2D enemyCollider)
    {
        if (boxCollider != null && enemyCollider != null)
        {
            Physics2D.IgnoreCollision(boxCollider, enemyCollider, true);
        }
    }
    
    private void AttackNearbyEnemies()
    {
        // Check cooldown
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }
        
        // Don't attack if already attacking
        if (isAttacking)
        {
            return;
        }
        
        if (playerHealth == null) return;
        
        // Play attack animation
        if (animator != null)
        {
            animator.Play(attack01AnimationName, 0, 0f);
            currentAnimationState = attack01AnimationName;
            isAttacking = true;
        }
        
        // Find all enemies in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        
        bool hitEnemy = false;
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // Deal damage to enemy
                    enemyHealth.TakeDamage(playerHealth.AttackDamage);
                    hitEnemy = true;
                    Debug.Log($"Player attacked enemy for {playerHealth.AttackDamage} damage!");
                }
            }
        }
        
        if (hitEnemy)
        {
            lastAttackTime = Time.time;
        }
    }
    
    /// <summary>
    /// Checks if the bow is currently equipped.
    /// </summary>
    /// <returns>True if bow is equipped</returns>
    private bool IsBowEquipped()
    {
        if (InventoryController.Instance == null)
        {
            return false;
        }
        
        // Get weapon cell from InventoryController
        CellController weaponCell = InventoryController.Instance.WeaponCell;
        if (weaponCell == null)
        {
            // Try to get from weaponCellGameObject
            if (InventoryController.Instance.weaponCellGameObject != null)
            {
                weaponCell = InventoryController.Instance.weaponCellGameObject.GetComponent<CellController>();
            }
        }
        
        if (weaponCell == null)
        {
            // Try to find weapon cell by name
            GameObject weaponCellObj = GameObject.Find("WeaponCell");
            if (weaponCellObj != null)
            {
                weaponCell = weaponCellObj.GetComponent<CellController>();
            }
        }
        
        if (weaponCell != null && weaponCell.currentItem != null)
        {
            // Check if equipped weapon is the bow (by name)
            return weaponCell.currentItem.itemName == "Bow";
        }
        
        return false;
    }
    
    /// <summary>
    /// Fires an arrow projectile toward the mouse cursor.
    /// </summary>
    private void FireArrow()
    {
        // Check cooldown
        if (Time.time - lastRangedAttackTime < rangedAttackCooldown)
        {
            return;
        }
        
        // Check if player has arrows
        if (ArrowInventory.Instance == null)
        {
            Debug.LogWarning("ArrowInventory not found! Cannot fire arrow.");
            return;
        }
        
        if (!ArrowInventory.Instance.HasArrows())
        {
            Debug.Log("No arrows available!");
            return;
        }
        
        // Use an arrow
        if (!ArrowInventory.Instance.UseArrow())
        {
            return;
        }
        
        if (playerHealth == null) return;
        
        // Play Attack03 animation
        if (animator != null)
        {
            animator.Play(attack03AnimationName, 0, 0f);
            currentAnimationState = attack03AnimationName;
            isAttacking = true;
        }
        
        // Calculate direction based on player's facing direction
        Vector2 direction;
        
        // Use current movement direction if moving
        if (moveAction != null)
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            if (moveInput.magnitude > 0.1f)
            {
                // Player is moving - use movement direction
                direction = moveInput.normalized;
            }
            else
            {
                // Player is not moving - use last movement direction or sprite facing
                if (lastMovementDirection.magnitude > 0.1f)
                {
                    direction = lastMovementDirection;
                }
                else if (spriteRenderer != null)
                {
                    // Fall back to sprite facing direction (left or right)
                    direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
                }
                else
                {
                    // Default to right if nothing else available
                    direction = Vector2.right;
                }
            }
        }
        else
        {
            // No move action - use sprite facing or default
            if (spriteRenderer != null)
            {
                direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            }
            else
            {
                direction = lastMovementDirection.magnitude > 0.1f ? lastMovementDirection : Vector2.right;
            }
        }
        
        // Create arrow GameObject dynamically
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.position = transform.position;
        arrowObj.tag = "Arrow";
        
        // Add ArrowProjectile component
        ArrowProjectile arrow = arrowObj.AddComponent<ArrowProjectile>();
        
        // Initialize arrow with direction, damage, and sprite
        if (arrow != null)
        {
            arrow.Initialize(direction, playerHealth.AttackDamage, arrowSprite);
        }
        else
        {
            Debug.LogError("ArrowProjectile component not found on arrow GameObject!");
        }
        
        lastRangedAttackTime = Time.time;
        Debug.Log($"Fired arrow! Direction: {direction}, Remaining arrows: {ArrowInventory.Instance.GetArrowCount()}");
    }
}
