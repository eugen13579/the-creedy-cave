using UnityEngine;

/// <summary>
/// Manages global background music that persists across scenes and loops continuously.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource settings
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false; // We'll start manually in Start()
    }

    void Start()
    {
        // Start playing music if clip is assigned
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("MusicManager: Background music clip is not assigned. Please assign time_for_adventure.mp3 in the inspector.");
        }
    }

    /// <summary>
    /// Set the volume of the background music (0-1).
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Get the current volume.
    /// </summary>
    public float GetVolume()
    {
        return volume;
    }
}

