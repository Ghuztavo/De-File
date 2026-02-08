using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject StartButton;

    [Header("Game State")]
    public float Score = 0;
    public float NegativeScore = 0;
    public float Ink = 100f;
    public float maxInk = 1000f;
    public int TotalObjectSpawns = 500;
    public bool gameStarted = false;
    public float levelTime = 25f;
    public bool timeUp = false;

    [Header("UI References")]
    [SerializeField] private LevelTimer timer;
    [SerializeField] private Image inkFillImage;

    [Header("Score Bar References")]
    [SerializeField] private Image positiveScoreBar;
    [SerializeField] private Image negativeScoreBar;
    [SerializeField] private TextMeshProUGUI positiveScoreText;
    [SerializeField] private TextMeshProUGUI negativeScoreText;

    [Header("Ink UI Settings")]
    [SerializeField] private float inkUpdateSpeed = 5f;

    [Header("Score Bar Settings")]
    [SerializeField] private float scoreBarUpdateSpeed = 8f;
    [SerializeField] private float targetPositiveScore = 100f;
    [SerializeField] private float targetNegativeScore = 50f;

    [Header("Win/Lose Settings")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private float resultScreenDelay = 1.5f;
    [SerializeField] private float slideUpDuration = 0.8f;
    [SerializeField] private AnimationCurve slideEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [SerializeField] private AudioSource STAMPaudioSource;
    [SerializeField] private AudioClip stampSound;
    [SerializeField] private AudioSource TimerSource;
    [SerializeField] private AudioClip timerSound;

    private float targetInkFill;
    private float targetPositiveBarFill;
    private float targetNegativeBarFill;
    private bool timerStarted = false;
    private bool levelEnded = false;
    private Vector3 winScreenStartPos;
    private Vector3 loseScreenStartPos;
    private bool soundsPlayed = false;

    void Start()
    {
        if (inkFillImage != null)
        {
            targetInkFill = Ink / maxInk;
            inkFillImage.fillAmount = targetInkFill;
        }

        if (positiveScoreBar != null)
        {
            targetPositiveBarFill = 0f;
            positiveScoreBar.fillAmount = 0f;
        }

        if (negativeScoreBar != null)
        {
            targetNegativeBarFill = 0f;
            negativeScoreBar.fillAmount = 0f;
        }

        UpdateScoreTexts();

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
        if (gameStarted && !timerStarted)
        {
            if (timer != null)
            {
                timer.StartCountdown(levelTime);
                timerStarted = true;
                Debug.Log("Timer started!");
            }
        }

        if (timeUp && !levelEnded)
        {
            if (timeUp) TimerSource.PlayOneShot(timerSound);
            levelEnded = true;
            StartCoroutine(ShowResultScreen());
        }

        if (NegativeScore >= targetNegativeScore && !levelEnded)
        {
            levelEnded = true;
            timeUp = true;
            Debug.Log("Player lost due to negative score!");
            StartCoroutine(ShowResultScreen());
        }

        if (inkFillImage != null)
        {
            targetInkFill = Mathf.Clamp01(Ink / maxInk);
            inkFillImage.fillAmount = Mathf.Lerp(
                inkFillImage.fillAmount,
                targetInkFill,
                Time.deltaTime * inkUpdateSpeed
            );
        }

        if (positiveScoreBar != null)
        {
            targetPositiveBarFill = Mathf.Clamp01(Score / targetPositiveScore);
            positiveScoreBar.fillAmount = Mathf.Lerp(
                positiveScoreBar.fillAmount,
                targetPositiveBarFill,
                Time.deltaTime * scoreBarUpdateSpeed
            );
        }

        if (negativeScoreBar != null)
        {
            targetNegativeBarFill = Mathf.Clamp01(NegativeScore / targetNegativeScore);
            negativeScoreBar.fillAmount = Mathf.Lerp(
                negativeScoreBar.fillAmount,
                targetNegativeBarFill,
                Time.deltaTime * scoreBarUpdateSpeed
            );
        }

        UpdateScoreTexts();
    }

    private void UpdateScoreTexts()
    {
        if (positiveScoreText != null)
        {
            positiveScoreText.text = $"{Mathf.FloorToInt(Score)}/{Mathf.FloorToInt(targetPositiveScore)}";
        }

        if (negativeScoreText != null)
        {
            negativeScoreText.text = $"{Mathf.FloorToInt(NegativeScore)}/{Mathf.FloorToInt(targetNegativeScore)}";
        }
    }

    private IEnumerator ShowResultScreen()
    {
        yield return new WaitForSeconds(resultScreenDelay);

        bool didWin = Score >= targetPositiveScore && NegativeScore < targetNegativeScore;

        GameObject screenToShow = didWin ? winScreen : loseScreen;
        Vector3 startPos = didWin ? winScreenStartPos : loseScreenStartPos;

        if (screenToShow != null)
        {
            Debug.Log(didWin ? "Player Won! Score: " + Score : "Player Lost. Positive Score: " + Score + " | Negative Score: " + NegativeScore);

            RectTransform rectTransform = screenToShow.GetComponent<RectTransform>();
            Vector3 offScreenPos = startPos + new Vector3(0, -Screen.height, 0);
            rectTransform.anchoredPosition = offScreenPos;
            screenToShow.SetActive(true);

            float elapsed = 0f;

            while (elapsed < slideUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideUpDuration;
                float easedT = slideEaseCurve.Evaluate(t);
                rectTransform.anchoredPosition = Vector3.Lerp(offScreenPos, startPos, easedT);
                yield return null;
            }

            rectTransform.anchoredPosition = startPos;
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        Debug.Log("Game started!");

        if (!soundsPlayed)
        {
            STAMPaudioSource.PlayOneShot(stampSound);
            soundsPlayed = true;
        }
    }

    public void UseInk(float amount)
    {
        Ink -= amount;
        Ink = Mathf.Clamp(Ink, 0f, maxInk);
    }

    public void AddInk(float amount)
    {
        Ink += amount;
        Ink = Mathf.Clamp(Ink, 0f, maxInk);
    }

    public bool HasEnoughInk(float amount)
    {
        return Ink >= amount;
    }

    public void UpdateScore(float amount)
    {
        if (amount > 0)
        {
            Score += amount;
            Debug.Log("Positive Score Updated: " + Score);
        }
        else
        {
            NegativeScore += Mathf.Abs(amount);
            Debug.Log("Negative Score Updated: " + NegativeScore);
        }
    }

    public void Restart()
    {
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.ReloadCurrentScene();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void BackToMain()
    {
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}