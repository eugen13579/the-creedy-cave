using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

/// <summary>
/// Runtime save/load system for managing game state persistence.
/// Handles serialization to/from JSON and file I/O operations.
/// All methods are static and can be called without an instance.
/// Inherits from MonoBehaviour to allow attachment to GameObjects if needed.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    private const int MAX_SAVE_SLOTS = 5;
    private const string SAVE_FILE_PREFIX = "save_slot_";
    private const string SAVE_FILE_EXTENSION = ".json";
    
    // Static variable to store save data temporarily during scene load
    private static GameSaveData pendingLoadData = null;
    
    /// <summary>
    /// Gets the file path for a specific save slot.
    /// </summary>
    /// <param name="slot">Save slot number (1-5)</param>
    /// <returns>Full file path for the save slot</returns>
    public static string GetSaveFilePath(int slot)
    {
        if (slot < 1 || slot > MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid save slot: {slot}. Must be between 1 and {MAX_SAVE_SLOTS}");
            return null;
        }
        
        string fileName = $"{SAVE_FILE_PREFIX}{slot}{SAVE_FILE_EXTENSION}";
        return Path.Combine(Application.persistentDataPath, fileName);
    }
    
    /// <summary>
    /// Checks if a save file exists for the specified slot.
    /// </summary>
    /// <param name="slot">Save slot number (1-5)</param>
    /// <returns>True if save file exists, false otherwise</returns>
    public static bool SaveExists(int slot)
    {
        string filePath = GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }
        
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// Saves the current game state to the specified slot.
    /// </summary>
    /// <param name="slot">Save slot number (1-5)</param>
    /// <returns>True if save was successful, false otherwise</returns>
    public static bool SaveGame(int slot)
    {
        if (slot < 1 || slot > MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid save slot: {slot}. Must be between 1 and {MAX_SAVE_SLOTS}");
            return false;
        }
        
        try
        {
            GameSaveData saveData = CollectGameState();
            
            if (saveData == null)
            {
                Debug.LogError("Failed to collect game state. Some components may be missing.");
                return false;
            }
            
            string json = JsonUtility.ToJson(saveData, true);
            string filePath = GetSaveFilePath(slot);
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(filePath, json);
            Debug.Log($"Game saved successfully to slot {slot}: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving game to slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Loads game state from the specified slot.
    /// </summary>
    /// <param name="slot">Save slot number (1-5)</param>
    /// <returns>True if load was successful, false otherwise</returns>
    public static bool LoadGame(int slot)
    {
        if (slot < 1 || slot > MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid save slot: {slot}. Must be between 1 and {MAX_SAVE_SLOTS}");
            return false;
        }
        
        string filePath = GetSaveFilePath(slot);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Save file not found: {filePath}");
            return false;
        }
        
        try
        {
            string json = File.ReadAllText(filePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            if (saveData == null)
            {
                Debug.LogError("Failed to deserialize save data.");
                return false;
            }
            
            // Always reload the scene to ensure clean state
            if (!string.IsNullOrEmpty(saveData.currentSceneName))
            {
                // Store save data temporarily to restore after scene loads
                // We'll use a coroutine or scene loaded callback to restore state
                // For now, we'll load the scene and restore state after
                string sceneToLoad = saveData.currentSceneName;
                
                // Use SceneManager.sceneLoaded to restore state after scene loads
                SceneManager.sceneLoaded += OnSceneLoadedForLoad;
                
                // Store save data temporarily (using a static variable)
                pendingLoadData = saveData;
                
                // Load the scene (this will trigger scene reload)
                SceneManager.LoadScene(sceneToLoad);
                
                Debug.Log($"Game loading from slot {slot} - reloading scene: {sceneToLoad}");
                return true; // Return true as load will complete after scene loads
            }
            else
            {
                // No scene name, just restore state in current scene
                bool success = RestoreGameState(saveData);
                
                if (success)
                {
                    Debug.Log($"Game loaded successfully from slot {slot}");
                }
                
                return success;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Collects current game state from all relevant components.
    /// </summary>
    /// <returns>GameSaveData containing current game state, or null if collection fails</returns>
    private static GameSaveData CollectGameState()
    {
        GameSaveData saveData = new GameSaveData();
        
        // Get player position
        PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            saveData.playerPosition = playerController.transform.position;
        }
        else
        {
            Debug.LogWarning("PlayerController not found. Player position will be zero.");
        }
        
        // Get player health and stats
        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            saveData.currentHealth = playerHealth.CurrentHealth;
            saveData.maxHealth = playerHealth.MaxHealth;
            saveData.attackDamage = playerHealth.AttackDamage;
        }
        else
        {
            Debug.LogWarning("PlayerHealth not found. Health data will be zero.");
        }
        
        // Get inventory items
        InventoryController inventoryController = InventoryController.Instance;
        if (inventoryController != null)
        {
            // Collect inventory items from grid cells
            System.Collections.Generic.List<string> weaponNames = new System.Collections.Generic.List<string>();
            
            if (inventoryController.gridContainer != null)
            {
                for (int i = 0; i < inventoryController.gridContainer.childCount; i++)
                {
                    Transform cellTransform = inventoryController.gridContainer.GetChild(i);
                    CellController cell = cellTransform.GetComponent<CellController>();
                    if (cell != null && cell.currentItem != null)
                    {
                        weaponNames.Add(cell.currentItem.itemName);
                    }
                }
            }
            
            saveData.inventoryWeaponNames = weaponNames.ToArray();
            
            // Get equipped weapon
            if (inventoryController.WeaponCell != null && inventoryController.WeaponCell.currentItem != null)
            {
                saveData.equippedWeaponName = inventoryController.WeaponCell.currentItem.itemName;
            }
        }
        else
        {
            Debug.LogWarning("InventoryController not found. Inventory data will be empty.");
        }
        
        // Get coin count
        CoinManager coinManager = Object.FindFirstObjectByType<CoinManager>();
        if (coinManager != null)
        {
            saveData.coinCount = coinManager.CoinCount;
        }
        else
        {
            Debug.LogWarning("CoinManager not found. Coin count will be zero.");
        }
        
        // Get arrow count
        ArrowInventory arrowInventory = ArrowInventory.Instance;
        if (arrowInventory != null)
        {
            saveData.arrowCount = arrowInventory.ArrowCount;
        }
        else
        {
            Debug.LogWarning("ArrowInventory not found. Arrow count will be zero.");
        }
        
        // Get current scene name
        saveData.currentSceneName = SceneManager.GetActiveScene().name;
        
        return saveData;
    }
    
    /// <summary>
    /// Restores game state from save data to all relevant components.
    /// </summary>
    /// <param name="saveData">GameSaveData to restore from</param>
    /// <returns>True if restoration was successful, false otherwise</returns>
    private static bool RestoreGameState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("Cannot restore game state: saveData is null");
            return false;
        }
        
        // Restore player position
        PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.transform.position = saveData.playerPosition;
        }
        else
        {
            Debug.LogWarning("PlayerController not found. Cannot restore player position.");
        }
        
        // Restore player health and stats
        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(saveData.maxHealth);
            
            // Set current health by calculating difference
            float currentHealth = playerHealth.CurrentHealth;
            float targetHealth = saveData.currentHealth;
            float difference = targetHealth - currentHealth;
            
            if (difference > 0)
            {
                // Need to heal
                playerHealth.Heal(difference);
            }
            else if (difference < 0)
            {
                // Need to take damage (but avoid death logic)
                // Use reflection to set private field directly to avoid triggering death
                var healthField = typeof(PlayerHealth).GetField("currentHealth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (healthField != null)
                {
                    healthField.SetValue(playerHealth, targetHealth);
                    // Manually trigger health changed event
                    var onHealthChanged = typeof(PlayerHealth).GetField("OnHealthChanged", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (onHealthChanged != null)
                    {
                        var action = onHealthChanged.GetValue(playerHealth) as System.Action<float, float>;
                        action?.Invoke(targetHealth, saveData.maxHealth);
                    }
                }
                else
                {
                    // Fallback: use TakeDamage (may trigger death if health goes to 0)
                    playerHealth.TakeDamage(-difference);
                }
            }
            
            playerHealth.SetAttackDamage(saveData.attackDamage);
        }
        else
        {
            Debug.LogWarning("PlayerHealth not found. Cannot restore health data.");
        }
        
        // Restore inventory
        InventoryController inventoryController = InventoryController.Instance;
        if (inventoryController != null)
        {
            // Clear existing inventory
            if (inventoryController.gridContainer != null)
            {
                for (int i = 0; i < inventoryController.gridContainer.childCount; i++)
                {
                    Transform cellTransform = inventoryController.gridContainer.GetChild(i);
                    CellController cell = cellTransform.GetComponent<CellController>();
                    if (cell != null)
                    {
                        cell.ClearItem();
                    }
                }
            }
            
            // Load inventory items
            if (saveData.inventoryWeaponNames != null)
            {
                foreach (string weaponName in saveData.inventoryWeaponNames)
                {
                    ItemData item = LoadItemDataByName(weaponName);
                    if (item != null)
                    {
                        inventoryController.AddItem(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not load item: {weaponName}");
                    }
                }
            }
            
            // Restore equipped weapon
            if (!string.IsNullOrEmpty(saveData.equippedWeaponName))
            {
                ItemData equippedWeapon = LoadItemDataByName(saveData.equippedWeaponName);
                if (equippedWeapon != null && inventoryController.WeaponCell != null)
                {
                    inventoryController.WeaponCell.SetItem(equippedWeapon);
                }
            }
        }
        else
        {
            Debug.LogWarning("InventoryController not found. Cannot restore inventory.");
        }
        
        // Restore coin count
        CoinManager coinManager = Object.FindFirstObjectByType<CoinManager>();
        if (coinManager != null)
        {
            coinManager.CoinCount = saveData.coinCount;
        }
        else
        {
            Debug.LogWarning("CoinManager not found. Cannot restore coin count.");
        }
        
        // Restore arrow count
        ArrowInventory arrowInventory = ArrowInventory.Instance;
        if (arrowInventory != null)
        {
            // ArrowInventory doesn't have a public setter, so we need to use reflection or add arrows
            // Since we can't easily set the count, we'll calculate the difference and add/remove arrows
            int currentCount = arrowInventory.ArrowCount;
            int targetCount = saveData.arrowCount;
            int difference = targetCount - currentCount;
            
            if (difference > 0)
            {
                arrowInventory.AddArrows(difference);
            }
            else if (difference < 0)
            {
                // Remove arrows (use arrow multiple times)
                for (int i = 0; i < Mathf.Abs(difference); i++)
                {
                    if (arrowInventory.HasArrows())
                    {
                        arrowInventory.UseArrow();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("ArrowInventory not found. Cannot restore arrow count.");
        }
        
        return true;
    }
    
    /// <summary>
    /// Loads an ItemData ScriptableObject by name.
    /// </summary>
    /// <param name="itemName">Name of the item to load</param>
    /// <returns>ItemData if found, null otherwise</returns>
    private static ItemData LoadItemDataByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            return null;
        }
        
        #if UNITY_EDITOR
        // In editor, use AssetDatabase
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{itemName} t:ItemData");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null && item.itemName == itemName)
            {
                return item;
            }
        }
        
        // Try searching all ItemData assets
        guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null && item.itemName == itemName)
            {
                return item;
            }
        }
        #else
        // At runtime, try Resources folder
        ItemData item = Resources.Load<ItemData>(itemName);
        if (item != null)
        {
            return item;
        }
        #endif
        
        return null;
    }
    
    /// <summary>
    /// Callback for when scene is loaded during game load operation.
    /// Restores game state after scene has finished loading.
    /// </summary>
    private static void OnSceneLoadedForLoad(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe to avoid multiple calls
        SceneManager.sceneLoaded -= OnSceneLoadedForLoad;
        
        // Wait a frame to ensure all components are initialized
        if (pendingLoadData != null)
        {
            // Use a MonoBehaviour to run coroutine
            GameObject tempObj = new GameObject("TempLoadHelper");
            TempLoadHelper helper = tempObj.AddComponent<TempLoadHelper>();
            helper.StartCoroutine(helper.RestoreStateAfterLoad(pendingLoadData));
            
            GameSaveData dataToRestore = pendingLoadData;
            pendingLoadData = null; // Clear immediately
            
            Debug.Log("Scene loaded, restoring game state...");
        }
    }
    
    /// <summary>
    /// Temporary helper class to run coroutine for restoring state after scene load.
    /// </summary>
    private class TempLoadHelper : MonoBehaviour
    {
        public System.Collections.IEnumerator RestoreStateAfterLoad(GameSaveData saveData)
        {
            // Wait one frame to ensure all components are initialized
            yield return null;
            
            // Restore game state
            bool success = RestoreGameState(saveData);
            
            if (success)
            {
                Debug.Log("Game state restored successfully after scene load!");
            }
            else
            {
                Debug.LogError("Failed to restore game state after scene load!");
            }
            
            // Destroy this temporary object
            Destroy(gameObject);
        }
    }
}

