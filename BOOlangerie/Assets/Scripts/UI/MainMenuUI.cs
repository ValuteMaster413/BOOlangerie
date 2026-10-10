using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BOO.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Back Buttons")]
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button creditsBackButton;

        [Header("Scene Transition")]
        [SerializeField] private string targetSceneName = "";

        private void Awake()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (creditsButton != null)
                creditsButton.onClick.AddListener(OpenCredits);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);

            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(OpenMainMenu);

            if (creditsBackButton != null)
                creditsBackButton.onClick.AddListener(OpenMainMenu);
        }

        private void Start()
        {
#if UNITY_WEBGL
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);
        }
#endif

            OpenMainMenu();
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayButtonClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
            if (creditsButton != null) creditsButton.onClick.RemoveListener(OpenCredits);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitButtonClicked);

            if (settingsBackButton != null) settingsBackButton.onClick.RemoveListener(OpenMainMenu);
            if (creditsBackButton != null) creditsBackButton.onClick.RemoveListener(OpenMainMenu);
        }

        #region Navigation Methods

        public void OpenMainMenu()
        {
            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(creditsPanel, false);
        }

        public void OpenSettings()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(settingsPanel, true);
            SetPanelActive(creditsPanel, false);
        }

        public void OpenCredits()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(creditsPanel, true);
        }

        #endregion

        #region Button Handlers

        private void OnPlayButtonClicked()
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(targetSceneName);
                }
                else
                {
                    SceneManager.LoadScene(targetSceneName);
                }
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] Target scene name is empty! Please set it in the Inspector when the scene is ready.");
            }
        }

        private void OnQuitButtonClicked()
        {
            Debug.Log("[MainMenuUI] Quitting game...");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
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
}