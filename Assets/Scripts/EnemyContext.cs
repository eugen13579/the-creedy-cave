using UnityEngine;

/// <summary>
/// Data container for enemy state and references.
/// Reduces public property exposure and improves encapsulation.
/// </summary>
public class EnemyContext
{
    // State
    public bool IsDead { get; set; }
    public bool IsHurt { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsChasing { get; set; }
    
    // Timing
    public float LastAttackTime { get; set; }
    public float HurtAnimationStartTime { get; set; }
    public float PreviousHealth { get; set; }
    
    // Movement
    public Vector2 Movement { get; set; }
    public Vector2 LastMovementDirection { get; set; }
    
    // Oscillation detection (for 90-degree stuck case)
    public float OscillationDetectionStartTime { get; set; }
    public int HorizontalDirectionChanges { get; set; }
    public float LastHorizontalDirection { get; set; } // -1 for left, 1 for right, 0 for none
    public bool IsOscillating { get; set; }
    
    // Animation
    public string CurrentAnimationState { get; set; }
    
    // References
    public Transform PlayerTransform { get; set; }
    public SpriteRenderer SpriteRenderer { get; set; }
    public Rigidbody2D Rigidbody { get; set; }
    public Animator Animator { get; set; }
    public EnemyHealth EnemyHealth { get; set; }
    public SimplePathfinding2D Pathfinding { get; set; }
    
    // Configuration
    public float AttackRange { get; set; }
    public float DetectionRange { get; set; }
    public float AttackCooldown { get; set; }
    public bool UsePathfinding { get; set; }
    public string Attack01AnimationName { get; set; }
    public string Attack02AnimationName { get; set; }
    public bool EnableDebugLogs { get; set; }
    
    // Cached values
    public float DistanceToPlayer { get; set; }
}

