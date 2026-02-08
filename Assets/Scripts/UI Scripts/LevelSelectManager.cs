using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public void Level_One()
    {
        LoadSceneWithTransition(4);
    }

    public void Level_Two()
    {
        LoadSceneWithTransition(5);
    }

    public void Level_Three()
    {
        LoadSceneWithTransition(6);
    }

    public void Level_Four()
    {
        LoadSceneWithTransition(7);
    }

    public void Level_Five()
    {
        LoadSceneWithTransition(8);
    }

    public void Level_Six()
    {
        LoadSceneWithTransition(9);
    }

    public void Level_Seven()
    {
        LoadSceneWithTransition(10);
    }

    public void Level_Eight()
    {
        LoadSceneWithTransition(11);
    }

    public void BackToMain()
    {
        LoadSceneWithTransition(0);
    }

    public void LevelSelect()
    {
        LoadSceneWithTransition(1);
    }

    public void NextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        LoadSceneWithTransition(currentSceneIndex + 1);
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