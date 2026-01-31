using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject StartButton;

    [Header("Game State")]
    public int Score = 0;
    public float Ink = 100f;
    public float maxInk = 1000f;
    public int TotalObjectSpawns = 500;
    public bool gameStarted = false;
    public float levelTime = 25f;
    public bool timeUp = false;

    [Header("UI References")]
    [SerializeField] private LevelTimer timer;
    [SerializeField] private Image inkFillImage;

    [Header("Ink UI Settings")]
    [SerializeField] private float inkUpdateSpeed = 5f;

    [Header("Win/Lose Settings")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private int minimumScoreToWin = 100; // Adjust based on your level difficulty
    [SerializeField] private float resultScreenDelay = 1.5f; // Delay after timer ends
    [SerializeField] private float slideUpDuration = 0.8f; // Animation duration
    [SerializeField] private AnimationCurve slideEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float targetInkFill;
    private bool timerStarted = false;
    private bool levelEnded = false;

    // Store initial positions for animation
    private Vector3 winScreenStartPos;
    private Vector3 loseScreenStartPos;

    void Start()
    {
        if (inkFillImage != null)
        {
            targetInkFill = Ink / maxInk;
            inkFillImage.fillAmount = targetInkFill;
        }

        // Store initial positions and hide screens
        if (winScreen != null)
        {
            winScreenStartPos = winScreen.GetComponent<RectTransform>().anchoredPosition;
            winScreen.SetActive(false);
        }

        if (loseScreen != null)
        {
            loseScreenStartPos = loseScreen.GetComponent<RectTransform>().anchoredPosition;
            loseScreen.SetActive(false);
        }
    }

    void Update()
    {
        // Only start timer once when game starts
        if (gameStarted && !timerStarted)
        {
            if (timer != null)
            {
                timer.StartCountdown(levelTime);
                timerStarted = true;
                Debug.Log("Timer started!");
            }
        }

        // Check if timer ended and show result screen
        if (timeUp && !levelEnded)
        {
            levelEnded = true;
            StartCoroutine(ShowResultScreen());
        }

        // Smoothly update ink UI with easing
        if (inkFillImage != null)
        {
            targetInkFill = Mathf.Clamp01(Ink / maxInk);
            inkFillImage.fillAmount = Mathf.Lerp(
                inkFillImage.fillAmount,
                targetInkFill,
                Time.deltaTime * inkUpdateSpeed
            );
        }
    }

    private IEnumerator ShowResultScreen()
    {
        // Wait a moment after timer ends
        yield return new WaitForSeconds(resultScreenDelay);

        // Determine win or lose
        bool didWin = Score >= minimumScoreToWin;

        GameObject screenToShow = didWin ? winScreen : loseScreen;
        Vector3 startPos = didWin ? winScreenStartPos : loseScreenStartPos;

        if (screenToShow != null)
        {
            Debug.Log(didWin ? "Player Won! Score: " + Score : "Player Lost. Score: " + Score);

            RectTransform rectTransform = screenToShow.GetComponent<RectTransform>();

            // Position screen below the canvas (off-screen)
            Vector3 offScreenPos = startPos + new Vector3(0, -Screen.height, 0);
            rectTransform.anchoredPosition = offScreenPos;

            // Activate the screen
            screenToShow.SetActive(true);

            // Animate slide up
            float elapsed = 0f;

            while (elapsed < slideUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideUpDuration;

                // Apply easing curve for smooth animation
                float easedT = slideEaseCurve.Evaluate(t);

                rectTransform.anchoredPosition = Vector3.Lerp(offScreenPos, startPos, easedT);

                yield return null;
            }

            // Ensure final position is exact
            rectTransform.anchoredPosition = startPos;
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        StartButton.SetActive(false);
        Debug.Log("Game started!");
    }

    public void UseInk(float amount)
    {
        Ink -= amount;
        Ink = Mathf.Clamp(Ink, 0f, maxInk);
        Debug.Log("Ink used. Remaining: " + Ink);
    }

    public void AddInk(float amount)
    {
        Ink += amount;
        Ink = Mathf.Clamp(Ink, 0f, maxInk);
        Debug.Log("Ink added. Current: " + Ink);
    }

    public bool HasEnoughInk(float amount)
    {
        return Ink >= amount;
    }

    public void UpdateScore(int amount)
    {
        Score += amount;
        Debug.Log("Score Updated: " + Score);
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}