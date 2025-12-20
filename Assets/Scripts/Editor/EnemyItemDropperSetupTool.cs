using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class EnemyItemDropperSetupTool : EditorWindow
{
    private float spawnOffsetRange = 0.5f;
    private GameObject coinPrefab;
    
    private Vector2 scrollPosition;
    private List<GameObject> enemiesWithItemDroppers = new List<GameObject>();
    
    [MenuItem("Tools/Enemy Item Dropper Setup")]
    public static void ShowWindow()
    {
        GetWindow<EnemyItemDropperSetupTool>("Enemy Item Dropper Setup");
    }
    
    void OnEnable()
    {
        RefreshEnemyList();
        LoadCoinPrefab();
    }
    
    void LoadCoinPrefab()
    {
        // Try to find Coin prefab in Assets
        if (coinPrefab == null)
        {
            string[] guids = AssetDatabase.FindAssets("Coin t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Enemy Item Dropper Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Configuration Section
        EditorGUILayout.LabelField("Default Settings", EditorStyles.boldLabel);
        spawnOffsetRange = EditorGUILayout.FloatField("Spawn Offset Range", spawnOffsetRange);
        coinPrefab = (GameObject)EditorGUILayout.ObjectField("Coin Prefab (Optional)", coinPrefab, typeof(GameObject), false);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("These settings will be applied to newly added EnemyItemDropper components.", MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Setup Section
        EditorGUILayout.LabelField("Setup Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add Item Dropper to Selected Enemies", GUILayout.Height(30)))
        {
            AddItemDropperToSelected();
        }
        
        if (GUILayout.Button("Add Item Dropper to All Enemies in Scene", GUILayout.Height(30)))
        {
            AddItemDropperToAllEnemies();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Remove Item Dropper from Selected Enemies", GUILayout.Height(25)))
        {
            RemoveItemDropperFromSelected();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Item Drop Data Creation
        EditorGUILayout.LabelField("Item Drop Data Assets", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create ItemDropData ScriptableObject assets that can be assigned to enemies.", MessageType.Info);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Weapon Drop Data", GUILayout.Height(25)))
        {
            CreateItemDropDataAsset(ItemDropData.ItemType.Weapon);
        }
        
        if (GUILayout.Button("Create Coin Drop Data", GUILayout.Height(25)))
        {
            CreateItemDropDataAsset(ItemDropData.ItemType.Coin);
        }
        EditorGUILayout.EndHorizontal();
        
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
        
        // Enemy List Display
        EditorGUILayout.LabelField($"Enemies with Item Droppers ({enemiesWithItemDroppers.Count})", EditorStyles.boldLabel);
        
        if (enemiesWithItemDroppers.Count == 0)
        {
            EditorGUILayout.HelpBox("No enemies with EnemyItemDropper found in the scene.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (GameObject enemy in enemiesWithItemDroppers)
            {
                if (enemy == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(enemy.name, GUILayout.Width(200));
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = enemy;
                    EditorGUIUtility.PingObject(enemy);
                }
                
                EnemyItemDropper dropper = enemy.GetComponent<EnemyItemDropper>();
                if (dropper != null)
                {
                    SerializedObject so = new SerializedObject(dropper);
                    SerializedProperty dropsProp = so.FindProperty("possibleDrops");
                    int dropCount = dropsProp != null ? dropsProp.arraySize : 0;
                    EditorGUILayout.LabelField($"{dropCount} drop(s)", GUILayout.Width(80));
                }
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void AddItemDropperToSelected()
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
            if (AddItemDropperToEnemy(obj))
            {
                added++;
            }
            else
            {
                skipped++;
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Added item droppers to {added} enemies. Skipped {skipped}.");
        EditorUtility.DisplayDialog("Setup Complete", 
            $"Added item droppers to {added} enemy/enemies.\nSkipped {skipped} (already had EnemyItemDropper or missing EnemyHealth).", "OK");
    }
    
    void AddItemDropperToAllEnemies()
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
            $"This will add item droppers to {allEnemies.Length} enemy/enemies. Continue?", "Yes", "No"))
        {
            return;
        }
        
        int added = 0;
        int skipped = 0;
        
        foreach (GameObject enemy in allEnemies)
        {
            if (AddItemDropperToEnemy(enemy))
            {
                added++;
            }
            else
            {
                skipped++;
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Added item droppers to {added} enemies. Skipped {skipped}.");
        EditorUtility.DisplayDialog("Setup Complete", 
            $"Added item droppers to {added} enemy/enemies.\nSkipped {skipped} (already had EnemyItemDropper or missing EnemyHealth).", "OK");
    }
    
    bool AddItemDropperToEnemy(GameObject enemy)
    {
        if (enemy == null) return false;
        
        // Check if already has item dropper
        if (enemy.GetComponent<EnemyItemDropper>() != null)
        {
            Debug.Log($"Enemy {enemy.name} already has EnemyItemDropper component.");
            return false;
        }
        
        // Ensure EnemyHealth component exists
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogWarning($"Enemy {enemy.name} does not have EnemyHealth component. Item dropper requires EnemyHealth to work.");
            return false;
        }
        
        // Add EnemyItemDropper component
        EnemyItemDropper itemDropper = enemy.AddComponent<EnemyItemDropper>();
        
        // Configure using SerializedObject
        SerializedObject serializedDropper = new SerializedObject(itemDropper);
        serializedDropper.FindProperty("spawnOffsetRange").floatValue = spawnOffsetRange;
        
        if (coinPrefab != null)
        {
            serializedDropper.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
        }
        
        serializedDropper.ApplyModifiedProperties();
        
        // Mark as dirty
        EditorUtility.SetDirty(enemy);
        
        Debug.Log($"Added EnemyItemDropper to {enemy.name}");
        return true;
    }
    
    void RemoveItemDropperFromSelected()
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
            EnemyItemDropper itemDropper = obj.GetComponent<EnemyItemDropper>();
            if (itemDropper != null)
            {
                DestroyImmediate(itemDropper);
                EditorUtility.SetDirty(obj);
                removed++;
            }
        }
        
        RefreshEnemyList();
        
        Debug.Log($"Removed item droppers from {removed} enemies.");
        EditorUtility.DisplayDialog("Removal Complete", $"Removed item droppers from {removed} enemy/enemies.", "OK");
    }
    
    void RefreshEnemyList()
    {
        enemiesWithItemDroppers.Clear();
        
        // Find all enemies with item droppers
        EnemyItemDropper[] itemDroppers = FindObjectsByType<EnemyItemDropper>(FindObjectsSortMode.None);
        foreach (EnemyItemDropper dropper in itemDroppers)
        {
            if (dropper != null && dropper.gameObject != null)
            {
                enemiesWithItemDroppers.Add(dropper.gameObject);
            }
        }
    }
    
    void CreateItemDropDataAsset(ItemDropData.ItemType itemType)
    {
        ItemDropData newDropData = ScriptableObject.CreateInstance<ItemDropData>();
        newDropData.itemType = itemType;
        newDropData.spawnRate = 0.5f;
        
        if (itemType == ItemDropData.ItemType.Coin)
        {
            newDropData.coinValue = 1;
        }
        
        // Default path
        string defaultPath = "Assets/Assets/Items";
        if (!Directory.Exists(defaultPath))
        {
            defaultPath = "Assets";
        }
        
        string defaultName = itemType == ItemDropData.ItemType.Weapon ? "NewWeaponDrop" : "NewCoinDrop";
        string path = EditorUtility.SaveFilePanelInProject(
            $"Create {itemType} Drop Data",
            defaultName,
            "asset",
            "Please enter a file name to save the ItemDropData asset to",
            defaultPath);
        
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newDropData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newDropData;
            
            Debug.Log($"Created ItemDropData asset at: {path}");
            EditorUtility.DisplayDialog("Asset Created", 
                $"Created {itemType} ItemDropData asset at:\n{path}\n\nYou can now assign it to enemies in the Inspector.", "OK");
        }
    }
}
