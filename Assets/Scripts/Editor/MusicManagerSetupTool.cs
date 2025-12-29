using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to set up MusicManager in the scene with time_for_adventure.mp3.
/// </summary>
public class MusicManagerSetupTool
{
    [MenuItem("Tools/Setup Global Music")]
    public static void SetupGlobalMusic()
    {
        // Check if MusicManager already exists
        MusicManager existingManager = Object.FindFirstObjectByType<MusicManager>();
        if (existingManager != null)
        {
            bool replace = EditorUtility.DisplayDialog("MusicManager Already Exists",
                "A MusicManager already exists in the scene.\n\n" +
                "Would you like to replace it with a new one?",
                "Replace", "Cancel");

            if (!replace)
            {
                return;
            }

            Object.DestroyImmediate(existingManager.gameObject);
        }

        // Create new GameObject for MusicManager
        GameObject musicManagerObj = new GameObject("MusicManager");
        MusicManager musicManager = musicManagerObj.AddComponent<MusicManager>();

        // Load the music clip
        AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Music/time_for_adventure.mp3");
        if (musicClip == null)
        {
            EditorUtility.DisplayDialog("Music File Not Found",
                "Could not find time_for_adventure.mp3 at Assets/Music/time_for_adventure.mp3.\n\n" +
                "Please make sure the file exists and try again.", "OK");
            Object.DestroyImmediate(musicManagerObj);
            return;
        }

        // Set the music clip using SerializedObject
        SerializedObject serializedManager = new SerializedObject(musicManager);
        serializedManager.FindProperty("backgroundMusic").objectReferenceValue = musicClip;
        serializedManager.ApplyModifiedProperties();

        // Mark scene as dirty
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        EditorUtility.DisplayDialog("Global Music Setup Complete",
            "MusicManager has been set up successfully!\n\n" +
            "The music will:\n" +
            "- Play automatically when the scene loads\n" +
            "- Loop continuously\n" +
            "- Persist across scene changes\n\n" +
            "Make sure this scene is loaded first (e.g., in your main menu or first level scene).",
            "OK");

        Debug.Log("MusicManager setup complete. Music: time_for_adventure.mp3");
    }
}


