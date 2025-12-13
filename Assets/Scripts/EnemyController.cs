using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on Enemy. Sprite flipping will not work.");
        }
        
        // Find player by tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found. Make sure Player has 'Player' tag.");
        }
    }

    void Update()
    {
        if (playerTransform == null || spriteRenderer == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // If player is within detection range, flip enemy to face player
        if (distanceToPlayer <= detectionRange)
        {
            // Determine direction to player
            float directionToPlayer = playerTransform.position.x - transform.position.x;
            
            // Flip sprite based on player position
            if (directionToPlayer < 0)
            {
                // Player is to the left - flip sprite
                spriteRenderer.flipX = true;
            }
            else if (directionToPlayer > 0)
            {
                // Player is to the right - unflip sprite
                spriteRenderer.flipX = false;
            }
        }
    }
}
