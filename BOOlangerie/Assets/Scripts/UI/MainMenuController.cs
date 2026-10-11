using UnityEngine;
using UnityEngine.SceneManagement;

namespace BOO.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuUI mainMenu;
        [SerializeField] private SettingsUI settings;
        [SerializeField] private CreditsUI credits;
        [SerializeField] private string targetSceneName = "Bakery";

        private void OnEnable()
        {
            mainMenu.PlayClicked += Play;
            mainMenu.SettingsClicked += ShowSettings;
            mainMenu.CreditsClicked += ShowCredits;
            mainMenu.QuitClicked += Quit;
            settings.BackClicked += ShowMainMenu;
            credits.BackClicked += ShowMainMenu;
        }

        private void OnDisable()
        {
            mainMenu.PlayClicked -= Play;
            mainMenu.SettingsClicked -= ShowSettings;
            mainMenu.CreditsClicked -= ShowCredits;
            mainMenu.QuitClicked -= Quit;
            settings.BackClicked -= ShowMainMenu;
            credits.BackClicked -= ShowMainMenu;
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            Show(mainMenu.gameObject);
        }

        private void ShowSettings()
        {
            Show(settings.gameObject);
        }

        private void ShowCredits()
        {
            Show(credits.gameObject);
        }

        private void Show(GameObject panel)
        {
            mainMenu.gameObject.SetActive(panel == mainMenu.gameObject);
            settings.gameObject.SetActive(panel == settings.gameObject);
            credits.gameObject.SetActive(panel == credits.gameObject);
        }

        private void Play()
        {
            if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene(targetSceneName);
            else SceneManager.LoadScene(targetSceneName);
        }

        private void Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}