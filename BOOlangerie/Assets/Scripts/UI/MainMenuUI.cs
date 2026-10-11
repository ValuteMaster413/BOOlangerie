using System;
using UnityEngine;
using UnityEngine.UI;

namespace BOO.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button quitButton;

        public event Action PlayClicked;
        public event Action SettingsClicked;
        public event Action CreditsClicked;
        public event Action QuitClicked;

        private void Awake()
        {
#if UNITY_WEBGL
            if (quitButton != null) quitButton.gameObject.SetActive(false);
#endif
        }

        public void OnClickPlayButton()
        {
            PlayClicked?.Invoke();
        }

        public void OnClickSettingsButton()
        {
            SettingsClicked?.Invoke();
        }

        public void OnClickCreditsButton()
        {
            CreditsClicked?.Invoke();
        }

        public void OnClickQuitButton()
        {
            QuitClicked?.Invoke();
        }
    }
}