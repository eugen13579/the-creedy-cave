using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to set up health flask in the scene.
/// Creates a pickupable health flask GameObject with proper components.
/// </summary>
public class HealthFlaskSetupTool : EditorWindow
{
    private Vector3 flaskPosition = Vector3.zero;
    private float healAmount = 500f;
    private string flaskSpritePath = "Assets/Assets/items and trap_animation/flasks/flasks_1_1.png";
    
    [MenuItem("Tools/Setup Health Flask")]
    public static void ShowWindow()
    {
        GetWindow<HealthFlaskSetupTool>("Health Flask Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Health Flask Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This will create:\n" +
            "1. Health Flask GameObject with Pickupable component\n" +
            "2. ItemData asset for the flask (if not exists)\n" +
            "3. SpriteRenderer with flask sprite\n" +
            "4. CircleCollider2D for pickup detection\n\n" +
            "The flask can be picked up and added to inventory.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        flaskPosition = EditorGUILayout.Vector3Field("Flask Position", flaskPosition);
        healAmount = EditorGUILayout.FloatField("Heal Amount", healAmount);
        flaskSpritePath = EditorGUILayout.TextField("Flask Sprite Path", flaskSpritePath);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Browse Flask Sprite", GUILayout.Height(25)))
        {
            string path = EditorUtility.OpenFilePanel("Select Flask Sprite", "Assets", "png");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative path
                if (path.StartsWith(Application.dataPath))
                {
                    flaskSpritePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    flaskSpritePath = path;
                }
            }
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Health Flask", GUILayout.Height(30)))
        {
            SetupHealthFlask();
        }
    }
    
    void SetupHealthFlask()
    {
        // Step 1: Create or load ItemData asset for health flask
        ItemData flaskData = CreateOrLoadFlaskData();
        if (flaskData == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to create or load flask ItemData asset!", "OK");
            return;
        }
        
        // Step 2: Create Health Flask GameObject
        GameObject flask = new GameObject("HealthFlask");
        flask.transform.position = flaskPosition;
        
        // Step 3: Add SpriteRenderer
        SpriteRenderer spriteRenderer = flask.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Player";
        
        // Load and assign sprite
        Sprite flaskSprite = AssetDatabase.LoadAssetAtPath<Sprite>(flaskSpritePath);
        if (flaskSprite == null)
        {
            // Try loading as texture and creating sprite
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(flaskSpritePath);
            if (texture != null)
            {
                flaskSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        
        if (flaskSprite != null)
        {
            spriteRenderer.sprite = flaskSprite;
            Debug.Log($"Assigned flask sprite: {flaskSpritePath}");
        }
        else
        {
            Debug.LogWarning($"Could not load sprite from {flaskSpritePath}. Using default sprite.");
        }
        
        // Step 4: Add CircleCollider2D
        CircleCollider2D collider = flask.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.isTrigger = true;
        
        // Step 5: Add Pickupable component
        Pickupable pickupable = flask.AddComponent<Pickupable>();
        
        // Link ItemData to Pickupable using SerializedObject
        SerializedObject serializedPickupable = new SerializedObject(pickupable);
        serializedPickupable.FindProperty("itemData").objectReferenceValue = flaskData;
        serializedPickupable.ApplyModifiedProperties();
        
        Debug.Log("Created Health Flask GameObject with all components");
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        // Select flask in hierarchy
        Selection.activeGameObject = flask;
        
            EditorUtility.DisplayDialog("Setup Complete", 
            "Health Flask has been set up successfully!\n\n" +
            "Created:\n" +
            $"- HealthFlask GameObject at position {flaskPosition}\n" +
            $"- ItemData asset: {AssetDatabase.GetAssetPath(flaskData)}\n" +
            "- Pickupable component configured\n\n" +
            "You can now:\n" +
            "1. Move HealthFlask to desired position\n" +
            "2. Adjust collider radius if needed\n" +
            "3. Test pickup in Play mode", "OK");
    }
    
    ItemData CreateOrLoadFlaskData()
    {
        string assetPath = "Assets/Assets/Items/HealthFlask.asset";
        
        // Try to load existing asset
        ItemData flaskData = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
        
        if (flaskData == null)
        {
            // Create new ItemData asset
            flaskData = ScriptableObject.CreateInstance<ItemData>();
            flaskData.itemName = "Health Flask";
            flaskData.itemType = ItemData.ItemType.Consumable;
            flaskData.damage = 0; // Flasks don't deal damage
            flaskData.attackSpeed = 0f;
            flaskData.defense = 0;
            flaskData.sellPrice = 1;
            
            // Try to load sprite for the asset
            Sprite flaskSprite = AssetDatabase.LoadAssetAtPath<Sprite>(flaskSpritePath);
            if (flaskSprite == null)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(flaskSpritePath);
                if (texture != null)
                {
                    flaskSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
            
            if (flaskSprite != null)
            {
                flaskData.icon = flaskSprite;
            }
            
            // Create asset directory if it doesn't exist
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                string parentDir = "Assets/Assets";
                if (!AssetDatabase.IsValidFolder(parentDir))
                {
                    AssetDatabase.CreateFolder("Assets", "Assets");
                }
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    AssetDatabase.CreateFolder("Assets/Assets", "Items");
                }
            }
            
            AssetDatabase.CreateAsset(flaskData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created new HealthFlask ItemData asset at {assetPath}");
        }
        else
        {
            Debug.Log($"Loaded existing HealthFlask ItemData asset from {assetPath}");
        }
        
        return flaskData;
    }
}

