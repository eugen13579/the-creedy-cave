using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


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

    void HideAll()
    {
        mainPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        tutorialsPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ShowMain() => Switch(mainPanel);
    public void ShowLevelSelect() => Switch(levelSelectPanel);
    public void ShowTutorials() => Switch(tutorialsPanel);
    public void ShowSettings() => Switch(settingsPanel);

    void Switch(GameObject panel)
    {
        HideAll();
        panel.SetActive(true);
    }

    public void PlayLevel(int level)
    {
        SceneManager.LoadScene("Dungeon" + level);
    }

    public void Quit()
    {
        Application.Quit();
    }

    // ---------- SETTINGS ----------

    void InitSettings()
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

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void SetResolution(int index)
    {
        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }
}
