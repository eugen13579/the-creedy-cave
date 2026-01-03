using UnityEngine;
using TMPro;

/// <summary>
/// Displays player stats (Gold, Attack Damage, Defense) in the top-left corner of the screen.
/// </summary>
public class PlayerStatsDisplay : MonoBehaviour
{
    public static PlayerStatsDisplay Instance;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI defenseText;
    
    private CoinManager coinManager;
    private PlayerHealth playerHealth;
    private InventoryController inventoryController;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Find required components
        coinManager = FindFirstObjectByType<CoinManager>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        inventoryController = InventoryController.Instance;
        
        if (coinManager == null)
        {
            Debug.LogWarning("PlayerStatsDisplay: CoinManager not found!");
        }
        
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerStatsDisplay: PlayerHealth not found!");
        }
        
        if (inventoryController == null)
        {
            Debug.LogWarning("PlayerStatsDisplay: InventoryController not found!");
        }
    }
    
    void Update()
    {
        UpdateStats();
    }
    
    /// <summary>
    /// Updates the displayed stats.
    /// </summary>
    private void UpdateStats()
    {
        // Update gold
        if (goldText != null && coinManager != null)
        {
            goldText.text = $"Gold: {coinManager.CoinCount}";
        }
        
        // Update attack damage
        if (attackDamageText != null && playerHealth != null)
        {
            attackDamageText.text = $"Attack: {playerHealth.AttackDamage:F0}";
        }
        
        // Update defense (calculate from equipped items)
        if (defenseText != null)
        {
            int totalDefense = CalculateTotalDefense();
            defenseText.text = $"Defense: {totalDefense}";
        }
    }
    
    /// <summary>
    /// Calculates total defense from all equipped armor items.
    /// </summary>
    private int CalculateTotalDefense()
    {
        int totalDefense = 0;
        
        if (inventoryController == null)
        {
            return totalDefense;
        }
        
        // Get equipment cell GameObjects from InventoryController
        GameObject armorObj = inventoryController.armorCellGameObject;
        GameObject hatObj = inventoryController.hatCellGameObject;
        GameObject glovesObj = inventoryController.glovesCellGameObject;
        GameObject shoesObj = inventoryController.shoesCellGameObject;
        
        // Try to find by name if not assigned
        if (armorObj == null)
        {
            armorObj = GameObject.Find("ArmorCell");
        }
        if (hatObj == null)
        {
            hatObj = GameObject.Find("HatCell");
        }
        if (glovesObj == null)
        {
            glovesObj = GameObject.Find("GlovesCell");
        }
        if (shoesObj == null)
        {
            shoesObj = GameObject.Find("ShoesCell");
        }
        
        // Get CellController components and add defense
        if (armorObj != null)
        {
            CellController armorCell = armorObj.GetComponent<CellController>();
            if (armorCell != null && armorCell.currentItem != null)
            {
                totalDefense += armorCell.currentItem.defense;
            }
        }
        
        if (hatObj != null)
        {
            CellController hatCell = hatObj.GetComponent<CellController>();
            if (hatCell != null && hatCell.currentItem != null)
            {
                totalDefense += hatCell.currentItem.defense;
            }
        }
        
        if (glovesObj != null)
        {
            CellController glovesCell = glovesObj.GetComponent<CellController>();
            if (glovesCell != null && glovesCell.currentItem != null)
            {
                totalDefense += glovesCell.currentItem.defense;
            }
        }
        
        if (shoesObj != null)
        {
            CellController shoesCell = shoesObj.GetComponent<CellController>();
            if (shoesCell != null && shoesCell.currentItem != null)
            {
                totalDefense += shoesCell.currentItem.defense;
            }
        }
        
        return totalDefense;
    }
}

