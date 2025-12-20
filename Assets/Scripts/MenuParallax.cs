using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MenuParallax : MonoBehaviour
{
    //Settings property
    public GameObject settingMenu;

    public GameObject mainMenu;

    public AudioMixer audioMixer;
    
    private void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void PlayGame()
    {
        //Load Scene
        SceneManager.LoadScene("Dungeon1");
    }   

    public void Settings()
    {
        settingMenu.SetActive(true);
    }    
    
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }  
    
    public void OK()
    {
        mainMenu.SetActive(true);
    }
    
    public void changeVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
        AudioListener.volume = volume;
    }    
}
