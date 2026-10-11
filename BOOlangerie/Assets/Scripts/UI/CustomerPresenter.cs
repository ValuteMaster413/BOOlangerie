using System.Collections;
using BOO.Core;
using BOO.Data;
using UnityEngine;
using UnityEngine.UI;

namespace BOO.UI
{
    public class CustomerPresenter : MonoBehaviour
    {
        [Header("Game")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private DialogueRunner runner;

        [Header("Counter view")]
        [SerializeField] private Image body;
        [SerializeField] private Image monster;
        [SerializeField] private Image darkness;
        [SerializeField] private GameObject lightsOn;
        [SerializeField] private GameObject lightsBroken;

        [Header("Tuning")]
        [SerializeField] private float fadeTime = 0.4f;
        [SerializeField] private float shiverAmount = 6f;
        [SerializeField] private float shiverSpeed = 45f;
        [SerializeField] private float hopHeight = 40f;
        [SerializeField] private float darkAlpha = 0.9f;
        [SerializeField] private int flickers = 4;

        private CustomerData shown;
        private Vector2 bodyHome;
        private bool shivering;
        private float hopOffset;
        private Coroutine swap;
        private Coroutine effect;

        private void Awake()
        {
            bodyHome = body.rectTransform.anchoredPosition;
            body.raycastTarget = false;
            monster.raycastTarget = false;
            darkness.raycastTarget = false;

            SetAlpha(body, 0f);
            ResetLights();
        }

        private void OnEnable()
        {
            gameManager.StateChanged += OnStateChanged;
            gameManager.OrderServed += OnOrderServed;
            runner.TagFired += OnTag;
        }

        private void OnDisable()
        {
            gameManager.StateChanged -= OnStateChanged;
            gameManager.OrderServed -= OnOrderServed;
            runner.TagFired -= OnTag;
        }

        private void Update()
        {
            float shake = shivering ? Mathf.Sin(Time.time * shiverSpeed) * shiverAmount : 0f;
            body.rectTransform.anchoredPosition = bodyHome + new Vector2(shake, hopOffset);
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.Ordering) return;

            CustomerData next = gameManager.CurrentCustomer;
            if (next == shown) return;

            shown = next;
            if (swap != null) StopCoroutine(swap);
            swap = StartCoroutine(SwapCustomer(next));
        }

        private void OnOrderServed(OrderResult result)
        {
            if (result == OrderResult.Good) Happy();
        }

        private void OnTag(string key, string value)
        {
            if (key != "anim") return;

            switch (value.ToLowerInvariant())
            {
                case "shiver":
                    shivering = true;
                    break;
                case "idle":
                    shivering = false;
                    break;
                case "happy":
                    Happy();
                    break;
                case "monster":
                    Monster();
                    break;
                default:
                    Debug.LogWarning($"[Customer] Unknown anim '{value}'.");
                    break;
            }
        }

        private IEnumerator SwapCustomer(CustomerData customer)
        {
            shivering = false;
            yield return FadeBody(0f);

            StopEffect();
            ResetLights();
            body.sprite = customer.sprite;
            shivering = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(customer.enterSfx);
                AudioManager.Instance.PlayMusic(customer.theme);
            }

            yield return FadeBody(1f);
        }

        private IEnumerator FadeBody(float target)
        {
            float start = body.color.a;
            if (Mathf.Approximately(start, target)) yield break;

            for (float t = 0f; t < fadeTime; t += Time.deltaTime)
            {
                SetAlpha(body, Mathf.Lerp(start, target, t / fadeTime));
                yield return null;
            }

            SetAlpha(body, target);
        }

        private void Happy()
        {
            shivering = false;
            StartEffect(Hop());
        }

        private IEnumerator Hop()
        {
            const float duration = 0.35f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                hopOffset = Mathf.Sin(t / duration * Mathf.PI) * hopHeight;
                yield return null;
            }

            hopOffset = 0f;
        }

        private void Monster()
        {
            shivering = false;
            StartEffect(LightsOut());
        }

        private IEnumerator LightsOut()
        {
            if (AudioManager.Instance != null && shown != null) AudioManager.Instance.PlaySFX(shown.monsterSfx);

            for (int i = 0; i < flickers; i++)
            {
                SetAlpha(darkness, darkAlpha);
                yield return new WaitForSeconds(0.07f);
                SetAlpha(darkness, 0f);
                yield return new WaitForSeconds(0.07f);
            }

            SetAlpha(darkness, darkAlpha);
            if (lightsOn != null) lightsOn.SetActive(false);
            if (lightsBroken != null) lightsBroken.SetActive(true);

            if (shown != null && shown.monsterSprite != null)
            {
                monster.sprite = shown.monsterSprite;
                monster.enabled = true;
            }
        }

        private void ResetLights()
        {
            SetAlpha(darkness, 0f);
            monster.enabled = false;
            if (lightsOn != null) lightsOn.SetActive(true);
            if (lightsBroken != null) lightsBroken.SetActive(false);
        }

        private void StartEffect(IEnumerator routine)
        {
            StopEffect();
            effect = StartCoroutine(routine);
        }

        private void StopEffect()
        {
            if (effect != null) StopCoroutine(effect);
            effect = null;
            hopOffset = 0f;
        }

        private static void SetAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}