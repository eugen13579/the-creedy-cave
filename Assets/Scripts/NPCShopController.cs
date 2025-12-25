using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controller for NPC shop that handles player interaction and shop UI display.
/// </summary>
public class NPCShopController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private bool useTrigger = true;
    
    [Header("UI References")]
    [SerializeField] private GameObject pressEPromptUI;
    [SerializeField] private ShopUI shopUI;
    
    private bool isPlayerNearby = false;
    private GameObject player;
    private Collider2D npcCollider;
    
    void Start()
    {
        // Find player
        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("NPCShopController: Player not found! Make sure Player has 'Player' tag.");
        }
        
        // Setup collider
        SetupCollider();
        
        // Hide prompt UI initially
        if (pressEPromptUI != null)
        {
            pressEPromptUI.SetActive(false);
        }
        else
        {
            // Auto-find prompt UI if not assigned
            FindPromptUI();
        }
        
        // Find ShopUI if not assigned
        if (shopUI == null)
        {
            shopUI = FindFirstObjectByType<ShopUI>();
            if (shopUI == null)
            {
                Debug.LogWarning("NPCShopController: ShopUI not found! Make sure ShopUI exists in the scene.");
            }
        }
    }
    
    void Update()
    {
        // If not using trigger, check distance
        if (!useTrigger && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            bool wasNearby = isPlayerNearby;
            isPlayerNearby = distance <= interactionRange;
            
            // Update UI when state changes
            if (wasNearby != isPlayerNearby)
            {
                UpdatePromptUI();
            }
        }
        
        // Check E key press when player is nearby
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OnPlayerInteract();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (useTrigger && other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            UpdatePromptUI();
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (useTrigger && other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            UpdatePromptUI();
        }
    }
    
    /// <summary>
    /// Handles player interaction with NPC.
    /// </summary>
    private void OnPlayerInteract()
    {
        if (shopUI != null)
        {
            shopUI.ShowShop();
            Debug.Log("NPCShopController: Player interacted with shop NPC. Showing shop UI.");
        }
        else
        {
            Debug.LogWarning("NPCShopController: ShopUI is null! Make sure ShopUI exists in the scene.");
        }
    }
    
    /// <summary>
    /// Updates the prompt UI display.
    /// </summary>
    private void UpdatePromptUI()
    {
        if (pressEPromptUI != null)
        {
            pressEPromptUI.SetActive(isPlayerNearby);
        }
    }
    
    /// <summary>
    /// Auto-finds prompt UI if not assigned.
    /// </summary>
    private void FindPromptUI()
    {
        // Look in HUDCanvas first
        Canvas hudCanvas = null;
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "HUDCanvas" || c.name == "HUD")
            {
                hudCanvas = c;
                break;
            }
        }
        
        if (hudCanvas != null)
        {
            // Look in HUDCanvas
            Transform promptTransform = hudCanvas.transform.Find("PressEPrompt");
            if (promptTransform != null)
            {
                pressEPromptUI = promptTransform.gameObject;
                Debug.Log($"NPCShopController: Found prompt UI in HUDCanvas: {promptTransform.name}");
                return;
            }
        }
        
        // Fallback: Search entire scene
        TMPro.TextMeshProUGUI[] textComponents = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (TMPro.TextMeshProUGUI text in textComponents)
        {
            if (text.name.Contains("PressE") || text.name.Contains("Press E") || 
                text.text.Contains("Press E") || text.text.Contains("PressE"))
            {
                pressEPromptUI = text.gameObject;
                Debug.Log($"NPCShopController: Found prompt UI: {text.name}");
                return;
            }
        }
        
        Debug.LogWarning("NPCShopController: Press E prompt UI not found. You may need to create it using Tools > Setup Press E Prompt UI.");
    }
    
    /// <summary>
    /// Sets up collider for NPC interaction.
    /// </summary>
    private void SetupCollider()
    {
        npcCollider = GetComponent<Collider2D>();
        if (npcCollider == null)
        {
            // Add CircleCollider2D if no collider exists
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.radius = 1f;
            circleCollider.isTrigger = useTrigger;
            npcCollider = circleCollider;
            Debug.Log("NPCShopController: Added CircleCollider2D for NPC.");
        }
        else
        {
            if (useTrigger)
            {
                npcCollider.isTrigger = true;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw interaction range in editor
        if (!useTrigger)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

