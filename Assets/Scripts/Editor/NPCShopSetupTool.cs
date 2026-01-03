using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool to set up NPC shop in the scene and automatically create shop UI.
/// </summary>
public class NPCShopSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup NPC Shop")]
    public static void ShowWindow()
    {
        GetWindow<NPCShopSetupTool>("NPC Shop Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("NPC Shop Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This will create:\n" +
            "1. NPC GameObject with NPCShopController\n" +
            "2. Shop UI (Canvas, panel, buttons)\n" +
            "3. ArrowInventory GameObject (if not exists)\n" +
            "4. Link all components together\n\n" +
            "Make sure Bow.asset exists in Assets/Assets/Items/", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup NPC Shop", GUILayout.Height(30)))
        {
            SetupNPCShop();
        }
    }
    
    void SetupNPCShop()
    {
        // Step 1: Create or find ArrowInventory
        ArrowInventory arrowInventory = FindFirstObjectByType<ArrowInventory>();
        if (arrowInventory == null)
        {
            GameObject arrowInventoryObj = new GameObject("ArrowInventory");
            arrowInventory = arrowInventoryObj.AddComponent<ArrowInventory>();
            Debug.Log("Created ArrowInventory GameObject");
        }
        
        // Step 2: Create NPC GameObject
        GameObject npc = new GameObject("NPC_Shop");
        npc.transform.position = Vector3.zero; // User can move it
        
        // Add SpriteRenderer (user can assign sprite later)
        SpriteRenderer npcSprite = npc.AddComponent<SpriteRenderer>();
        npcSprite.sortingLayerName = "Default";
        
        // Add NPCShopController
        NPCShopController npcController = npc.AddComponent<NPCShopController>();
        
        // Add CircleCollider2D for interaction
        CircleCollider2D npcCollider = npc.AddComponent<CircleCollider2D>();
        npcCollider.radius = 1f;
        npcCollider.isTrigger = true;
        
        Debug.Log("Created NPC GameObject with NPCShopController");
        
        // Step 3: Create Shop UI
        Canvas shopCanvas = GetOrCreateShopCanvas();
        GameObject shopPanel = CreateShopPanel(shopCanvas);
        ShopUI shopUI = CreateShopUIComponent(shopPanel);
        
        // Step 4: Link NPC to ShopUI
        SerializedObject serializedNPC = new SerializedObject(npcController);
        serializedNPC.FindProperty("shopUI").objectReferenceValue = shopUI;
        serializedNPC.ApplyModifiedProperties();
        
        // Step 5: Link Press E prompt if exists
        GameObject pressEPrompt = FindPressEPrompt();
        if (pressEPrompt != null)
        {
            serializedNPC.FindProperty("pressEPromptUI").objectReferenceValue = pressEPrompt;
            serializedNPC.ApplyModifiedProperties();
            Debug.Log("Linked Press E prompt to NPC");
        }
        
        // Step 6: Load Bow ItemData and assign to ShopUI
        ItemData bowData = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Assets/Items/Bow.asset");
        if (bowData != null)
        {
            SerializedObject serializedShopUI = new SerializedObject(shopUI);
            serializedShopUI.FindProperty("bowWeaponData").objectReferenceValue = bowData;
            serializedShopUI.ApplyModifiedProperties();
            Debug.Log("Loaded and assigned Bow ItemData to ShopUI");
        }
        else
        {
            Debug.LogWarning("Bow.asset not found at Assets/Assets/Items/Bow.asset. Please create it or assign manually.");
        }
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        // Select NPC in hierarchy
        Selection.activeGameObject = npc;
        
        EditorUtility.DisplayDialog("Setup Complete", 
            "NPC Shop has been set up successfully!\n\n" +
            "Created:\n" +
            "- NPC_Shop GameObject (with NPCShopController)\n" +
            "- ShopUI in ShopCanvas\n" +
            "- ArrowInventory (if needed)\n\n" +
            "You can now:\n" +
            "1. Move NPC_Shop to desired position\n" +
            "2. Assign NPC sprite if needed\n" +
            "3. Test the shop in Play mode", "OK");
    }
    
    Canvas GetOrCreateShopCanvas()
    {
        // Try to find HUDCanvas first
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
        
        // Use HUDCanvas if found, otherwise create ShopCanvas
        if (hudCanvas != null)
        {
            Debug.Log("Using existing HUDCanvas for shop UI");
            return hudCanvas;
        }
        
        // Create new ShopCanvas
        GameObject canvasObj = new GameObject("ShopCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        canvas.sortingOrder = 200; // Higher than HUD
        
        Debug.Log("Created ShopCanvas for shop UI");
        return canvas;
    }
    
    GameObject CreateShopPanel(Canvas parentCanvas)
    {
        // Check if shop panel already exists
        Transform existingPanel = parentCanvas.transform.Find("ShopPanel");
        if (existingPanel != null)
        {
            if (EditorUtility.DisplayDialog("Shop Panel Exists", 
                "A shop panel already exists. Do you want to recreate it?", "Yes", "No"))
            {
                DestroyImmediate(existingPanel.gameObject);
            }
            else
            {
                return existingPanel.gameObject;
            }
        }
        
        // Create shop panel
        GameObject panel = new GameObject("ShopPanel");
        panel.transform.SetParent(parentCanvas.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600, 500);
        
        // Add background image
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f); // Dark semi-transparent background
        
        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(500, 60);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Shop";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Create coin display
        GameObject coinDisplayObj = new GameObject("CoinDisplay");
        coinDisplayObj.transform.SetParent(panel.transform, false);
        RectTransform coinRect = coinDisplayObj.AddComponent<RectTransform>();
        coinRect.anchorMin = new Vector2(0.5f, 1f);
        coinRect.anchorMax = new Vector2(0.5f, 1f);
        coinRect.pivot = new Vector2(0.5f, 1f);
        coinRect.anchoredPosition = new Vector2(0, -80);
        coinRect.sizeDelta = new Vector2(500, 40);
        
        TextMeshProUGUI coinText = coinDisplayObj.AddComponent<TextMeshProUGUI>();
        coinText.text = "Coins: 0";
        coinText.fontSize = 32;
        coinText.color = Color.yellow;
        coinText.alignment = TextAlignmentOptions.Center;
        coinText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Create bow purchase button
        GameObject bowButtonObj = CreatePurchaseButton("BowButton", panel.transform, new Vector2(0, 50));
        Button bowButton = bowButtonObj.GetComponent<Button>();
        TextMeshProUGUI bowButtonText = bowButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        bowButtonText.text = "Bow - 1 coin";
        
        // Create arrow bundle purchase button
        GameObject arrowButtonObj = CreatePurchaseButton("ArrowBundleButton", panel.transform, new Vector2(0, -50));
        Button arrowButton = arrowButtonObj.GetComponent<Button>();
        TextMeshProUGUI arrowButtonText = arrowButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        arrowButtonText.text = "Arrow Bundle (x20) - 1 coin";
        
        // Create sell all unequipped button
        GameObject sellAllButtonObj = CreateSellAllButton("SellAllButton", panel.transform, new Vector2(0, -150));
        
        // Create confirmation dialog
        GameObject confirmationDialog = CreateConfirmationDialog(panel.transform);
        
        // Create close button
        GameObject closeButtonObj = CreateCloseButton(panel.transform);
        
        // Initially hide panel
        panel.SetActive(false);
        
        Debug.Log("Created ShopPanel with all UI elements");
        return panel;
    }
    
    GameObject CreatePurchaseButton(string name, Transform parent, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(400, 60);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.6f, 0.3f, 1f); // Green button
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.6f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.7f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.5f, 0.2f, 1f);
        button.colors = colors;
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Purchase";
        buttonText.fontSize = 32;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        return buttonObj;
    }
    
    GameObject CreateSellAllButton(string name, Transform parent, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(400, 60);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Orange/brown button
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.6f, 0.4f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.7f, 0.5f, 0.3f, 1f);
        colors.pressedColor = new Color(0.5f, 0.3f, 0.1f, 1f);
        button.colors = colors;
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Sell All Unequipped";
        buttonText.fontSize = 32;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        return buttonObj;
    }
    
    GameObject CreateConfirmationDialog(Transform parent)
    {
        // Create dialog panel
        GameObject dialog = new GameObject("ConfirmationDialog");
        dialog.transform.SetParent(parent, false);
        
        RectTransform dialogRect = dialog.AddComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(500, 200);
        
        Image dialogImage = dialog.AddComponent<Image>();
        dialogImage.color = new Color(0.1f, 0.1f, 0.1f, 0.98f); // Dark background
        
        // Create message text
        GameObject messageObj = new GameObject("MessageText");
        messageObj.transform.SetParent(dialog.transform, false);
        RectTransform messageRect = messageObj.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageRect.anchorMax = new Vector2(0.5f, 0.5f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.anchoredPosition = new Vector2(0, 30);
        messageRect.sizeDelta = new Vector2(450, 80);
        
        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "Sell all items?";
        messageText.fontSize = 28;
        messageText.color = Color.white;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Create confirm button
        GameObject confirmButtonObj = CreateDialogButton("ConfirmButton", dialog.transform, new Vector2(-100, -50));
        Button confirmButton = confirmButtonObj.GetComponent<Button>();
        TextMeshProUGUI confirmText = confirmButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        confirmText.text = "Confirm";
        Image confirmImage = confirmButtonObj.GetComponent<Image>();
        confirmImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green
        ColorBlock confirmColors = confirmButton.colors;
        confirmColors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        confirmColors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        confirmColors.pressedColor = new Color(0.1f, 0.5f, 0.1f, 1f);
        confirmButton.colors = confirmColors;
        
        // Create cancel button
        GameObject cancelButtonObj = CreateDialogButton("CancelButton", dialog.transform, new Vector2(100, -50));
        Button cancelButton = cancelButtonObj.GetComponent<Button>();
        TextMeshProUGUI cancelText = cancelButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        cancelText.text = "Cancel";
        Image cancelImage = cancelButtonObj.GetComponent<Image>();
        cancelImage.color = new Color(0.6f, 0.2f, 0.2f, 1f); // Red
        ColorBlock cancelColors = cancelButton.colors;
        cancelColors.normalColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        cancelColors.highlightedColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        cancelColors.pressedColor = new Color(0.5f, 0.1f, 0.1f, 1f);
        cancelButton.colors = cancelColors;
        
        // Initially hide dialog
        dialog.SetActive(false);
        
        return dialog;
    }
    
    GameObject CreateDialogButton(string name, Transform parent, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(150, 50);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        button.colors = colors;
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Button";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        return buttonObj;
    }
    
    GameObject CreateCloseButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("CloseButton");
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-10, -10);
        buttonRect.sizeDelta = new Vector2(80, 40);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.6f, 0.2f, 0.2f, 1f); // Red button
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.5f, 0.1f, 0.1f, 1f);
        button.colors = colors;
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "X";
        buttonText.fontSize = 28;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        return buttonObj;
    }
    
    ShopUI CreateShopUIComponent(GameObject shopPanel)
    {
        ShopUI shopUI = shopPanel.GetComponent<ShopUI>();
        if (shopUI == null)
        {
            shopUI = shopPanel.AddComponent<ShopUI>();
        }
        
        // Link UI references via SerializedObject
        SerializedObject serializedShopUI = new SerializedObject(shopUI);
        
        // Link shop panel
        serializedShopUI.FindProperty("shopPanel").objectReferenceValue = shopPanel;
        
        // Link coin display text
        Transform coinDisplay = shopPanel.transform.Find("CoinDisplay");
        if (coinDisplay != null)
        {
            TextMeshProUGUI coinText = coinDisplay.GetComponent<TextMeshProUGUI>();
            serializedShopUI.FindProperty("coinDisplayText").objectReferenceValue = coinText;
        }
        
        // Link bow purchase button
        Transform bowButton = shopPanel.transform.Find("BowButton");
        if (bowButton != null)
        {
            Button button = bowButton.GetComponent<Button>();
            serializedShopUI.FindProperty("bowPurchaseButton").objectReferenceValue = button;
            
            TextMeshProUGUI buttonText = bowButton.GetComponentInChildren<TextMeshProUGUI>();
            serializedShopUI.FindProperty("bowButtonText").objectReferenceValue = buttonText;
        }
        
        // Link arrow bundle purchase button
        Transform arrowButton = shopPanel.transform.Find("ArrowBundleButton");
        if (arrowButton != null)
        {
            Button button = arrowButton.GetComponent<Button>();
            serializedShopUI.FindProperty("arrowBundlePurchaseButton").objectReferenceValue = button;
            
            TextMeshProUGUI buttonText = arrowButton.GetComponentInChildren<TextMeshProUGUI>();
            serializedShopUI.FindProperty("arrowButtonText").objectReferenceValue = buttonText;
        }
        
        // Link sell all unequipped button
        Transform sellAllButton = shopPanel.transform.Find("SellAllButton");
        if (sellAllButton != null)
        {
            Button button = sellAllButton.GetComponent<Button>();
            serializedShopUI.FindProperty("sellAllUnequippedButton").objectReferenceValue = button;
            
            TextMeshProUGUI buttonText = sellAllButton.GetComponentInChildren<TextMeshProUGUI>();
            serializedShopUI.FindProperty("sellAllButtonText").objectReferenceValue = buttonText;
        }
        
        // Link confirmation dialog
        Transform confirmationDialog = shopPanel.transform.Find("ConfirmationDialog");
        if (confirmationDialog != null)
        {
            serializedShopUI.FindProperty("confirmationDialog").objectReferenceValue = confirmationDialog.gameObject;
            
            Transform messageText = confirmationDialog.Find("MessageText");
            if (messageText != null)
            {
                TextMeshProUGUI text = messageText.GetComponent<TextMeshProUGUI>();
                serializedShopUI.FindProperty("confirmationMessageText").objectReferenceValue = text;
            }
            
            Transform confirmButton = confirmationDialog.Find("ConfirmButton");
            if (confirmButton != null)
            {
                Button button = confirmButton.GetComponent<Button>();
                serializedShopUI.FindProperty("confirmButton").objectReferenceValue = button;
            }
            
            Transform cancelButton = confirmationDialog.Find("CancelButton");
            if (cancelButton != null)
            {
                Button button = cancelButton.GetComponent<Button>();
                serializedShopUI.FindProperty("cancelButton").objectReferenceValue = button;
            }
        }
        
        // Link close button
        Transform closeButton = shopPanel.transform.Find("CloseButton");
        if (closeButton != null)
        {
            Button button = closeButton.GetComponent<Button>();
            serializedShopUI.FindProperty("closeButton").objectReferenceValue = button;
        }
        
        serializedShopUI.ApplyModifiedProperties();
        
        Debug.Log("Created and configured ShopUI component");
        return shopUI;
    }
    
    GameObject FindPressEPrompt()
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
            Transform promptTransform = hudCanvas.transform.Find("PressEPrompt");
            if (promptTransform != null)
            {
                return promptTransform.gameObject;
            }
        }
        
        return null;
    }
}


