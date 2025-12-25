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
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI bowButtonText;
    [SerializeField] private TextMeshProUGUI arrowButtonText;
    
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
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
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
                    Debug.LogWarning("Inventory is full! Cannot purchase bow.");
                }
            }
        }
        else
        {
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
}

