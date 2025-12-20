using UnityEngine;

/// <summary>
/// Main controller for enemy AI using behavior tree pattern.
/// Delegates to specialized helper classes for better separation of concerns.
/// </summary>
public class EnemyController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string walkAnimationName = "Walk";
    [SerializeField] private string attack01AnimationName = "attack01";
    [SerializeField] private string attack02AnimationName = "attack02";
    [SerializeField] private string hurtAnimationName = "hurt";
    [SerializeField] private string deathAnimationName = "death";
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool usePathfinding = true;

    #endregion

    #region Private Fields

    private EnemyContext context;
    private BTNode behaviorTree;
    private EnemyAnimationController animationController;
    private EnemyDebugDisplay debugDisplay;
    private EnemyComponentInitializer.InitializedComponents components;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        InitializeComponents();
        InitializeContext();
        InitializePlayer();
        InitializeAnimationController();
        InitializeDebugDisplay();
        SubscribeToHealthEvents();
        BuildBehaviorTree();
    }

    void OnDestroy()
    {
        UnsubscribeFromHealthEvents();
    }

    void Update()
    {
        if (context.PlayerTransform == null || context.SpriteRenderer == null) return;

        // Don't process if dead
        if (context.IsDead)
        {
            context.Movement = Vector2.zero;
            animationController.UpdateAnimation(context.IsDead, context.IsHurt, context.IsAttacking, context.Movement);
            return;
        }

        // Handle hurt animation completion
        bool stillHurt = animationController.HandleHurtAnimation(context.IsHurt, context.HurtAnimationStartTime);
        context.IsHurt = stillHurt;

        // Don't process behavior tree if hurt (let animation play)
        if (context.IsHurt)
        {
            animationController.UpdateAnimation(context.IsDead, context.IsHurt, context.IsAttacking, context.Movement);
            return;
        }

        // Update cached distance for debug display
        context.DistanceToPlayer = EnemyRangeDetector.GetDistanceToPlayer(transform.position, context.PlayerTransform);

        // Debug logging
        if (enableDebugLogs && Time.frameCount % EnemyConstants.DEBUG_LOG_FRAME_INTERVAL == 0)
        {
            Debug.Log($"[Enemy {gameObject.name}] Distance={context.DistanceToPlayer:F2}, Range={detectionRange}, " +
                      $"isChasing={context.IsChasing}, movement={context.Movement}, " +
                      $"RB.velocity={context.Rigidbody?.linearVelocity}");
        }

        // Execute behavior tree
        if (behaviorTree != null)
        {
            behaviorTree.Execute(context);
        }

        // Update animation based on movement and state
        animationController.UpdateAnimation(context.IsDead, context.IsHurt, context.IsAttacking, context.Movement);
    }

    void FixedUpdate()
    {
        // Apply movement in FixedUpdate
        if (context.Rigidbody == null) return;

        if (context.Movement.magnitude > EnemyConstants.MOVEMENT_THRESHOLD)
        {
            // Use MovePosition for kinematic rigidbodies (prevents pushing player)
            // For kinematic bodies, MovePosition is the correct way to move
            Vector2 newPosition = (Vector2)transform.position + context.Movement * chaseSpeed * Time.fixedDeltaTime;
            context.Rigidbody.MovePosition(newPosition);

            if (enableDebugLogs && Time.frameCount % EnemyConstants.DEBUG_LOG_FRAME_INTERVAL == 0)
            {
                Debug.Log($"[Enemy {gameObject.name}] FixedUpdate: movement={context.Movement}, " +
                          $"speed={chaseSpeed}, position={transform.position}");
            }
        }
        // No need to set velocity to zero for kinematic bodies - they don't use velocity
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        components = EnemyComponentInitializer.InitializeComponents(gameObject, enableDebugLogs);
    }

    private void InitializeContext()
    {
        context = new EnemyContext
        {
            // References
            SpriteRenderer = components.SpriteRenderer,
            Rigidbody = components.Rigidbody,
            Animator = components.Animator,
            EnemyHealth = components.EnemyHealth,
            Pathfinding = components.Pathfinding,

            // Configuration
            AttackRange = attackRange,
            DetectionRange = detectionRange,
            AttackCooldown = attackCooldown,
            UsePathfinding = usePathfinding,
            Attack01AnimationName = attack01AnimationName,
            Attack02AnimationName = attack02AnimationName,
            EnableDebugLogs = enableDebugLogs,

            // Initial state
            IsDead = false,
            IsHurt = false,
            IsAttacking = false,
            IsChasing = false,
            Movement = Vector2.zero,
            LastMovementDirection = Vector2.zero,
            CurrentAnimationState = "",
            
            // Oscillation detection
            IsOscillating = false,
            OscillationDetectionStartTime = 0f,
            HorizontalDirectionChanges = 0,
            LastHorizontalDirection = 0f
        };
    }

    private void InitializePlayer()
    {
        context.PlayerTransform = EnemyPlayerFinder.FindPlayer(enableDebugLogs);
        
        // Ignore collision with player to prevent pushing
        if (context.PlayerTransform != null)
        {
            PlayerController playerController = context.PlayerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Collider2D enemyCollider = GetComponent<Collider2D>();
                if (enemyCollider != null)
                {
                    playerController.IgnoreCollisionWithEnemy(enemyCollider);
                }
            }
        }
    }

    private void InitializeAnimationController()
    {
        animationController = new EnemyAnimationController(
            components.Animator,
            idleAnimationName,
            walkAnimationName,
            attack01AnimationName,
            attack02AnimationName,
            hurtAnimationName,
            deathAnimationName,
            enableDebugLogs
        );
    }

    private void InitializeDebugDisplay()
    {
        debugDisplay = GetComponent<EnemyDebugDisplay>();
        if (debugDisplay == null)
        {
            debugDisplay = gameObject.AddComponent<EnemyDebugDisplay>();
        }
        debugDisplay.Initialize(
            context,
            transform,
            components.Rigidbody,
            detectionRange,
            attackRange,
            chaseSpeed,
            usePathfinding,
            components.Pathfinding
        );
    }

    private void SubscribeToHealthEvents()
    {
        if (context.EnemyHealth != null)
        {
            context.PreviousHealth = context.EnemyHealth.CurrentHealth;
            context.EnemyHealth.OnHealthChanged += OnHealthChanged;
            context.EnemyHealth.OnDeath += OnDeath;
        }
    }

    private void UnsubscribeFromHealthEvents()
    {
        if (context.EnemyHealth != null)
        {
            context.EnemyHealth.OnHealthChanged -= OnHealthChanged;
            context.EnemyHealth.OnDeath -= OnDeath;
        }
    }

    #endregion

    #region Behavior Tree

    private void BuildBehaviorTree()
    {
        // If already attacking, continue the attack sequence
        BTSequence continueAttackSequence = new BTSequence(
            new IsAttacking(),
            new Attack(transform, animationController)
        );

        // New attack sequence: Check range -> Face player -> Stop movement -> Can attack -> Attack
        BTSequence newAttackSequence = new BTSequence(
            new IsPlayerInAttackRange(transform),
            new FacePlayer(transform),
            new StopMovement(),
            new CanAttack(),
            new Attack(transform, animationController)
        );

        // Attack selector: Continue existing attack OR start new attack
        BTSelector attackSelector = new BTSelector(
            continueAttackSequence,
            newAttackSequence
        );

        // When player is in attack range but can't attack (cooldown), stop movement and face player
        BTSequence waitInAttackRangeSequence = new BTSequence(
            new IsPlayerInAttackRange(transform),
            new FacePlayer(transform),
            new StopMovement()
        );

        // Chase sequence: Check NOT in attack range -> Check detection range -> Calculate direction -> Face movement -> Move
        BTSequence chaseSequence = new BTSequence(
            new IsPlayerNotInAttackRange(transform), // Prevent chase when player is in attack range (including on top)
            new IsPlayerInDetectionRange(transform),
            new CalculateDirectionToPlayer(transform),
            new FaceMovementDirection(),
            new MoveTowardPlayer()
        );

        // Root selector: Try attack, then wait in attack range, then chase, then idle
        behaviorTree = new BTSelector(
            attackSelector,
            waitInAttackRangeSequence, // Stop and face player when in attack range but can't attack
            chaseSequence,
            new Idle()
        );
    }

    #endregion

    #region Health Events

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        // Play hurt animation when taking damage (but not if dead or dying)
        if (!context.IsDead && currentHealth > 0 && components.Animator != null)
        {
            // Check if health actually decreased (damage was taken)
            if (currentHealth < context.PreviousHealth && !context.IsHurt)
            {
                PlayHurtAnimation();
            }
        }

        // Update previous health for next comparison
        context.PreviousHealth = currentHealth;
    }

    private void OnDeath()
    {
        if (context.IsDead) return; // Already dead

        context.IsDead = true;

        // Stop all movement
        context.Movement = Vector2.zero;
        if (context.Rigidbody != null)
        {
            context.Rigidbody.linearVelocity = Vector2.zero;
        }

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Play death animation
        animationController.PlayDeathAnimation();
        context.CurrentAnimationState = deathAnimationName;

        Debug.Log($"Enemy {gameObject.name} died!");
    }

    private void PlayHurtAnimation()
    {
        if (components.Animator == null || context.IsDead) return;

        context.IsHurt = true;
        context.HurtAnimationStartTime = Time.time;
        context.Movement = Vector2.zero; // Stop movement during hurt animation
        
        // Reset attack state when hurt animation interrupts attack
        if (context.IsAttacking)
        {
            context.IsAttacking = false;
        }

        animationController.PlayHurtAnimation();
        context.CurrentAnimationState = hurtAnimationName;
    }

    #endregion
}
