using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton manager that subscribes to damage events and spawns damage numbers.
/// Automatically finds and subscribes to EnemyHealth and PlayerHealth components.
/// </summary>
public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }
    
    [SerializeField] private GameObject damageNumberPrefab;
    
    private List<EnemyHealth> trackedEnemies = new List<EnemyHealth>();
    private List<PlayerHealth> trackedPlayers = new List<PlayerHealth>();
    private bool isInitialized = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DamageNumberManager: Initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureManagerExists()
    {
        if (Instance == null)
        {
            GameObject managerObj = new GameObject("DamageNumberManager");
            managerObj.AddComponent<DamageNumberManager>();
            Debug.Log("DamageNumberManager: Auto-created in scene");
        }
    }
    
    void Start()
    {
        Initialize();
        
        // Periodically refresh subscriptions to catch newly spawned entities
        InvokeRepeating(nameof(RefreshSubscriptions), 1f, 2f);
    }
    
    void OnEnable()
    {
        // Re-initialize when enabled to catch newly spawned entities
        if (isInitialized)
        {
            RefreshSubscriptions();
        }
    }
    
    void OnDisable()
    {
        CancelInvoke(nameof(RefreshSubscriptions));
    }
    
    void OnDestroy()
    {
        UnsubscribeAll();
    }
    
    /// <summary>
    /// Initializes the manager and subscribes to existing health components.
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;
        
        // Set prefab in spawner if available
        if (damageNumberPrefab != null)
        {
            DamageNumberSpawner.SetDamageNumberPrefab(damageNumberPrefab);
        }
        
        // Subscribe to existing entities
        RefreshSubscriptions();
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Refreshes subscriptions to all EnemyHealth and PlayerHealth components in the scene.
    /// </summary>
    private void RefreshSubscriptions()
    {
        UnsubscribeAll();
        
        // Find and subscribe to all enemies
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        Debug.Log($"DamageNumberManager: Found {enemies.Length} enemies to subscribe to");
        foreach (EnemyHealth enemy in enemies)
        {
            SubscribeToEnemy(enemy);
        }
        
        // Find and subscribe to all players
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        Debug.Log($"DamageNumberManager: Found {players.Length} players to subscribe to");
        foreach (PlayerHealth player in players)
        {
            SubscribeToPlayer(player);
        }
    }
    
    /// <summary>
    /// Manually subscribe to an enemy's damage events.
    /// </summary>
    public void SubscribeToEnemy(EnemyHealth enemy)
    {
        if (enemy == null || trackedEnemies.Contains(enemy)) return;
        
        enemy.OnDamageTaken += OnEnemyDamageTaken;
        trackedEnemies.Add(enemy);
    }
    
    /// <summary>
    /// Manually subscribe to a player's damage events.
    /// </summary>
    public void SubscribeToPlayer(PlayerHealth player)
    {
        if (player == null || trackedPlayers.Contains(player)) return;
        
        player.OnDamageTaken += OnPlayerDamageTaken;
        trackedPlayers.Add(player);
    }
    
    /// <summary>
    /// Unsubscribe from an enemy's damage events.
    /// </summary>
    public void UnsubscribeFromEnemy(EnemyHealth enemy)
    {
        if (enemy == null) return;
        
        enemy.OnDamageTaken -= OnEnemyDamageTaken;
        trackedEnemies.Remove(enemy);
    }
    
    /// <summary>
    /// Unsubscribe from a player's damage events.
    /// </summary>
    public void UnsubscribeFromPlayer(PlayerHealth player)
    {
        if (player == null) return;
        
        player.OnDamageTaken -= OnPlayerDamageTaken;
        trackedPlayers.Remove(player);
    }
    
    private void UnsubscribeAll()
    {
        // Unsubscribe from all enemies
        foreach (EnemyHealth enemy in trackedEnemies)
        {
            if (enemy != null)
            {
                enemy.OnDamageTaken -= OnEnemyDamageTaken;
            }
        }
        trackedEnemies.Clear();
        
        // Unsubscribe from all players
        foreach (PlayerHealth player in trackedPlayers)
        {
            if (player != null)
            {
                player.OnDamageTaken -= OnPlayerDamageTaken;
            }
        }
        trackedPlayers.Clear();
    }
    
    private void OnEnemyDamageTaken(EnemyHealth enemy, float damage)
    {
        if (enemy != null && enemy.transform != null)
        {
            // Get enemy's position - try to use sprite renderer bounds for better positioning
            Vector3 spawnPosition = enemy.transform.position;
            
            // If enemy has a SpriteRenderer, use its bounds to get the top center
            SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.bounds.size.y > 0)
            {
                // Position at the top of the sprite
                spawnPosition = spriteRenderer.bounds.center + Vector3.up * (spriteRenderer.bounds.extents.y + 0.3f);
            }
            else
            {
                // Fallback: use transform position with offset
                spawnPosition = enemy.transform.position + Vector3.up * 1.0f;
            }
            
            Debug.Log($"DamageNumberManager: Enemy {enemy.gameObject.name} took {damage} damage. Transform pos: {enemy.transform.position}, Spawn pos: {spawnPosition}");
            
            // Get sprite renderer for better positioning
            SpriteRenderer enemySpriteRenderer = enemy.GetComponent<SpriteRenderer>();
            
            // Spawn damage number and make it follow the enemy
            DamageNumberSpawner.SpawnDamageNumber(spawnPosition, damage, enemy.transform, enemySpriteRenderer);
        }
        else
        {
            Debug.LogWarning($"DamageNumberManager: OnEnemyDamageTaken called but enemy is null!");
        }
    }
    
    private void OnPlayerDamageTaken(PlayerHealth player, float damage)
    {
        if (player != null && player.transform != null)
        {
            // Get player's position - try to use sprite renderer bounds for better positioning
            Vector3 spawnPosition = player.transform.position;
            
            // If player has a SpriteRenderer, use its bounds to get the top center
            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.bounds.size.y > 0)
            {
                // Position at the top of the sprite
                spawnPosition = spriteRenderer.bounds.center + Vector3.up * (spriteRenderer.bounds.extents.y + 0.3f);
            }
            else
            {
                // Fallback: use transform position with offset
                spawnPosition = player.transform.position + Vector3.up * 1.0f;
            }
            
            Debug.Log($"DamageNumberManager: Player took {damage} damage at {spawnPosition}");
            
            // Get sprite renderer for better positioning
            SpriteRenderer playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            
            // Spawn damage number and make it follow the player
            DamageNumberSpawner.SpawnDamageNumber(spawnPosition, damage, player.transform, playerSpriteRenderer);
        }
    }
    
    /// <summary>
    /// Called when new enemies or players are spawned. Refreshes subscriptions.
    /// </summary>
    public void OnEntitySpawned()
    {
        RefreshSubscriptions();
    }
}


