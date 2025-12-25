using UnityEngine;

/// <summary>
/// Arrow projectile that travels and damages enemies on hit.
/// </summary>
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f; // Auto-destroy after this time
    [SerializeField] private float damage = 150f; // Default damage, can be overridden
    [SerializeField] private bool hasHit = false;
    
    private Vector2 direction = Vector2.zero;
    private bool isInitialized = false;
    private Rigidbody2D rb;
    private float spawnTime;
    private SpriteRenderer spriteRenderer;
    private Sprite providedSprite = null;
    
    void Awake()
    {
        // Set up components early to avoid timing issues
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0; // No gravity for arrows
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Set up sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingLayerName = "Player";
        
        // Set up collider if not present
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.radius = 0.2f;
            circleCollider.isTrigger = true;
        }
        else
        {
            collider.isTrigger = true;
        }
    }
    
    void Start()
    {
        spawnTime = Time.time;
        
        // Load arrow sprite if not provided via Initialize
        if (providedSprite == null)
        {
            LoadArrowSprite();
        }
        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = providedSprite;
            }
        }
    }
    
    /// <summary>
    /// Loads the arrow sprite from Assets/Assets/Characters/Soldier/Arrow(projectile)
    /// </summary>
    private void LoadArrowSprite()
    {
        if (providedSprite != null)
        {
            // Use provided sprite if available
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = providedSprite;
            }
            return;
        }
        
        #if UNITY_EDITOR
        // Try to load the arrow sprite
        Sprite arrowSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(32x32).png");
        
        if (arrowSprite == null)
        {
            // Try the 100x100 version
            arrowSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(100x100).png");
        }
        
        if (arrowSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = arrowSprite;
        }
        else
        {
            Debug.LogWarning("ArrowProjectile: Could not load arrow sprite from Assets/Assets/Characters/Soldier/Arrow(projectile)/");
        }
        #else
        // At runtime, load from Resources folder if needed
        // Note: Sprites need to be moved to Resources folder for this to work
        Sprite arrowSprite = Resources.Load<Sprite>("Characters/Soldier/Arrow(projectile)/Arrow01(32x32)");
        if (arrowSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = arrowSprite;
        }
        #endif
    }
    
    void Update()
    {
        // Auto-destroy after lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void FixedUpdate()
    {
        if (!hasHit && rb != null && isInitialized && direction.magnitude > 0.01f)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    /// <summary>
    /// Initializes the arrow with direction and damage.
    /// </summary>
    /// <param name="dir">Direction to travel</param>
    /// <param name="dmg">Damage to deal</param>
    /// <param name="sprite">Optional sprite to use for the arrow</param>
    public void Initialize(Vector2 dir, float dmg, Sprite sprite = null)
    {
        direction = dir.normalized;
        damage = dmg;
        isInitialized = true;
        
        // Set sprite if provided
        if (sprite != null)
        {
            providedSprite = sprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
        
        // Rotate arrow to face direction
        if (direction.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // Apply velocity immediately if rigidbody is ready
        if (rb != null && direction.magnitude > 0.01f)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    /// <summary>
    /// Sets the sprite for this arrow.
    /// </summary>
    /// <param name="sprite">Sprite to use</param>
    public void SetSprite(Sprite sprite)
    {
        providedSprite = sprite;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit player or other arrows
        if (other.CompareTag("Player") || other.CompareTag("Arrow") || hasHit)
        {
            return;
        }
        
        // Hit enemy
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Arrow hit enemy for {damage} damage!");
                hasHit = true;
                
                // Stop movement
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                
                // Destroy arrow after a short delay
                Destroy(gameObject, 0.1f);
            }
        }
        // Hit walls/obstacles
        else if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            hasHit = true;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            Destroy(gameObject, 0.1f);
        }
    }
}

