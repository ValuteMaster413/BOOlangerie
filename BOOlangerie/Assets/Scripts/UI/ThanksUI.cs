using System;
using System.Collections;
using UnityEngine;

namespace BOO.UI
{
    public class ThanksUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float fadeSeconds = 1.5f;
        [SerializeField] private float holdSeconds = 3f;

        private void Awake()
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }

        public void Play(Action finished)
        {
            gameObject.SetActive(true);
            StartCoroutine(PlayRoutine(finished));
        }

        private IEnumerator PlayRoutine(Action finished)
        {
            group.blocksRaycasts = true;

            for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                group.alpha = t / fadeSeconds;
                yield return null;
            }

            group.alpha = 1f;
            yield return new WaitForSecondsRealtime(holdSeconds);

            finished?.Invoke();
        }
    }
}
