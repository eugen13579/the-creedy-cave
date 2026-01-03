using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tool to debug and set up the arrow system.
/// </summary>
public class ArrowSystemDebugTool : EditorWindow
{
    private Vector2 scrollPosition;
    private PlayerController playerController;
    private ArrowInventory arrowInventory;
    private Sprite arrowSprite;
    
    [MenuItem("Tools/Arrow System Debug & Setup")]
    public static void ShowWindow()
    {
        GetWindow<ArrowSystemDebugTool>("Arrow System Debug & Setup");
    }
    
    void OnEnable()
    {
        RefreshReferences();
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Arrow System Debug & Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Refresh button
        if (GUILayout.Button("Refresh References", GUILayout.Height(25)))
        {
            RefreshReferences();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Setup Section
        EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Use these tools to set up the arrow system components.", MessageType.Info);
        
        if (GUILayout.Button("Create/Find ArrowInventory", GUILayout.Height(30)))
        {
            SetupArrowInventory();
        }
        
        if (GUILayout.Button("Verify Arrow Sprite Path", GUILayout.Height(30)))
        {
            VerifyArrowSpritePath();
        }
        
        if (GUILayout.Button("Auto-Configure PlayerController", GUILayout.Height(30)))
        {
            AutoConfigurePlayerController();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Debug Section
        EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
        
        // ArrowInventory Status
        EditorGUILayout.LabelField("ArrowInventory Status:", EditorStyles.boldLabel);
        if (arrowInventory != null)
        {
            EditorGUILayout.HelpBox($"✓ ArrowInventory found\nCurrent Arrow Count: {arrowInventory.ArrowCount}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("✗ ArrowInventory not found in scene", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // PlayerController Status
        EditorGUILayout.LabelField("PlayerController Status:", EditorStyles.boldLabel);
        if (playerController != null)
        {
            // Use SerializedObject to read private fields
            SerializedObject serializedPlayer = new SerializedObject(playerController);
            SerializedProperty arrowSpriteProp = serializedPlayer.FindProperty("arrowSprite");
            
            string spriteStatus = "Not assigned";
            if (arrowSpriteProp != null && arrowSpriteProp.objectReferenceValue != null)
            {
                Sprite sprite = arrowSpriteProp.objectReferenceValue as Sprite;
                spriteStatus = sprite != null ? $"✓ Assigned: {sprite.name}" : "✗ Invalid reference";
            }
            
            EditorGUILayout.HelpBox($"✓ PlayerController found\nArrow Sprite: {spriteStatus}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("✗ PlayerController not found in scene", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Arrow Sprite Status
        EditorGUILayout.LabelField("Arrow Sprite Status:", EditorStyles.boldLabel);
        TestArrowSpriteLoading();
        
        EditorGUILayout.Space();
        
        // Input Keys
        EditorGUILayout.LabelField("Input Keys:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("J Key: Fire arrow (if bow equipped) or melee attack\nR Key: Fire arrow (if bow equipped)", MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Test Section (Play Mode)
        EditorGUILayout.LabelField("Test Tools (Play Mode Only)", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play Mode Active - Test tools available", MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add 10 Arrows", GUILayout.Height(25)))
            {
                AddTestArrows(10);
            }
            
            if (GUILayout.Button("Add 50 Arrows", GUILayout.Height(25)))
            {
                AddTestArrows(50);
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Fire Test Arrow", GUILayout.Height(30)))
            {
                FireTestArrow();
            }
            
            if (GUILayout.Button("Log Arrow System Status", GUILayout.Height(25)))
            {
                LogArrowSystemStatus();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use test tools", MessageType.Warning);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void RefreshReferences()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        arrowInventory = FindFirstObjectByType<ArrowInventory>();
        
        // Try to load arrow sprite
        #if UNITY_EDITOR
        arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(32x32).png");
        
        if (arrowSprite == null)
        {
            arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(100x100).png");
        }
        #endif
    }
    
    void SetupArrowInventory()
    {
        if (arrowInventory == null)
        {
            GameObject arrowInventoryObj = new GameObject("ArrowInventory");
            arrowInventory = arrowInventoryObj.AddComponent<ArrowInventory>();
            Debug.Log("Created ArrowInventory GameObject");
            EditorUtility.DisplayDialog("Setup Complete", "ArrowInventory GameObject created successfully!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Already Exists", "ArrowInventory already exists in the scene.", "OK");
        }
        
        RefreshReferences();
    }
    
    void VerifyArrowSpritePath()
    {
        #if UNITY_EDITOR
        List<string> foundSprites = new List<string>();
        List<string> missingSprites = new List<string>();
        
        string[] paths = new string[]
        {
            "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(32x32).png",
            "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(100x100).png"
        };
        
        foreach (string path in paths)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                foundSprites.Add(path);
            }
            else
            {
                missingSprites.Add(path);
            }
        }
        
        string message = "Arrow Sprite Verification:\n\n";
        if (foundSprites.Count > 0)
        {
            message += "✓ Found Sprites:\n";
            foreach (string path in foundSprites)
            {
                message += $"  - {path}\n";
            }
        }
        
        if (missingSprites.Count > 0)
        {
            message += "\n✗ Missing Sprites:\n";
            foreach (string path in missingSprites)
            {
                message += $"  - {path}\n";
            }
        }
        
        if (foundSprites.Count == 0)
        {
            message += "\n⚠ No arrow sprites found at expected paths!";
        }
        
        EditorUtility.DisplayDialog("Arrow Sprite Verification", message, "OK");
        #endif
    }
    
    void AutoConfigurePlayerController()
    {
        if (playerController == null)
        {
            EditorUtility.DisplayDialog("Error", "PlayerController not found in scene. Please add it first.", "OK");
            return;
        }
        
        SerializedObject serializedPlayer = new SerializedObject(playerController);
        SerializedProperty arrowSpriteProp = serializedPlayer.FindProperty("arrowSprite");
        
        if (arrowSpriteProp == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find arrowSprite property. Make sure PlayerController has been updated with the latest code.", "OK");
            return;
        }
        
        // Try to assign arrow sprite if not already assigned
        if (arrowSpriteProp.objectReferenceValue == null && arrowSprite != null)
        {
            arrowSpriteProp.objectReferenceValue = arrowSprite;
            serializedPlayer.ApplyModifiedProperties();
            EditorUtility.DisplayDialog("Configuration Complete", 
                $"Arrow sprite assigned to PlayerController:\n{arrowSprite.name}", "OK");
        }
        else if (arrowSpriteProp.objectReferenceValue != null)
        {
            EditorUtility.DisplayDialog("Already Configured", 
                "PlayerController already has an arrow sprite assigned.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", 
                "Could not find arrow sprite at expected path. Please assign it manually in the Inspector.", "OK");
        }
        
        RefreshReferences();
    }
    
    void TestArrowSpriteLoading()
    {
        #if UNITY_EDITOR
        Sprite testSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(32x32).png");
        
        if (testSprite == null)
        {
            testSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(100x100).png");
        }
        
        if (testSprite != null)
        {
            EditorGUILayout.HelpBox($"✓ Arrow sprite can be loaded\nSprite: {testSprite.name}\nSize: {testSprite.texture.width}x{testSprite.texture.height}", 
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("✗ Could not load arrow sprite from expected paths", MessageType.Warning);
        }
        #else
        EditorGUILayout.HelpBox("Sprite loading test only available in Editor mode", MessageType.Info);
        #endif
    }
    
    void AddTestArrows(int count)
    {
        if (arrowInventory == null)
        {
            Debug.LogError("ArrowInventory not found!");
            return;
        }
        
        arrowInventory.AddArrows(count);
        Debug.Log($"Added {count} arrows. Total: {arrowInventory.ArrowCount}");
    }
    
    void FireTestArrow()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found!");
            return;
        }
        
        if (arrowInventory == null || !arrowInventory.HasArrows())
        {
            Debug.LogWarning("No arrows available! Adding 10 test arrows...");
            if (arrowInventory != null)
            {
                arrowInventory.AddArrows(10);
            }
        }
        
        // Use reflection to call FireArrow (it's private)
        var fireArrowMethod = typeof(PlayerController).GetMethod("FireArrow", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (fireArrowMethod != null)
        {
            fireArrowMethod.Invoke(playerController, null);
            Debug.Log("Test arrow fired!");
        }
        else
        {
            Debug.LogError("Could not find FireArrow method!");
        }
    }
    
    void LogArrowSystemStatus()
    {
        Debug.Log("=== Arrow System Status ===");
        
        if (arrowInventory != null)
        {
            Debug.Log($"ArrowInventory: Found - Count: {arrowInventory.ArrowCount}");
        }
        else
        {
            Debug.LogWarning("ArrowInventory: Not found!");
        }
        
        if (playerController != null)
        {
            SerializedObject serializedPlayer = new SerializedObject(playerController);
            SerializedProperty arrowSpriteProp = serializedPlayer.FindProperty("arrowSprite");
            
            if (arrowSpriteProp != null && arrowSpriteProp.objectReferenceValue != null)
            {
                Sprite sprite = arrowSpriteProp.objectReferenceValue as Sprite;
                Debug.Log($"PlayerController: Found - Arrow Sprite: {(sprite != null ? sprite.name : "Invalid")}");
            }
            else
            {
                Debug.LogWarning("PlayerController: Found - Arrow Sprite: Not assigned");
            }
        }
        else
        {
            Debug.LogWarning("PlayerController: Not found!");
        }
        
        #if UNITY_EDITOR
        Sprite testSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(32x32).png");
        if (testSprite == null)
        {
            testSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Assets/Characters/Soldier/Arrow(projectile)/Arrow01(100x100).png");
        }
        Debug.Log($"Arrow Sprite Asset: {(testSprite != null ? $"Found - {testSprite.name}" : "Not found at expected paths")}");
        #endif
        
        Debug.Log("=== End Status ===");
    }
}



