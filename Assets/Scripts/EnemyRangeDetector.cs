using UnityEngine;

/// <summary>
/// Centralized logic for detecting player range.
/// Handles both distance and overlap checks to avoid code duplication.
/// </summary>
public static class EnemyRangeDetector
{
    /// <summary>
    /// Checks if player is within attack range using both distance and overlap checks.
    /// </summary>
    /// <param name="enemyPosition">Enemy's world position</param>
    /// <param name="playerTransform">Player's transform</param>
    /// <param name="attackRange">Attack range distance</param>
    /// <returns>True if player is in attack range</returns>
    public static bool IsPlayerInAttackRange(Vector2 enemyPosition, Transform playerTransform, float attackRange)
    {
        if (playerTransform == null) return false;

        float distance = Vector2.Distance(enemyPosition, playerTransform.position);

        // Check distance first (fastest check)
        if (distance <= attackRange || distance < EnemyConstants.MIN_DISTANCE_THRESHOLD)
        {
            return true;
        }

        // Check overlap if close enough (more expensive, so only check when close)
        if (distance < attackRange * EnemyConstants.ATTACK_RANGE_OVERLAP_MULTIPLIER)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(enemyPosition, attackRange);
            foreach (Collider2D col in colliders)
            {
                if (col != null && (col.transform == playerTransform || col.CompareTag("Player")))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if player is within detection range.
    /// </summary>
    /// <param name="enemyPosition">Enemy's world position</param>
    /// <param name="playerTransform">Player's transform</param>
    /// <param name="detectionRange">Detection range distance</param>
    /// <returns>True if player is in detection range</returns>
    public static bool IsPlayerInDetectionRange(Vector2 enemyPosition, Transform playerTransform, float detectionRange)
    {
        if (playerTransform == null) return false;

        float distance = Vector2.Distance(enemyPosition, playerTransform.position);
        return distance <= detectionRange;
    }

    /// <summary>
    /// Calculates distance to player.
    /// </summary>
    /// <param name="enemyPosition">Enemy's world position</param>
    /// <param name="playerTransform">Player's transform</param>
    /// <returns>Distance to player, or float.MaxValue if player not found</returns>
    public static float GetDistanceToPlayer(Vector2 enemyPosition, Transform playerTransform)
    {
        if (playerTransform == null) return float.MaxValue;
        return Vector2.Distance(enemyPosition, playerTransform.position);
    }
}



