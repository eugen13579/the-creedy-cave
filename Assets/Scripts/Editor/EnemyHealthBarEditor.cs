using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

[CustomEditor(typeof(EnemyHealthBar))]
[CanEditMultipleObjects]
public class EnemyHealthBarEditor : Editor
{
    private SerializedProperty offsetYProp;
    private SerializedProperty healthBarSizeProp;
    private SerializedProperty fullHealthColorProp;
    private SerializedProperty lowHealthColorProp;
    private SerializedProperty lowHealthThresholdProp;
    private SerializedProperty hideWhenFullHealthProp;
    private SerializedProperty hideWhenDeadProp;
    private SerializedProperty showHealthTextProp;
    private SerializedProperty alwaysShowForTestingProp;
    
    private bool showPreviewSettings = true;
    
    void OnEnable()
    {
        offsetYProp = serializedObject.FindProperty("offsetY");
        healthBarSizeProp = serializedObject.FindProperty("healthBarSize");
        fullHealthColorProp = serializedObject.FindProperty("fullHealthColor");
        lowHealthColorProp = serializedObject.FindProperty("lowHealthColor");
        lowHealthThresholdProp = serializedObject.FindProperty("lowHealthThreshold");
        hideWhenFullHealthProp = serializedObject.FindProperty("hideWhenFullHealth");
        hideWhenDeadProp = serializedObject.FindProperty("hideWhenDead");
        showHealthTextProp = serializedObject.FindProperty("showHealthText");
        alwaysShowForTestingProp = serializedObject.FindProperty("alwaysShowForTesting");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EnemyHealthBar healthBar = (EnemyHealthBar)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Health Bar Settings", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.PropertyField(offsetYProp, new GUIContent("Offset Y", "Height above enemy sprite"));
        EditorGUILayout.PropertyField(healthBarSizeProp, new GUIContent("Health Bar Size", "Width and height in world units"));
        EditorGUILayout.PropertyField(fullHealthColorProp);
        EditorGUILayout.PropertyField(lowHealthColorProp);
        EditorGUILayout.PropertyField(lowHealthThresholdProp);
        EditorGUILayout.PropertyField(hideWhenFullHealthProp);
        EditorGUILayout.PropertyField(hideWhenDeadProp);
        EditorGUILayout.PropertyField(showHealthTextProp);
        EditorGUILayout.PropertyField(alwaysShowForTestingProp);
        
        bool valuesChanged = EditorGUI.EndChangeCheck();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        // Quick create button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create HealthBarCanvas", GUILayout.Height(30)))
        {
            CreateOrUpdatePreview(healthBar);
            EditorUtility.DisplayDialog("Health Bar Created", 
                "HealthBarCanvas has been created for this enemy.\n\n" +
                "You can now edit it directly in the Scene view.", "OK");
        }
        
        Transform existingCanvas = healthBar.transform.Find("HealthBarCanvas");
        if (existingCanvas != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Button("✓ HealthBarCanvas Exists", GUILayout.Height(30));
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        showPreviewSettings = EditorGUILayout.Foldout(showPreviewSettings, "Advanced Preview Options", true);
        
        if (showPreviewSettings)
        {
            EditorGUILayout.HelpBox("HealthBarCanvas is visible in the Scene view and updates automatically when you change settings above.", MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Update HealthBarCanvas"))
            {
                CreateOrUpdatePreview(healthBar);
            }
            
            if (GUILayout.Button("Remove HealthBarCanvas"))
            {
                RemovePreview(healthBar);
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Update All HealthBars in Scene"))
            {
                UpdateAllPreviews();
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        
        // Update preview if values changed
        if (valuesChanged && !Application.isPlaying)
        {
            UpdatePreview(healthBar);
        }
    }
    
    void CreateOrUpdatePreview(EnemyHealthBar healthBar)
    {
        if (healthBar == null) return;
        
        // Remove existing preview
        RemovePreview(healthBar);
        
        // Get values from serialized properties
        float offsetY = offsetYProp.floatValue;
        Vector2 size = healthBarSizeProp.vector2Value;
        bool showText = showHealthTextProp.boolValue;
        Color fullColor = fullHealthColorProp.colorValue;
        
        // Get camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogWarning("No camera found. Cannot create preview.");
            return;
        }
        
        // Get enemy health for current health value
        EnemyHealth enemyHealth = healthBar.GetComponent<EnemyHealth>();
        float currentHealth = enemyHealth != null ? enemyHealth.CurrentHealth : 1000f;
        float maxHealth = enemyHealth != null ? enemyHealth.MaxHealth : 1000f;
        float fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 1f;
        
        // Create Canvas (use same name as runtime version)
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(healthBar.transform);
        canvasObj.transform.localPosition = new Vector3(0, offsetY, 0);
        canvasObj.transform.localRotation = Quaternion.identity;
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
        canvas.sortingOrder = 10;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(size.x * 100, size.y * 100);
        canvasRect.localScale = Vector3.one * 0.01f;
        canvasRect.pivot = new Vector2(0.5f, 0f);
        canvasRect.anchorMin = new Vector2(0.5f, 0f);
        canvasRect.anchorMax = new Vector2(0.5f, 0f);
        
        // Create background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(canvasObj.transform, false);
        
        Image bgImage = backgroundObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        bgImage.raycastTarget = false;
        
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, Color.white);
        bgTex.Apply();
        bgImage.sprite = Sprite.Create(bgTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
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
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fullColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = fillAmount;
        fillImage.raycastTarget = false;
        
        Texture2D fillTex = new Texture2D(1, 1);
        fillTex.SetPixel(0, 0, Color.white);
        fillTex.Apply();
        fillImage.sprite = Sprite.Create(fillTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        // Create health text if enabled
        if (showText)
        {
            TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            
            // Create shadow text
            GameObject shadowObj = new GameObject("TextShadow");
            shadowObj.transform.SetParent(canvasObj.transform, false);
            
            TextMeshProUGUI shadowText = shadowObj.AddComponent<TextMeshProUGUI>();
            if (defaultFont != null) shadowText.font = defaultFont;
            shadowText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
            shadowText.fontSize = 240;
            shadowText.color = new Color(0f, 0f, 0f, 0.8f);
            shadowText.alignment = TextAlignmentOptions.Center;
            shadowText.fontStyle = FontStyles.Bold;
            
            RectTransform shadowRect = shadowObj.GetComponent<RectTransform>();
            shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
            shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
            shadowRect.sizeDelta = new Vector2(size.x * 100, 40);
            shadowRect.anchoredPosition = new Vector2(2, (size.y * 100) / 2 + 30);
            
            // Create main text
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(canvasObj.transform, false);
            
            TextMeshProUGUI healthText = textObj.AddComponent<TextMeshProUGUI>();
            if (defaultFont != null) healthText.font = defaultFont;
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
            healthText.fontSize = 240;
            healthText.color = Color.white;
            healthText.alignment = TextAlignmentOptions.Center;
            healthText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(size.x * 100, 40);
            textRect.anchoredPosition = new Vector2(0, (size.y * 100) / 2 + 30);
        }
        
        // Mark scene as dirty
        EditorUtility.SetDirty(healthBar.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    
    void UpdatePreview(EnemyHealthBar healthBar)
    {
        if (healthBar == null || Application.isPlaying) return;
        
        Transform previewCanvas = healthBar.transform.Find("HealthBarCanvas");
        if (previewCanvas == null) return;
        
        // Update position
        float offsetY = offsetYProp.floatValue;
        previewCanvas.localPosition = new Vector3(0, offsetY, 0);
        
        // Update size
        Vector2 size = healthBarSizeProp.vector2Value;
        RectTransform canvasRect = previewCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(size.x * 100, size.y * 100);
        }
        
        // Update fill color
        Color fullColor = fullHealthColorProp.colorValue;
        Transform fillTransform = previewCanvas.Find("Background/Fill");
        if (fillTransform != null)
        {
            Image fillImage = fillTransform.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = fullColor;
            }
        }
        
        // Update text visibility and size
        bool showText = showHealthTextProp.boolValue;
        Transform textTransform = previewCanvas.Find("HealthText");
        Transform shadowTransform = previewCanvas.Find("TextShadow");
        
        if (textTransform != null)
        {
            textTransform.gameObject.SetActive(showText);
            if (showText)
            {
                TextMeshProUGUI text = textTransform.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    RectTransform textRect = textTransform.GetComponent<RectTransform>();
                    if (textRect != null)
                    {
                        textRect.sizeDelta = new Vector2(size.x * 100, 40);
                    }
                }
            }
        }
        
        if (shadowTransform != null)
        {
            shadowTransform.gameObject.SetActive(showText);
            if (showText)
            {
                RectTransform shadowRect = shadowTransform.GetComponent<RectTransform>();
                if (shadowRect != null)
                {
                    shadowRect.sizeDelta = new Vector2(size.x * 100, 40);
                }
            }
        }
        
        // Update camera reference
        Canvas canvas = previewCanvas.GetComponent<Canvas>();
        if (canvas != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
            }
        }
        
        EditorUtility.SetDirty(previewCanvas.gameObject);
    }
    
    void RemovePreview(EnemyHealthBar healthBar)
    {
        if (healthBar == null) return;
        
        Transform previewCanvas = healthBar.transform.Find("HealthBarCanvas");
        if (previewCanvas != null)
        {
            DestroyImmediate(previewCanvas.gameObject);
            EditorUtility.SetDirty(healthBar.gameObject);
        }
    }
    
    void UpdateAllPreviews()
    {
        EnemyHealthBar[] allHealthBars = FindObjectsByType<EnemyHealthBar>(FindObjectsSortMode.None);
        foreach (EnemyHealthBar healthBar in allHealthBars)
        {
            if (healthBar != null)
            {
                UpdatePreview(healthBar);
            }
        }
        
        Debug.Log($"Updated {allHealthBars.Length} preview health bars.");
    }
}
