using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Singleton manager that handles level complete UI and scene transitions.
/// </summary>
public class LevelCompleteManager : MonoBehaviour
{
    public static LevelCompleteManager Instance { get; private set; }
    
    [Header("Level Complete UI")]
    [SerializeField] private GameObject levelCompleteUI; // Canvas hoặc Panel chứa level complete screen
    [SerializeField] private TextMeshProUGUI pointsText; // Text hiển thị points/coins
    [SerializeField] private float fadeInDuration = 0.5f; // Thời gian fade in level complete screen
    
    [Header("Options")]
    [SerializeField] private bool pauseGameOnComplete = true; // Có pause game khi hoàn thành không
    [SerializeField] private string levelSelectSceneName = "LevelSelect"; // Tên scene level select (nếu có)
    
    [Header("Next Level")]
    [SerializeField] private bool useAutoProgression = true; // Tự động tăng level (Dungeon1 -> Dungeon2)
    [SerializeField] private string nextSceneName = ""; // Tên scene tiếp theo (nếu không dùng auto progression)
    
    private bool isLevelCompleteActive = false;
    private Coroutine fadeCoroutine;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LevelCompleteManager: Initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Ẩn level complete screen khi bắt đầu
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(false);
        }
        
        // Setup button listeners
        SetupButtonListeners();
    }
    
    /// <summary>
    /// Setup button click listeners for level complete UI
    /// </summary>
    private void SetupButtonListeners()
    {
        if (levelCompleteUI == null) return;
        
        // Find Next Level button
        Transform nextButtonTransform = levelCompleteUI.transform.Find("ContentPanel/NextLevelButton");
        if (nextButtonTransform != null)
        {
            Button nextLevelButton = nextButtonTransform.GetComponent<Button>();
            if (nextLevelButton != null)
            {
                // Remove existing listeners and add new one
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(LoadNextLevel);
                Debug.Log("LevelCompleteManager: Next Level button listener set up.");
            }
        }
    }
    
    void OnEnable()
    {
        // Reset state khi scene được load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset level complete state khi load scene mới
        isLevelCompleteActive = false;
        Time.timeScale = 1f;
        
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(false);
        }
        
        // Stop fade coroutine nếu đang chạy
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }
    
    /// <summary>
    /// Hiển thị level complete screen
    /// </summary>
    public void ShowLevelComplete()
    {
        if (isLevelCompleteActive) return; // Tránh gọi nhiều lần
        
        isLevelCompleteActive = true;
        
        // Pause game nếu cần
        if (pauseGameOnComplete)
        {
            Time.timeScale = 0f;
        }
        
        // Hiển thị level complete UI
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
            
            // Update points display
            UpdatePointsDisplay();
            
            // Nếu có CanvasGroup, có thể làm fade in
            CanvasGroup canvasGroup = levelCompleteUI.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                // Stop previous fade coroutine nếu có
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeInLevelComplete(canvasGroup));
            }
        }
        else
        {
            Debug.LogWarning("LevelCompleteManager: Level Complete UI chưa được gán! Vui lòng gán GameObject chứa UI level complete vào LevelCompleteManager.");
        }
        
        Debug.Log("LevelCompleteManager: Level complete screen đã được hiển thị");
    }
    
    /// <summary>
    /// Ẩn level complete screen
    /// </summary>
    public void HideLevelComplete()
    {
        isLevelCompleteActive = false;
        Time.timeScale = 1f; // Resume game
        
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Load next level scene
    /// </summary>
    public void LoadNextLevel()
    {
        HideLevelComplete();
        Time.timeScale = 1f;
        
        string sceneToLoad = GetNextSceneName();
        
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
            Debug.Log($"LevelCompleteManager: Đang load level tiếp theo: {sceneToLoad}");
        }
        else
        {
            Debug.LogWarning("LevelCompleteManager: Không tìm thấy level tiếp theo. Quay về Level Select.");
            ReturnToLevelSelect();
        }
    }
    
    /// <summary>
    /// Quay về level select screen
    /// </summary>
    public void ReturnToLevelSelect()
    {
        HideLevelComplete();
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(levelSelectSceneName))
        {
            SceneManager.LoadScene(levelSelectSceneName);
            Debug.Log($"LevelCompleteManager: Đã quay về {levelSelectSceneName}");
        }
        else
        {
            Debug.LogWarning("LevelCompleteManager: Level select scene name chưa được thiết lập!");
        }
    }
    
    /// <summary>
    /// Xác định tên scene tiếp theo
    /// </summary>
    private string GetNextSceneName()
    {
        // Nếu có nextSceneName được set thủ công, dùng nó
        if (!useAutoProgression && !string.IsNullOrEmpty(nextSceneName))
        {
            return nextSceneName;
        }
        
        // Tự động tăng level (Dungeon1 -> Dungeon2, etc.)
        if (useAutoProgression)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string currentSceneName = currentScene.name;
            
            // Tìm số trong tên scene (ví dụ: "Dungeon1" -> 1)
            int currentLevel = ExtractLevelNumber(currentSceneName);
            
            if (currentLevel > 0)
            {
                // Tăng level và tạo tên scene mới
                int nextLevel = currentLevel + 1;
                string baseName = GetBaseSceneName(currentSceneName);
                string nextScene = $"{baseName}{nextLevel}";
                
                // Kiểm tra xem scene có tồn tại không
                if (SceneExists(nextScene))
                {
                    return nextScene;
                }
            }
        }
        
        // Fallback: dùng nextSceneName nếu có
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            return nextSceneName;
        }
        
        return null; // Không tìm thấy level tiếp theo
    }
    
    /// <summary>
    /// Trích xuất số level từ tên scene (ví dụ: "Dungeon1" -> 1)
    /// </summary>
    private int ExtractLevelNumber(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return 0;
        
        // Tìm số ở cuối tên scene
        string numberStr = "";
        for (int i = sceneName.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(sceneName[i]))
            {
                numberStr = sceneName[i] + numberStr;
            }
            else
            {
                break;
            }
        }
        
        if (int.TryParse(numberStr, out int level))
        {
            return level;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Lấy phần base name của scene (ví dụ: "Dungeon1" -> "Dungeon")
    /// </summary>
    private string GetBaseSceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return "";
        
        // Loại bỏ số ở cuối
        for (int i = sceneName.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(sceneName[i]))
            {
                return sceneName.Substring(0, i + 1);
            }
        }
        
        return sceneName;
    }
    
    /// <summary>
    /// Kiểm tra xem scene có tồn tại trong build settings không
    /// </summary>
    private bool SceneExists(string sceneName)
    {
        #if UNITY_EDITOR
        foreach (UnityEditor.EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes)
        {
            if (scene.path.EndsWith($"/{sceneName}.unity"))
            {
                return true;
            }
        }
        #else
        // At runtime, we can't check build settings, so we'll try to load it
        // For now, we'll assume it exists if the name is valid
        // The actual loading will fail gracefully if the scene doesn't exist
        #endif
        // Return true to allow attempt - SceneManager.LoadScene will handle errors
        return !string.IsNullOrEmpty(sceneName);
    }
    
    /// <summary>
    /// Set next scene name manually (có thể gọi từ GateController)
    /// </summary>
    public void SetNextSceneName(string sceneName)
    {
        nextSceneName = sceneName;
        useAutoProgression = false; // Disable auto progression khi set manual
    }
    
    /// <summary>
    /// Update points display with current coin count
    /// </summary>
    private void UpdatePointsDisplay()
    {
        if (pointsText == null)
        {
            // Try to find points text automatically
            if (levelCompleteUI != null)
            {
                Transform pointsTransform = levelCompleteUI.transform.Find("ContentPanel/PointsText");
                if (pointsTransform != null)
                {
                    pointsText = pointsTransform.GetComponent<TextMeshProUGUI>();
                }
            }
        }
        
        if (pointsText != null)
        {
            // Find CoinManager in scene
            CoinManager coinManager = FindFirstObjectByType<CoinManager>();
            int points = 0;
            
            if (coinManager != null)
            {
                points = coinManager.CoinCount;
            }
            
            pointsText.text = $"Points: {points}";
        }
    }
    
    /// <summary>
    /// Fade in level complete screen với animation
    /// </summary>
    private IEnumerator FadeInLevelComplete(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null || levelCompleteUI == null) yield break;
        
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration && levelCompleteUI != null && canvasGroup != null)
        {
            elapsedTime += Time.unscaledDeltaTime; // Dùng unscaledDeltaTime vì game đã pause
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        
        fadeCoroutine = null;
    }
    
    void OnDestroy()
    {
        // Unsubscribe từ scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Stop fade coroutine nếu đang chạy
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Đảm bảo time scale được reset khi destroy
        if (Instance == this)
        {
            Time.timeScale = 1f;
        }
    }
}

