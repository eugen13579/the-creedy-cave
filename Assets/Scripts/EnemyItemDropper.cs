using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles spawning pickupable items when an enemy dies.
/// Supports both weapons and coins with configurable spawn rates.
/// </summary>
public class EnemyItemDropper : MonoBehaviour
{
    [Header("Item Drop Configuration")]
    [Tooltip("List of possible items that can drop when this enemy dies")]
    [SerializeField] private List<ItemDropData> possibleDrops = new List<ItemDropData>();
    
    [Header("Spawn Settings")]
    [Tooltip("Random offset range for item spawn positions to prevent overlap")]
    [SerializeField] private float spawnOffsetRange = 0.5f;
    
    [Tooltip("Coin prefab to instantiate (optional, will create at runtime if not set)")]
    [SerializeField] private GameObject coinPrefab;
    
    private EnemyHealth enemyHealth;
    
    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        
        if (enemyHealth == null)
        {
            Debug.LogWarning($"EnemyItemDropper on {gameObject.name} requires an EnemyHealth component!");
            return;
        }
        
        // Subscribe to death event
        enemyHealth.OnDeath += OnEnemyDeath;
        
        Debug.Log($"[EnemyItemDropper] Subscribed to death event on {gameObject.name}. Drop count: {possibleDrops.Count}");
    }
    
    void OnEnable()
    {
        // Also subscribe in OnEnable in case EnemyHealth wasn't ready at Start
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }
        
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnEnemyDeath;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from death event
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= OnEnemyDeath;
        }
    }
    
    private void OnEnemyDeath()
    {
        Debug.Log($"[EnemyItemDropper] OnEnemyDeath called for {gameObject.name}");
        SpawnItems();
    }
    
    /// <summary>
    /// Spawns items based on the possibleDrops list and their spawn rates.
    /// </summary>
    private void SpawnItems()
    {
        Debug.Log($"[EnemyItemDropper] SpawnItems called for {gameObject.name}");
        
        if (possibleDrops == null || possibleDrops.Count == 0)
        {
            Debug.LogWarning($"[EnemyItemDropper] No possible drops configured for {gameObject.name}!");
            return;
        }
        
        Vector3 basePosition = transform.position;
        Debug.Log($"[EnemyItemDropper] Base spawn position: {basePosition}, Drop count: {possibleDrops.Count}");
        
        int spawnedCount = 0;
        
        foreach (ItemDropData dropData in possibleDrops)
        {
            if (dropData == null)
            {
                Debug.LogWarning($"[EnemyItemDropper] Null drop data found in list!");
                continue;
            }
            
            // Roll for spawn chance
            float roll = Random.Range(0f, 1f);
            Debug.Log($"[EnemyItemDropper] Rolling for {dropData.itemType}: roll={roll:F2}, spawnRate={dropData.spawnRate:F2}");
            
            if (roll <= dropData.spawnRate)
            {
                // Calculate spawn position with random offset
                Vector3 spawnPosition = basePosition + GetRandomOffset();
                
                // Spawn the appropriate item type
                switch (dropData.itemType)
                {
                    case ItemDropData.ItemType.Weapon:
                        SpawnWeapon(dropData.weaponData, spawnPosition);
                        spawnedCount++;
                        break;
                    
                    case ItemDropData.ItemType.Coin:
                        SpawnCoin(dropData.coinValue, spawnPosition);
                        spawnedCount++;
                        break;
                }
            }
            else
            {
                Debug.Log($"[EnemyItemDropper] Failed spawn roll for {dropData.itemType} (roll {roll:F2} > rate {dropData.spawnRate:F2})");
            }
        }
        
        Debug.Log($"[EnemyItemDropper] Spawned {spawnedCount} item(s) from {possibleDrops.Count} possible drop(s)");
    }
    
    /// <summary>
    /// Gets a random offset vector within the spawn offset range.
    /// </summary>
    private Vector3 GetRandomOffset()
    {
        float offsetX = Random.Range(-spawnOffsetRange, spawnOffsetRange);
        float offsetY = Random.Range(-spawnOffsetRange, spawnOffsetRange);
        return new Vector3(offsetX, offsetY, 0f);
    }
    
    /// <summary>
    /// Spawns a weapon pickup at the specified position.
    /// </summary>
    private void SpawnWeapon(WeaponData weaponData, Vector3 position)
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"[EnemyItemDropper] Cannot spawn weapon: WeaponData is null!");
            return;
        }
        
        // Create new GameObject for weapon pickup
        GameObject weaponPickup = new GameObject($"WeaponPickup_{weaponData.weaponName}");
        weaponPickup.transform.position = position;
        
        // Ensure it's not parented to the enemy (in case enemy gets destroyed)
        weaponPickup.transform.SetParent(null);
        
        // Add required components
        SpriteRenderer spriteRenderer = weaponPickup.AddComponent<SpriteRenderer>();
        CircleCollider2D collider = weaponPickup.AddComponent<CircleCollider2D>();
        Pickupable pickupable = weaponPickup.AddComponent<Pickupable>();
        
        // Configure Pickupable component
        pickupable.weaponData = weaponData;
        
        // Set sprite
        if (weaponData.icon != null)
        {
            spriteRenderer.sprite = weaponData.icon;
        }
        else
        {
            Debug.LogWarning($"[EnemyItemDropper] WeaponData {weaponData.weaponName} has no icon sprite!");
        }
        
        // Configure collider
        collider.isTrigger = true;
        collider.radius = 0.5f; // Default radius, can be adjusted
        
        // Set sorting layer (matching Pickupable component behavior)
        spriteRenderer.sortingLayerName = "Player";
        
        Debug.Log($"[EnemyItemDropper] ✓ Spawned weapon pickup: {weaponData.weaponName} at {position}");
    }
    
    /// <summary>
    /// Spawns a coin at the specified position.
    /// </summary>
    private void SpawnCoin(int coinValue, Vector3 position)
    {
        GameObject coin = null;
        
        // Try to use prefab if available
        if (coinPrefab != null)
        {
            coin = Instantiate(coinPrefab, position, Quaternion.identity);
            Debug.Log($"[EnemyItemDropper] Using coin prefab from field");
        }
        else
        {
            // Try to find Coin prefab in Resources folder as fallback
            GameObject resourceCoinPrefab = Resources.Load<GameObject>("Coin");
            if (resourceCoinPrefab != null)
            {
                coin = Instantiate(resourceCoinPrefab, position, Quaternion.identity);
                Debug.Log($"[EnemyItemDropper] Using coin prefab from Resources folder");
            }
            else
            {
                // Try to find Coin prefab in Assets
                string[] guids = UnityEditor.AssetDatabase.FindAssets("Coin t:Prefab");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    resourceCoinPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (resourceCoinPrefab != null)
                    {
                        coin = Instantiate(resourceCoinPrefab, position, Quaternion.identity);
                        Debug.Log($"[EnemyItemDropper] Using coin prefab found in Assets: {path}");
                    }
                }
            }
            
            // Create basic coin structure if prefab not found
            if (coin == null)
            {
                coin = new GameObject("Coin");
                coin.transform.position = position;
                coin.tag = "Coin";
                
                SpriteRenderer spriteRenderer = coin.AddComponent<SpriteRenderer>();
                CircleCollider2D collider = coin.AddComponent<CircleCollider2D>();
                Animator animator = coin.AddComponent<Animator>();
                
                // Configure collider
                collider.isTrigger = true;
                collider.radius = 0.4f;
                
                // Set sorting layer
                spriteRenderer.sortingLayerName = "Default";
                
                Debug.LogWarning($"[EnemyItemDropper] Coin prefab not found. Created basic coin GameObject. Consider assigning coinPrefab in EnemyItemDropper.");
            }
        }
        
        // Ensure it's not parented to the enemy (in case enemy gets destroyed)
        if (coin != null)
        {
            coin.transform.SetParent(null);
            Debug.Log($"[EnemyItemDropper] ✓ Spawned coin (value: {coinValue}) at {position}");
        }
        else
        {
            Debug.LogError($"[EnemyItemDropper] Failed to spawn coin at {position}!");
        }
    }
}
