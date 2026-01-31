using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameManager gameManager;

    private float timeRemaining;
    private bool isRunning = false;

    void Start()
    {
        // Get TextMeshProUGUI if not assigned
        if (timerText == null)
        {
            timerText = GetComponent<TextMeshProUGUI>();
        }

        // Display initial time from GameManager
        if (gameManager != null)
        {
            timeRemaining = gameManager.levelTime;
            UpdateUI();
        }
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;

            // Notify GameManager
            if (gameManager != null)
            {
                gameManager.timeUp = true;
            }
        }

        UpdateUI();
    }

    public void StartCountdown(float startTime)
    {
        if (!isRunning) // Only start if not already running
        {
            timeRemaining = startTime;
            isRunning = true;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        float t = Mathf.Max(0f, timeRemaining);

        // Format: SS:MS (seconds:milliseconds)
        int seconds = Mathf.FloorToInt(t);
        int centiseconds = Mathf.FloorToInt((t * 100f) % 100f);

        timerText.text = string.Format("{0:00}:{1:00}", seconds, centiseconds);
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }
}