using UnityEngine;

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
    }

    public void QuitButton()
    {
        Debug.Log("Quit Button Pressed");
        Application.Quit();
    }

    public void LevelSelect()
    {
        Debug.Log("Level Select Button Pressed");
    }

    public void YourJob()
    {
        Debug.Log("Your Job Button Pressed");
    }

    public void Credits()
    {
        Debug.Log("Credits Button Pressed");
    }

    public void Options()
    {
        Debug.Log("Options Button Pressed");
    }
}