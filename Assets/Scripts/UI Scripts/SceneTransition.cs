using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private Image transitionBar;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.8f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float delayBeforeReveal = 0.15f;

    [Header("Bar Style")]
    [SerializeField] private TransitionStyle style = TransitionStyle.SlideRight;
    [SerializeField] private Color barColor = Color.black;

    public enum TransitionStyle
    {
        SlideRight,
        SlideLeft,
        SlideDown,
        SlideUp,
        DoubleWipeH,
        DoubleWipeV
    }

    private RectTransform barRect;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (transitionBar != null)
        {
            barRect = transitionBar.GetComponent<RectTransform>();
            transitionBar.color = barColor;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void LoadScene(int sceneIndex)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToSceneAsync(sceneIndex));
        }
    }

    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToSceneAsync(sceneName));
        }
    }

    public void ReloadCurrentScene()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToSceneAsync(SceneManager.GetActiveScene().buildIndex));
        }
    }

    private IEnumerator TransitionToSceneAsync(int sceneIndex)
    {
        isTransitioning = true;

        yield return StartCoroutine(AnimateTransition(true));

        // Load scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        // Wait until scene is almost loaded (90%)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(delayBeforeReveal);

        yield return StartCoroutine(AnimateTransition(false));

        isTransitioning = false;
    }

    private IEnumerator TransitionToSceneAsync(string sceneName)
    {
        isTransitioning = true;

        yield return StartCoroutine(AnimateTransition(true));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(delayBeforeReveal);
        yield return StartCoroutine(AnimateTransition(false));

        isTransitioning = false;
    }

    private IEnumerator AnimateTransition(bool isClosing)
    {
        if (canvasGroup == null || barRect == null) yield break;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        switch (style)
        {
            case TransitionStyle.SlideRight:
                barRect.anchorMin = new Vector2(0, 0);
                barRect.anchorMax = new Vector2(0, 1);
                barRect.pivot = new Vector2(0, 0.5f);
                barRect.sizeDelta = new Vector2(screenWidth, screenHeight);
                startPos = isClosing ? new Vector2(-screenWidth, 0) : new Vector2(0, 0);
                endPos = isClosing ? new Vector2(0, 0) : new Vector2(screenWidth, 0);
                break;

            case TransitionStyle.SlideLeft:
                barRect.anchorMin = new Vector2(1, 0);
                barRect.anchorMax = new Vector2(1, 1);
                barRect.pivot = new Vector2(1, 0.5f);
                barRect.sizeDelta = new Vector2(screenWidth, screenHeight);
                startPos = isClosing ? new Vector2(screenWidth, 0) : new Vector2(0, 0);
                endPos = isClosing ? new Vector2(0, 0) : new Vector2(-screenWidth, 0);
                break;

            case TransitionStyle.SlideDown:
                barRect.anchorMin = new Vector2(0, 1);
                barRect.anchorMax = new Vector2(1, 1);
                barRect.pivot = new Vector2(0.5f, 1);
                barRect.sizeDelta = new Vector2(screenWidth, screenHeight);
                startPos = isClosing ? new Vector2(0, screenHeight) : new Vector2(0, 0);
                endPos = isClosing ? new Vector2(0, 0) : new Vector2(0, -screenHeight);
                break;

            case TransitionStyle.SlideUp:
                barRect.anchorMin = new Vector2(0, 0);
                barRect.anchorMax = new Vector2(1, 0);
                barRect.pivot = new Vector2(0.5f, 0);
                barRect.sizeDelta = new Vector2(screenWidth, screenHeight);
                startPos = isClosing ? new Vector2(0, -screenHeight) : new Vector2(0, 0);
                endPos = isClosing ? new Vector2(0, 0) : new Vector2(0, screenHeight);
                break;
        }

        barRect.anchoredPosition = startPos;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float easedT = easeCurve.Evaluate(t);

            barRect.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);

            yield return null;
        }

        // final position is exact
        barRect.anchoredPosition = endPos;

        yield return null;

        if (!isClosing)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}