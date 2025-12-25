using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý Main Menu - xử lý các button và navigation
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string levelSelectSceneName = "LevelSelect"; // Tên scene level selection
    
    [Header("UI Buttons")]
    [SerializeField] private Button playButton; // Button để vào level selection
    [SerializeField] private Button quitButton; // Button để quit game
    
    void Start()
    {
        // Setup buttons nếu chưa được gán
        SetupButtons();
        
        // Đảm bảo time scale là 1 (tránh bị pause từ scene trước)
        Time.timeScale = 1f;
    }
    
    void SetupButtons()
    {
        // Auto-find buttons nếu chưa được gán
        if (playButton == null)
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("play") || btn.name.ToLower().Contains("start"))
                {
                    playButton = btn;
                    break;
                }
            }
        }
        
        if (quitButton == null)
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("quit") || btn.name.ToLower().Contains("exit"))
                {
                    quitButton = btn;
                    break;
                }
            }
        }
        
        // Subscribe to button events
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogWarning("MenuManager: Play button không tìm thấy! Vui lòng gán button vào inspector hoặc đặt tên button có chứa 'Play' hoặc 'Start'.");
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        else
        {
            Debug.LogWarning("MenuManager: Quit button không tìm thấy! Vui lòng gán button vào inspector hoặc đặt tên button có chứa 'Quit' hoặc 'Exit'.");
        }
    }
    
    /// <summary>
    /// Xử lý khi click Play button - chuyển đến level selection
    /// </summary>
    public void OnPlayButtonClicked()
    {
        if (!string.IsNullOrEmpty(levelSelectSceneName))
        {
            SceneManager.LoadScene(levelSelectSceneName);
            Debug.Log($"MenuManager: Đang chuyển đến {levelSelectSceneName}");
        }
        else
        {
            Debug.LogWarning("MenuManager: Level select scene name chưa được thiết lập!");
        }
    }
    
    /// <summary>
    /// Xử lý khi click Quit button
    /// </summary>
    public void OnQuitButtonClicked()
    {
        Debug.Log("MenuManager: Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Public method để load level select scene (có thể gọi từ UI)
    /// </summary>
    public void LoadLevelSelect()
    {
        OnPlayButtonClicked();
    }
    
    /// <summary>
    /// Public method để quit game (có thể gọi từ UI)
    /// </summary>
    public void QuitGame()
    {
        OnQuitButtonClicked();
    }
}

