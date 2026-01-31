using UnityEngine;
using UnityEngine.UI;

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

    private float targetInkFill;
    private bool timerStarted = false;

    void Start()
    {
        if (inkFillImage != null)
        {
            targetInkFill = Ink / maxInk;
            inkFillImage.fillAmount = targetInkFill;
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