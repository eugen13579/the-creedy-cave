using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using System;

/// <summary>
/// Editor tool for saving game state from the Unity Editor.
/// Provides a visual interface with a save button icon and save slot selection.
/// </summary>
public class SaveSystemEditorTool : EditorWindow
{
    private const int MAX_SAVE_SLOTS = 5;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Save System")]
    public static void ShowWindow()
    {
        GetWindow<SaveSystemEditorTool>("Save System");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Save System Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool creates a save button UI component in the game scene.\n\n" +
            "The save button will appear at the bottom-right of the screen during gameplay.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Save Button UI", GUILayout.Height(40)))
        {
            CreateSaveButtonUI();
        }
        
        if (GUILayout.Button("Update Existing UI", GUILayout.Height(40)))
        {
            UpdateExistingUI();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Show save slot information
        EditorGUILayout.LabelField("Save Slots:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 1; i <= MAX_SAVE_SLOTS; i++)
        {
            DrawSaveSlotInfo(i);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    /// <summary>
    /// Draws information about a save slot.
    /// </summary>
    /// <param name="slot">Slot number (1-5)</param>
    void DrawSaveSlotInfo(int slot)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        string filePath = SaveSystem.GetSaveFilePath(slot);
        bool exists = !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Save Slot {slot}:", EditorStyles.boldLabel);
        
        if (exists)
        {
            EditorGUILayout.LabelField("✓ Exists", EditorStyles.miniLabel);
            
            // Try to read timestamp
            try
            {
                string json = File.ReadAllText(filePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                if (saveData != null && !string.IsNullOrEmpty(saveData.saveTimestamp))
                {
                    EditorGUILayout.LabelField($"Saved: {saveData.saveTimestamp}", EditorStyles.miniLabel);
                }
            }
            catch
            {
                // Ignore errors when reading timestamp
            }
        }
        else
        {
            EditorGUILayout.LabelField("Empty", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (exists && !string.IsNullOrEmpty(filePath))
        {
            EditorGUILayout.LabelField($"Path: {filePath}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    /// <summary>
    /// Creates the save button UI component in the game scene.
    /// </summary>
    void CreateSaveButtonUI()
    {
        // Step 1: Get or create HUD Canvas
        Canvas hudCanvas = GetOrCreateHUDCanvas();
        
        // Step 2: Check if save button already exists
        Transform existingButton = hudCanvas.transform.Find("SaveButton");
        if (existingButton != null)
        {
            if (EditorUtility.DisplayDialog("Save Button Exists", 
                "A save button already exists. Do you want to recreate it?", "Yes", "No"))
            {
                DestroyImmediate(existingButton.gameObject);
            }
            else
            {
                Debug.Log("Save button setup cancelled.");
                return;
            }
        }
        
        // Step 3: Create save button
        GameObject saveButtonObj = new GameObject("SaveButton");
        saveButtonObj.transform.SetParent(hudCanvas.transform, false);
        
        RectTransform buttonRect = saveButtonObj.AddComponent<RectTransform>();
        // Position at bottom-right
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-20, 20); // 20 pixels from bottom-right
        buttonRect.sizeDelta = new Vector2(80, 80); // Square button for icon
        
        Image buttonImage = saveButtonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.9f); // Green, slightly transparent
        
        Button button = saveButtonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.1f, 1f);
        button.colors = colors;
        
        // Create icon text (💾 or "Save")
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(saveButtonObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "💾";
        iconText.fontSize = 40;
        iconText.color = Color.white;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Step 4: Create save slot panel
        GameObject panelObj = CreateSaveSlotPanel(hudCanvas);
        
        // Step 5: Create Load button next to Save button
        GameObject loadButtonObj = CreateLoadButton(hudCanvas, saveButtonObj.transform);
        
        // Step 6: Add SaveButtonUI component and link references
        SaveButtonUI saveButtonUI = saveButtonObj.AddComponent<SaveButtonUI>();
        SerializedObject serializedUI = new SerializedObject(saveButtonUI);
        serializedUI.FindProperty("saveButton").objectReferenceValue = button;
        serializedUI.FindProperty("saveSlotPanel").objectReferenceValue = panelObj;
        
        if (loadButtonObj != null)
        {
            serializedUI.FindProperty("loadButton").objectReferenceValue = loadButtonObj.GetComponent<Button>();
        }
        
        Transform slotContainer = panelObj.transform.Find("SlotButtonContainer");
        if (slotContainer != null)
        {
            serializedUI.FindProperty("slotButtonContainer").objectReferenceValue = slotContainer;
        }
        
        Transform titleObj = panelObj.transform.Find("Title");
        if (titleObj != null)
        {
            serializedUI.FindProperty("panelTitleText").objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();
        }
        
        Transform closeButton = panelObj.transform.Find("CloseButton");
        if (closeButton != null)
        {
            serializedUI.FindProperty("closePanelButton").objectReferenceValue = closeButton.GetComponent<Button>();
        }
        
        serializedUI.ApplyModifiedProperties();
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        // Select save button in hierarchy
        Selection.activeGameObject = saveButtonObj;
        
        EditorUtility.DisplayDialog("Setup Complete", 
            "Save/Load button UI has been created successfully!\n\n" +
            "The save and load buttons will appear at the bottom-right during gameplay.\n" +
            "Click Save to save your game, or Load to load from a saved slot.", "OK");
    }
    
    /// <summary>
    /// Creates the load button next to the save button.
    /// </summary>
    GameObject CreateLoadButton(Canvas parentCanvas, Transform saveButtonTransform)
    {
        GameObject loadButtonObj = new GameObject("LoadButton");
        loadButtonObj.transform.SetParent(parentCanvas.transform, false);
        
        RectTransform buttonRect = loadButtonObj.AddComponent<RectTransform>();
        // Position to the left of save button
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-110, 20); // 110 pixels to the left of save button
        buttonRect.sizeDelta = new Vector2(80, 80); // Same size as save button
        
        Image buttonImage = loadButtonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.5f, 0.8f, 0.3f, 0.9f); // Green, slightly transparent
        
        Button button = loadButtonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.5f, 0.8f, 0.3f, 0.9f);
        colors.highlightedColor = new Color(0.6f, 0.9f, 0.4f, 1f);
        colors.pressedColor = new Color(0.4f, 0.7f, 0.2f, 1f);
        button.colors = colors;
        
        // Create icon text (📂 or "Load")
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(loadButtonObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "📂";
        iconText.fontSize = 40;
        iconText.color = Color.white;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        Debug.Log("Created LoadButton");
        return loadButtonObj;
    }
    
    /// <summary>
    /// Gets or creates the HUD Canvas.
    /// </summary>
    Canvas GetOrCreateHUDCanvas()
    {
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
            Debug.Log("Using existing HUDCanvas for save button");
            return hudCanvas;
        }
        
        // Create new HUDCanvas
        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        canvas.sortingOrder = 100;
        
        Debug.Log("Created HUDCanvas for save button");
        return canvas;
    }
    
    /// <summary>
    /// Creates the save slot selection panel.
    /// </summary>
    GameObject CreateSaveSlotPanel(Canvas parentCanvas)
    {
        GameObject panelObj = new GameObject("SaveSlotPanel");
        panelObj.transform.SetParent(parentCanvas.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500, 400);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark semi-transparent background
        
        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(450, 50);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Select Save Slot";
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Create slot button container
        GameObject containerObj = new GameObject("SlotButtonContainer");
        containerObj.transform.SetParent(panelObj.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, -20);
        containerRect.sizeDelta = new Vector2(450, 280);
        
        VerticalLayoutGroup layoutGroup = containerObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        
        // Create close button
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(panelObj.transform, false);
        RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0, 20);
        closeRect.sizeDelta = new Vector2(200, 40);
        
        Image closeImage = closeButtonObj.AddComponent<Image>();
        closeImage.color = new Color(0.6f, 0.2f, 0.2f, 1f); // Red button
        
        Button closeButton = closeButtonObj.AddComponent<Button>();
        ColorBlock closeColors = closeButton.colors;
        closeColors.normalColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        closeColors.highlightedColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        closeColors.pressedColor = new Color(0.5f, 0.1f, 0.1f, 1f);
        closeButton.colors = closeColors;
        
        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "Close";
        closeText.fontSize = 24;
        closeText.color = Color.white;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Initially hide panel
        panelObj.SetActive(false);
        
        Debug.Log("Created SaveSlotPanel");
        return panelObj;
    }
    
    /// <summary>
    /// Updates existing Save/Load UI to match the new design.
    /// </summary>
    void UpdateExistingUI()
    {
        // Find existing SaveButtonUI components
        SaveButtonUI[] existingUIs = FindObjectsByType<SaveButtonUI>(FindObjectsSortMode.None);
        
        if (existingUIs.Length == 0)
        {
            EditorUtility.DisplayDialog("No UI Found", 
                "No existing Save/Load UI found in the scene.\n\n" +
                "Please use 'Create Save Button UI' first.", "OK");
            return;
        }
        
        int updatedCount = 0;
        
        foreach (SaveButtonUI saveButtonUI in existingUIs)
        {
            if (UpdateSaveButtonUI(saveButtonUI))
            {
                updatedCount++;
            }
        }
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        EditorUtility.DisplayDialog("Update Complete", 
            $"Updated {updatedCount} Save/Load UI component(s) to the new design.\n\n" +
            "Changes:\n" +
            "- Added Load button (if missing)\n" +
            "- Linked panel title text\n" +
            "- Updated component references", "OK");
    }
    
    /// <summary>
    /// Updates a single SaveButtonUI component to match the new design.
    /// </summary>
    bool UpdateSaveButtonUI(SaveButtonUI saveButtonUI)
    {
        if (saveButtonUI == null) return false;
        
        SerializedObject serializedUI = new SerializedObject(saveButtonUI);
        bool updated = false;
        
        // Check if save button exists
        SerializedProperty saveButtonProp = serializedUI.FindProperty("saveButton");
        if (saveButtonProp.objectReferenceValue == null)
        {
            // Try to find save button in children or by name
            Button saveButton = saveButtonUI.GetComponent<Button>();
            if (saveButton == null)
            {
                Transform saveButtonTransform = saveButtonUI.transform.Find("SaveButton");
                if (saveButtonTransform != null)
                {
                    saveButton = saveButtonTransform.GetComponent<Button>();
                }
            }
            
            if (saveButton != null)
            {
                saveButtonProp.objectReferenceValue = saveButton;
                updated = true;
            }
        }
        
        // Check if load button exists, create if missing
        SerializedProperty loadButtonProp = serializedUI.FindProperty("loadButton");
        if (loadButtonProp.objectReferenceValue == null)
        {
            // Check if load button already exists in scene
            Transform loadButtonTransform = saveButtonUI.transform.parent?.Find("LoadButton");
            Button loadButton = null;
            
            if (loadButtonTransform != null)
            {
                loadButton = loadButtonTransform.GetComponent<Button>();
            }
            
            // If doesn't exist, create it
            if (loadButton == null && saveButtonUI.transform.parent != null)
            {
                Canvas parentCanvas = saveButtonUI.transform.parent.GetComponent<Canvas>();
                if (parentCanvas == null)
                {
                    parentCanvas = saveButtonUI.transform.parent.GetComponentInParent<Canvas>();
                }
                
                if (parentCanvas != null)
                {
                    GameObject loadButtonObj = CreateLoadButton(parentCanvas, saveButtonUI.transform);
                    if (loadButtonObj != null)
                    {
                        loadButton = loadButtonObj.GetComponent<Button>();
                    }
                }
            }
            
            if (loadButton != null)
            {
                loadButtonProp.objectReferenceValue = loadButton;
                updated = true;
            }
        }
        
        // Check if panel title text is linked
        SerializedProperty panelTitleProp = serializedUI.FindProperty("panelTitleText");
        if (panelTitleProp.objectReferenceValue == null)
        {
            // Find save slot panel
            SerializedProperty panelProp = serializedUI.FindProperty("saveSlotPanel");
            if (panelProp.objectReferenceValue != null)
            {
                GameObject panel = panelProp.objectReferenceValue as GameObject;
                if (panel != null)
                {
                    Transform titleTransform = panel.transform.Find("Title");
                    if (titleTransform != null)
                    {
                        TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                        if (titleText != null)
                        {
                            panelTitleProp.objectReferenceValue = titleText;
                            updated = true;
                        }
                    }
                }
            }
        }
        
        // Check if slot button container is linked
        SerializedProperty containerProp = serializedUI.FindProperty("slotButtonContainer");
        if (containerProp.objectReferenceValue == null)
        {
            SerializedProperty panelProp = serializedUI.FindProperty("saveSlotPanel");
            if (panelProp.objectReferenceValue != null)
            {
                GameObject panel = panelProp.objectReferenceValue as GameObject;
                if (panel != null)
                {
                    Transform containerTransform = panel.transform.Find("SlotButtonContainer");
                    if (containerTransform != null)
                    {
                        containerProp.objectReferenceValue = containerTransform;
                        updated = true;
                    }
                }
            }
        }
        
        // Check if close button is linked
        SerializedProperty closeButtonProp = serializedUI.FindProperty("closePanelButton");
        if (closeButtonProp.objectReferenceValue == null)
        {
            SerializedProperty panelProp = serializedUI.FindProperty("saveSlotPanel");
            if (panelProp.objectReferenceValue != null)
            {
                GameObject panel = panelProp.objectReferenceValue as GameObject;
                if (panel != null)
                {
                    Transform closeTransform = panel.transform.Find("CloseButton");
                    if (closeTransform != null)
                    {
                        Button closeButton = closeTransform.GetComponent<Button>();
                        if (closeButton != null)
                        {
                            closeButtonProp.objectReferenceValue = closeButton;
                            updated = true;
                        }
                    }
                }
            }
        }
        
        if (updated)
        {
            serializedUI.ApplyModifiedProperties();
            EditorUtility.SetDirty(saveButtonUI);
        }
        
        return updated;
    }
}

