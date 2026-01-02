using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the main menu UI, including panel navigation, level selection, and settings.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject levelSelectPanel;
    public GameObject tutorialsPanel;
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public Slider volumeSlider;
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    void Start()
    {
        ShowMain();
        InitSettings();
    }

    /// <summary>
    /// Hides all panels.
    /// </summary>
    private void HideAll()
    {
        mainPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        tutorialsPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    /// <summary>
    /// Shows the main panel.
    /// </summary>
    public void ShowMain() => Switch(mainPanel);

    /// <summary>
    /// Shows the level select panel.
    /// </summary>
    public void ShowLevelSelect() => Switch(levelSelectPanel);

    /// <summary>
    /// Shows the tutorials panel.
    /// </summary>
    public void ShowTutorials() => Switch(tutorialsPanel);

    /// <summary>
    /// Shows the settings panel.
    /// </summary>
    public void ShowSettings() => Switch(settingsPanel);

    /// <summary>
    /// Switches to the specified panel, hiding all others.
    /// </summary>
    private void Switch(GameObject panel)
    {
        HideAll();
        panel.SetActive(true);
    }

    /// <summary>
    /// Loads the specified dungeon level.
    /// </summary>
    /// <param name="level">The level number to load (e.g., 1, 2, 3)</param>
    public void PlayLevel(int level)
    {
        SceneManager.LoadScene("Dungeon" + level);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Initializes settings UI components.
    /// </summary>
    private void InitSettings()
    {
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(SetVolume);

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    /// <summary>
    /// Sets the game volume.
    /// </summary>
    /// <param name="value">Volume value (0.0 to 1.0)</param>
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    /// <summary>
    /// Sets the screen resolution.
    /// </summary>
    /// <param name="index">Index of the resolution in the resolutions array</param>
    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length)
        {
            Debug.LogWarning($"Invalid resolution index: {index}");
            return;
        }

        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }
}
