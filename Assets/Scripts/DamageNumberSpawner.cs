using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Static helper class for spawning damage numbers at world positions.
/// Handles Canvas lookup and UI coordinate conversion.
/// </summary>
public static class DamageNumberSpawner
{
    private static Canvas cachedCanvas;
    private static GameObject damageNumberPrefab;
    
    /// <summary>
    /// Helper MonoBehaviour for running coroutines from static class.
    /// </summary>
    private class DamageNumberCoroutineRunner : MonoBehaviour { }
    
    private static IEnumerator SetPositionAfterFrame(DamageNumber damageNumber, Vector3 worldPosition)
    {
        yield return null; // Wait one frame for Start() to run
        if (damageNumber != null)
        {
            damageNumber.SetWorldPosition(worldPosition);
            Debug.Log($"DamageNumberSpawner: Set position after frame for world position {worldPosition}");
        }
    }
    
    /// <summary>
    /// Spawns a damage number at the specified world position.
    /// </summary>
    /// <param name="worldPosition">World position where the damage occurred</param>
    /// <param name="damage">Damage amount to display</param>
    /// <param name="targetTransform">Optional: Transform to follow for positioning</param>
    /// <param name="targetSpriteRenderer">Optional: SpriteRenderer to use for bounds calculation</param>
    public static void SpawnDamageNumber(Vector3 worldPosition, float damage, Transform targetTransform = null, SpriteRenderer targetSpriteRenderer = null)
    {
        Debug.Log($"DamageNumberSpawner: Spawning damage number {damage} at {worldPosition}");
        
        // Find or cache canvas
        Canvas canvas = GetCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("DamageNumberSpawner: No Canvas found. Cannot spawn damage number.");
            return;
        }
        
        Debug.Log($"DamageNumberSpawner: Using canvas {canvas.name}");
        
        // Get or create damage number prefab
        GameObject damageNumberObj = CreateDamageNumberObject(canvas);
        if (damageNumberObj == null)
        {
            Debug.LogWarning("DamageNumberSpawner: Failed to create damage number object.");
            return;
        }
        
        // Get DamageNumber component
        DamageNumber damageNumber = damageNumberObj.GetComponent<DamageNumber>();
        if (damageNumber == null)
        {
            damageNumber = damageNumberObj.AddComponent<DamageNumber>();
        }
        
        // Set damage value first (before Start() runs)
        damageNumber.SetDamage(damage);
        
        // Also update the text directly if it exists (for prefabs)
        Text existingText = damageNumberObj.GetComponentInChildren<Text>();
        if (existingText != null)
        {
            existingText.text = $"-{Mathf.CeilToInt(damage)}";
        }
        
        // Set target to follow if provided - this is the key to centering on enemy
        if (targetTransform != null)
        {
            damageNumber.SetTarget(targetTransform, targetSpriteRenderer);
            Debug.Log($"DamageNumberSpawner: Set target to {targetTransform.name} for damage number");
        }
        else
        {
            Debug.LogWarning("DamageNumberSpawner: No target transform provided - damage number may not be positioned correctly");
        }
        
        // Set initial world position - use coroutine to wait for Start() to complete
        // But FloatAnimation will handle positioning from target, so this is just for initial setup
        if (canvas != null)
        {
            MonoBehaviour coroutineRunner = canvas.GetComponent<MonoBehaviour>();
            if (coroutineRunner == null)
            {
                coroutineRunner = canvas.gameObject.AddComponent<DamageNumberCoroutineRunner>();
            }
            coroutineRunner.StartCoroutine(SetPositionAfterFrame(damageNumber, worldPosition));
        }
        else
        {
            // Fallback: set position immediately
            damageNumber.SetWorldPosition(worldPosition);
        }
        
        Debug.Log($"DamageNumberSpawner: Damage number created successfully");
    }
    
    /// <summary>
    /// Sets a prefab to use for damage numbers. If null, creates a basic one.
    /// </summary>
    public static void SetDamageNumberPrefab(GameObject prefab)
    {
        damageNumberPrefab = prefab;
    }
    
    private static Canvas GetCanvas()
    {
        if (cachedCanvas != null && cachedCanvas.gameObject.activeInHierarchy)
        {
            return cachedCanvas;
        }
        
        // Try to find canvas in scene
        cachedCanvas = Object.FindFirstObjectByType<Canvas>();
        
        if (cachedCanvas == null)
        {
            // Create a canvas if none exists
            GameObject canvasObj = new GameObject("DamageNumberCanvas");
            cachedCanvas = canvasObj.AddComponent<Canvas>();
            cachedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        return cachedCanvas;
    }
    
    private static GameObject CreateDamageNumberObject(Canvas canvas)
    {
        // Use prefab if available
        if (damageNumberPrefab != null)
        {
            return Object.Instantiate(damageNumberPrefab, canvas.transform);
        }
        
        // Otherwise create a basic damage number object
        GameObject damageNumberObj = new GameObject("DamageNumber");
        damageNumberObj.transform.SetParent(canvas.transform, false);
        
        // Add RectTransform
        RectTransform rectTransform = damageNumberObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200f, 100f);
        
        // Add Text component
        GameObject textObj = new GameObject("DamageText");
        textObj.transform.SetParent(damageNumberObj.transform, false);
        
        Text text = textObj.AddComponent<Text>();
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        text.text = "-0";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.red;
        
        return damageNumberObj;
    }
}
