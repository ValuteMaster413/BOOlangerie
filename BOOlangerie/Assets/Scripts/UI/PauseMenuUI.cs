using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Pause Main Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Back Buttons")]
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button creditsBackButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public static bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        // Привязываем кнопки
        if (continueButton != null)
            continueButton.onClick.AddListener(ResumeGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(OpenCredits);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToMainMenu);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OpenPauseMenu);

        if (creditsBackButton != null)
            creditsBackButton.onClick.AddListener(OpenPauseMenu);
    }

    private void Start()
    {
        ResumeGame();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(ResumeGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (creditsButton != null) creditsButton.onClick.RemoveListener(OpenCredits);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitToMainMenu);

        if (settingsBackButton != null) settingsBackButton.onClick.RemoveListener(OpenPauseMenu);
        if (creditsBackButton != null) creditsBackButton.onClick.RemoveListener(OpenPauseMenu);
        
        Time.timeScale = 1f;
        IsPaused = false;
    }

    #region Pause Controls

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        
        OpenPauseMenu();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(creditsPanel, false);
    }

    #endregion

    #region Navigation

    public void OpenPauseMenu()
    {
        SetPanelActive(pauseMenuPanel, true);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(creditsPanel, false);
    }

    public void OpenSettings()
    {
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, true);
        SetPanelActive(creditsPanel, false);
    }

    public void OpenCredits()
    {
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(creditsPanel, true);
    }

    private void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    #endregion

    #region Helpers

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    #endregion
}