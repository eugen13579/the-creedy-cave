using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;

public class EnemyHealthBarSetupTool : EditorWindow
{
    private float offsetY = 1.5f;
    private Vector2 healthBarSize = new Vector2(1f, 0.15f);
    private bool hideWhenFullHealth = false; // Default to false so health bars are visible
    private bool hideWhenDead = true;
    private bool showHealthText = true;
    private bool alwaysShowForTesting = true; // Default to true for better visibility
    
    private Vector2 scrollPosition;
    private List<GameObject> enemiesWithHealthBars = new List<GameObject>();
    
    [MenuItem("Tools/Enemy Health Bar Setup")]
    public static void ShowWindow()
    {
        GetWindow<EnemyHealthBarSetupTool>("Enemy Health Bar Setup");
    }
    
    void OnEnable()
    {
        RefreshEnemyList();
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Enemy Health Bar Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Configuration Section
        EditorGUILayout.LabelField("Health Bar Settings", EditorStyles.boldLabel);
        offsetY = EditorGUILayout.FloatField("Offset Y (Height Above Enemy)", offsetY);
        healthBarSize = EditorGUILayout.Vector2Field("Health Bar Size (World Units)", healthBarSize);
        hideWhenFullHealth = EditorGUILayout.Toggle("Hide When Full Health", hideWhenFullHealth);
        hideWhenDead = EditorGUILayout.Toggle("Hide When Dead", hideWhenDead);
        showHealthText = EditorGUILayout.Toggle("Show Health Text", showHealthText);
        alwaysShowForTesting = EditorGUILayout.Toggle("Always Show For Testing", alwaysShowForTesting);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("These settings will be applied to newly added health bars.", MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Setup Section
        EditorGUILayout.LabelField("Setup Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add Health Bar to Selected Enemies", GUILayout.Height(30)))
        {
            AddHealthBarToSelected();
        }
        
        if (GUILayout.Button("Add Health Bar to All Enemies in Scene", GUILayout.Height(30)))
        {
            AddHealthBarToAllEnemies();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create health bar UI in editor mode so you can see it in the Scene view.", MessageType.Info);
        
        if (GUILayout.Button("Create Health Bar UI for Selected Enemies (Editor Preview)", GUILayout.Height(30)))
        {
            CreateHealthBarUIForSelected();
        }
        
        if (GUILayout.Button("Create Health Bar UI for All Enemies (Editor Preview)", GUILayout.Height(30)))
        {
            CreateHealthBarUIForAll();
        }
        
        if (GUILayout.Button("Remove All Preview Health Bars", GUILayout.Height(25)))
        {
            RemovePreviewHealthBars();
        }
        
        if (GUILayout.Button("Remove Health Bar from Selected Enemies", GUILayout.Height(25)))
        {
            RemoveHealthBarFromSelected();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Debug Section
        EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Refresh Enemy List", GUILayout.Height(25)))
        {
            RefreshEnemyList();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Show All Health Bars (Force Visible)", GUILayout.Height(25)))
        {
            ShowAllHealthBars();
        }
        
        if (GUILayout.Button("Enable Always Show For All Enemies", GUILayout.Height(25)))
        {
            EnableAlwaysShowForAll();
        }
        
        if (GUILayout.Button("Hide All Health Bars", GUILayout.Height(25)))
        {
            HideAllHealthBars();
        }
        
        if (GUILayout.Button("Log Health Bar Status", GUILayout.Height(25)))
        {
            LogHealthBarStatus();
        }
        
        if (GUILayout.Button("Test Health Bar Visibility (Play Mode)", GUILayout.Height(25)))
        {
            TestHealthBarVisibility();
        }
        
        EditorGUILayout.HelpBox("Note: Health bars are created at runtime in Start(). Make sure you're in Play Mode to see them.", MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Enemy List Section
        EditorGUILayout.LabelField($"Enemies with Health Bars ({enemiesWithHealthBars.Count})", EditorStyles.boldLabel);
        
        if (enemiesWithHealthBars.Count == 0)
        {
            EditorGUILayout.HelpBox("No enemies with health bars found in the scene.", MessageType.Info);
        }
        else
        {
            foreach (GameObject enemy in enemiesWithHealthBars)
            {
                if (enemy == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                
                // Select button
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = enemy;
                    EditorGUIUtility.PingObject(enemy);
                }
                
                // Enemy name
                EditorGUILayout.LabelField(enemy.name);
                
                // Health bar status
                EnemyHealthBar healthBar = enemy.GetComponent<EnemyHealthBar>();
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                
                if (healthBar != null)
                {
                    EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                    
                    if (enemyHealth != null)
                    {
                        EditorGUILayout.LabelField($"{enemyHealth.CurrentHealth:F0}/{enemyHealth.MaxHealth:F0}", GUILayout.Width(80));
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("✗", GUILayout.Width(20));
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void AddHealthBarToSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more enemy GameObjects.", "OK");
            return;
        }
        
        int added = 0;
        int skipped = 0;
        
        foreach (GameObject obj in selected)
        {
            if (AddHealthBarToEnemy(obj))
            {
                added++;
            }
            else
            {
                skipped++;
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Added health bars to {added} enemies. Skipped {skipped}.");
        EditorUtility.DisplayDialog("Setup Complete", 
            $"Added health bars to {added} enemy/enemies.\nSkipped {skipped} (already had health bar or missing EnemyHealth).", "OK");
    }
    
    void AddHealthBarToAllEnemies()
    {
        // Find all enemies (by tag or by component)
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (allEnemies.Length == 0)
        {
            // Try finding by EnemyController component
            EnemyController[] controllers = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            allEnemies = new GameObject[controllers.Length];
            for (int i = 0; i < controllers.Length; i++)
            {
                allEnemies[i] = controllers[i].gameObject;
            }
        }
        
        if (allEnemies.Length == 0)
        {
            EditorUtility.DisplayDialog("No Enemies Found", 
                "No enemies found in the scene. Make sure enemies have the 'Enemy' tag or an EnemyController component.", "OK");
            return;
        }
        
        if (!EditorUtility.DisplayDialog("Add to All Enemies", 
            $"This will add health bars to {allEnemies.Length} enemy/enemies. Continue?", "Yes", "No"))
        {
            return;
        }
        
        int added = 0;
        int skipped = 0;
        
        foreach (GameObject enemy in allEnemies)
        {
            if (AddHealthBarToEnemy(enemy))
            {
                added++;
            }
            else
            {
                skipped++;
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Added health bars to {added} enemies. Skipped {skipped}.");
        EditorUtility.DisplayDialog("Setup Complete", 
            $"Added health bars to {added} enemy/enemies.\nSkipped {skipped} (already had health bar or missing EnemyHealth).", "OK");
    }
    
    bool AddHealthBarToEnemy(GameObject enemy)
    {
        if (enemy == null) return false;
        
        // Check if already has health bar
        if (enemy.GetComponent<EnemyHealthBar>() != null)
        {
            Debug.Log($"Enemy {enemy.name} already has EnemyHealthBar component.");
            return false;
        }
        
        // Ensure EnemyHealth component exists
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = enemy.AddComponent<EnemyHealth>();
            Debug.Log($"Added EnemyHealth component to {enemy.name}");
        }
        
        // Add EnemyHealthBar component
        EnemyHealthBar healthBar = enemy.AddComponent<EnemyHealthBar>();
        
        // Configure using SerializedObject
        SerializedObject serializedHealthBar = new SerializedObject(healthBar);
        serializedHealthBar.FindProperty("offsetY").floatValue = offsetY;
        serializedHealthBar.FindProperty("healthBarSize").vector2Value = healthBarSize;
        serializedHealthBar.FindProperty("hideWhenFullHealth").boolValue = hideWhenFullHealth;
        serializedHealthBar.FindProperty("hideWhenDead").boolValue = hideWhenDead;
        serializedHealthBar.FindProperty("showHealthText").boolValue = showHealthText;
        // Use the tool's setting
        serializedHealthBar.FindProperty("alwaysShowForTesting").boolValue = alwaysShowForTesting;
        serializedHealthBar.ApplyModifiedProperties();
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        Debug.Log($"Added EnemyHealthBar to {enemy.name}");
        return true;
    }
    
    void RemoveHealthBarFromSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more enemy GameObjects.", "OK");
            return;
        }
        
        int removed = 0;
        
        foreach (GameObject obj in selected)
        {
            EnemyHealthBar healthBar = obj.GetComponent<EnemyHealthBar>();
            if (healthBar != null)
            {
                // Destroy the health bar canvas if it exists
                Transform canvasTransform = obj.transform.Find("HealthBarCanvas");
                if (canvasTransform != null)
                {
                    DestroyImmediate(canvasTransform.gameObject);
                }
                
                DestroyImmediate(healthBar);
                removed++;
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Removed health bars from {removed} enemies.");
        EditorUtility.DisplayDialog("Removal Complete", $"Removed health bars from {removed} enemy/enemies.", "OK");
    }
    
    void RefreshEnemyList()
    {
        enemiesWithHealthBars.Clear();
        
        // Find all enemies with health bars
        EnemyHealthBar[] healthBars = FindObjectsByType<EnemyHealthBar>(FindObjectsSortMode.None);
        foreach (EnemyHealthBar healthBar in healthBars)
        {
            if (healthBar != null && healthBar.gameObject != null)
            {
                enemiesWithHealthBars.Add(healthBar.gameObject);
            }
        }
    }
    
    void ShowAllHealthBars()
    {
        int shown = 0;
        
        foreach (GameObject enemy in enemiesWithHealthBars)
        {
            if (enemy == null) continue;
            
            EnemyHealthBar healthBar = enemy.GetComponent<EnemyHealthBar>();
            if (healthBar != null)
            {
                // Set alwaysShowForTesting to true
                SerializedObject serializedHealthBar = new SerializedObject(healthBar);
                serializedHealthBar.FindProperty("alwaysShowForTesting").boolValue = true;
                serializedHealthBar.ApplyModifiedProperties();
                
                // Find and activate canvas
                Transform canvasTransform = enemy.transform.Find("HealthBarCanvas");
                if (canvasTransform != null)
                {
                    canvasTransform.gameObject.SetActive(true);
                    shown++;
                }
            }
        }
        
        Debug.Log($"Forced {shown} health bars to be visible.");
        EditorUtility.DisplayDialog("Debug", $"Forced {shown} health bars to be visible.\nSet 'Always Show For Testing' to true on all.", "OK");
    }
    
    void HideAllHealthBars()
    {
        int hidden = 0;
        
        foreach (GameObject enemy in enemiesWithHealthBars)
        {
            if (enemy == null) continue;
            
            Transform canvasTransform = enemy.transform.Find("HealthBarCanvas");
            if (canvasTransform != null)
            {
                canvasTransform.gameObject.SetActive(false);
                hidden++;
            }
        }
        
        Debug.Log($"Hidden {hidden} health bars.");
        EditorUtility.DisplayDialog("Debug", $"Hidden {hidden} health bars.", "OK");
    }
    
    void EnableAlwaysShowForAll()
    {
        int enabled = 0;
        
        foreach (GameObject enemy in enemiesWithHealthBars)
        {
            if (enemy == null) continue;
            
            EnemyHealthBar healthBar = enemy.GetComponent<EnemyHealthBar>();
            if (healthBar != null)
            {
                SerializedObject so = new SerializedObject(healthBar);
                so.FindProperty("alwaysShowForTesting").boolValue = true;
                so.FindProperty("hideWhenFullHealth").boolValue = false; // Also disable hide when full
                so.ApplyModifiedProperties();
                enabled++;
            }
        }
        
        // Also find enemies without health bars but with EnemyHealth
        EnemyHealth[] allEnemyHealth = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemyHealth in allEnemyHealth)
        {
            if (enemyHealth == null) continue;
            
            EnemyHealthBar healthBar = enemyHealth.GetComponent<EnemyHealthBar>();
            if (healthBar == null)
            {
                // Add health bar if missing
                if (AddHealthBarToEnemy(enemyHealth.gameObject))
                {
                    healthBar = enemyHealth.GetComponent<EnemyHealthBar>();
                }
            }
            
            if (healthBar != null)
            {
                SerializedObject so = new SerializedObject(healthBar);
                so.FindProperty("alwaysShowForTesting").boolValue = true;
                so.FindProperty("hideWhenFullHealth").boolValue = false;
                so.ApplyModifiedProperties();
                if (!enemiesWithHealthBars.Contains(enemyHealth.gameObject))
                {
                    enabled++;
                }
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Enabled 'Always Show For Testing' for {enabled} enemies.");
        EditorUtility.DisplayDialog("Enabled", 
            $"Enabled 'Always Show For Testing' for {enabled} enemy/enemies.\n\n" +
            "Health bars will now be visible in Play Mode even at full health.", "OK");
    }
    
    void LogHealthBarStatus()
    {
        Debug.Log("=== Enemy Health Bar Status ===");
        
        int count = 0;
        int visible = 0;
        int hidden = 0;
        
        foreach (GameObject enemy in enemiesWithHealthBars)
        {
            if (enemy == null) continue;
            
            count++;
            EnemyHealthBar healthBar = enemy.GetComponent<EnemyHealthBar>();
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            Transform canvasTransform = enemy.transform.Find("HealthBarCanvas");
            
            string status = $"Enemy: {enemy.name}\n";
            
            if (healthBar != null)
            {
                status += $"  - Has EnemyHealthBar: ✓\n";
                
                SerializedObject so = new SerializedObject(healthBar);
                status += $"  - Offset Y: {so.FindProperty("offsetY").floatValue}\n";
                status += $"  - Health Bar Size: {so.FindProperty("healthBarSize").vector2Value}\n";
                status += $"  - Hide When Full: {so.FindProperty("hideWhenFullHealth").boolValue}\n";
                status += $"  - Always Show For Testing: {so.FindProperty("alwaysShowForTesting").boolValue}\n";
            }
            else
            {
                status += $"  - Has EnemyHealthBar: ✗\n";
            }
            
            if (enemyHealth != null)
            {
                status += $"  - Health: {enemyHealth.CurrentHealth:F0}/{enemyHealth.MaxHealth:F0}\n";
            }
            else
            {
                status += $"  - Has EnemyHealth: ✗\n";
            }
            
            if (canvasTransform != null)
            {
                bool isActive = canvasTransform.gameObject.activeSelf;
                status += $"  - Canvas Active: {isActive}\n";
                status += $"  - Canvas Position: {canvasTransform.position}\n";
                status += $"  - Canvas Scale: {canvasTransform.localScale}\n";
                
                if (isActive) visible++;
                else hidden++;
            }
            else
            {
                status += $"  - Canvas: Not found\n";
                hidden++;
            }
            
            Debug.Log(status);
        }
        
        Debug.Log($"=== Summary: {count} enemies, {visible} visible, {hidden} hidden ===");
        EditorUtility.DisplayDialog("Status Logged", 
            $"Health bar status logged to Console.\n\nTotal: {count}\nVisible: {visible}\nHidden: {hidden}", "OK");
    }
    
    void TestHealthBarVisibility()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Not in Play Mode", 
                "This test requires Play Mode. Please enter Play Mode first.", "OK");
            return;
        }
        
        int tested = 0;
        int visible = 0;
        int hidden = 0;
        int missing = 0;
        
        foreach (GameObject enemy in enemiesWithHealthBars)
        {
            if (enemy == null) continue;
            
            tested++;
            EnemyHealthBar healthBar = enemy.GetComponent<EnemyHealthBar>();
            
            if (healthBar == null)
            {
                missing++;
                continue;
            }
            
            // Force enable alwaysShowForTesting
            SerializedObject so = new SerializedObject(healthBar);
            so.FindProperty("alwaysShowForTesting").boolValue = true;
            so.ApplyModifiedProperties();
            
            // Find canvas
            Transform canvasTransform = enemy.transform.Find("HealthBarCanvas");
            if (canvasTransform != null)
            {
                canvasTransform.gameObject.SetActive(true);
                
                // Check if actually visible
                Canvas canvas = canvasTransform.GetComponent<Canvas>();
                if (canvas != null && canvas.gameObject.activeInHierarchy)
                {
                    visible++;
                    Debug.Log($"[Test] {enemy.name} - Health bar canvas is ACTIVE and VISIBLE at position {canvasTransform.position}");
                }
                else
                {
                    hidden++;
                    Debug.Log($"[Test] {enemy.name} - Health bar canvas exists but is NOT active");
                }
            }
            else
            {
                missing++;
                Debug.LogWarning($"[Test] {enemy.name} - Health bar canvas NOT FOUND (may not be created yet in Start())");
            }
        }
        
        string message = $"Tested {tested} enemies:\n";
        message += $"✓ Visible: {visible}\n";
        message += $"✗ Hidden: {hidden}\n";
        message += $"? Missing Canvas: {missing}\n\n";
        message += "Check Console for details.";
        
        EditorUtility.DisplayDialog("Visibility Test", message, "OK");
    }
    
    void CreateHealthBarUIForSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more enemy GameObjects.", "OK");
            return;
        }
        
        int created = 0;
        int skipped = 0;
        
        foreach (GameObject enemy in selected)
        {
            if (enemy == null) continue;
            
            // Check if preview canvas already exists
            Transform existingCanvas = enemy.transform.Find("HealthBarCanvas");
            if (existingCanvas != null)
            {
                skipped++;
                continue;
            }
            
            if (CreateHealthBarUIForEnemy(enemy))
            {
                created++;
            }
            else
            {
                skipped++;
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Created preview health bars for {created} enemies. Skipped {skipped}.");
        EditorUtility.DisplayDialog("Preview Created", 
            $"Created preview health bars for {created} enemy/enemies.\nSkipped {skipped} (already has preview).", "OK");
    }
    
    void CreateHealthBarUIForAll()
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (allEnemies.Length == 0)
        {
            EnemyController[] controllers = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            allEnemies = new GameObject[controllers.Length];
            for (int i = 0; i < controllers.Length; i++)
            {
                allEnemies[i] = controllers[i].gameObject;
            }
        }
        
        if (allEnemies.Length == 0)
        {
            EditorUtility.DisplayDialog("No Enemies Found", 
                "No enemies found in the scene.", "OK");
            return;
        }
        
        if (!EditorUtility.DisplayDialog("Create Preview for All", 
            $"This will create preview health bars for {allEnemies.Length} enemy/enemies. Continue?", "Yes", "No"))
        {
            return;
        }
        
        int created = 0;
        int skipped = 0;
        
        foreach (GameObject enemy in allEnemies)
        {
            Transform existingCanvas = enemy.transform.Find("HealthBarCanvas");
            if (existingCanvas != null)
            {
                skipped++;
                continue;
            }
            
            if (CreateHealthBarUIForEnemy(enemy))
            {
                created++;
            }
            else
            {
                skipped++;
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Created preview health bars for {created} enemies. Skipped {skipped}.");
        EditorUtility.DisplayDialog("Preview Created", 
            $"Created preview health bars for {created} enemy/enemies.\nSkipped {skipped} (already has preview).", "OK");
    }
    
    bool CreateHealthBarUIForEnemy(GameObject enemy)
    {
        if (enemy == null) return false;
        
        // Get or add EnemyHealth component
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = enemy.AddComponent<EnemyHealth>();
        }
        
        // Get settings from EnemyHealthBar if it exists, otherwise use defaults
        EnemyHealthBar existingHealthBar = enemy.GetComponent<EnemyHealthBar>();
        float offsetY = this.offsetY;
        Vector2 size = this.healthBarSize;
        bool showText = this.showHealthText;
        
        if (existingHealthBar != null)
        {
            SerializedObject so = new SerializedObject(existingHealthBar);
            offsetY = so.FindProperty("offsetY").floatValue;
            size = so.FindProperty("healthBarSize").vector2Value;
            showText = so.FindProperty("showHealthText").boolValue;
        }
        
        // Get camera for world-space canvas
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogWarning($"No camera found. Cannot create preview health bar for {enemy.name}.");
            return false;
        }
        
        // Create Canvas for world-space UI
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(enemy.transform);
        canvasObj.transform.localPosition = new Vector3(0, offsetY, 0);
        canvasObj.transform.localRotation = Quaternion.identity;
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
        canvas.sortingOrder = 10;
        
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(size.x * 100, size.y * 100);
        canvasRect.localScale = Vector3.one * 0.01f;
        canvasRect.pivot = new Vector2(0.5f, 0f);
        canvasRect.anchorMin = new Vector2(0.5f, 0f);
        canvasRect.anchorMax = new Vector2(0.5f, 0f);
        
        // Create background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(canvasObj.transform, false);
        
        UnityEngine.UI.Image bgImage = backgroundObj.AddComponent<UnityEngine.UI.Image>();
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
        
        UnityEngine.UI.Image borderImage = borderObj.AddComponent<UnityEngine.UI.Image>();
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
        
        UnityEngine.UI.Image fillImage = fillObj.AddComponent<UnityEngine.UI.Image>();
        fillImage.color = new Color(1f, 0f, 0f, 1f); // Red
        fillImage.type = UnityEngine.UI.Image.Type.Filled;
        fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        fillImage.raycastTarget = false;
        
        Texture2D fillTex = new Texture2D(1, 1);
        fillTex.SetPixel(0, 0, Color.white);
        fillTex.Apply();
        fillImage.sprite = Sprite.Create(fillTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        // Set fill amount based on current health
        float fillAmount = 1f;
        if (enemyHealth != null && enemyHealth.MaxHealth > 0)
        {
            fillAmount = enemyHealth.CurrentHealth / enemyHealth.MaxHealth;
        }
        fillImage.fillAmount = fillAmount;
        
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
            shadowText.text = enemyHealth != null ? $"{Mathf.CeilToInt(enemyHealth.CurrentHealth)} / {Mathf.CeilToInt(enemyHealth.MaxHealth)}" : "0 / 0";
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
            healthText.text = enemyHealth != null ? $"{Mathf.CeilToInt(enemyHealth.CurrentHealth)} / {Mathf.CeilToInt(enemyHealth.MaxHealth)}" : "0 / 0";
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
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"Created preview health bar UI for {enemy.name}");
        return true;
    }
    
    void RemovePreviewHealthBars()
    {
        int removed = 0;
        
        // Find all preview canvases
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (Transform t in allTransforms)
        {
            // Skip runtime-created canvases, only remove editor-created ones
            // We'll check if it has the preview marker or was created in editor mode
            if (t.name == "HealthBarCanvas" && !Application.isPlaying)
            {
                DestroyImmediate(t.gameObject);
                removed++;
            }
        }
        
        RefreshEnemyList();
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"Removed {removed} preview health bars.");
        EditorUtility.DisplayDialog("Removal Complete", $"Removed {removed} preview health bar(s).", "OK");
    }
}

