using UnityEngine;

public class TrapController : MonoBehaviour
{
    [SerializeField]
    private float damage = 100f; // damage applied to player when triggered

    [SerializeField]
    private bool singleUse = false; // if true, trap only triggers once

    [SerializeField]
    private bool debug = false; // enable debug logs for testing

    private bool hasTriggered = false;
    private Collider2D _collider;

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider == null)
        {
            Debug.LogWarning(
                "TrapController: No Collider2D found. Add a Collider2D (set Is Trigger if you want trigger behavior)."
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || hasTriggered)
            return;
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!enabled || hasTriggered)
            return;
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject obj)
    {
        // Try to find PlayerHealth on this object or any parent (handles child colliders and nested objects)
        var playerHealth = obj.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            // Not the player (or PlayerHealth isn't present)
            return;
        }

        // Apply damage
        playerHealth.TakeDamage(damage);
        if (debug)
        {
            Debug.Log(
                $"TrapController: Dealt {damage} damage to {playerHealth.gameObject.name} (tag={playerHealth.gameObject.tag})"
            );
        }

        // If the trap should only work once, disable its collider so it doesn't trigger again
        if (singleUse)
        {
            hasTriggered = true;
            if (_collider != null)
                _collider.enabled = false;
            // Optionally, you can play an animation or destroy the trap here
            // Destroy(gameObject, 0.1f);
        }
    }
}
