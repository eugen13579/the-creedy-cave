using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private float offsetY = 1.5f; // Height above enemy sprite
    [SerializeField] private Vector2 healthBarSize = new Vector2(1.5f, 0.2f); // Width and height in world units (increased for better visibility)
    [SerializeField] private Color fullHealthColor = new Color(1f, 0f, 0f, 1f); // Red
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f, 1f); // Red (no color change)
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30% health
    [SerializeField] private bool hideWhenFullHealth = true; // Hide bar when at full health
    [SerializeField] private bool hideWhenDead = true; // Hide bar when dead
    [SerializeField] private bool showHealthText = true; // Show health text
    [SerializeField] private bool alwaysShowForTesting = false; // Temporarily always show health bar for testing
    
    private EnemyHealth enemyHealth;
    private Canvas healthBarCanvas;
    private Image healthBarFill;
    private Image healthBarBackground;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI healthTextShadow; // Optional shadow for better visibility
    private RectTransform canvasRect;
    private Camera mainCamera;
    
    void Awake()
    {
        // Get EnemyHealth component early and subscribe to events
        // This ensures we catch all events, including the one fired in EnemyHealth.Start()
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged += UpdateHealthBar;
            enemyHealth.OnDeath += OnEnemyDeath;
        }
        else
        {
            Debug.LogWarning($"[EnemyHealthBar] {gameObject.name}: EnemyHealth component not found in Awake(). Health bar will not work.");
        }
    }
    
    void Start()
    {
        // Check if health bar canvas already exists (created in editor)
        Transform existingCanvas = transform.Find("HealthBarCanvas");
        if (existingCanvas != null)
        {
            // Reuse existing canvas created in editor
            healthBarCanvas = existingCanvas.GetComponent<Canvas>();
            if (healthBarCanvas != null)
            {
                canvasRect = existingCanvas.GetComponent<RectTransform>();
                healthBarBackground = existingCanvas.Find("Background")?.GetComponent<Image>();
                healthBarFill = existingCanvas.Find("Background/Fill")?.GetComponent<Image>();
                healthText = existingCanvas.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
                healthTextShadow = existingCanvas.Find("TextShadow")?.GetComponent<TextMeshProUGUI>();
                
                // Ensure Fill Image is configured correctly
                if (healthBarFill != null)
                {
                    healthBarFill.type = Image.Type.Filled;
                    healthBarFill.fillMethod = Image.FillMethod.Horizontal;
                }
                
                // Update camera reference
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindFirstObjectByType<Camera>();
                }
                if (mainCamera != null)
                {
                    healthBarCanvas.worldCamera = mainCamera;
                }
                
                // Set sorting layer to UI
                healthBarCanvas.sortingLayerName = "UI";
                
                // Initial update
                if (healthBarCanvas != null)
                {
                    healthBarCanvas.gameObject.SetActive(true);
                }
                if (enemyHealth != null && healthBarFill != null)
                {
                    UpdateHealthBar(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
                }
                
                if (alwaysShowForTesting && healthBarCanvas != null)
                {
                    healthBarCanvas.gameObject.SetActive(true);
                }
                
                return; // Skip creating new canvas
            }
        }
        
        // Get main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogWarning("EnemyHealthBar: No camera found. Health bar will not work.");
            enabled = false;
            return;
        }
        
        // Create new health bar UI (only if we didn't reuse existing one above)
        CreateHealthBar();
        
        // Initial update - force show initially
        // Make sure canvas is visible before first update
        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(true);
        }
        
        // Force update to ensure visibility
        if (enemyHealth != null)
        {
            UpdateHealthBar(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
        }
        
        // If alwaysShowForTesting is true, ensure it's visible
        if (alwaysShowForTesting && healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(true);
        }
    }
    
    void CreateHealthBar()
    {
        // Check if canvas already exists (created in editor)
        Transform existingCanvas = transform.Find("HealthBarCanvas");
        if (existingCanvas != null)
        {
            // Canvas already exists, don't create a duplicate
            healthBarCanvas = existingCanvas.GetComponent<Canvas>();
            canvasRect = existingCanvas.GetComponent<RectTransform>();
            return;
        }
        
        // Create Canvas for world-space UI
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, offsetY, 0);
        canvasObj.transform.localRotation = Quaternion.identity;
        
        healthBarCanvas = canvasObj.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        healthBarCanvas.worldCamera = mainCamera;
        healthBarCanvas.sortingLayerName = "UI";
        healthBarCanvas.sortingOrder = 10; // Ensure it renders above other elements
        
        // For world-space Canvas, we don't need CanvasScaler - it can interfere
        // Instead, we'll control scale directly via RectTransform
        
        // Add GraphicRaycaster (optional, but good practice)
        canvasObj.AddComponent<GraphicRaycaster>();
        
        canvasRect = canvasObj.GetComponent<RectTransform>();
        // Set size in world units, then scale down
        canvasRect.sizeDelta = new Vector2(healthBarSize.x * 100, healthBarSize.y * 100); // Size in pixels
        canvasRect.localScale = Vector3.one * 0.01f; // Scale down to world units (100 pixels = 1 world unit)
        
        // Set pivot to center bottom for proper positioning
        canvasRect.pivot = new Vector2(0.5f, 0f);
        canvasRect.anchorMin = new Vector2(0.5f, 0f);
        canvasRect.anchorMax = new Vector2(0.5f, 0f);
        
        // Ensure Canvas is initially active and visible
        canvasObj.SetActive(true);
        // Don't use DontSave flag - we want it to persist during play mode
        // canvasObj.hideFlags = HideFlags.DontSave;
        
        Debug.Log($"[EnemyHealthBar] Created health bar canvas for {gameObject.name} - Position: {canvasObj.transform.position}, LocalPos: {canvasObj.transform.localPosition}, Scale: {canvasRect.localScale}, Size: {canvasRect.sizeDelta}, Active: {canvasObj.activeSelf}, Camera: {mainCamera?.name}");
        
        // Create background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(canvasObj.transform, false);
        
        healthBarBackground = backgroundObj.AddComponent<Image>();
        healthBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray background
        healthBarBackground.raycastTarget = false;
        
        // Create white sprite for background
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, Color.white);
        bgTex.Apply();
        healthBarBackground.sprite = Sprite.Create(bgTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        // Create border (white frame around the health bar)
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(canvasObj.transform, false);
        borderObj.transform.SetAsFirstSibling(); // Render first (behind everything)
        
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = new Color(1f, 1f, 1f, 1f); // White border
        borderImage.raycastTarget = false;
        
        Texture2D borderTex = new Texture2D(1, 1);
        borderTex.SetPixel(0, 0, Color.white);
        borderTex.Apply();
        borderImage.sprite = Sprite.Create(borderTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(4, 4); // 2px border on each side (scaled for world space)
        borderRect.anchoredPosition = Vector2.zero;
        
        // Create fill bar
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(backgroundObj.transform, false);
        
        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = fullHealthColor;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.raycastTarget = false;
        
        // Create white sprite for fill
        Texture2D fillTex = new Texture2D(1, 1);
        fillTex.SetPixel(0, 0, Color.white);
        fillTex.Apply();
        healthBarFill.sprite = Sprite.Create(fillTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        // Create health text if enabled
        if (showHealthText)
        {
            // Load default TMP font
            TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null)
            {
                // Try alternative path
                defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime SDF");
            }
            
            // Create shadow text (behind main text for better visibility)
            GameObject shadowObj = new GameObject("TextShadow");
            shadowObj.transform.SetParent(canvasObj.transform, false);
            
            healthTextShadow = shadowObj.AddComponent<TextMeshProUGUI>();
            if (defaultFont != null)
            {
                healthTextShadow.font = defaultFont;
            }
            healthTextShadow.text = "0 / 0";
            healthTextShadow.fontSize = 240; // Large size to account for 0.01f canvas scale
            healthTextShadow.color = new Color(0f, 0f, 0f, 0.8f); // Dark shadow
            healthTextShadow.alignment = TextAlignmentOptions.Center;
            healthTextShadow.fontStyle = FontStyles.Bold;
            
            RectTransform shadowRect = shadowObj.GetComponent<RectTransform>();
            shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
            shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
            shadowRect.sizeDelta = new Vector2(healthBarSize.x * 100, 40);
            shadowRect.anchoredPosition = new Vector2(0, (healthBarSize.y * 100) / 2 + 30); // Position above bar
            shadowRect.localScale = Vector3.one;
            
            // Create main health text
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(canvasObj.transform, false);
            
            healthText = textObj.AddComponent<TextMeshProUGUI>();
            if (defaultFont != null)
            {
                healthText.font = defaultFont;
            }
            healthText.text = "0 / 0";
            healthText.fontSize = 240; // Large size to account for 0.01f canvas scale
            healthText.color = Color.white;
            healthText.alignment = TextAlignmentOptions.Center;
            healthText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(healthBarSize.x * 100, 40);
            textRect.anchoredPosition = new Vector2(0, (healthBarSize.y * 100) / 2 + 30); // Position above bar, slightly offset from shadow
            textRect.localScale = Vector3.one;
            
            // Position shadow slightly behind main text
            shadowRect.anchoredPosition = textRect.anchoredPosition + new Vector2(2, -2); // Small offset for shadow effect
        }
    }
    
    void LateUpdate()
    {
        // Make health bar always face the camera (billboard effect)
        if (healthBarCanvas != null && healthBarCanvas.gameObject.activeInHierarchy && mainCamera != null && mainCamera.gameObject.activeInHierarchy)
        {
            try
            {
                healthBarCanvas.transform.LookAt(healthBarCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
            catch (System.Exception e)
            {
                // Silently handle any transform errors (e.g., if object was destroyed)
                Debug.LogWarning($"EnemyHealthBar: Error updating billboard - {e.Message}");
            }
        }
    }
    
    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // Attempt to re-find components if they're null (for edge cases)
        if (healthBarFill == null && healthBarCanvas != null)
        {
            Transform fillTransform = healthBarCanvas.transform.Find("Background/Fill");
            if (fillTransform != null)
            {
                healthBarFill = fillTransform.GetComponent<Image>();
                if (healthBarFill != null)
                {
                    healthBarFill.type = Image.Type.Filled;
                    healthBarFill.fillMethod = Image.FillMethod.Horizontal;
                }
            }
        }
        
        if (healthBarFill == null || enemyHealth == null || healthBarCanvas == null)
        {
            return;
        }
        
        // Calculate fill amount
        float fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        // Update fill amount safely
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = fillAmount;
            
            // Update color based on health percentage (matching HealthBarUI.cs logic)
            if (fillAmount <= lowHealthThreshold)
            {
                healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, fillAmount / lowHealthThreshold);
            }
            else
            {
                healthBarFill.color = fullHealthColor;
            }
        }
        
        // Update health text if available
        if (showHealthText && healthText != null)
        {
            // Display as "current / max" (matching HealthBarUI.cs format)
            string healthString = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
            healthText.text = healthString;
            
            // Update shadow text if available
            if (healthTextShadow != null)
            {
                healthTextShadow.text = healthString;
            }
        }
        
        // Show/hide based on health
        if (healthBarCanvas != null && healthBarCanvas.gameObject != null)
        {
            bool shouldShow = true;
            
            // Override hide logic if testing mode is enabled
            if (alwaysShowForTesting)
            {
                shouldShow = true; // Force show when testing
            }
            else
            {
                if (hideWhenFullHealth && fillAmount >= 1f)
                {
                    shouldShow = false;
                }
                
                if (hideWhenDead && currentHealth <= 0)
                {
                    shouldShow = false;
                }
            }
            
            bool wasActive = healthBarCanvas.gameObject.activeSelf;
            
            // Always ensure canvas is active if it should be shown
            if (shouldShow && !wasActive)
            {
                healthBarCanvas.gameObject.SetActive(true);
            }
            else if (!shouldShow && wasActive)
            {
                healthBarCanvas.gameObject.SetActive(false);
            }
            
            // Debug log when visibility changes or on first few frames
            if (wasActive != shouldShow || (Time.frameCount < 10 && Time.frameCount % 2 == 0))
            {
                Debug.Log($"[EnemyHealthBar] {gameObject.name} - Visibility: {shouldShow} (health: {currentHealth}/{maxHealth}, fill: {fillAmount:F2}, alwaysShow: {alwaysShowForTesting}, hideWhenFull: {hideWhenFullHealth}, canvas active: {healthBarCanvas.gameObject.activeSelf}, position: {healthBarCanvas.transform.position})");
            }
        }
        else
        {
            // Canvas is null - log warning
            if (Time.frameCount < 10)
            {
                Debug.LogWarning($"[EnemyHealthBar] {gameObject.name} - Health bar canvas is null! Cannot update visibility.");
            }
        }
    }
    
    void OnEnemyDeath()
    {
        // Hide health bar when enemy dies
        if (healthBarCanvas != null && hideWhenDead)
        {
            healthBarCanvas.gameObject.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= UpdateHealthBar;
            enemyHealth.OnDeath -= OnEnemyDeath;
        }
    }
    
    void OnValidate()
    {
        // Update preview health bar in editor when values change
        if (!Application.isPlaying)
        {
            UpdatePreviewHealthBar();
        }
    }
    
    void UpdatePreviewHealthBar()
    {
        Transform previewCanvas = transform.Find("HealthBarCanvas");
        if (previewCanvas == null) return;
        
        // Update position
        previewCanvas.localPosition = new Vector3(0, offsetY, 0);
        
        // Update size
        RectTransform canvasRect = previewCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(healthBarSize.x * 100, healthBarSize.y * 100);
        }
        
        // Update fill color
        Transform fillTransform = previewCanvas.Find("Background/Fill");
        if (fillTransform != null)
        {
            Image fillImage = fillTransform.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = fullHealthColor;
            }
        }
        
        // Update text visibility
        Transform textTransform = previewCanvas.Find("HealthText");
        Transform shadowTransform = previewCanvas.Find("TextShadow");
        
        if (textTransform != null)
        {
            textTransform.gameObject.SetActive(showHealthText);
        }
        
        if (shadowTransform != null)
        {
            shadowTransform.gameObject.SetActive(showHealthText);
        }
        
        // Update text size if visible
        if (showHealthText && textTransform != null)
        {
            RectTransform textRect = textTransform.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.sizeDelta = new Vector2(healthBarSize.x * 100, 40);
            }
        }
        
        if (showHealthText && shadowTransform != null)
        {
            RectTransform shadowRect = shadowTransform.GetComponent<RectTransform>();
            if (shadowRect != null)
            {
                shadowRect.sizeDelta = new Vector2(healthBarSize.x * 100, 40);
            }
        }
        
        // Update camera reference
        Canvas canvas = previewCanvas.GetComponent<Canvas>();
        if (canvas != null)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = FindFirstObjectByType<Camera>();
            }
            if (mainCam != null)
            {
                canvas.worldCamera = mainCam;
            }
        }
    }
}


