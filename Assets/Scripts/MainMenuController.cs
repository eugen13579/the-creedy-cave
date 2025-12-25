using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour 
{
    public GameObject tutorials;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Dungeon1");
    }

    public void ShowTutorials()
    {
        tutorials.SetActive(true);
    }


    public void Quit()
    {
        Debug.Log("Quit");
    }

    public void Back()
    {
        SceneManager.LoadScene("Menu");
    }
}
