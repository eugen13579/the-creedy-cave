using UnityEngine;

/// <summary>
/// Condition node: Checks if enemy is dead.
/// </summary>
public class IsDead : BTNode
{
    public override NodeStatus Execute(EnemyContext context)
    {
        return context.IsDead ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Condition node: Checks if enemy is hurt.
/// </summary>
public class IsHurt : BTNode
{
    public override NodeStatus Execute(EnemyContext context)
    {
        return context.IsHurt ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Condition node: Checks if enemy is currently attacking.
/// </summary>
public class IsAttacking : BTNode
{
    public override NodeStatus Execute(EnemyContext context)
    {
        return context.IsAttacking ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Condition node: Checks if player is within attack range.
/// Uses EnemyRangeDetector for consistent range checking.
/// </summary>
public class IsPlayerInAttackRange : BTNode
{
    private Transform enemyTransform;

    public IsPlayerInAttackRange(Transform enemyTransform)
    {
        this.enemyTransform = enemyTransform;
    }

    public override NodeStatus Execute(EnemyContext context)
    {
        if (context.PlayerTransform == null) return NodeStatus.Failure;

        bool inRange = EnemyRangeDetector.IsPlayerInAttackRange(
            enemyTransform.position,
            context.PlayerTransform,
            context.AttackRange
        );

        return inRange ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Condition node: Checks if player is NOT in attack range.
/// Inverts IsPlayerInAttackRange check to prevent chase when player is in attack range.
/// </summary>
public class IsPlayerNotInAttackRange : BTNode
{
    private Transform enemyTransform;

    public IsPlayerNotInAttackRange(Transform enemyTransform)
    {
        this.enemyTransform = enemyTransform;
    }

    public override NodeStatus Execute(EnemyContext context)
    {
        if (context.PlayerTransform == null) return NodeStatus.Failure;

        bool inRange = EnemyRangeDetector.IsPlayerInAttackRange(
            enemyTransform.position,
            context.PlayerTransform,
            context.AttackRange
        );

        return inRange ? NodeStatus.Failure : NodeStatus.Success;
    }
}

/// <summary>
/// Condition node: Checks if player is within detection range.
/// </summary>
public class IsPlayerInDetectionRange : BTNode
{
    private Transform enemyTransform;

    public IsPlayerInDetectionRange(Transform enemyTransform)
    {
        this.enemyTransform = enemyTransform;
    }

    public override NodeStatus Execute(EnemyContext context)
    {
        if (context.PlayerTransform == null) return NodeStatus.Failure;

        bool inRange = EnemyRangeDetector.IsPlayerInDetectionRange(
            enemyTransform.position,
            context.PlayerTransform,
            context.DetectionRange
        );

        return inRange ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Condition node: Checks if enemy can attack (cooldown has passed).
/// </summary>
public class CanAttack : BTNode
{
    public override NodeStatus Execute(EnemyContext context)
    {
        if (Time.time - context.LastAttackTime < context.AttackCooldown)
        {
            return NodeStatus.Failure;
        }
        return NodeStatus.Success;
    }
}



