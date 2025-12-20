using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;
    public GameObject cellPrefab;
    public Transform gridContainer;
    public int rows = 5;
    public int columns = 10;
    public float cellSize = 64f;
    public float spacing = 4f;
    
    [Header("Weapon Equipment")]
    [Tooltip("Weapon Cell GameObject (should have CellController component, like inventory cells)")]
    public GameObject weaponCellGameObject;
    [Tooltip("Use button GameObject (will get Button component from this)")]
    public GameObject useButtonGameObject;
    
    private CellController weaponCell;
    private Button useButton;

    private CellController selectedCell;
    private WeaponData[] items;
    
    void Awake()
    {
        Instance = this;
        items = new WeaponData[rows * columns];
        GenerateGrid();
        
        // Get CellController component from weaponCellGameObject (same as inventory cells)
        if (weaponCellGameObject != null)
        {
            Debug.Log($"[InventoryController] weaponCellGameObject is assigned: {weaponCellGameObject.name}");
            weaponCell = weaponCellGameObject.GetComponent<CellController>();
            if (weaponCell != null)
            {
                Debug.Log($"[InventoryController] ✓ Found CellController component on '{weaponCellGameObject.name}'");
            }
            else
            {
                Debug.LogError($"[InventoryController] ✗ weaponCellGameObject '{weaponCellGameObject.name}' does not have a CellController component! " +
                    $"Please add a CellController component to this GameObject (same as inventory cells).");
            }
        }
        else
        {
            Debug.LogWarning("[InventoryController] weaponCellGameObject is not assigned in Inspector.");
            // Auto-find as fallback (try to find by name or tag)
            GameObject weaponCellObj = GameObject.Find("WeaponCell");
            if (weaponCellObj != null)
            {
                weaponCell = weaponCellObj.GetComponent<CellController>();
                if (weaponCell != null)
                {
                    Debug.Log($"Auto-found WeaponCell by name: {weaponCellObj.name}");
                }
            }
            
            if (weaponCell == null)
            {
                Debug.LogWarning("WeaponCell not found. Please drag a Cell GameObject (with CellController component) from the scene into the 'weaponCellGameObject' field.");
            }
        }
        
        // Get Button component from useButtonGameObject
        if (useButtonGameObject != null)
        {
            useButton = useButtonGameObject.GetComponent<Button>();
            if (useButton != null)
            {
                Debug.Log($"Found Use button from useButtonGameObject: {useButtonGameObject.name}");
            }
            else
            {
                Debug.LogWarning($"useButtonGameObject '{useButtonGameObject.name}' does not have a Button component!");
            }
        }
        else
        {
            // Auto-find as fallback
            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("use"))
                {
                    useButton = btn;
                    Debug.Log($"Auto-found Use button in children: {btn.name}");
                    break;
                }
            }
            
            if (useButton == null)
            {
                Debug.LogWarning("Use button not found. Please drag the Use button GameObject from the scene into the 'useButtonGameObject' field.");
            }
        }
        
        // Setup Use button
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
            UpdateUseButtonState();
        }
    }

    void GenerateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cell = Instantiate(cellPrefab, gridContainer);
                float x = col * (cellSize + spacing);
                float y = -row * (cellSize + spacing);
                cell.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            }
        }
    }

    public void SelectCell(CellController cell)
    {
        if (selectedCell == cell)
        {
            DeselectCell();
            return;
        }

        if (selectedCell != null)
            selectedCell.SetHighlight(false);

        selectedCell = cell;
        selectedCell.SetHighlight(true);
        
        // Update Use button state when selection changes
        UpdateUseButtonState();
    }

    public void DeselectCell()
    {
        if (selectedCell != null)
            selectedCell.SetHighlight(false);
        selectedCell = null;
        
        // Update Use button state when deselected
        UpdateUseButtonState();
    }

    public void CloseInventory()
    {
        Debug.Log("CloseInventory called");
        DeselectCell();
        gameObject.SetActive(false);
    }

    public bool AddItem(WeaponData weapon)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = weapon;
                var cell = gridContainer.GetChild(i).GetComponent<CellController>();
                cell.SetItem(weapon);
                return true;
            }
        }
        return false; // Inventory full
    }
    
    /// <summary>
    /// Called when the Use button is clicked.
    /// Equips the selected weapon to the WeaponCell.
    /// </summary>
    public void OnUseButtonClicked()
    {
        if (selectedCell == null || selectedCell.currentItem == null)
        {
            Debug.LogWarning("No weapon selected to equip!");
            return;
        }
        
        // Try to get weaponCell if it's null (in case it wasn't set in Awake)
        if (weaponCell == null)
        {
            if (weaponCellGameObject != null)
            {
                weaponCell = weaponCellGameObject.GetComponent<CellController>();
                if (weaponCell == null)
                {
                    Debug.LogError($"CellController component not found on GameObject '{weaponCellGameObject.name}'. " +
                        $"Make sure the GameObject has a CellController component attached (same as inventory cells)!");
                    return;
                }
            }
            else
            {
                // Try auto-find as fallback
                GameObject weaponCellObj = GameObject.Find("WeaponCell");
                if (weaponCellObj != null)
                {
                    weaponCell = weaponCellObj.GetComponent<CellController>();
                }
                
                if (weaponCell == null)
                {
                    Debug.LogError("WeaponCell is not assigned! Please assign 'weaponCellGameObject' in the Inspector.");
                    return;
                }
                Debug.LogWarning($"Auto-found WeaponCell at runtime: {weaponCell.name}. Consider assigning weaponCellGameObject in Inspector.");
            }
        }
        
        // Equip the weapon (using SetItem like inventory cells)
        WeaponData weaponToEquip = selectedCell.currentItem;
        weaponCell.SetItem(weaponToEquip);
        
        // Update player stats if PlayerHealth exists
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Update player attack damage based on weapon
            // Note: This assumes PlayerHealth has a method to set attack damage
            // If not, we may need to add it or use a different approach
            UpdatePlayerWeaponStats(playerHealth, weaponToEquip);
        }
        
        Debug.Log($"Equipped weapon: {weaponToEquip.weaponName}");
    }
    
    /// <summary>
    /// Updates the player's weapon stats based on the equipped weapon.
    /// </summary>
    private void UpdatePlayerWeaponStats(PlayerHealth playerHealth, WeaponData weapon)
    {
        if (playerHealth != null && weapon != null)
        {
            playerHealth.SetAttackDamage(weapon.damage);
            Debug.Log($"Updated player attack damage to {weapon.damage} from weapon {weapon.weaponName}");
        }
    }
    
    /// <summary>
    /// Updates the Use button's interactable state based on selection.
    /// </summary>
    private void UpdateUseButtonState()
    {
        if (useButton == null) return;
        
        // Enable button only if a weapon is selected
        bool hasSelectedWeapon = selectedCell != null && selectedCell.currentItem != null;
        useButton.interactable = hasSelectedWeapon;
    }
}

