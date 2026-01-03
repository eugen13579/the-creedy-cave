using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool to create player stats display UI (Gold, Attack Damage, Defense) in the top-left corner.
/// </summary>
public class PlayerStatsDisplaySetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Player Stats Display")]
    public static void ShowWindow()
    {
        GetWindow<PlayerStatsDisplaySetupTool>("Player Stats Display Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Player Stats Display Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This will create a UI display in the top-left corner showing:\n" +
            "1. Gold (from CoinManager)\n" +
            "2. Attack Damage (from PlayerHealth)\n" +
            "3. Defense (calculated from equipped armor items)\n\n" +
            "The display will update automatically during gameplay.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Stats Display", GUILayout.Height(30)))
        {
            CreateStatsDisplay();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Find and Link Components", GUILayout.Height(25)))
        {
            LinkComponents();
        }
    }
    
    void CreateStatsDisplay()
    {
        // Find or create HUDCanvas
        Canvas hudCanvas = GetOrCreateHUDCanvas();
        
        // Check if stats display already exists
        Transform existingDisplay = hudCanvas.transform.Find("PlayerStatsDisplay");
        if (existingDisplay != null)
        {
            if (!EditorUtility.DisplayDialog("Stats Display Exists", 
                "A Player Stats Display already exists. Do you want to recreate it?", "Yes", "No"))
            {
                return;
            }
            DestroyImmediate(existingDisplay.gameObject);
        }
        
        // Create container for stats display
        GameObject statsContainer = new GameObject("PlayerStatsDisplay");
        statsContainer.transform.SetParent(hudCanvas.transform, false);
        
        RectTransform containerRect = statsContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20); // Top-left, 20px from edges
        containerRect.sizeDelta = new Vector2(300, 120);
        
        // Add PlayerStatsDisplay component
        PlayerStatsDisplay statsDisplay = statsContainer.AddComponent<PlayerStatsDisplay>();
        
        // Create Gold text
        GameObject goldObj = CreateStatText("GoldText", statsContainer.transform, new Vector2(0, 0));
        TextMeshProUGUI goldText = goldObj.GetComponent<TextMeshProUGUI>();
        goldText.text = "Gold: 0";
        goldText.color = Color.yellow;
        
        // Create Attack Damage text
        GameObject attackObj = CreateStatText("AttackDamageText", statsContainer.transform, new Vector2(0, -40));
        TextMeshProUGUI attackText = attackObj.GetComponent<TextMeshProUGUI>();
        attackText.text = "Attack: 0";
        attackText.color = Color.red;
        
        // Create Defense text
        GameObject defenseObj = CreateStatText("DefenseText", statsContainer.transform, new Vector2(0, -80));
        TextMeshProUGUI defenseText = defenseObj.GetComponent<TextMeshProUGUI>();
        defenseText.text = "Defense: 0";
        defenseText.color = Color.cyan;
        
        // Link text components to PlayerStatsDisplay
        SerializedObject serializedStats = new SerializedObject(statsDisplay);
        serializedStats.FindProperty("goldText").objectReferenceValue = goldText;
        serializedStats.FindProperty("attackDamageText").objectReferenceValue = attackText;
        serializedStats.FindProperty("defenseText").objectReferenceValue = defenseText;
        serializedStats.ApplyModifiedProperties();
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        // Select the created display
        Selection.activeGameObject = statsContainer;
        
        EditorUtility.DisplayDialog("Setup Complete", 
            "Player Stats Display has been created successfully!\n\n" +
            "The display shows:\n" +
            "- Gold (top)\n" +
            "- Attack Damage (middle)\n" +
            "- Defense (bottom)\n\n" +
            "Stats will update automatically during gameplay.", "OK");
        
        Debug.Log("PlayerStatsDisplaySetupTool: Created Player Stats Display UI.");
    }
    
    GameObject CreateStatText(string name, Transform parent, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(0, 1);
        textRect.pivot = new Vector2(0, 1);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(280, 35);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Stat: 0";
        text.fontSize = 28;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        text.verticalAlignment = VerticalAlignmentOptions.Top;
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;
        
        return textObj;
    }
    
    Canvas GetOrCreateHUDCanvas()
    {
        // Try to find HUDCanvas
        Canvas hudCanvas = null;
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "HUDCanvas" || c.name == "HUD")
            {
                hudCanvas = c;
                break;
            }
        }
        
        if (hudCanvas != null)
        {
            Debug.Log("Using existing HUDCanvas for stats display");
            return hudCanvas;
        }
        
        // Create new HUDCanvas
        GameObject canvasObj = new GameObject("HUDCanvas");
        hudCanvas = canvasObj.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        hudCanvas.sortingOrder = 100;
        
        Debug.Log("Created HUDCanvas for stats display");
        return hudCanvas;
    }
    
    void LinkComponents()
    {
        // Find PlayerStatsDisplay
        PlayerStatsDisplay statsDisplay = FindFirstObjectByType<PlayerStatsDisplay>();
        if (statsDisplay == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "PlayerStatsDisplay component not found!\n\n" +
                "Please create it first using 'Create Stats Display'.", "OK");
            return;
        }
        
        // Find text components
        Transform goldText = statsDisplay.transform.Find("GoldText");
        Transform attackText = statsDisplay.transform.Find("AttackDamageText");
        Transform defenseText = statsDisplay.transform.Find("DefenseText");
        
        if (goldText == null || attackText == null || defenseText == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Stats text components not found!\n\n" +
                "Please recreate the display using 'Create Stats Display'.", "OK");
            return;
        }
        
        // Link text components
        SerializedObject serializedStats = new SerializedObject(statsDisplay);
        serializedStats.FindProperty("goldText").objectReferenceValue = goldText.GetComponent<TextMeshProUGUI>();
        serializedStats.FindProperty("attackDamageText").objectReferenceValue = attackText.GetComponent<TextMeshProUGUI>();
        serializedStats.FindProperty("defenseText").objectReferenceValue = defenseText.GetComponent<TextMeshProUGUI>();
        serializedStats.ApplyModifiedProperties();
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        EditorUtility.DisplayDialog("Link Complete", 
            "Stats display components have been linked!", "OK");
        
        Debug.Log("PlayerStatsDisplaySetupTool: Linked stats display components.");
    }
}


