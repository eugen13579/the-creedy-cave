using UnityEngine;

/// <summary>
/// Action node: Faces sprite toward player.
/// Handles edge case when player is directly on top.
/// </summary>
public class FacePlayer : BTNode
{
    private Transform enemyTransform;

    public FacePlayer(Transform enemyTransform)
    {
        this.enemyTransform = enemyTransform;
    }

    public override NodeStatus Execute(EnemyContext context)
    {
        if (context.PlayerTransform == null || context.SpriteRenderer == null)
        {
            return NodeStatus.Failure;
        }

        float xDiff = context.PlayerTransform.position.x - enemyTransform.position.x;

        // Handle edge case when player is directly on top
        if (Mathf.Abs(xDiff) < EnemyConstants.MIN_DISTANCE_THRESHOLD)
        {
            // Use last movement direction or default to right
            if (context.LastMovementDirection.magnitude > EnemyConstants.MOVEMENT_THRESHOLD)
            {
                context.SpriteRenderer.flipX = context.LastMovementDirection.x < 0;
            }
            else
            {
                context.SpriteRenderer.flipX = false; // Default to facing right
            }
        }
        else
        {
            // Face player based on X position
            context.SpriteRenderer.flipX = xDiff < 0;
        }

        return NodeStatus.Success;
    }
}

/// <summary>
/// Action node: Faces sprite toward movement direction.
/// </summary>
public class FaceMovementDirection : BTNode
{
    public override NodeStatus Execute(EnemyContext context)
    {
        if (context.SpriteRenderer == null) return NodeStatus.Failure;

        Vector2 movement = context.Movement;
        if (movement.magnitude > EnemyConstants.MOVEMENT_THRESHOLD)
        {
            if (movement.x < -EnemyConstants.SPRITE_FLIP_X_THRESHOLD)
            {
                context.SpriteRenderer.flipX = true;
            }
            else if (movement.x > EnemyConstants.SPRITE_FLIP_X_THRESHOLD)
            {
                context.SpriteRenderer.flipX = false;
            }
        }

        return NodeStatus.Success;
    }
}

/// <summary>
/// Action node: Stops enemy movement.
/// For kinematic bodies, just setting Movement to zero is sufficient.
/// </summary>
public class StopMovement : BTNode
{
    public override NodeStatus Execute(EnemyContext context)
    {
        context.Movement = Vector2.zero;
        // Note: For kinematic bodies, we don't need to set velocity
        // Movement is applied via MovePosition in FixedUpdate
        return NodeStatus.Success;
    }
}

/// <summary>
/// Action node: Performs attack on player.
/// Checks range, plays animation, and deals damage.
/// </summary>
public class Attack : BTNode
{
    private Transform enemyTransform;
    private EnemyAnimationController animationController;

    public Attack(Transform enemyTransform, EnemyAnimationController animationController)
    {
        this.enemyTransform = enemyTransform;
        this.animationController = animationController;
    }

    public override NodeStatus Execute(EnemyContext context)
    {
        if (context.PlayerTransform == null) return NodeStatus.Failure;

        // If already attacking, check if animation is still playing
        if (context.IsAttacking)
        {
            if (animationController.IsAttackAnimationPlaying())
            {
                return NodeStatus.Running; // Still attacking
            }
            // Animation finished, reset attack state
            context.IsAttacking = false;
            return NodeStatus.Success;
        }

        // Check if player is still in range
        bool playerInRange = EnemyRangeDetector.IsPlayerInAttackRange(
            enemyTransform.position,
            context.PlayerTransform,
            context.AttackRange
        );

        if (!playerInRange) return NodeStatus.Failure;

        PlayerHealth playerHealth = context.PlayerTransform.GetComponent<PlayerHealth>();
        if (playerHealth == null || context.EnemyHealth == null) return NodeStatus.Failure;

        // Perform attack
        context.IsAttacking = true;

        // Play attack animation
        string attackAnimation = animationController.PlayAttackAnimation();
        context.CurrentAnimationState = attackAnimation;

        // Deal damage
        playerHealth.TakeDamage(context.EnemyHealth.AttackDamage);
        context.LastAttackTime = Time.time;

        if (context.EnableDebugLogs)
        {
            float distance = EnemyRangeDetector.GetDistanceToPlayer(enemyTransform.position, context.PlayerTransform);
            Debug.Log($"[Attack Node] Attacked player for {context.EnemyHealth.AttackDamage} damage! Distance: {distance:F4}");
        }

        return NodeStatus.Running; // Attack animation is running
    }
}

/// <summary>
/// Action node: Calculates direction toward player.
/// Handles pathfinding and edge cases.
/// </summary>
public class CalculateDirectionToPlayer : BTNode
{
    private Transform enemyTransform;

    public CalculateDirectionToPlayer(Transform enemyTransform)
    {
        this.enemyTransform = enemyTransform;
    }

