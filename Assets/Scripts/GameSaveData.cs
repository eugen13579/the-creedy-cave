using UnityEngine;
using System;

/// <summary>
/// Serializable data structure for storing game save state.
/// Contains all necessary information to restore the game to a saved state.
/// </summary>
[System.Serializable]
public class GameSaveData
{
    // Player position
    public Vector3 playerPosition;
    
    // Player health and stats
    public float currentHealth;
    public float maxHealth;
    public float attackDamage;
    
    // Inventory data
    public string[] inventoryWeaponNames; // Store weapon names, load by name
    public string equippedWeaponName;
    
    // Currency and resources
    public int coinCount;
    public int arrowCount;
    
    // Scene information
    public string currentSceneName;
    
    // Metadata
    public string saveTimestamp;
    
    /// <summary>
    /// Creates a new GameSaveData with default values.
    /// </summary>
    public GameSaveData()
    {
        playerPosition = Vector3.zero;
        currentHealth = 0f;
        maxHealth = 0f;
        attackDamage = 0f;
        inventoryWeaponNames = new string[0];
        equippedWeaponName = "";
        coinCount = 0;
        arrowCount = 0;
        currentSceneName = "";
        saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}



