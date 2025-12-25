using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

/// <summary>
/// Helper tool to set up Gate prefabs in the scene with GateController component.
/// </summary>
public class GateSetupHelper
{
    [MenuItem("Tools/Setup Gates in Scene")]
    public static void SetupGatesInScene()
    {
        // Find all GameObjects with "Gate" tag
        GameObject[] gates = GameObject.FindGameObjectsWithTag("Gate");
        
        if (gates.Length == 0)
        {
            EditorUtility.DisplayDialog("No Gates Found", 
                "No GameObjects with 'Gate' tag found in the scene.\n\n" +
                "Please make sure your Gate prefabs have the 'Gate' tag assigned.", "OK");
            return;
        }
        
        int setupCount = 0;
        
        foreach (GameObject gate in gates)
        {
            // Check if GateController already exists
            GateController controller = gate.GetComponent<GateController>();
            if (controller != null)
            {
                Debug.Log($"Gate '{gate.name}' already has GateController component.");
                continue;
            }
            
            // Add GateController component
            controller = gate.AddComponent<GateController>();
            
            // Ensure collider exists and is set as trigger
            // Handle Tilemap gates specially
            Tilemap tilemap = gate.GetComponent<Tilemap>();
            TilemapCollider2D tilemapCollider = gate.GetComponent<TilemapCollider2D>();
            
            if (tilemap != null)
            {
                // For Tilemap gates, check if there's already a trigger collider
                Collider2D[] allColliders = gate.GetComponents<Collider2D>();
                Collider2D triggerCollider = null;
                
                foreach (Collider2D col in allColliders)
                {
                    // Find a non-TilemapCollider2D that we can use as trigger
                    if (!(col is TilemapCollider2D))
                    {
                        triggerCollider = col;
                        break;
                    }
                }
                
                if (triggerCollider == null)
                {
                    // Add a BoxCollider2D as trigger for interaction
                    BoxCollider2D boxCollider = gate.AddComponent<BoxCollider2D>();
                    boxCollider.isTrigger = true;
                    
                    // Set size based on tilemap bounds
                    Bounds bounds = tilemap.localBounds;
                    boxCollider.size = bounds.size;
                    boxCollider.offset = bounds.center - gate.transform.position;
                    
                    Debug.Log($"Added BoxCollider2D trigger to Tilemap Gate '{gate.name}'");
                }
                else
                {
                    // Configure existing collider as trigger
                    triggerCollider.isTrigger = true;
                    Debug.Log($"Configured existing collider as trigger for Tilemap Gate '{gate.name}'");
                }
            }
            else
            {
                // Standard gate (not Tilemap)
                Collider2D collider = gate.GetComponent<Collider2D>();
                if (collider == null)
                {
                    // Add BoxCollider2D if no collider exists
                    collider = gate.AddComponent<BoxCollider2D>();
                    Debug.Log($"Added BoxCollider2D to Gate '{gate.name}'");
                }
                
                // Set collider as trigger
                collider.isTrigger = true;
            }
            
            // Try to find Press E prompt UI and link it
            Canvas hudCanvas = Object.FindFirstObjectByType<Canvas>();
            if (hudCanvas != null && hudCanvas.name == "HUDCanvas")
            {
                Transform promptTransform = hudCanvas.transform.Find("PressEPrompt");
                if (promptTransform != null)
                {
                    SerializedObject serializedController = new SerializedObject(controller);
                    serializedController.FindProperty("pressEPromptUI").objectReferenceValue = promptTransform.gameObject;
                    serializedController.ApplyModifiedProperties();
                }
            }
            
            setupCount++;
            Debug.Log($"Set up Gate '{gate.name}' with GateController component.");
        }
        
        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        EditorUtility.DisplayDialog("Gate Setup Complete", 
            $"Successfully set up {setupCount} gate(s) in the scene.\n\n" +
            "Each gate now has:\n" +
            "- GateController component\n" +
            "- Trigger collider (if not already present)\n\n" +
            "Make sure to run 'Tools > Setup Level Complete UI' first if you haven't already!", "OK");
    }
}

