using System;
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
        [SerializeField] private GameObject[] lightsOn;
        [SerializeField] private GameObject[] lightsBroken;
        [SerializeField] private GameObject ambientLight;

        [Header("Tuning")]
        [SerializeField] private float shiverPixels = 2f;
        [SerializeField] private float shiverRate = 30f;
        [SerializeField] private float hopHeight = 40f;
        [SerializeField] private float darkAlpha = 0.9f;
        [SerializeField] private int flickers = 4;
        [SerializeField] private float flickerStep = 0.07f;

        private CustomerData current;
        private bool visible;
        private bool lit = true;
        private bool warm;
        private Vector2 bodyHome;
        private bool shivering;
        private float hopOffset;
        private Coroutine sequence;
        private Coroutine monsterLoop;
        private Coroutine hop;

        private void Awake()
        {
            bodyHome = body.rectTransform.anchoredPosition;
            body.raycastTarget = false;
            monster.raycastTarget = false;
            darkness.raycastTarget = false;

            if (ambientLight == null) ambientLight = FindByName(lightsOn, "AmbientLight");

            SetAlpha(body, 0f);
            monster.enabled = false;
            SetActive(lightsBroken, false);
            SetLit(true);
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
            float shake = shivering ? Mathf.Sin(Time.time * shiverRate) * shiverPixels : 0f;
            body.rectTransform.anchoredPosition = bodyHome + new Vector2(shake, hopOffset);
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Ordering)
            {
                if (gameManager.CurrentCustomer == current) return;

                SetWarm(false);
                if (visible) Exit();
                return;
            }

            if (state == GameState.Prep) Enter();
        }

        private void OnOrderServed(OrderResult result)
        {
            SetWarm(result == OrderResult.Good);
        }

        private void SetWarm(bool value)
        {
            warm = value;
            if (ambientLight != null) ambientLight.SetActive(lit && warm);
        }

        private void OnTag(string key, string value)
        {
            switch (key)
            {
                case "voice":
                    PlayVoice();
                    break;
                case "music":
                    PlayMusicCue(value.ToLowerInvariant());
                    break;
                case "anim":
                    PlayAnimCue(value.ToLowerInvariant());
                    break;
            }
        }

        private void PlayAnimCue(string cue)
        {
            switch (cue)
            {
                case "enter":
                    Enter();
                    break;
                case "exit":
                    Exit();
                    break;
                case "success":
                case "happy":
                    Success();
                    break;
                case "fail":
                case "monster":
                    Fail();
                    break;
                case "shiver":
                    shivering = true;
                    break;
                case "idle":
                    shivering = false;
                    break;
                default:
                    Debug.LogWarning($"[Customer] Unknown anim '{cue}'.");
                    break;
            }
        }

        private void PlayMusicCue(string cue)
        {
            if (AudioManager.Instance == null) return;

            if (cue == "stop") AudioManager.Instance.StopMusic();
            else if (cue == "theme") AudioManager.Instance.PlayMusic(gameManager.CurrentCustomer.theme);
            else Debug.LogWarning($"[Customer] Unknown music cue '{cue}'.");
        }

        private void PlayVoice()
        {
            if (runner.IsSkipping || AudioManager.Instance == null) return;

            AudioManager.Instance.PlaySFX(gameManager.CurrentCustomer.voiceSfx);
        }

        private void Enter()
        {
            CustomerData customer = gameManager.CurrentCustomer;
            if (visible && customer == current) return;

            current = customer;
            StartSequence(EnterRoutine(customer));
        }

        private void Exit()
        {
            if (!visible) return;

            StartSequence(ExitRoutine());
        }

        private void Success()
        {
            if (!visible) return;

            shivering = false;
            if (hop != null) StopCoroutine(hop);
            hop = StartCoroutine(Hop());

            StartSequence(SuccessRoutine(current));
        }

        private void Fail()
        {
            if (!visible) return;

            shivering = false;
            StartSequence(current.lightsOutOnFail ? LightsOutRoutine(current) : FailRoutine(current));
        }

        private IEnumerator EnterRoutine(CustomerData customer)
        {
            visible = true;
            shivering = false;
            StopMonster();
            SetAlpha(body, 0f);
            SetActive(lightsBroken, false);

            if (customer.flickerOnEnter)
            {
                yield return Flicker(() => ShowBody(IdleStart(customer)), true);
            }
            else
            {
                SetLit(true);
                ShowBody(IdleStart(customer));
            }

            yield return PlayFrames(body, customer.entranceFrames, customer.framesPerSecond, false);
            shivering = true;
        }

        private IEnumerator ExitRoutine()
        {
            visible = false;
            shivering = false;

            yield return Flicker(HideCustomer, true);
            SetActive(lightsBroken, false);
        }

        private IEnumerator SuccessRoutine(CustomerData customer)
        {
            if (!HasFrames(customer.successFrames)) yield break;

            yield return PlayFrames(body, customer.successFrames, customer.framesPerSecond, false);
        }

        private IEnumerator FailRoutine(CustomerData customer)
        {
            if (!HasFrames(customer.failFrames)) yield break;

            yield return PlayFrames(body, customer.failFrames, customer.framesPerSecond, false);
        }

        private IEnumerator LightsOutRoutine(CustomerData customer)
        {
            yield return Flicker(null, false);
            SetActive(lightsBroken, true);

            if (!HasFrames(customer.failFrames)) yield break;

            SetAlpha(body, 0f);
            monster.enabled = true;
            monsterLoop = StartCoroutine(PlayFrames(monster, customer.failFrames, customer.framesPerSecond, true));
        }

        private IEnumerator Flicker(Action atLastDark, bool endLit)
        {
            int count = Mathf.Max(1, flickers);

            for (int i = 0; i < count; i++)
            {
                bool last = i == count - 1;

                SetLit(false);
                if (last && atLastDark != null) atLastDark();
                yield return new WaitForSeconds(flickerStep);

                if (last && !endLit) break;

                SetLit(true);
                yield return new WaitForSeconds(flickerStep);
            }
        }

        private IEnumerator PlayFrames(Image target, Sprite[] frames, float framesPerSecond, bool loop)
        {
            if (!HasFrames(frames)) yield break;

            WaitForSeconds wait = new WaitForSeconds(1f / Mathf.Max(framesPerSecond, 1f));

            do
            {
                foreach (Sprite frame in frames)
                {
                    target.sprite = frame;
                    yield return wait;
                }
            }
            while (loop);
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

        private static Sprite IdleStart(CustomerData customer)
        {
            return HasFrames(customer.entranceFrames) ? customer.entranceFrames[0] : customer.sprite;
        }

        private void ShowBody(Sprite sprite)
        {
            body.sprite = sprite;
            SetAlpha(body, sprite != null ? 1f : 0f);
        }

        private void HideCustomer()
        {
            SetAlpha(body, 0f);
            StopMonster();
        }

        private void StopMonster()
        {
            if (monsterLoop != null) StopCoroutine(monsterLoop);
            monsterLoop = null;
            monster.enabled = false;
        }

        private void SetLit(bool value)
        {
            lit = value;
            SetActive(lightsOn, lit);
            if (ambientLight != null) ambientLight.SetActive(lit && warm);
            SetAlpha(darkness, lit ? 0f : darkAlpha);
        }

        private static GameObject FindByName(GameObject[] objects, string objectName)
        {
            if (objects == null) return null;

            foreach (GameObject item in objects)
            {
                if (item != null && item.name == objectName) return item;
            }

            return null;
        }

        private void StartSequence(IEnumerator routine)
        {
            if (sequence != null) StopCoroutine(sequence);
            sequence = StartCoroutine(routine);
        }

        private static bool HasFrames(Sprite[] frames)
        {
            return frames != null && frames.Length > 0;
        }

        private static void SetActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;

            foreach (GameObject item in objects)
            {
                if (item != null) item.SetActive(active);
            }
        }

        private static void SetAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
