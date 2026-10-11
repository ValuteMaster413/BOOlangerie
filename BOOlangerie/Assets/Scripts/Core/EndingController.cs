using UnityEngine;
using UnityEngine.SceneManagement;

namespace BOO.Core
{
    public class EndingController : MonoBehaviour
    {
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private string menuScene = "MainMenu";

        private void OnEnable()
        {
            runner.Finished += ReturnToMenu;
        }

        private void OnDisable()
        {
            runner.Finished -= ReturnToMenu;
        }

        private void ReturnToMenu()
        {
            if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene(menuScene);
            else SceneManager.LoadScene(menuScene);
        }
    }
}