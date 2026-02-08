using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }


    public void StartButton()
    {
        Debug.Log("Start Button Pressed");
        LoadSceneWithTransition(4); // Level 1
    }

    public void QuitButton()
    {
        Debug.Log("Quit Button Pressed");
        Application.Quit();
    }

    public void LevelSelect()
    {
        Debug.Log("Level Select Button Pressed");
        LoadSceneWithTransition(1);
    }

    public void YourJob()
    {
        Debug.Log("Your Job Button Pressed");
        LoadSceneWithTransition(2);
    }

    public void Credits()
    {
        Debug.Log("Credits Button Pressed");
        LoadSceneWithTransition(3);
    }


    private void LoadSceneWithTransition(int sceneIndex)
    {
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene(sceneIndex);
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}