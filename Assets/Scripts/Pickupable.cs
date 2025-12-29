using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Pickupable : MonoBehaviour
{
    public WeaponData weaponData;
    [SerializeField] private float bobAmount = 0.1f;
    [SerializeField] private float bobSpeed = 2f;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 startPos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Player";
        
        var collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
        
        if (weaponData != null && weaponData.icon != null)
        {
            spriteRenderer.sprite = weaponData.icon;
        }
    }

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void OnValidate()
    {
        // Update sprite in editor when weaponData changes
        if (weaponData != null && weaponData.icon != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = weaponData.icon;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
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
            if (InventoryController.Instance.AddItem(weaponData))
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

