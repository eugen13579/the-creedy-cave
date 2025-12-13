using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

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
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem if it doesn't exist
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            Debug.Log("Created Canvas");
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
        
        // Create Border (outer frame for visibility)
        GameObject border = new GameObject("Border");
        border.transform.SetParent(healthBarContainer.transform, false);
        
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(4, 4); // 2px border on each side
        borderRect.anchoredPosition = Vector2.zero;
        
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(1f, 1f, 1f, 1f); // White border for visibility
        
        // Create Background (dark bar)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarContainer.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.01f, 0.01f); // Slight inset for border
        bgRect.anchorMax = new Vector2(0.99f, 0.99f);
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Darker, fully opaque background
        
        // Create Fill (green bar that shrinks)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthBarContainer.transform, false);
        
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.01f, 0.01f); // Match background inset
        fillRect.anchorMax = new Vector2(0.99f, 0.99f);
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0f, 1f, 0f, 1f); // Bright green, fully opaque
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        
        // Create Health Text
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(healthBarContainer.transform, false);
        
        RectTransform textRect = healthTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Text healthText = healthTextObj.AddComponent<Text>();
        healthText.text = "2000 / 2000";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (healthText.font == null)
        {
            healthText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        healthText.fontSize = 22;
        healthText.fontStyle = FontStyle.Bold;
        healthText.color = new Color(0f, 0f, 0f, 1f); // Black text for better contrast on green
        healthText.alignment = TextAnchor.MiddleCenter;
        
        // Add white outline/shadow effect for visibility on all backgrounds
        GameObject shadowObj = new GameObject("TextShadow");
        shadowObj.transform.SetParent(healthTextObj.transform, false);
        
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.sizeDelta = Vector2.zero;
        shadowRect.anchoredPosition = new Vector2(2, -2); // Offset for shadow
        
        Text shadowText = shadowObj.AddComponent<Text>();
        shadowText.text = "2000 / 2000";
        shadowText.font = healthText.font;
        shadowText.fontSize = healthText.fontSize;
        shadowText.fontStyle = healthText.fontStyle;
        shadowText.color = new Color(1f, 1f, 1f, 0.9f); // White shadow/outline
        shadowText.alignment = TextAnchor.MiddleCenter;
        
        // Make shadow render behind main text
        shadowObj.transform.SetAsFirstSibling();
        
        // Add HealthBarUI component
        HealthBarUI healthBarUI = healthBarContainer.AddComponent<HealthBarUI>();
        
        // Set values using SerializedObject
        SerializedObject serializedHealthBar = new SerializedObject(healthBarUI);
        serializedHealthBar.FindProperty("healthBarFill").objectReferenceValue = fillImage;
        serializedHealthBar.FindProperty("healthText").objectReferenceValue = healthText;
        serializedHealthBar.FindProperty("showHealthText").boolValue = true;
        serializedHealthBar.FindProperty("fullHealthColor").colorValue = Color.green;
        serializedHealthBar.FindProperty("lowHealthColor").colorValue = Color.red;
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

