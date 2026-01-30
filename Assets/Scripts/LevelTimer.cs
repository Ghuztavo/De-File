using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private TextMeshProUGUI timerText;
    private float timeRemaining;
    private bool isRunning = false;

    // NEW — stores the final formatted time
    private string finalFormattedTime = "00:00.00";


    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;

            // notify GameManager
            gameManager.timeUp = true;
        }

        UpdateUI();
    }

    public void StartCountdown(float startTime)
    {
        timeRemaining = startTime;
        isRunning = true;
        UpdateUI();
    }

    private void UpdateUI()
    {
        float t = timeRemaining;

        string minutes = ((int)t / 60).ToString("00");
        string seconds = ((int)t % 60).ToString("00");
        string milliseconds = ((int)(t * 100) % 100).ToString("00");

        timerText.text = $"{minutes}:{seconds}.{milliseconds}";
    }

}
