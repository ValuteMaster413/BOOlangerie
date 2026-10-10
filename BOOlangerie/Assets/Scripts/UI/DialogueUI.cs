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
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Typewriter")]
        [SerializeField] private float slowest = 20f;
        [SerializeField] private float fastest = 90f;

        private Coroutine typing;

        private void Awake()
        {
            advanceButton.onClick.AddListener(OnAdvanceClicked);
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            runner.LineShown += ShowLine;
            runner.ChoicesShown += ShowChoices;
            runner.Finished += Hide;
        }

        private void OnDisable()
        {
            runner.LineShown -= ShowLine;
            runner.ChoicesShown -= ShowChoices;
            runner.Finished -= Hide;
        }

        private void ShowLine(string speaker, string text)
        {
            panel.SetActive(true);
            ClearChoices();

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
        }

        private void OnAdvanceClicked()
        {
            if (PauseMenuUI.IsPaused) return;

            if (typing != null)
            {
                StopCoroutine(typing);
                FinishLine();
                return;
            }

            if (runner.HasChoices) return;
            runner.Advance();
        }

        private void ShowChoices()
        {
            ClearChoices();
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
            ClearChoices();
            runner.Choose(index);
        }

        private void ClearChoices()
        {
            foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        }

        private void Hide()
        {
            ClearChoices();
            panel.SetActive(false);
        }
    }
}