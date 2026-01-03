using UnityEngine;

/// <summary>
/// Simple 2D pathfinding using raycasting for obstacle avoidance.
/// Better alternative to NavMesh for 2D games.
/// </summary>
public class SimplePathfinding2D : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    [SerializeField] private float raycastDistance = 2f; // How far ahead to check for obstacles
    [SerializeField] private float avoidanceDistance = 1.5f; // Distance to maintain from obstacles
    [SerializeField] private int raycastCount = 8; // Number of rays to cast in a circle
    [SerializeField] private LayerMask obstacleLayerMask = -1; // What layers count as obstacles
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRays = false;
    
    /// <summary>
    /// Gets a safe direction toward target, avoiding obstacles.
    /// Returns normalized direction vector.
    /// </summary>
    public Vector2 GetDirectionToTarget(Vector2 currentPosition, Vector2 targetPosition)
    {
        Vector2 directDirection = (targetPosition - currentPosition).normalized;
        
        // Check if direct path is clear
        if (IsPathClear(currentPosition, directDirection))
        {
            return directDirection;
        }
        
        // Direct path blocked, find alternative path
        return FindBestDirection(currentPosition, targetPosition, directDirection);
    }
    
    /// <summary>
    /// Checks if a path in the given direction is clear of obstacles.
    /// </summary>
    public bool IsPathClear(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, raycastDistance, obstacleLayerMask);
        
        if (showDebugRays)
        {
            Color rayColor = hit.collider == null ? Color.green : Color.red;
            Debug.DrawRay(origin, direction * raycastDistance, rayColor, 0.1f);
        }
        
        return hit.collider == null;
    }
    
    /// <summary>
    /// Finds the best direction to move, avoiding obstacles.
    /// Uses multiple raycasts in a circle to find open paths.
    /// </summary>
    private Vector2 FindBestDirection(Vector2 currentPosition, Vector2 targetPosition, Vector2 preferredDirection)
    {
        Vector2 bestDirection = preferredDirection;
        float bestScore = float.NegativeInfinity;
        
        // Cast rays in a circle around the preferred direction
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (360f / raycastCount) * i;
            Vector2 testDirection = RotateVector(preferredDirection, angle);
            
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, testDirection, raycastDistance, obstacleLayerMask);
            
            if (showDebugRays)
            {
                Color rayColor = hit.collider == null ? Color.yellow : Color.red;
                Debug.DrawRay(currentPosition, testDirection * raycastDistance, rayColor, 0.1f);
            }
            
            if (hit.collider == null)
            {
                // Path is clear, calculate score based on how close to target direction
                float distanceToTarget = Vector2.Distance(currentPosition + testDirection * raycastDistance, targetPosition);
                float alignmentScore = Vector2.Dot(testDirection, preferredDirection); // How aligned with preferred direction
                float score = alignmentScore * 2f - distanceToTarget; // Prefer aligned directions closer to target
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = testDirection;
                }
            }
            else
            {
                // Path blocked, but check if we can get closer by moving along the obstacle
                float hitDistance = hit.distance;
                if (hitDistance > avoidanceDistance)
                {
                    // We can move closer before hitting the obstacle
                    float distanceToTarget = Vector2.Distance(currentPosition + testDirection * hitDistance, targetPosition);
                    float alignmentScore = Vector2.Dot(testDirection, preferredDirection);
                    float score = alignmentScore - distanceToTarget * 0.5f;
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDirection = testDirection;
                    }
                }
            }
        }
        
        return bestDirection.normalized;
    }
    
    /// <summary>
    /// Rotates a vector by the given angle in degrees.
    /// </summary>
    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
    
    /// <summary>
    /// Checks if the enemy can reach the target (not completely blocked).
    /// </summary>
    public bool CanReachTarget(Vector2 currentPosition, Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - currentPosition).normalized;
        float distance = Vector2.Distance(currentPosition, targetPosition);
        
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, distance, obstacleLayerMask);
        return hit.collider == null;
    }
}



