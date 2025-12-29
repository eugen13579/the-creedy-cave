using UnityEngine;

/// <summary>
/// Utility class for finding the player GameObject.
/// Handles multiple fallback methods for player detection.
/// </summary>
public static class EnemyPlayerFinder
{
    /// <summary>
    /// Finds the player using multiple fallback methods.
    /// </summary>
    /// <param name="enableDebugLogs">Whether to log debug messages</param>
    /// <returns>Player Transform if found, null otherwise</returns>
    public static Transform FindPlayer(bool enableDebugLogs = false)
    {
        // Try finding by tag first
        GameObject player = GameObject.FindWithTag("Player");
        
        // Try finding by name as fallback
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        // Try finding by PlayerController component as last resort
        if (player == null)
        {
            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.gameObject;
            }
        }

        if (player != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyPlayerFinder] Found player at {player.transform.position}");
            }
            return player.transform;
        }
        else
        {
            Debug.LogError("[EnemyPlayerFinder] Player not found! Make sure Player has 'Player' tag or is named 'Player'.");
            return null;
        }
    }
}


