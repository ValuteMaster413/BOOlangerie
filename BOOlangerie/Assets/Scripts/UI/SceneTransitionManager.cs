using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool fadeInOnStart;

    private const float MaxStep = 0.05f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetFade(fadeInOnStart ? 1f : 0f, false);
    }

    private void Start()
    {
        if (Instance == this && fadeInOnStart) StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneTransitionManager] Scene name is empty!");
            return;
        }

        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;

        yield return FadeOut();

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        if (load != null)
        {
            while (!load.isDone) yield return null;
            yield return null;
        }

        yield return FadeIn();

        isTransitioning = false;
    }

    public IEnumerator FadeOut()
    {
        yield return Fade(1f);
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(0f);
    }

    private IEnumerator Fade(float target)
    {
        if (fadeCanvasGroup == null) yield break;

        float start = fadeCanvasGroup.alpha;
        if (!Mathf.Approximately(start, target))
        {
            fadeCanvasGroup.blocksRaycasts = true;

            for (float t = 0f; t < fadeDuration; t += Mathf.Min(Time.unscaledDeltaTime, MaxStep))
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }
        }

        SetFade(target, target > 0f);
    }

    private void SetFade(float alpha, bool blockClicks)
    {
        if (fadeCanvasGroup == null) return;

        fadeCanvasGroup.alpha = alpha;
        fadeCanvasGroup.blocksRaycasts = blockClicks;
    }
}