    public override NodeStatus Execute(EnemyContext context)
    {
        if (context.PlayerTransform == null) return NodeStatus.Failure;

        Vector2 rawDirection = (Vector2)context.PlayerTransform.position - (Vector2)enemyTransform.position;
        float rawDistance = rawDirection.magnitude;

        // Handle case where player is directly on top - stop movement to prevent oscillation
        if (rawDistance < EnemyConstants.MIN_DISTANCE_THRESHOLD)
        {
            // Set movement to zero to prevent any movement when player is directly on top
            context.Movement = Vector2.zero;
            context.IsOscillating = false;
            return NodeStatus.Success;
        }

        // Check if player is at 90 degrees (directly above or below)
        bool isPerpendicular = Mathf.Abs(rawDirection.x) < EnemyConstants.PERPENDICULAR_X_THRESHOLD;
        
        Vector2 directionToPlayer;
        if (context.UsePathfinding && context.Pathfinding != null)
        {
            directionToPlayer = context.Pathfinding.GetDirectionToTarget(
                enemyTransform.position,
                context.PlayerTransform.position
            );
            // Fallback to direct if pathfinding returns zero
            if (directionToPlayer.magnitude < EnemyConstants.PATHFINDING_ZERO_THRESHOLD)
            {
                directionToPlayer = rawDirection.normalized;
            }
        }
        else
        {
            directionToPlayer = rawDirection.normalized;
        }

        // Detect oscillation when player is perpendicular (90 degrees)
        if (isPerpendicular)
        {
            // Track horizontal direction changes
            float currentHorizontalDir = 0f;
            if (directionToPlayer.x < -EnemyConstants.MOVEMENT_THRESHOLD)
                currentHorizontalDir = -1f; // Moving left
            else if (directionToPlayer.x > EnemyConstants.MOVEMENT_THRESHOLD)
                currentHorizontalDir = 1f; // Moving right

            // Check if direction changed
            if (Mathf.Abs(currentHorizontalDir) > 0.1f && 
                Mathf.Abs(context.LastHorizontalDirection) > 0.1f &&
                Mathf.Sign(currentHorizontalDir) != Mathf.Sign(context.LastHorizontalDirection))
            {
                // Direction flipped - increment counter
                context.HorizontalDirectionChanges++;
                
                // Initialize detection time if this is the first change
                if (context.HorizontalDirectionChanges == 1)
                {
                    context.OscillationDetectionStartTime = Time.time;
                }
                
                // Check if we've detected enough oscillations
                if (context.HorizontalDirectionChanges >= EnemyConstants.OSCILLATION_DIRECTION_COUNT)
                {
                    float timeSinceStart = Time.time - context.OscillationDetectionStartTime;
                    if (timeSinceStart < EnemyConstants.OSCILLATION_DETECTION_TIME)
                    {
                        // Oscillating detected! Force vertical movement
                        context.IsOscillating = true;
                        
                        if (context.EnableDebugLogs)
                        {
                            Debug.Log($"[Enemy {enemyTransform.name}] Detected oscillation at 90°! Forcing vertical movement.");
                        }
                        
                        // Force vertical movement (prioritize Y direction)
                        directionToPlayer = new Vector2(0f, Mathf.Sign(rawDirection.y)).normalized;
                        
                        // Try to move vertically, but if blocked, try slight diagonal
                        if (context.Pathfinding != null && 
                            !context.Pathfinding.IsPathClear(enemyTransform.position, directionToPlayer))
                        {
                            // Try slight left diagonal
                            Vector2 leftDiag = new Vector2(-0.3f, Mathf.Sign(rawDirection.y)).normalized;
                            if (context.Pathfinding.IsPathClear(enemyTransform.position, leftDiag))
                            {
                                directionToPlayer = leftDiag;
                            }
                            else
                            {
                                // Try slight right diagonal
                                Vector2 rightDiag = new Vector2(0.3f, Mathf.Sign(rawDirection.y)).normalized;
                                if (context.Pathfinding.IsPathClear(enemyTransform.position, rightDiag))
                                {
                                    directionToPlayer = rightDiag;
                                }
                            }
                        }
                    }
                }
            }
            else if (currentHorizontalDir == 0f || Mathf.Sign(currentHorizontalDir) == Mathf.Sign(context.LastHorizontalDirection))
            {
                // No direction change or same direction - reset if enough time passed
                float timeSinceStart = Time.time - context.OscillationDetectionStartTime;
                if (timeSinceStart > EnemyConstants.OSCILLATION_DETECTION_TIME)
                {
                    context.HorizontalDirectionChanges = 0;
                    context.IsOscillating = false;
                }
            }
            
            context.LastHorizontalDirection = currentHorizontalDir;
        }
        else
        {
            // Not perpendicular - reset oscillation tracking
            context.HorizontalDirectionChanges = 0;
            context.IsOscillating = false;
            context.LastHorizontalDirection = 0f;
        }

        context.Movement = directionToPlayer;
        context.LastMovementDirection = directionToPlayer;

        return NodeStatus.Success;
    }
}

/// <summary>
/// Action node: Validates that movement is set for moving toward player.
/// Actual movement is applied in FixedUpdate.
/// </summary>
public class MoveTowardPlayer : BTNode
{
    public override NodeStatus Execute(EnemyContext context)
    {
        // Movement is applied in FixedUpdate, this just ensures it's set
        if (context.Movement.magnitude > EnemyConstants.MOVEMENT_THRESHOLD)
        {
            return NodeStatus.Success;
        }
        return NodeStatus.Failure;
    }
}

/// <summary>
/// Action node: Sets enemy to idle state.
/// </summary>
public class Idle : BTNode
{
    public override NodeStatus Execute(EnemyContext context)
    {
        context.Movement = Vector2.zero;
        context.IsChasing = false;
        return NodeStatus.Success;
    }
}



