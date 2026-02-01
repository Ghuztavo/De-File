using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButton()
    {
        Debug.Log("Start Button Pressed");
        SceneManager.LoadScene(4); // We always start on level 1 from the mian menu
    }

    public void QuitButton()
    {
        Debug.Log("Quit Button Pressed");
        Application.Quit();
    }

    public void LevelSelect()
    {
        Debug.Log("Level Select Button Pressed");
        SceneManager.LoadScene(1);
    }

    public void YourJob()
    {
        Debug.Log("Your Job Button Pressed");
        SceneManager.LoadScene(2);
    }

    public void Credits()
    {
        Debug.Log("Credits Button Pressed");
        SceneManager.LoadScene(3);
    }

    public void Options()
    {
        Debug.Log("Options Button Pressed");
    }
}