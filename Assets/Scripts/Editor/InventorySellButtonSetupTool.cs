using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool to create a Sell button in the inventory UI.
/// </summary>
public class InventorySellButtonSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Inventory Sell Button")]
    public static void ShowWindow()
    {
        GetWindow<InventorySellButtonSetupTool>("Inventory Sell Button Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Inventory Sell Button Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This will create a Sell button in the inventory UI.\n\n" +
            "The button will be positioned near the Use and Drop buttons.\n" +
            "It will be initially hidden and only shown in sell mode.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Sell Button", GUILayout.Height(30)))
        {
            CreateSellButton();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Find and Link to Inventory", GUILayout.Height(25)))
        {
            LinkSellButtonToInventory();
        }
    }
    
    void CreateSellButton()
    {
        // Find Inventory GameObject
        GameObject inventoryObj = GameObject.Find("Inventory");
        if (inventoryObj == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Inventory GameObject not found!\n\n" +
                "Please make sure there is a GameObject named 'Inventory' in the scene.", "OK");
            return;
        }
        
        InventoryController inventoryController = inventoryObj.GetComponent<InventoryController>();
        if (inventoryController == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "InventoryController component not found on Inventory GameObject!", "OK");
            return;
        }
        
        // Check if Sell button already exists
        Transform existingSellButton = inventoryObj.transform.Find("SellButton");
        if (existingSellButton != null)
        {
            if (!EditorUtility.DisplayDialog("Sell Button Exists", 
                "A Sell button already exists. Do you want to recreate it?", "Yes", "No"))
            {
                return;
            }
            DestroyImmediate(existingSellButton.gameObject);
        }
        
        // Find Use button to position Sell button relative to it
        GameObject useButtonObj = null;
        if (inventoryController.useButtonGameObject != null)
        {
            useButtonObj = inventoryController.useButtonGameObject;
        }
        else
        {
            // Try to find Use button by name
            Transform useButtonTransform = inventoryObj.transform.Find("UseButton");
            if (useButtonTransform != null)
            {
                useButtonObj = useButtonTransform.gameObject;
            }
            else
            {
                // Search in children
                Button[] buttons = inventoryObj.GetComponentsInChildren<Button>();
                foreach (Button btn in buttons)
                {
                    if (btn.name.ToLower().Contains("use"))
                    {
                        useButtonObj = btn.gameObject;
                        break;
                    }
                }
            }
        }
        
        Vector2 sellButtonPosition = new Vector2(0, 0);
        if (useButtonObj != null)
        {
            RectTransform useButtonRect = useButtonObj.GetComponent<RectTransform>();
            if (useButtonRect != null)
            {
                // Position Sell button to the right of Use button, or below if Use is on the right
                sellButtonPosition = useButtonRect.anchoredPosition;
                // If Use button is on the left, place Sell to the right; otherwise place below
                if (useButtonRect.anchoredPosition.x < 0)
                {
                    sellButtonPosition.x += 150; // Offset to the right
                }
                else
                {
                    sellButtonPosition.y -= 60; // Offset below
                }
            }
        }
        else
        {
            // Default position if Use button not found
            sellButtonPosition = new Vector2(100, -100);
        }
        
        // Create Sell button
        GameObject sellButtonObj = new GameObject("SellButton");
        sellButtonObj.transform.SetParent(inventoryObj.transform, false);
        
        RectTransform sellButtonRect = sellButtonObj.AddComponent<RectTransform>();
        sellButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        sellButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        sellButtonRect.pivot = new Vector2(0.5f, 0.5f);
        sellButtonRect.anchoredPosition = sellButtonPosition;
        sellButtonRect.sizeDelta = new Vector2(150, 50);
        
        Image sellButtonImage = sellButtonObj.AddComponent<Image>();
        sellButtonImage.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Orange/brown color similar to shop sell button
        
        Button sellButton = sellButtonObj.AddComponent<Button>();
        ColorBlock colors = sellButton.colors;
        colors.normalColor = new Color(0.6f, 0.4f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.7f, 0.5f, 0.3f, 1f);
        colors.pressedColor = new Color(0.5f, 0.3f, 0.1f, 1f);
        sellButton.colors = colors;
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(sellButtonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Sell";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Initially hide the button (will be shown in sell mode)
        sellButtonObj.SetActive(false);
        
        // Link to InventoryController
        SerializedObject serializedInventory = new SerializedObject(inventoryController);
        serializedInventory.FindProperty("sellButtonGameObject").objectReferenceValue = sellButtonObj;
        serializedInventory.ApplyModifiedProperties();
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        // Select the created button
        Selection.activeGameObject = sellButtonObj;
        
        EditorUtility.DisplayDialog("Setup Complete", 
            "Sell button has been created successfully!\n\n" +
            "The button is:\n" +
            "- Positioned near the Use/Drop buttons\n" +
            "- Initially hidden (will show in sell mode)\n" +
            "- Linked to InventoryController\n\n" +
            "You can now test it in Play mode.", "OK");
        
        Debug.Log("InventorySellButtonSetupTool: Created Sell button and linked to InventoryController.");
    }
    
    void LinkSellButtonToInventory()
    {
        // Find Inventory GameObject
        GameObject inventoryObj = GameObject.Find("Inventory");
        if (inventoryObj == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Inventory GameObject not found!", "OK");
            return;
        }
        
        InventoryController inventoryController = inventoryObj.GetComponent<InventoryController>();
        if (inventoryController == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "InventoryController component not found!", "OK");
            return;
        }
        
        // Find Sell button
        Transform sellButtonTransform = inventoryObj.transform.Find("SellButton");
        if (sellButtonTransform == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Sell button not found!\n\n" +
                "Please create it first using 'Create Sell Button'.", "OK");
            return;
        }
        
        // Link to InventoryController
        SerializedObject serializedInventory = new SerializedObject(inventoryController);
        serializedInventory.FindProperty("sellButtonGameObject").objectReferenceValue = sellButtonTransform.gameObject;
        serializedInventory.ApplyModifiedProperties();
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        EditorUtility.DisplayDialog("Link Complete", 
            "Sell button has been linked to InventoryController!", "OK");
        
        Debug.Log("InventorySellButtonSetupTool: Linked existing Sell button to InventoryController.");
    }
}


