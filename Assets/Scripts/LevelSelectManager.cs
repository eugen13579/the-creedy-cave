using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Quản lý Level Selection - cho phép player chọn level để chơi
/// </summary>
public class LevelSelectManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Tên scene main menu để quay lại
    
    [Header("Level Data")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>(); // Danh sách các level
    
    [Header("UI Buttons")]
    [SerializeField] private Button backButton; // Button để quay về menu
    
    [System.Serializable]
    public class LevelData
    {
        public string levelName; // Tên hiển thị của level
        public string sceneName; // Tên scene của level
        public Button levelButton; // Button để chọn level này (optional - có thể tự tìm)
        public bool isUnlocked = true; // Level có được unlock không
    }
    
    void Start()
    {
        // Setup buttons
        SetupButtons();
        
        // Setup level buttons
        SetupLevelButtons();
        
        // Đảm bảo time scale là 1
        Time.timeScale = 1f;
    }
    
    void SetupButtons()
    {
        // Auto-find back button nếu chưa được gán
        if (backButton == null)
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("back") || btn.name.ToLower().Contains("menu"))
                {
                    backButton = btn;
                    break;
                }
            }
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        else
        {
            Debug.LogWarning("LevelSelectManager: Back button không tìm thấy! Vui lòng gán button vào inspector hoặc đặt tên button có chứa 'Back' hoặc 'Menu'.");
        }
    }
    
    void SetupLevelButtons()
    {
        // Nếu level buttons chưa được gán, tự động tìm và setup
        for (int i = 0; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            
            // Nếu button chưa được gán, tìm button có tên chứa level name hoặc số thứ tự
            if (level.levelButton == null)
            {
                Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
                foreach (Button btn in buttons)
                {
                    string btnName = btn.name.ToLower();
                    if (btnName.Contains(level.levelName.ToLower()) || 
                        btnName.Contains($"level{i + 1}") ||
                        btnName.Contains($"level {i + 1}"))
                    {
                        level.levelButton = btn;
                        break;
                    }
                }
            }
            
            // Setup button click event
            if (level.levelButton != null)
            {
                // Lưu index để tránh closure issue
                int levelIndex = i;
                level.levelButton.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
                
                // Disable button nếu level chưa unlock
                if (!level.isUnlocked)
                {
                    level.levelButton.interactable = false;
                }
            }
        }
    }
    
    /// <summary>
    /// Xử lý khi click level button
    /// </summary>
    void OnLevelButtonClicked(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogWarning($"LevelSelectManager: Level index {levelIndex} không hợp lệ!");
            return;
        }
        
        LevelData level = levels[levelIndex];
        
        if (!level.isUnlocked)
        {
            Debug.Log($"LevelSelectManager: Level {level.levelName} chưa được unlock!");
            return;
        }
        
        if (string.IsNullOrEmpty(level.sceneName))
        {
            Debug.LogWarning($"LevelSelectManager: Scene name của level {level.levelName} chưa được thiết lập!");
            return;
        }
        
        // Load level scene
        SceneManager.LoadScene(level.sceneName);
        Debug.Log($"LevelSelectManager: Đang load level {level.levelName} ({level.sceneName})");
    }
    
    /// <summary>
    /// Xử lý khi click Back button - quay về main menu
    /// </summary>
    public void OnBackButtonClicked()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log($"LevelSelectManager: Đang quay về {mainMenuSceneName}");
        }
        else
        {
            Debug.LogWarning("LevelSelectManager: Main menu scene name chưa được thiết lập!");
        }
    }
    
    /// <summary>
    /// Public method để load level (có thể gọi từ UI)
    /// </summary>
    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("LevelSelectManager: Scene name không được để trống!");
            return;
        }
        
        SceneManager.LoadScene(sceneName);
        Debug.Log($"LevelSelectManager: Đang load scene {sceneName}");
    }
    
    /// <summary>
    /// Public method để quay về menu (có thể gọi từ UI)
    /// </summary>
    public void ReturnToMenu()
    {
        OnBackButtonClicked();
    }
    
    /// <summary>
    /// Unlock một level (có thể dùng để unlock level sau khi hoàn thành level trước)
    /// </summary>
    public void UnlockLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Count)
        {
            levels[levelIndex].isUnlocked = true;
            if (levels[levelIndex].levelButton != null)
            {
                levels[levelIndex].levelButton.interactable = true;
            }
            Debug.Log($"LevelSelectManager: Level {levels[levelIndex].levelName} đã được unlock!");
        }
    }
}

