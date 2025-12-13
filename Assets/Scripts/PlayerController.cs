using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] [Range(0.1f, 1f)] private float colliderSizeScale = 0.8f; // Scale down collider to fit through narrow passages
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private InputAction moveAction;

    void Start()
    {
        // Get or add Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // No gravity for top-down movement
            rb.freezeRotation = true; // Prevent rotation
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        }

        // Get SpriteRenderer component for flipping
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on Player. Sprite flipping will not work.");
        }

        // Get or add BoxCollider2D and adjust size for narrow passages
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // Adjust collider size to be smaller than sprite for narrow passages
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            boxCollider.size = spriteSize * colliderSizeScale;
        }

        // Setup Input System - use project-wide actions if available
        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.Enable();
            }
            else
            {
                Debug.LogWarning("Move action not found in InputSystem.actions. Make sure project-wide actions are configured.");
            }
        }
        else
        {
            Debug.LogWarning("InputSystem.actions is null. Please configure project-wide actions in Project Settings > Input System Package > Input Actions.");
        }
    }

    void Update()
    {
        if (moveAction == null) return;

        // Read movement input
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        
        // Apply movement
        Vector2 movement = moveInput * moveSpeed;
        rb.linearVelocity = movement;
        
        // Flip sprite when moving left
        if (spriteRenderer != null)
        {
            if (moveInput.x < 0)
            {
                // Moving left - flip sprite
                spriteRenderer.flipX = true;
            }
            else if (moveInput.x > 0)
            {
                // Moving right - unflip sprite
                spriteRenderer.flipX = false;
            }
        }
    }

    void OnDisable()
    {
        // Disable input action when object is disabled
        if (moveAction != null)
        {
            moveAction.Disable();
        }
    }
}
