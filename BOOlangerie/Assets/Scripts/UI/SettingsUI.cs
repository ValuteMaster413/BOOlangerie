using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BOO.Core;

namespace BOO.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI Controls")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider textSpeedSlider;

        [Header("Value Texts (Optional)")]
        [SerializeField] private TextMeshProUGUI musicValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;
        [SerializeField] private TextMeshProUGUI textSpeedValueText;

        private const string TEXT_SPEED_KEY = "TextSpeed";

        public event Action BackClicked;

        private void OnEnable()
        {
            InitializeSliders();
        }

        public void OnClickBackButton()
        {
            BackClicked?.Invoke();
        }

        public void OnMusicSliderChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
            UpdateText(musicValueText, value);
        }

        public void OnSFXSliderChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
            UpdateText(sfxValueText, value);
        }

        public void OnTextSpeedSliderChanged(float value)
        {
            PlayerPrefs.SetFloat(TEXT_SPEED_KEY, value);
            PlayerPrefs.Save();
            UpdateText(textSpeedValueText, value);
        }

        private void InitializeSliders()
        {
            float currentMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            float currentSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            float currentTextSpeed = PlayerPrefs.GetFloat(TEXT_SPEED_KEY, 1.0f);

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(currentMusic);
                UpdateText(musicValueText, currentMusic);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(currentSFX);
                UpdateText(sfxValueText, currentSFX);
            }

            if (textSpeedSlider != null)
            {
                textSpeedSlider.SetValueWithoutNotify(currentTextSpeed);
                UpdateText(textSpeedValueText, currentTextSpeed);
            }
        }

        private void UpdateText(TextMeshProUGUI textComp, float value)
        {
            if (textComp != null)
            {
                textComp.text = Mathf.RoundToInt(value * 100f) + "%";
            }
        }

        public static float GetTextSpeed()
        {
            return PlayerPrefs.GetFloat(TEXT_SPEED_KEY, 1.0f);
        }
    }
}