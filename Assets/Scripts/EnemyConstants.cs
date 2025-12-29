using UnityEngine;

/// <summary>
/// Constants used throughout the enemy AI system.
/// Centralizes magic numbers for better maintainability.
/// </summary>
public static class EnemyConstants
{
    // Distance thresholds
    public const float MIN_DISTANCE_THRESHOLD = 0.01f; // Minimum distance to consider positions overlapping
    public const float MOVEMENT_THRESHOLD = 0.1f; // Minimum movement magnitude to consider moving
    public const float PATHFINDING_ZERO_THRESHOLD = 0.1f; // Threshold for pathfinding zero vector detection
    
    // Animation timing
    public const float HURT_ANIMATION_START_DELAY = 0.05f; // Delay before checking if hurt animation started
    public const float HURT_ANIMATION_TIMEOUT = 0.2f; // Timeout if hurt animation doesn't start
    
    // Range detection
    public const float ATTACK_RANGE_OVERLAP_MULTIPLIER = 1.2f; // Multiplier for overlap check radius
    
    // Sprite facing
    public const float SPRITE_FLIP_X_THRESHOLD = 0.1f; // Threshold for determining sprite flip direction
    
    // Stuck detection (90-degree case)
    public const float PERPENDICULAR_ANGLE_THRESHOLD = 15f; // Degrees from 90° to consider player perpendicular
    public const float PERPENDICULAR_X_THRESHOLD = 0.3f; // Max X difference when player is directly above/below
    public const float OSCILLATION_DETECTION_TIME = 1.0f; // Time to track for oscillation detection
    public const int OSCILLATION_DIRECTION_COUNT = 4; // Number of direction changes to detect oscillation
    
    // Debug
    public const int DEBUG_LOG_FRAME_INTERVAL = 60; // Log every N frames to avoid spam
}


