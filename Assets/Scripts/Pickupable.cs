using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Pickupable : MonoBehaviour
{
    public ItemData itemData;
    [SerializeField] private float bobAmount = 0.1f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float pickupDelay = 0.5f; // Delay before item can be picked up
    [SerializeField] private float spriteSize = 1; // Fixed size for the sprite (in world units)
    
    private SpriteRenderer spriteRenderer;
    private Vector3 startPos;
    private bool canPickup = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Player";
        spriteRenderer.sortingOrder = 1;
        
        var collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
        
        if (itemData != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
    }
    
    void OnEnable()
    {
        // Set sprite size when component is enabled (after itemData might be set)
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // If sprite is already set (e.g., by EnemyItemDropper), apply size
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            SetSpriteSize();
        }
    }
    
    private void SetSpriteSize()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;
        
        // Get the sprite's bounds in world units
        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        float spriteWidth = spriteBounds.size.x;
        float spriteHeight = spriteBounds.size.y;
        float maxDimension = Mathf.Max(spriteWidth, spriteHeight);
        
        // Scale sprite so its largest dimension equals spriteSize
        if (maxDimension > 0)
        {
            float scaleFactor = spriteSize / maxDimension;
            transform.localScale = Vector3.one * scaleFactor;
        }
    }
    
    /// <summary>
    /// Public method to apply sprite size. Can be called externally after itemData is set.
    /// </summary>
    public void ApplySpriteSize()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Player";
            spriteRenderer.sortingOrder = 1;
        }
        
        if (itemData != null && itemData.icon != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
        
        SetSpriteSize();
    }

    void Start()
    {
        startPos = transform.position;
        
        // Ensure sprite is set and size is applied
        if (itemData != null && itemData.icon != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
        
        // Apply size in Start to ensure sprite is ready
        SetSpriteSize();
        
        StartCoroutine(EnablePickupAfterDelay());
    }
    
    private IEnumerator EnablePickupAfterDelay()
    {
        yield return new WaitForSeconds(pickupDelay);
        canPickup = true;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void OnValidate()
    {
        // Update sprite in editor when itemData changes
        if (itemData != null && itemData.icon != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = itemData.icon;
                spriteRenderer = sr;
                SetSpriteSize();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canPickup)
        {
            StartCoroutine(TryPickupItem());
        }
    }
    
    private IEnumerator TryPickupItem()
    {
        // Ensure InventoryController is initialized
        if (InventoryController.Instance == null)
        {
            Debug.LogWarning("Pickupable: InventoryController.Instance is null. Trying to find it...");
            
            InventoryController controller = null;
            
            // Use Resources.FindObjectsOfTypeAll to find inactive objects
            InventoryController[] allControllers = Resources.FindObjectsOfTypeAll<InventoryController>();
            foreach (InventoryController c in allControllers)
            {
                // Only get objects from the active scene (not prefabs)
                if (c.gameObject.scene.IsValid() && c.gameObject.scene.isLoaded)
                {
                    controller = c;
                    Debug.Log($"Pickupable: Found InventoryController on '{controller.gameObject.name}' (parent: '{controller.transform.parent?.name ?? "none"}')");
                    break;
                }
            }
            
            // Fallback: Try to find "Inventory" GameObject by name
            if (controller == null)
            {
                GameObject inventoryObj = GameObject.Find("Inventory");
                if (inventoryObj != null)
                {
                    Debug.Log($"Pickupable: Found 'Inventory' GameObject. Searching for InventoryController...");
                    controller = inventoryObj.GetComponent<InventoryController>();
                    if (controller == null)
                    {
                        controller = inventoryObj.GetComponentInChildren<InventoryController>(true);
                    }
                }
            }
            
            if (controller != null)
            {
                Debug.Log($"Pickupable: Found InventoryController on '{controller.gameObject.name}'. Activating to initialize...");
                // Activate the root Inventory GameObject if needed
                GameObject rootObj = controller.gameObject;
                while (rootObj.transform.parent != null)
                {
                    rootObj = rootObj.transform.parent.gameObject;
                }
                
                bool wasActive = rootObj.activeSelf;
                if (!wasActive)
                {
                    rootObj.SetActive(true);
                    // Wait one frame for Awake() to run
                    yield return null;
                }
            }
            
            if (InventoryController.Instance == null)
            {
                if (MessageDisplay.Instance != null)
                {
                    MessageDisplay.Instance.ShowError("Could not find InventoryController! Item cannot be picked up.");
                }
                Debug.LogError("Pickupable: Could not find InventoryController! Item cannot be picked up.");
                yield break;
            }
        }
        
        // Now try to add the item
        if (InventoryController.Instance != null)
        {
            if (InventoryController.Instance.AddItem(itemData))
            {
                Destroy(gameObject);
            }
            else
            {
                // Inventory is full
                if (MessageDisplay.Instance != null)
                {
                    MessageDisplay.Instance.ShowError("Inventory is full! Cannot pick up item.");
                }
            }
        }
    }
}

