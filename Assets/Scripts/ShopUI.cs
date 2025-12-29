using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the shop interface.
/// Handles display and purchase interactions.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI coinDisplayText;
    [SerializeField] private Button bowPurchaseButton;
    [SerializeField] private Button arrowBundlePurchaseButton;
    [SerializeField] private Button sellAllUnequippedButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI bowButtonText;
    [SerializeField] private TextMeshProUGUI arrowButtonText;
    [SerializeField] private TextMeshProUGUI sellAllButtonText;
    
    [Header("Confirmation Dialog")]
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private TextMeshProUGUI confirmationMessageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Shop Settings")]
    [SerializeField] private int bowPrice = 1;
    [SerializeField] private int arrowBundlePrice = 1;
    [SerializeField] private int arrowsPerBundle = 20;
    [SerializeField] private WeaponData bowWeaponData;
    
    private CoinManager coinManager;
    private ArrowInventory arrowInventory;
    private InventoryController inventoryController;
    
    void Start()
    {
        // Find required components
        coinManager = FindFirstObjectByType<CoinManager>();
        arrowInventory = FindFirstObjectByType<ArrowInventory>();
        
        // Try to get InventoryController - try Instance first, then search for it
        inventoryController = InventoryController.Instance;
        if (inventoryController == null)
        {
            // Try FindFirstObjectByType (active objects only)
            inventoryController = FindFirstObjectByType<InventoryController>();
            
            // If still not found, try finding the "Inventory" GameObject directly (works even if inactive)
            if (inventoryController == null)
            {
                GameObject inventoryObj = GameObject.Find("Inventory");
                if (inventoryObj != null)
                {
                    inventoryController = inventoryObj.GetComponent<InventoryController>();
                }
            }
            
            // Last resort: Search all root GameObjects and their children (including inactive)
            if (inventoryController == null)
            {
                GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (GameObject rootObj in rootObjects)
                {
                    // Search in children recursively (including inactive)
                    inventoryController = rootObj.GetComponentInChildren<InventoryController>(true);
                    if (inventoryController != null)
                    {
                        Debug.Log($"ShopUI: Found InventoryController in '{rootObj.name}' hierarchy during Start.");
                        break;
                    }
                }
            }
        }
        
        if (coinManager == null)
        {
            Debug.LogError("ShopUI: CoinManager not found in scene!");
        }
        
        if (arrowInventory == null)
        {
            Debug.LogError("ShopUI: ArrowInventory not found in scene!");
        }
        
        if (inventoryController == null)
        {
            Debug.LogWarning("ShopUI: InventoryController not found. Will try to find it when needed.");
        }
        
        // Setup button listeners
        if (bowPurchaseButton != null)
        {
            bowPurchaseButton.onClick.AddListener(OnBowPurchaseClicked);
        }
        
        if (arrowBundlePurchaseButton != null)
        {
            arrowBundlePurchaseButton.onClick.AddListener(OnArrowBundlePurchaseClicked);
        }
        
        if (sellAllUnequippedButton != null)
        {
            sellAllUnequippedButton.onClick.AddListener(OnSellAllUnequippedClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        
        // Setup confirmation dialog buttons
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmSellAll);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelSellAll);
        }
        
        // Hide confirmation dialog initially
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
        
        // Update button text
        UpdateButtonTexts();
        
        // Initially hide shop
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // Update coin display if visible
        if (shopPanel != null && shopPanel.activeSelf && coinDisplayText != null && coinManager != null)
        {
            coinDisplayText.text = $"Coins: {coinManager.coinCount}";
        }
    }
    
    /// <summary>
    /// Shows the shop UI.
    /// </summary>
    public void ShowShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdateButtonTexts();
            UpdateCoinDisplay();
        }
    }
    
    /// <summary>
    /// Hides the shop UI.
    /// </summary>
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Updates the coin display.
    /// </summary>
    private void UpdateCoinDisplay()
    {
        if (coinDisplayText != null && coinManager != null)
        {
            coinDisplayText.text = $"Coins: {coinManager.coinCount}";
        }
    }
    
    /// <summary>
    /// Updates button texts with prices.
    /// </summary>
    private void UpdateButtonTexts()
    {
        if (bowButtonText != null)
        {
            bowButtonText.text = $"Bow - {bowPrice} coin";
        }
        
        if (arrowButtonText != null)
        {
            arrowButtonText.text = $"Arrow Bundle (x{arrowsPerBundle}) - {arrowBundlePrice} coin";
        }
        
        if (sellAllButtonText != null)
        {
            sellAllButtonText.text = "Sell All Unequipped";
        }
    }
    
    /// <summary>
    /// Handles bow purchase button click.
    /// </summary>
    private void OnBowPurchaseClicked()
    {
        if (coinManager == null)
        {
            Debug.LogWarning("ShopUI: CoinManager not found!");
            return;
        }
        
        if (bowWeaponData == null)
        {
            Debug.LogWarning("ShopUI: Bow WeaponData not assigned!");
            return;
        }
        
        // Try to get InventoryController if not already set
        if (inventoryController == null)
        {
            // Try Instance first
            inventoryController = InventoryController.Instance;
            
            // If Instance is null, try to find it in the scene
            if (inventoryController == null)
            {
                // Try FindFirstObjectByType (active objects only)
                inventoryController = FindFirstObjectByType<InventoryController>();
                
                // If still not found, try finding the "Inventory" GameObject directly (works even if inactive)
                if (inventoryController == null)
                {
                    GameObject inventoryObj = GameObject.Find("Inventory");
                    if (inventoryObj != null)
                    {
                        inventoryController = inventoryObj.GetComponent<InventoryController>();
                        if (inventoryController != null)
                        {
                            Debug.Log("ShopUI: Found InventoryController on inactive 'Inventory' GameObject.");
                        }
                    }
                }
                
                // Last resort: Search all root GameObjects and their children (including inactive)
                if (inventoryController == null)
                {
                    GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                    foreach (GameObject rootObj in rootObjects)
                    {
                        // Search in children recursively (including inactive)
                        inventoryController = rootObj.GetComponentInChildren<InventoryController>(true);
                        if (inventoryController != null)
                        {
                            Debug.Log($"ShopUI: Found InventoryController in '{rootObj.name}' hierarchy.");
                            break;
                        }
                    }
                }
            }
            
            if (inventoryController == null)
            {
                Debug.LogWarning("ShopUI: InventoryController not found! Make sure InventoryController exists in the scene (even if inactive).");
                return;
            }
        }
        
        // Check if player has enough coins
        if (coinManager.coinCount >= bowPrice)
        {
            // Spend coins
            if (coinManager.SpendCoins(bowPrice))
            {
                // Add bow to inventory
                if (inventoryController.AddItem(bowWeaponData))
                {
                    Debug.Log("Purchased Bow!");
                    UpdateCoinDisplay();
                }
                else
                {
                    // Refund coins if inventory is full
                    coinManager.coinCount += bowPrice;
                    if (MessageDisplay.Instance != null)
                    {
                        MessageDisplay.Instance.ShowError("Inventory is full! Cannot purchase bow.");
                    }
                    Debug.LogWarning("Inventory is full! Cannot purchase bow.");
                }
            }
        }
        else
        {
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowError("Not enough coins to purchase bow!");
            }
            Debug.Log("Not enough coins to purchase bow!");
        }
    }
    
    /// <summary>
    /// Handles arrow bundle purchase button click.
    /// </summary>
    private void OnArrowBundlePurchaseClicked()
    {
        if (coinManager == null)
        {
            Debug.LogWarning("ShopUI: CoinManager not found!");
            return;
        }
        
        if (arrowInventory == null)
        {
            Debug.LogWarning("ShopUI: ArrowInventory not found!");
            return;
        }
        
        // Check if player has enough coins
        if (coinManager.coinCount >= arrowBundlePrice)
        {
            // Spend coins
            if (coinManager.SpendCoins(arrowBundlePrice))
            {
                // Add arrows
                arrowInventory.AddArrows(arrowsPerBundle);
                Debug.Log($"Purchased Arrow Bundle! Added {arrowsPerBundle} arrows.");
                UpdateCoinDisplay();
            }
        }
        else
        {
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowError("Not enough coins to purchase arrow bundle!");
            }
            Debug.Log("Not enough coins to purchase arrow bundle!");
        }
    }
    
    /// <summary>
    /// Sets the bow weapon data reference.
    /// </summary>
    public void SetBowWeaponData(WeaponData bowData)
    {
        bowWeaponData = bowData;
    }
    
    /// <summary>
    /// Gets all unequipped items from inventory with their sell prices.
    /// </summary>
    private System.Collections.Generic.List<System.Tuple<WeaponData, int>> GetUnequippedItems()
    {
        System.Collections.Generic.List<System.Tuple<WeaponData, int>> unequippedItems = 
            new System.Collections.Generic.List<System.Tuple<WeaponData, int>>();
        
        if (inventoryController == null)
        {
            // Try to find InventoryController
            inventoryController = InventoryController.Instance;
            if (inventoryController == null)
            {
                inventoryController = FindFirstObjectByType<InventoryController>();
            }
        }
        
        if (inventoryController == null)
        {
            Debug.LogWarning("ShopUI: InventoryController not found!");
            return unequippedItems;
        }
        
        // Get equipped weapon
        WeaponData equippedWeapon = null;
        if (inventoryController.WeaponCell != null)
        {
            equippedWeapon = inventoryController.WeaponCell.currentItem;
        }
        
        // Get all items from inventory
        var allItems = inventoryController.GetAllItems();
        
        // Count occurrences of each weapon (to handle duplicates)
        // Only exclude ONE instance of the equipped weapon, not all instances
        bool equippedWeaponExcluded = false;
        
        foreach (var itemTuple in allItems)
        {
            WeaponData weapon = itemTuple.Item2;
            if (weapon != null)
            {
                // If this is the equipped weapon, only exclude the first occurrence
                if (equippedWeapon != null && weapon == equippedWeapon)
                {
                    if (!equippedWeaponExcluded)
                    {
                        // Skip this one (it's the equipped weapon)
                        equippedWeaponExcluded = true;
                        continue;
                    }
                    // If we've already excluded one, include the rest
                }
                
                int sellPrice = weapon.sellPrice;
                unequippedItems.Add(new System.Tuple<WeaponData, int>(weapon, sellPrice));
            }
        }
        
        return unequippedItems;
    }
    
    /// <summary>
    /// Handles sell all unequipped button click.
    /// Shows confirmation dialog with total value.
    /// </summary>
    private void OnSellAllUnequippedClicked()
    {
        if (coinManager == null)
        {
            Debug.LogWarning("ShopUI: CoinManager not found!");
            return;
        }
        
        // Get unequipped items
        var unequippedItems = GetUnequippedItems();
        
        if (unequippedItems.Count == 0)
        {
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowError("No unequipped items to sell!");
            }
            Debug.Log("No unequipped items to sell!");
            return;
        }
        
        // Calculate total value
        int totalValue = 0;
        foreach (var item in unequippedItems)
        {
            totalValue += item.Item2;
        }
        
        // Show confirmation dialog
        ShowConfirmationDialog(totalValue, unequippedItems.Count);
    }
    
    /// <summary>
    /// Shows the confirmation dialog with total value.
    /// </summary>
    private void ShowConfirmationDialog(int totalValue, int itemCount)
    {
        if (confirmationDialog != null)
        {
            if (confirmationMessageText != null)
            {
                confirmationMessageText.text = $"Sell all {itemCount} unequipped items for {totalValue} coins?";
            }
            confirmationDialog.SetActive(true);
        }
        else
        {
            // Fallback: Use simple confirmation (for runtime without dialog UI)
            Debug.Log($"Would sell {itemCount} items for {totalValue} coins. Confirmation dialog not set up.");
            // For now, just proceed with selling (can be improved later)
            ConfirmSellAll();
        }
    }
    
    /// <summary>
    /// Confirms and executes the sell all operation.
    /// </summary>
    private void ConfirmSellAll()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
        
        if (coinManager == null)
        {
            Debug.LogWarning("ShopUI: CoinManager not found!");
            return;
        }
        
        if (inventoryController == null)
        {
            Debug.LogWarning("ShopUI: InventoryController not found!");
            return;
        }
        
        // Get unequipped items
        var unequippedItems = GetUnequippedItems();
        
        if (unequippedItems.Count == 0)
        {
            Debug.Log("No unequipped items to sell!");
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowError("No unequipped items to sell!");
            }
            return;
        }
        
        // Calculate total value and sell items
        int totalValue = 0;
        int itemsSold = 0;
        
        foreach (var item in unequippedItems)
        {
            WeaponData weapon = item.Item1;
            int sellPrice = item.Item2;
            
            if (inventoryController.RemoveItem(weapon))
            {
                totalValue += sellPrice;
                itemsSold++;
            }
        }
        
        // Add coins
        for (int i = 0; i < totalValue; i++)
        {
            coinManager.CollectCoin();
        }
        
        Debug.Log($"Sold {itemsSold} items for {totalValue} coins!");
        UpdateCoinDisplay();
    }
    
    /// <summary>
    /// Cancels the sell all operation.
    /// </summary>
    private void CancelSellAll()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
    }
}
