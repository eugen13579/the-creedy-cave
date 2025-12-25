using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class LevelCompleteSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Level Complete UI")]
    public static void ShowWindow()
    {
        GetWindow<LevelCompleteSetupTool>("Level Complete Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Level Complete UI Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This will create:\n" +
            "1. 'Press E' prompt UI (bottom-center of screen)\n" +
            "2. Level Complete Canvas with title and Next Level button\n" +
            "3. LevelCompleteManager GameObject", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Level Complete UI", GUILayout.Height(30)))
        {
            SetupLevelCompleteUI();
        }
    }
    
    void SetupLevelCompleteUI()
    {
        // Step 1: Create or find HUD Canvas for "Press E" prompt
        Canvas hudCanvas = GetOrCreateHUDCanvas();
        
        // Step 2: Create "Press E" prompt UI
        CreatePressEPrompt(hudCanvas);
        
        // Step 3: Create Level Complete Canvas
        GameObject levelCompleteCanvas = CreateLevelCompleteCanvas();
        
        // Step 4: Create or find LevelCompleteManager
        CreateLevelCompleteManager(levelCompleteCanvas);
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        Debug.Log("Level Complete UI setup complete!");
        EditorUtility.DisplayDialog("Setup Complete", 
            "Level Complete UI has been set up successfully!\n\n" +
            "1. 'Press E' prompt created in HUDCanvas\n" +
            "2. Level Complete Canvas created\n" +
            "3. LevelCompleteManager GameObject created\n\n" +
            "Make sure to add GateController component to your Gate prefab!", "OK");
    }
    
    Canvas GetOrCreateHUDCanvas()
    {
        Canvas canvas = null;
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        // Look for existing HUD Canvas
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "HUDCanvas" || c.name == "HUD")
            {
                canvas = c;
                break;
            }
        }
        
        // Create new HUD Canvas if not found
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("HUDCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.sortingOrder = 100;
            
            Debug.Log("Created HUDCanvas for Press E prompt");
        }
        
        return canvas;
    }
    
    void CreatePressEPrompt(Canvas parentCanvas)
    {
        // Check if already exists
        Transform existingPrompt = parentCanvas.transform.Find("PressEPrompt");
        if (existingPrompt != null)
        {
            if (EditorUtility.DisplayDialog("Press E Prompt Exists", 
                "A 'Press E' prompt already exists. Do you want to recreate it?", "Yes", "No"))
            {
                DestroyImmediate(existingPrompt.gameObject);
            }
            else
            {
                return;
            }
        }
        
        // Create Press E prompt container
        GameObject promptObj = new GameObject("PressEPrompt");
        promptObj.transform.SetParent(parentCanvas.transform, false);
        
        RectTransform promptRect = promptObj.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.anchoredPosition = new Vector2(0, 100); // 100 pixels from bottom
        promptRect.sizeDelta = new Vector2(300, 60);
        
        // Create TextMeshPro text
        TextMeshProUGUI promptText = promptObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "Press E";
        promptText.fontSize = 36;
        promptText.fontStyle = FontStyles.Bold;
        promptText.color = new Color(1f, 1f, 1f, 1f); // White
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Add outline for better visibility
        promptText.outlineWidth = 0.2f;
        promptText.outlineColor = new Color(0f, 0f, 0f, 1f); // Black outline
        
        // Initially hidden (will be shown by GateController when player is nearby)
        promptObj.SetActive(false);
        
        Debug.Log("Created Press E prompt UI");
    }
    
    GameObject CreateLevelCompleteCanvas()
    {
        // Check if already exists
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null && existingCanvas.name == "LevelCompleteCanvas")
        {
            if (EditorUtility.DisplayDialog("Level Complete Canvas Exists", 
                "A Level Complete Canvas already exists. Do you want to recreate it?", "Yes", "No"))
            {
                DestroyImmediate(existingCanvas.gameObject);
            }
            else
            {
                return existingCanvas.gameObject;
            }
        }
        
        // Create Level Complete Canvas
        GameObject canvasObj = new GameObject("LevelCompleteCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // Higher than HUD to appear on top
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Add CanvasGroup for fade effects
        CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // Create EventSystem if it doesn't exist
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Create background panel (semi-transparent overlay)
        GameObject backgroundPanel = new GameObject("BackgroundPanel");
        backgroundPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform bgRect = backgroundPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = backgroundPanel.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.8f); // Semi-transparent black
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, Color.white);
        bgTex.Apply();
        bgImage.sprite = Sprite.Create(bgTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        // Create content panel (centered)
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(600, 450);
        
        Image contentBg = contentPanel.AddComponent<Image>();
        contentBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f); // Dark gray background
        Texture2D contentTex = new Texture2D(1, 1);
        contentTex.SetPixel(0, 0, Color.white);
        contentTex.Apply();
        contentBg.sprite = Sprite.Create(contentTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        // Create title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(contentPanel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(500, 100);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Level Complete!";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(1f, 0.84f, 0f, 1f); // Gold color
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.verticalAlignment = VerticalAlignmentOptions.Middle;
        titleText.outlineWidth = 0.2f;
        titleText.outlineColor = new Color(0f, 0f, 0f, 1f);
        
        // Create points text
        GameObject pointsObj = new GameObject("PointsText");
        pointsObj.transform.SetParent(contentPanel.transform, false);
        
        RectTransform pointsRect = pointsObj.AddComponent<RectTransform>();
        pointsRect.anchorMin = new Vector2(0.5f, 0.5f);
        pointsRect.anchorMax = new Vector2(0.5f, 0.5f);
        pointsRect.pivot = new Vector2(0.5f, 0.5f);
        pointsRect.anchoredPosition = new Vector2(0, 50);
        pointsRect.sizeDelta = new Vector2(500, 60);
        
        TextMeshProUGUI pointsText = pointsObj.AddComponent<TextMeshProUGUI>();
        pointsText.text = "Points: 0";
        pointsText.fontSize = 32;
        pointsText.fontStyle = FontStyles.Bold;
        pointsText.color = new Color(1f, 1f, 1f, 1f); // White
        pointsText.alignment = TextAlignmentOptions.Center;
        pointsText.verticalAlignment = VerticalAlignmentOptions.Middle;
        pointsText.outlineWidth = 0.15f;
        pointsText.outlineColor = new Color(0f, 0f, 0f, 1f);
        
        // Create "Next Level" button (centered)
        GameObject nextLevelButtonObj = new GameObject("NextLevelButton");
        nextLevelButtonObj.transform.SetParent(contentPanel.transform, false);
        
        RectTransform nextButtonRect = nextLevelButtonObj.AddComponent<RectTransform>();
        nextButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        nextButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        nextButtonRect.pivot = new Vector2(0.5f, 0.5f);
        nextButtonRect.anchoredPosition = new Vector2(0, -100);
        nextButtonRect.sizeDelta = new Vector2(300, 60);
        
        Image nextButtonImage = nextLevelButtonObj.AddComponent<Image>();
        nextButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green
        Texture2D buttonTex = new Texture2D(1, 1);
        buttonTex.SetPixel(0, 0, Color.white);
        buttonTex.Apply();
        nextButtonImage.sprite = Sprite.Create(buttonTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        Button nextLevelButton = nextLevelButtonObj.AddComponent<Button>();
        
        // Button text
        GameObject nextButtonTextObj = new GameObject("Text");
        nextButtonTextObj.transform.SetParent(nextLevelButtonObj.transform, false);
        
        RectTransform nextButtonTextRect = nextButtonTextObj.AddComponent<RectTransform>();
        nextButtonTextRect.anchorMin = Vector2.zero;
        nextButtonTextRect.anchorMax = Vector2.one;
        nextButtonTextRect.sizeDelta = Vector2.zero;
        nextButtonTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI nextButtonText = nextButtonTextObj.AddComponent<TextMeshProUGUI>();
        nextButtonText.text = "Next Level";
        nextButtonText.fontSize = 28;
        nextButtonText.fontStyle = FontStyles.Bold;
        nextButtonText.color = Color.white;
        nextButtonText.alignment = TextAlignmentOptions.Center;
        nextButtonText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Link buttons to LevelCompleteManager (will be done after manager is created)
        // Store references for later
        EditorUtility.SetDirty(canvasObj);
        
        Debug.Log("Created Level Complete Canvas");
        
        // Initially hidden
        canvasObj.SetActive(false);
        
        return canvasObj;
    }
    
    void CreateLevelCompleteManager(GameObject levelCompleteCanvas)
    {
        // Check if LevelCompleteManager already exists
        LevelCompleteManager existingManager = FindFirstObjectByType<LevelCompleteManager>();
        if (existingManager != null)
        {
            // Update reference to level complete UI
            SerializedObject serializedExisting = new SerializedObject(existingManager);
            serializedExisting.FindProperty("levelCompleteUI").objectReferenceValue = levelCompleteCanvas;
            
            // Find and link points text
            Transform pointsTransformExisting = levelCompleteCanvas.transform.Find("ContentPanel/PointsText");
            if (pointsTransformExisting != null)
            {
                TextMeshProUGUI pointsTextExisting = pointsTransformExisting.GetComponent<TextMeshProUGUI>();
                if (pointsTextExisting != null)
                {
                    serializedExisting.FindProperty("pointsText").objectReferenceValue = pointsTextExisting;
                }
            }
            
            serializedExisting.ApplyModifiedProperties();
            Debug.Log("Updated existing LevelCompleteManager with Level Complete Canvas reference");
            return;
        }
        
        // Create LevelCompleteManager GameObject
        GameObject managerObj = new GameObject("LevelCompleteManager");
        LevelCompleteManager manager = managerObj.AddComponent<LevelCompleteManager>();
        
        // Set level complete UI reference
        SerializedObject serializedNew = new SerializedObject(manager);
        serializedNew.FindProperty("levelCompleteUI").objectReferenceValue = levelCompleteCanvas;
        
        // Find and link points text
        Transform pointsTransformNew = levelCompleteCanvas.transform.Find("ContentPanel/PointsText");
        if (pointsTransformNew != null)
        {
            TextMeshProUGUI pointsTextNew = pointsTransformNew.GetComponent<TextMeshProUGUI>();
            if (pointsTextNew != null)
            {
                serializedNew.FindProperty("pointsText").objectReferenceValue = pointsTextNew;
            }
        }
        
        serializedNew.FindProperty("levelSelectSceneName").stringValue = "LevelSelect";
        serializedNew.FindProperty("useAutoProgression").boolValue = true;
        serializedNew.ApplyModifiedProperties();
        
        // Button listeners will be set up in LevelCompleteManager.Start() at runtime
        Debug.Log("Created LevelCompleteManager GameObject");
    }
}

