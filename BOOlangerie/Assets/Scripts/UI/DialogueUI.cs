using System.Collections;
using System.Collections.Generic;
using BOO.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BOO.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject nameplate;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button advanceButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button autoButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsUI settings;
        [SerializeField] private GameObject choiceOverlay;
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private AudioClip advanceSfx;

        [Header("Typewriter")]
        [SerializeField] private float slowest = 20f;
        [SerializeField] private float fastest = 90f;

        [Header("Auto")]
        [SerializeField] private float autoDelay = 1.2f;
        [SerializeField] private float autoDelayPerCharacter = 0.03f;

        private static bool autoOn;

        private Coroutine typing;
        private Coroutine autoWait;

        private bool SettingsOpen => settings != null && settings.gameObject.activeSelf;

        private void Awake()
        {
            advanceButton.onClick.AddListener(OnAdvanceClicked);
            if (skipButton != null) skipButton.onClick.AddListener(OnSkipClicked);
            if (autoButton != null) autoButton.onClick.AddListener(OnAutoClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);

            if (settings != null) settings.gameObject.SetActive(false);
            if (choiceOverlay != null) choiceOverlay.SetActive(false);

            RefreshAutoButton();
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            runner.LineShown += ShowLine;
            runner.ChoicesShown += ShowChoices;
            runner.Finished += Hide;
            if (settings != null) settings.BackClicked += CloseSettings;
        }

        private void OnDisable()
        {
            runner.LineShown -= ShowLine;
            runner.ChoicesShown -= ShowChoices;
            runner.Finished -= Hide;
            if (settings != null) settings.BackClicked -= CloseSettings;
        }

        private void ShowLine(string speaker, string text)
        {
            panel.SetActive(true);
            ClearChoices();
            StopAutoWait();

            nameplate.SetActive(speaker.Length > 0);
            nameText.text = speaker;

            if (typing != null) StopCoroutine(typing);
            typing = StartCoroutine(TypeLine(text));
        }

        private IEnumerator TypeLine(string text)
        {
            bodyText.text = text;
            bodyText.maxVisibleCharacters = 0;
            bodyText.ForceMeshUpdate();

            int total = bodyText.textInfo.characterCount;
            float speed = Mathf.Lerp(slowest, fastest, SettingsUI.GetTextSpeed());
            float shown = 0f;

            while (shown < total)
            {
                shown += speed * Time.deltaTime;
                bodyText.maxVisibleCharacters = Mathf.FloorToInt(shown);
                yield return null;
            }

            FinishLine();
        }

        private void FinishLine()
        {
            typing = null;
            bodyText.maxVisibleCharacters = 99999;

            if (runner.HasChoices) ShowChoices();
            else StartAutoWait();
        }

        private bool CompleteLine()
        {
            if (typing == null) return false;

            StopCoroutine(typing);
            FinishLine();
            return true;
        }

        private void OnAdvanceClicked()
        {
            if (PauseMenuUI.IsPaused || SettingsOpen) return;

            if (CompleteLine()) return;
            if (runner.HasChoices) return;

            StopAutoWait();
            PlayAdvanceSfx();
            runner.Advance();
        }

        private void OnSkipClicked()
        {
            if (PauseMenuUI.IsPaused || SettingsOpen) return;

            StopAutoWait();
            CompleteLine();
            if (runner.HasChoices) return;

            runner.Skip();
            CompleteLine();
        }

        private void OnAutoClicked()
        {
            autoOn = !autoOn;
            RefreshAutoButton();

            if (autoOn && typing == null) StartAutoWait();
            else if (!autoOn) StopAutoWait();
        }

        private void RefreshAutoButton()
        {
            if (autoButton == null) return;

            TMP_Text label = autoButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.fontStyle = autoOn ? FontStyles.Underline : FontStyles.Normal;
        }

        private void OnSettingsClicked()
        {
            if (PauseMenuUI.IsPaused || settings == null) return;

            StopAutoWait();
            settings.gameObject.SetActive(true);
        }

        private void CloseSettings()
        {
            settings.gameObject.SetActive(false);
            if (typing == null) StartAutoWait();
        }

        private void StartAutoWait()
        {
            StopAutoWait();
            if (!autoOn || SettingsOpen || !panel.activeSelf || !runner.IsPlaying || runner.HasChoices) return;

            autoWait = StartCoroutine(AutoAdvance());
        }

        private IEnumerator AutoAdvance()
        {
            float wait = autoDelay + autoDelayPerCharacter * bodyText.textInfo.characterCount;
            yield return new WaitForSeconds(wait);

            autoWait = null;
            runner.Advance();
        }

        private void StopAutoWait()
        {
            if (autoWait != null) StopCoroutine(autoWait);
            autoWait = null;
        }

        private void ShowChoices()
        {
            ClearChoices();
            if (choiceOverlay != null) choiceOverlay.SetActive(true);

            List<string> choices = runner.GetChoices();

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                Button button = Instantiate(choiceButtonPrefab, choiceContainer);
                button.GetComponentInChildren<TMP_Text>().text = choices[i];
                button.onClick.AddListener(() => OnChoiceClicked(index));
            }
        }

        private void OnChoiceClicked(int index)
        {
            PlayAdvanceSfx();
            ClearChoices();
            runner.Choose(index);
        }

        private void PlayAdvanceSfx()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(advanceSfx);
        }

        private void ClearChoices()
        {
            foreach (Transform child in choiceContainer) Destroy(child.gameObject);
            if (choiceOverlay != null) choiceOverlay.SetActive(false);
        }

        private void Hide()
        {
            StopAutoWait();
            ClearChoices();
            panel.SetActive(false);
        }
    }
}