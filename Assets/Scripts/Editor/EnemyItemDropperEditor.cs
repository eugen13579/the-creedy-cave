using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(EnemyItemDropper))]
[CanEditMultipleObjects]
public class EnemyItemDropperEditor : Editor
{
    private SerializedProperty possibleDropsProp;
    private SerializedProperty spawnOffsetRangeProp;
    private SerializedProperty coinPrefabProp;
    
    private bool showItemDrops = true;
    private Dictionary<int, bool> foldoutStates = new Dictionary<int, bool>();
    
    void OnEnable()
    {
        possibleDropsProp = serializedObject.FindProperty("possibleDrops");
        spawnOffsetRangeProp = serializedObject.FindProperty("spawnOffsetRange");
        coinPrefabProp = serializedObject.FindProperty("coinPrefab");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EnemyItemDropper dropper = (EnemyItemDropper)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Drop Configuration", EditorStyles.boldLabel);
        
        // Spawn Settings
        EditorGUILayout.PropertyField(spawnOffsetRangeProp, new GUIContent("Spawn Offset Range", 
            "Random offset range for item spawn positions to prevent overlap"));
        EditorGUILayout.PropertyField(coinPrefabProp, new GUIContent("Coin Prefab", 
            "Optional coin prefab to instantiate (will create at runtime if not set)"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        // Item Drops List
        showItemDrops = EditorGUILayout.Foldout(showItemDrops, $"Item Drops ({possibleDropsProp.arraySize})", true);
        
        if (showItemDrops)
        {
            EditorGUI.indentLevel++;
            
            // Add button
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Item Drop", GUILayout.Height(25)))
            {
                AddNewItemDrop();
            }
            
            if (GUILayout.Button("Create ItemDropData Asset", GUILayout.Height(25)))
            {
                CreateItemDropDataAsset();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Display list of item drops
            if (possibleDropsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No item drops configured. Click 'Add Item Drop' to add one.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < possibleDropsProp.arraySize; i++)
                {
                    DrawItemDropElement(i);
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void AddNewItemDrop()
    {
        possibleDropsProp.arraySize++;
        SerializedProperty newElement = possibleDropsProp.GetArrayElementAtIndex(possibleDropsProp.arraySize - 1);
        newElement.objectReferenceValue = null;
        
        // Create a new ItemDropData ScriptableObject instance
        ItemDropData newDropData = ScriptableObject.CreateInstance<ItemDropData>();
        newDropData.itemType = ItemDropData.ItemType.Weapon;
        newDropData.spawnRate = 0.5f;
        
        // Generate unique asset path
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Assets/Items/NewItemDrop.asset");
        
        // Create the asset
        AssetDatabase.CreateAsset(newDropData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Assign to the property
        newElement.objectReferenceValue = newDropData;
        
        serializedObject.ApplyModifiedProperties();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newDropData;
    }
    
    private void CreateItemDropDataAsset()
    {
        ItemDropData newDropData = ScriptableObject.CreateInstance<ItemDropData>();
        newDropData.itemType = ItemDropData.ItemType.Weapon;
        newDropData.spawnRate = 0.5f;
        
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Item Drop Data",
            "NewItemDrop",
            "asset",
            "Please enter a file name to save the ItemDropData asset to");
        
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newDropData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newDropData;
            
            Debug.Log($"Created ItemDropData asset at: {path}");
        }
    }
    
    private void DrawItemDropElement(int index)
    {
        SerializedProperty elementProp = possibleDropsProp.GetArrayElementAtIndex(index);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        // Foldout for item details
        int foldoutKey = index;
        if (!foldoutStates.ContainsKey(foldoutKey))
        {
            foldoutStates[foldoutKey] = true;
        }
        
        foldoutStates[foldoutKey] = EditorGUILayout.Foldout(foldoutStates[foldoutKey], 
            GetItemDropDisplayName(elementProp, index), true);
        
        // Remove button
        if (GUILayout.Button("Remove", GUILayout.Width(60), GUILayout.Height(20)))
        {
            RemoveItemDrop(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (foldoutStates[foldoutKey])
        {
            EditorGUI.indentLevel++;
            
            // Item Drop Data reference
            EditorGUILayout.PropertyField(elementProp, new GUIContent("Item Drop Data"));
            
            ItemDropData dropData = elementProp.objectReferenceValue as ItemDropData;
            
            if (dropData != null)
            {
                EditorGUILayout.Space(5);
                
                // Create a serialized object for the ItemDropData to edit it inline
                SerializedObject dropDataObject = new SerializedObject(dropData);
                dropDataObject.Update();
                
                SerializedProperty itemTypeProp = dropDataObject.FindProperty("itemType");
                SerializedProperty spawnRateProp = dropDataObject.FindProperty("spawnRate");
                SerializedProperty weaponDataProp = dropDataObject.FindProperty("weaponData");
                SerializedProperty coinValueProp = dropDataObject.FindProperty("coinValue");
                
                // Item Type
                EditorGUILayout.PropertyField(itemTypeProp, new GUIContent("Item Type"));
                
                // Spawn Rate with slider and percentage
                EditorGUILayout.BeginHorizontal();
                float spawnRate = spawnRateProp.floatValue;
                EditorGUILayout.LabelField("Spawn Rate", GUILayout.Width(100));
                spawnRate = EditorGUILayout.Slider(spawnRate, 0f, 1f);
                EditorGUILayout.LabelField($"{(spawnRate * 100):F1}%", GUILayout.Width(50));
                spawnRateProp.floatValue = spawnRate;
                EditorGUILayout.EndHorizontal();
                
                // Conditional fields based on item type
                ItemDropData.ItemType itemType = (ItemDropData.ItemType)itemTypeProp.enumValueIndex;
                
                if (itemType == ItemDropData.ItemType.Weapon)
                {
                    EditorGUILayout.PropertyField(weaponDataProp, new GUIContent("Weapon Data"));
                    
                    if (weaponDataProp.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("Weapon Data is required for Weapon type items.", MessageType.Warning);
                    }
                }
                else if (itemType == ItemDropData.ItemType.Coin)
                {
                    EditorGUILayout.PropertyField(coinValueProp, new GUIContent("Coin Value", 
                        "Optional coin value (default: 1)"));
                }
                
                dropDataObject.ApplyModifiedProperties();
                
                // Visual feedback
                EditorGUILayout.Space(3);
                EditorGUILayout.BeginHorizontal();
                float barWidth = EditorGUIUtility.currentViewWidth - 40;
                Rect rect = GUILayoutUtility.GetRect(barWidth, 5);
                EditorGUI.ProgressBar(rect, spawnRate, $"{(spawnRate * 100):F1}% chance");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Assign an ItemDropData ScriptableObject or create a new one.", MessageType.Info);
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private string GetItemDropDisplayName(SerializedProperty elementProp, int index)
    {
        ItemDropData dropData = elementProp.objectReferenceValue as ItemDropData;
        
        if (dropData == null)
        {
            return $"Item Drop {index + 1} (Not Assigned)";
        }
        
        string typeName = dropData.itemType.ToString();
        string spawnRateText = $"{(dropData.spawnRate * 100):F0}%";
        
        if (dropData.itemType == ItemDropData.ItemType.Weapon && dropData.weaponData != null)
        {
            return $"Item Drop {index + 1}: {typeName} ({dropData.weaponData.weaponName}) - {spawnRateText}";
        }
        else if (dropData.itemType == ItemDropData.ItemType.Coin)
        {
            return $"Item Drop {index + 1}: {typeName} (Value: {dropData.coinValue}) - {spawnRateText}";
        }
        
        return $"Item Drop {index + 1}: {typeName} - {spawnRateText}";
    }
    
    private void RemoveItemDrop(int index)
    {
        possibleDropsProp.DeleteArrayElementAtIndex(index);
        serializedObject.ApplyModifiedProperties();
    }
}

