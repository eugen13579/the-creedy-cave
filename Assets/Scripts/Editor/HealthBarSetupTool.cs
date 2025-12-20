using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class HealthBarSetupTool : EditorWindow
{
    private float barWidth = 400f;
    private float barHeight = 40f;
    private Vector2 barPosition = new Vector2(20, -20);
    
    [MenuItem("Tools/Setup Health Bar")]
    public static void ShowWindow()
    {
        GetWindow<HealthBarSetupTool>("Health Bar Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Health Bar Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        barWidth = EditorGUILayout.FloatField("Bar Width", barWidth);
        barHeight = EditorGUILayout.FloatField("Bar Height", barHeight);
        barPosition = EditorGUILayout.Vector2Field("Position (Top-Left)", barPosition);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will create a health bar in the top-left corner of the screen.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Health Bar", GUILayout.Height(30)))
        {
            SetupHealthBar();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Player Health Only", GUILayout.Height(25)))
        {
            SetupPlayerHealth();
        }
    }
    
    void SetupHealthBar()
    {
        // Find or create a dedicated HUD Canvas (separate from Inventory)
        Canvas canvas = null;
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        // Look for a Canvas named "HUDCanvas" or "HUD" first
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "HUDCanvas" || c.name == "HUD" || c.name == "HealthBarCanvas")
            {
                canvas = c;
                break;
            }
        }
        
        // If no HUD Canvas found, create a new one specifically for health bar
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("HUDCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Set higher sort order so HUD appears above other UI
            canvas.sortingOrder = 100;
            
            Debug.Log("Created HUDCanvas for health bar");
        }
        
        // Create EventSystem if it doesn't exist
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Check if HealthBar already exists
        HealthBarUI existingHealthBar = canvas.GetComponentInChildren<HealthBarUI>();
        if (existingHealthBar != null)
        {
            if (EditorUtility.DisplayDialog("Health Bar Exists", 
                "A Health Bar already exists. Do you want to recreate it?", "Yes", "No"))
            {
                DestroyImmediate(existingHealthBar.gameObject);
            }
            else
            {
                return;
            }
        }
        
        // Create Health Bar Container
        GameObject healthBarContainer = new GameObject("HealthBar");
        healthBarContainer.transform.SetParent(canvas.transform, false);
        
        RectTransform containerRect = healthBarContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = barPosition;
        containerRect.sizeDelta = new Vector2(barWidth, barHeight);
        
        // Create Mask Container (for clean clipping of health bar fill) - create first so it renders behind border
        GameObject maskContainer = new GameObject("MaskContainer");
        maskContainer.transform.SetParent(healthBarContainer.transform, false);
        
        RectTransform maskRect = maskContainer.AddComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.01f, 0.01f); // Slight inset for border
        maskRect.anchorMax = new Vector2(0.99f, 0.99f);
        maskRect.sizeDelta = Vector2.zero;
        maskRect.anchoredPosition = Vector2.zero;
        
        // Add Mask component for clean clipping
        Mask mask = maskContainer.AddComponent<Mask>();
        mask.showMaskGraphic = false; // Don't show the mask graphic itself
        
        // Add Image component required for Mask to work
        Image maskImage = maskContainer.AddComponent<Image>();
        maskImage.color = new Color(1f, 1f, 1f, 1f); // White for mask (needs to be visible for mask to work)
        maskImage.raycastTarget = false;
        // Create white sprite for mask
        Texture2D maskTex = new Texture2D(1, 1);
        maskTex.SetPixel(0, 0, Color.white);
        maskTex.Apply();
        maskImage.sprite = Sprite.Create(maskTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        // Create Background (dark bar) - child of mask container
        GameObject background = new GameObject("Background");
        background.transform.SetParent(maskContainer.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 1f); // Black background (will be masked)
        bgImage.raycastTarget = false;
        // Create white sprite (will be colored black)
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, Color.white);
        bgTex.Apply();
        bgImage.sprite = Sprite.Create(bgTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        // Create Fill (red bar that shrinks) - child of mask container
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(maskContainer.transform, false);
        
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(1f, 0f, 0f, 1f); // Red fill
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        fillImage.raycastTarget = false;
        // Create white sprite (will be colored red)
        Texture2D fillTex = new Texture2D(1, 1);
        fillTex.SetPixel(0, 0, Color.white);
        fillTex.Apply();
        fillImage.sprite = Sprite.Create(fillTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        // Ensure fill is on top of background (render after) - last sibling renders on top
        fill.transform.SetAsLastSibling();
        
        // Create Border (outer frame) - create after mask so it renders behind
        GameObject border = new GameObject("Border");
        border.transform.SetParent(healthBarContainer.transform, false);
        border.transform.SetAsFirstSibling(); // Render first (behind everything)
        
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(4, 4); // 2px border on each side
        borderRect.anchoredPosition = Vector2.zero;
        
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(1f, 1f, 1f, 1f); // White border
        borderImage.raycastTarget = false;
        // Create white sprite
        Texture2D whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
        borderImage.sprite = Sprite.Create(whiteTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        // Create Health Text
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(healthBarContainer.transform, false);
        
        RectTransform textRect = healthTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI healthText = healthTextObj.AddComponent<TextMeshProUGUI>();
        healthText.text = "2000 / 2000";
        healthText.fontSize = 22;
        healthText.fontStyle = FontStyles.Bold;
        healthText.color = new Color(1f, 1f, 1f, 1f); // White text for better contrast on black/red
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Add HealthBarUI component
        HealthBarUI healthBarUI = healthBarContainer.AddComponent<HealthBarUI>();
        
        // Set values using SerializedObject
        SerializedObject serializedHealthBar = new SerializedObject(healthBarUI);
        serializedHealthBar.FindProperty("healthBarFill").objectReferenceValue = fillImage;
        serializedHealthBar.FindProperty("healthText").objectReferenceValue = healthText;
        serializedHealthBar.FindProperty("showHealthText").boolValue = true;
        serializedHealthBar.FindProperty("fullHealthColor").colorValue = Color.red; // Red fill
        serializedHealthBar.FindProperty("lowHealthColor").colorValue = Color.red; // Red fill (no color change)
        serializedHealthBar.FindProperty("lowHealthThreshold").floatValue = 0.3f;
        serializedHealthBar.ApplyModifiedProperties();
        
        // Setup Player Health
        SetupPlayerHealth();
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        Debug.Log("Health Bar setup complete!");
        EditorUtility.DisplayDialog("Setup Complete", 
            "Health Bar has been set up successfully!\n\n" +
            "The health bar is positioned in the top-left corner.", "OK");
    }
    
    void SetupPlayerHealth()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found. Make sure Player has 'Player' tag.");
            return;
        }
        
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.AddComponent<PlayerHealth>();
            Debug.Log("Added PlayerHealth component to Player");
        }
        
        // Set max health using SerializedObject
        SerializedObject serializedHealth = new SerializedObject(playerHealth);
        serializedHealth.FindProperty("maxHealth").floatValue = 2000f;
        serializedHealth.ApplyModifiedProperties();
        
        Debug.Log("Player Health setup complete with 2000 max health");
    }
}

