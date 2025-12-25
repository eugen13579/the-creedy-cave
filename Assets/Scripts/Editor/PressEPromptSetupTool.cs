using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool to create "Press E" prompt UI for gate interactions.
/// </summary>
public class PressEPromptSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Press E Prompt UI")]
    public static void ShowWindow()
    {
        GetWindow<PressEPromptSetupTool>("Press E Prompt Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Press E Prompt UI Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This will create a 'Press E' prompt UI in the HUDCanvas.\n\n" +
            "The prompt will appear at the bottom-center of the screen and can be " +
            "shown/hidden by GateController when player approaches gates.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Press E Prompt", GUILayout.Height(30)))
        {
            CreatePressEPrompt();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Find and Link to Gates", GUILayout.Height(25)))
        {
            LinkPromptToGates();
        }
    }
    
    void CreatePressEPrompt()
    {
        // Step 1: Get or create HUD Canvas
        Canvas hudCanvas = GetOrCreateHUDCanvas();
        
        // Step 2: Check if prompt already exists
        Transform existingPrompt = hudCanvas.transform.Find("PressEPrompt");
        if (existingPrompt != null)
        {
            if (EditorUtility.DisplayDialog("Press E Prompt Exists", 
                "A 'Press E' prompt already exists. Do you want to recreate it?", "Yes", "No"))
            {
                DestroyImmediate(existingPrompt.gameObject);
            }
            else
            {
                Debug.Log("Press E Prompt setup cancelled.");
                return;
            }
        }
        
        // Step 3: Create Press E prompt
        GameObject promptObj = new GameObject("PressEPrompt");
        promptObj.transform.SetParent(hudCanvas.transform, false);
        
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
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        Debug.Log("Created Press E prompt UI in HUDCanvas");
        EditorUtility.DisplayDialog("Setup Complete", 
            "Press E prompt UI has been created successfully!\n\n" +
            "Location: HUDCanvas > PressEPrompt\n" +
            "Position: Bottom-center of screen\n\n" +
            "The prompt will be automatically shown/hidden by GateController when player approaches gates.", "OK");
    }
    
    void LinkPromptToGates()
    {
        // Find Press E prompt
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
        
        if (hudCanvas == null)
        {
            EditorUtility.DisplayDialog("HUDCanvas Not Found", 
                "HUDCanvas not found in the scene.\n\n" +
                "Please run 'Tools > Setup Press E Prompt UI' first to create the prompt.", "OK");
            return;
        }
        
        Transform promptTransform = hudCanvas.transform.Find("PressEPrompt");
        if (promptTransform == null)
        {
            EditorUtility.DisplayDialog("Press E Prompt Not Found", 
                "Press E prompt not found in HUDCanvas.\n\n" +
                "Please run 'Tools > Setup Press E Prompt UI' first to create the prompt.", "OK");
            return;
        }
        
        // Find all gates with GateController
        GameObject[] gates = GameObject.FindGameObjectsWithTag("Gate");
        if (gates.Length == 0)
        {
            EditorUtility.DisplayDialog("No Gates Found", 
                "No GameObjects with 'Gate' tag found in the scene.\n\n" +
                "Please make sure your Gate prefabs have the 'Gate' tag assigned.", "OK");
            return;
        }
        
        int linkedCount = 0;
        
        foreach (GameObject gate in gates)
        {
            GateController controller = gate.GetComponent<GateController>();
            if (controller == null)
            {
                continue; // Skip gates without GateController
            }
            
            // Link prompt UI to GateController
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("pressEPromptUI").objectReferenceValue = promptTransform.gameObject;
            serializedController.ApplyModifiedProperties();
            
            linkedCount++;
        }
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        EditorUtility.DisplayDialog("Linking Complete", 
            $"Successfully linked Press E prompt to {linkedCount} gate(s).\n\n" +
            "The prompt will now be shown/hidden automatically when player approaches gates.", "OK");
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
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvas.sortingOrder = 100;
            
            Debug.Log("Created HUDCanvas for Press E prompt");
        }
        
        return canvas;
    }
}

