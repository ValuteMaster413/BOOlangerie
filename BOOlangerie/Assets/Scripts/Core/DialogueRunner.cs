using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Choice = Ink.Runtime.Choice;
using InkFile = Ink.UnityIntegration.InkFile;
using Story = Ink.Runtime.Story;

namespace BOO.Core
{
    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager; // only assign in Bakery
        [SerializeField] private InkFile inkFile;
        [SerializeField] private string introKnot = "intro";
        [SerializeField] private string endingKnot = "game_end";
        [SerializeField] private bool playIntroOnStart;

        private Story story;
        private string speaker = "";
        private OrderResult lastResult;

        public bool IsPlaying { get; private set; }
        public bool HasChoices => story != null && story.currentChoices.Count > 0;
        public bool IsSkipping { get; private set; }

        public event Action<string, string> LineShown;
        public event Action ChoicesShown;
        public event Action<string, string> TagFired;
        public event Action Finished;

        private void Awake()
        {
            if (inkFile == null || !inkFile.isCompiled)
            {
                Debug.LogError("[Dialogue] No compiled ink file assigned.");
                return;
            }

            story = new Story(inkFile.storyJson);
            story.onError += (message, type) => Debug.LogWarning($"[Ink] {type}: {message}");
        }

        private void OnEnable()
        {
            if (gameManager == null) return;

            gameManager.StateChanged += OnStateChanged;
            gameManager.OrderServed += OnOrderServed;
        }

        private void OnDisable()
        {
            if (gameManager == null) return;

            gameManager.StateChanged -= OnStateChanged;
            gameManager.OrderServed -= OnOrderServed;
        }

        private void Start()
        {
            if (playIntroOnStart) Play(introKnot);
        }

        private void OnOrderServed(OrderResult result)
        {
            lastResult = result;
        }

        private void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Intro:
                    Play(introKnot);
                    break;
                case GameState.Ordering:
                    Play(gameManager.OrderKnot);
                    break;
                case GameState.Reaction:
                    Play(gameManager.ResultKnot(lastResult));
                    break;
                case GameState.Ending:
                    Play(endingKnot);
                    break;
            }
        }

        public void Play(string knot)
        {
            if (story == null || story.KnotContainerWithName(knot) == null)
            {
                Debug.LogWarning($"[Dialogue] Knot '{knot}' not found, skipping.");
                StartCoroutine(FinishNextFrame());
                return;
            }

            IsPlaying = true;
            story.ChoosePathString(knot);
            Advance();
        }

        public void Advance()
        {
            if (!IsPlaying) return;

            while (story.canContinue)
            {
                string text = story.Continue().Trim();
                speaker = ""; // narrator
                ReadTags();
                if (text.Length == 0) continue;

                Debug.Log($"[Dialogue] {speaker}: {text}");
                LineShown?.Invoke(speaker, text);
                return;
            }

            if (HasChoices) ChoicesShown?.Invoke();
            else Finish();
        }

        public void Skip()
        {
            if (!IsPlaying || HasChoices) return;

            string lastSpeaker = "";
            string lastText = null;
            bool stoppedOnOrder = false;

            IsSkipping = true;
            while (story.canContinue)
            {
                string text = story.Continue().Trim();
                speaker = ""; // narrator
                bool isOrder = ReadTags();
                if (text.Length == 0) continue;

                lastSpeaker = speaker;
                lastText = text;

                if (isOrder)
                {
                    stoppedOnOrder = true;
                    break;
                }
            }
            IsSkipping = false;

            if (!stoppedOnOrder && !HasChoices)
            {
                Finish();
                return;
            }

            if (lastText != null) LineShown?.Invoke(lastSpeaker, lastText);
            else ChoicesShown?.Invoke();
        }

        public List<string> GetChoices()
        {
            List<string> texts = new List<string>();
            if (story == null) return texts;

            foreach (Choice choice in story.currentChoices) texts.Add(choice.text);
            return texts;
        }

        public void Choose(int index)
        {
            if (!HasChoices) return;

            story.ChooseChoiceIndex(index);
            Advance();
        }

        private bool ReadTags()
        {
            bool isOrder = false;
            List<string> tags = story.currentTags;
            if (tags == null) return false;

            foreach (string tag in tags)
            {
                int colon = tag.IndexOf(':');
                string key = (colon < 0 ? tag : tag.Substring(0, colon)).Trim().ToLowerInvariant();
                string value = colon < 0 ? "" : tag.Substring(colon + 1).Trim();

                if (key == "speaker")
                {
                    speaker = value;
                    continue;
                }

                if (key == "sfx")
                {
                    if (!IsSkipping && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(value);
                    continue;
                }

                if (key == "order") isOrder = true;

                Debug.Log($"[Dialogue] tag {key} = {value}");
                TagFired?.Invoke(key, value);
            }

            return isOrder;
        }

        private void Finish()
        {
            IsPlaying = false;
            Finished?.Invoke();
            if (gameManager != null) gameManager.DialogueFinished();
        }

        private IEnumerator FinishNextFrame()
        {
            yield return null;
            Finish();
        }
    }
}