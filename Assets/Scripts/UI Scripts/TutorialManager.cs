using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Pages")]
    [SerializeField] private GameObject[] tutorialPages; // Array of your 3 page GameObjects

    [Header("Navigation")]
    [SerializeField] private int currentPageIndex = 0;

    void Start()
    {
        // Show only the first page at start
        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        // Hide all pages first
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            if (tutorialPages[i] != null)
            {
                tutorialPages[i].SetActive(i == currentPageIndex);
            }
        }
    }

    public void NextPage()
    {
        if (currentPageIndex < tutorialPages.Length - 1)
        {
            currentPageIndex++;
            ShowCurrentPage();
            Debug.Log("Next Page: " + (currentPageIndex + 1));
        }
        else
        {
            Debug.Log("Already on last page");
        }
    }

    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            ShowCurrentPage();
            Debug.Log("Previous Page: " + (currentPageIndex + 1));
        }
        else
        {
            Debug.Log("Already on first page");
        }
    }

    public void GoToPage(int pageIndex)
    {
        // Go to a specific page
        if (pageIndex >= 0 && pageIndex < tutorialPages.Length)
        {
            currentPageIndex = pageIndex;
            ShowCurrentPage();
            Debug.Log("Jump to Page: " + (currentPageIndex + 1));
        }
        else
        {
            Debug.LogWarning("Invalid page index: " + pageIndex);
        }
    }

    public void BackToMainMenu()
    {
        if(SceneTransition.Instance != null)
    {
            SceneTransition.Instance.LoadScene(0);
        }
    else
        {
            SceneManager.LoadScene(0);
        }
    }
}