using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager that handles player death screen and game over logic.
/// Quản lý việc load death scene khi player chết.
/// </summary>
public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }
    
    [Header("Death Scene")]
    [SerializeField] private string deathSceneName = "DeathScene"; // Tên scene death screen
    
    [Header("Options")]
    [SerializeField] private bool pauseGameOnDeath = true; // Có pause game khi chết không
    
    private bool isDeathScreenActive = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isDeathScreenActive = false;
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Hiển thị death screen khi player chết - Load death scene
    /// </summary>
    public void ShowDeathScreen()
    {
        if (isDeathScreenActive)
        {
            return;
        }
        
        isDeathScreenActive = true;
        
        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }
        
        LoadDeathScene();
    }
    
    /// <summary>
    /// Load death scene
    /// </summary>
    private void LoadDeathScene()
    {
        if (string.IsNullOrEmpty(deathSceneName))
        {
            Debug.LogError("DeathManager: Death scene name chưa được thiết lập!");
            return;
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(deathSceneName);
    }
    
    /// <summary>
    /// Ẩn death screen (khi quay về menu)
    /// </summary>
    public void HideDeathScreen()
    {
        isDeathScreenActive = false;
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (Instance == this)
        {
            Time.timeScale = 1f;
        }
    }
}

