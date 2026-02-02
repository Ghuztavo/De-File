using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelSelectManager : MonoBehaviour
{
    public void Level_One()
    {
        SceneManager.LoadScene(4);
    }
    public void Level_Two()
    {
        SceneManager.LoadScene(5);
    }
    public void Level_Three()
    {
        SceneManager.LoadScene(6);
    }
    public void Level_Four()
    {
        SceneManager.LoadScene(7);
    }
    public void Level_Five()
    {
        SceneManager.LoadScene(8);
    }
    public void Level_Six()
    {
        SceneManager.LoadScene(9);
    }
    public void Level_Seven()
    {
        SceneManager.LoadScene(10);
    }
    public void Level_Eight()
    {
        SceneManager.LoadScene(11);
    }

    public void BackToMain()
    {
        // Load Main Menu Scene
        SceneManager.LoadScene(0);
    }

    public void LevelSelect()
    {
        // Load Level Select Scene
        SceneManager.LoadScene(1);
    }

    public void NextLevel()
    {
        // Load the next level based on the current active scene's build index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}
