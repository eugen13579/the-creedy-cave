using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using TMPro;

/// <summary>
/// Controller for Gate prefab that handles player interaction and level completion.
/// </summary>
public class GateController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f; // Khoảng cách để tương tác (nếu dùng distance check)
    [SerializeField] private bool useTrigger = true; // Dùng trigger collider hoặc distance check
    [SerializeField] private string nextSceneName = ""; // Tên scene tiếp theo (optional, sẽ dùng auto progression nếu để trống)
    
    [Header("UI Prompt")]
    [SerializeField] private GameObject pressEPromptUI; // TextMeshPro UI hiển thị "Press E"
    
    private bool isPlayerNearby = false;
    private GameObject player;
    private Collider2D gateCollider;
    
    void Start()
    {
        // Tìm player
        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("GateController: Player not found! Make sure Player has 'Player' tag.");
        }
        
        // Setup collider - handle Tilemap case specially
        SetupCollider();
        
        // Ẩn prompt UI ban đầu
        if (pressEPromptUI != null)
        {
            pressEPromptUI.SetActive(false);
        }
        else
        {
            // Tự động tìm prompt UI nếu chưa được gán
            FindPromptUI();
        }
        
        // Set next scene name cho LevelCompleteManager nếu có
        if (!string.IsNullOrEmpty(nextSceneName) && LevelCompleteManager.Instance != null)
        {
            LevelCompleteManager.Instance.SetNextSceneName(nextSceneName);
        }
    }
    
    void Update()
    {
        // Nếu không dùng trigger, check distance
        if (!useTrigger && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            bool wasNearby = isPlayerNearby;
            isPlayerNearby = distance <= interactionRange;
            
            // Update UI khi trạng thái thay đổi
            if (wasNearby != isPlayerNearby)
            {
                UpdatePromptUI();
            }
        }
        
        // Check E key press khi player ở gần
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
    /// Xử lý khi player tương tác với gate
    /// </summary>
    private void OnPlayerInteract()
    {
        if (LevelCompleteManager.Instance != null)
        {
            // Set next scene name nếu có
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                LevelCompleteManager.Instance.SetNextSceneName(nextSceneName);
            }
            
            // Hiển thị level complete screen
            LevelCompleteManager.Instance.ShowLevelComplete();
            Debug.Log("GateController: Player interacted with gate. Showing level complete screen.");
        }
        else
        {
            Debug.LogWarning("GateController: LevelCompleteManager.Instance is null! Make sure LevelCompleteManager exists in the scene.");
        }
    }
    
    /// <summary>
    /// Cập nhật hiển thị prompt UI
    /// </summary>
    private void UpdatePromptUI()
    {
        if (pressEPromptUI != null)
        {
            pressEPromptUI.SetActive(isPlayerNearby);
        }
    }
    
    /// <summary>
    /// Tự động tìm prompt UI nếu chưa được gán
    /// </summary>
    private void FindPromptUI()
    {
        // Tìm trong HUDCanvas trước
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
            // Tìm trong HUDCanvas
            Transform promptTransform = hudCanvas.transform.Find("PressEPrompt");
            if (promptTransform != null)
            {
                pressEPromptUI = promptTransform.gameObject;
                Debug.Log($"GateController: Found prompt UI in HUDCanvas: {promptTransform.name}");
                return;
            }
        }
        
        // Fallback: Tìm trong toàn bộ scene
        TextMeshProUGUI[] textComponents = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (TextMeshProUGUI text in textComponents)
        {
            if (text.name.Contains("PressE") || text.name.Contains("Press E") || 
                text.text.Contains("Press E") || text.text.Contains("PressE"))
            {
                pressEPromptUI = text.gameObject;
                Debug.Log($"GateController: Found prompt UI: {text.name}");
                return;
            }
        }
        
        Debug.LogWarning("GateController: Press E prompt UI not found. You may need to create it using Tools > Setup Level Complete UI.");
    }
    
    /// <summary>
    /// Setup collider for gate interaction. Handles Tilemap gates specially.
    /// </summary>
    private void SetupCollider()
    {
        // Check if this is a Tilemap gate
        Tilemap tilemap = GetComponent<Tilemap>();
        TilemapCollider2D tilemapCollider = GetComponent<TilemapCollider2D>();
        
        if (tilemap != null)
        {
            // For Tilemap gates, we need a separate trigger collider
            // Check if there's already a trigger collider (not the TilemapCollider2D)
            Collider2D[] allColliders = GetComponents<Collider2D>();
            Collider2D triggerCollider = null;
            
            foreach (Collider2D col in allColliders)
            {
                // Find a non-TilemapCollider2D that we can use as trigger
                if (!(col is TilemapCollider2D))
                {
                    triggerCollider = col;
                    break;
                }
            }
            
            if (triggerCollider == null)
            {
                // Add a BoxCollider2D as trigger for interaction
                BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.isTrigger = useTrigger;
                
                // Set size based on tilemap bounds
                Bounds bounds = tilemap.localBounds;
                boxCollider.size = bounds.size;
                boxCollider.offset = bounds.center - transform.position;
                
                gateCollider = boxCollider;
                Debug.Log("GateController: Added BoxCollider2D trigger for Tilemap gate.");
            }
            else
            {
                // Use existing collider and configure as trigger
                gateCollider = triggerCollider;
                if (useTrigger)
                {
                    gateCollider.isTrigger = true;
                }
                Debug.Log($"GateController: Using existing collider '{triggerCollider.GetType().Name}' for Tilemap gate.");
            }
        }
        else
        {
            // Not a Tilemap, use standard collider setup
            gateCollider = GetComponent<Collider2D>();
            if (gateCollider == null)
            {
                // Add BoxCollider2D if no collider exists
                gateCollider = gameObject.AddComponent<BoxCollider2D>();
                Debug.Log("GateController: Added BoxCollider2D for gate.");
            }
            
            if (useTrigger && gateCollider != null)
            {
                gateCollider.isTrigger = true;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Vẽ interaction range trong editor
        if (!useTrigger)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

