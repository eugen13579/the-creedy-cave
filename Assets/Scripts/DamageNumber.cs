using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private Text damageText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float floatSpeed = 50f;
    [SerializeField] private Color damageColor = Color.red;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private float damageValue = 0f; // Store damage value to apply after Start()
    private Transform targetTransform; // Optional: follow a target transform
    private SpriteRenderer targetSpriteRenderer; // Optional: use sprite bounds for positioning
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Find or create canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        // Setup text if not assigned
        if (damageText == null)
        {
            damageText = GetComponentInChildren<Text>();
            if (damageText == null)
            {
                GameObject textObj = new GameObject("DamageText");
                textObj.transform.SetParent(transform);
                damageText = textObj.AddComponent<Text>();
                rectTransform = textObj.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    rectTransform = textObj.AddComponent<RectTransform>();
                }
            }
        }
        
        // Setup text properties
        if (damageText != null)
        {
            damageText.color = damageColor;
            damageText.alignment = TextAnchor.MiddleCenter;
            damageText.fontSize = 36;
            damageText.fontStyle = FontStyle.Bold;
            
            // Apply stored damage value if SetDamage was called before Start()
            if (damageValue > 0)
            {
                damageText.text = $"-{Mathf.CeilToInt(damageValue)}";
            }
        }
        
        // Start auto-destroy coroutine
        StartCoroutine(DestroyAfterDelay());
        
        // Start float animation
        StartCoroutine(FloatAnimation());
    }
    
    public void SetDamage(float damage)
    {
        damageValue = damage;
        
        // Try to find text component if not set yet
        if (damageText == null)
        {
            damageText = GetComponentInChildren<Text>();
        }
        
        // Update text if available
        if (damageText != null)
        {
            damageText.text = $"-{Mathf.CeilToInt(damage)}";
        }
    }
    
    public void SetWorldPosition(Vector3 worldPos)
    {
        UpdatePosition(worldPos);
    }
    
    /// <summary>
    /// Sets a target to follow for positioning. The damage number will track the target's position.
    /// </summary>
    public void SetTarget(Transform target, SpriteRenderer spriteRenderer = null)
    {
        targetTransform = target;
        targetSpriteRenderer = spriteRenderer;
    }
    
    private void UpdatePosition(Vector3 worldPos)
    {
        // Ensure canvas and rectTransform are initialized
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = Object.FindFirstObjectByType<Canvas>();
            }
        }
        
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
        }
        
        if (Camera.main == null)
        {
            Debug.LogWarning("DamageNumber: Camera.main is null. Cannot convert world position to screen position.");
            return;
        }
        
        // Convert world position to screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        // Check if position is behind camera (z < 0)
        if (screenPos.z < 0)
        {
            // Don't show if behind camera
            return;
        }
        
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            if (rectTransform != null)
            {
                rectTransform.position = new Vector2(screenPos.x, screenPos.y);
            }
        }
        else if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
            if (rectTransform != null && canvasRect != null)
            {
                rectTransform.anchoredPosition = new Vector2(
                    (viewportPos.x - 0.5f) * canvasRect.sizeDelta.x,
                    (viewportPos.y - 0.5f) * canvasRect.sizeDelta.y
                );
            }
        }
    }
    
    // Removed Update() - position tracking is now handled in FloatAnimation coroutine
    
    private IEnumerator FloatAnimation()
    {
        float elapsed = 0f;
        
        // Wait a frame to ensure target is set and components are initialized
        yield return null;
        
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            
            if (rectTransform != null && Camera.main != null)
            {
                // Always get the current position from the target (enemy/player)
                Vector3 currentWorldPos;
                
                if (targetTransform != null)
                {
                    // Use sprite renderer bounds if available for accurate positioning
                    if (targetSpriteRenderer != null && targetSpriteRenderer.bounds.size.y > 0)
                    {
                        // Position at the top center of the sprite
                        currentWorldPos = targetSpriteRenderer.bounds.center + Vector3.up * (targetSpriteRenderer.bounds.extents.y + 0.3f);
                    }
                    else
                    {
                        // Fallback: use transform position with offset
                        currentWorldPos = targetTransform.position + Vector3.up * 1.0f;
                    }
                }
                else
                {
                    // No target - this shouldn't happen, but use last known position
                    Debug.LogWarning("DamageNumber: No target transform set, cannot update position");
                    yield return null;
                    continue;
                }
                
                // Convert world position to screen position
                Vector3 screenPos = Camera.main.WorldToScreenPoint(currentWorldPos);
                
                // Only update if in front of camera
                if (screenPos.z > 0)
                {
                    // Add upward float offset in screen space (pixels)
                    float floatOffset = elapsed * floatSpeed;
                    rectTransform.position = new Vector2(screenPos.x, screenPos.y + floatOffset);
                }
                
                // Fade out
                if (damageText != null)
                {
                    Color c = damageText.color;
                    c.a = 1f - (elapsed / displayDuration);
                    damageText.color = c;
                }
            }
            yield return null;
        }
    }
    
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        Destroy(gameObject);
    }
}



