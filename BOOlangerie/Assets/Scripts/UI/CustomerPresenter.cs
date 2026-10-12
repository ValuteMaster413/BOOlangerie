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
        [SerializeField] private GameObject fairyLight;
        [SerializeField] private Image sky;

        [Header("Service")]
        [SerializeField] private DishVisualizer servedDish;
        [SerializeField] private Color blackoutDishTint = new Color(0.4f, 0.4f, 0.4f);
        [SerializeField] private float dishFadeSeconds = 0.5f;
        [SerializeField] private Button recallButton;
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private float hoverHop = 14f;

        [Header("Night effects")]
        [SerializeField] private Image[] jarLayers;
        [SerializeField] private Sprite jarGoneSprite;

        [Header("Ending")]
        [SerializeField] private bool dawnAtEnding = true;
        [SerializeField] private Color dawnSkyColor = new Color(0.671f, 0.494f, 0.365f);
        [SerializeField] private float dawnSeconds = 3f;

        [Header("Tuning")]
        [SerializeField] private float shiverPixels = 2f;
        [SerializeField] private float shiverRate = 30f;
        [SerializeField] private float hopHeight = 40f;
        [SerializeField] private float darkAlpha = 0.9f;
        [SerializeField] private int flickers = 4;
        [SerializeField] private float flickerStep = 0.07f;

        private CustomerData current;
        private bool visible;
        private const float BlackoutFlickerAlpha = 0.95f;

        private bool lit = true;
        private bool warm;
        private bool blackout;
        private Vector2 bodyHome;
        private bool shivering;
        private float hopOffset;
        private Coroutine sequence;
        private Action pendingFinish;
        private Coroutine monsterLoop;
        private Coroutine hop;
        private Coroutine dishFade;
        private PointerHover recallHover;
        private bool wasHovered;
        private Color dishTint = Color.white;

        private void Awake()
        {
            bodyHome = body.rectTransform.anchoredPosition;
            body.raycastTarget = false;
            monster.raycastTarget = false;
            darkness.raycastTarget = false;

            if (ambientLight == null) ambientLight = FindByName(lightsOn, "AmbientLight");
            if (fairyLight == null) fairyLight = FindSibling(darkness.transform, "FairyFire");

            GameObject skyObject = sky == null ? FindSibling(darkness.transform, "NightSky") : null;
            if (skyObject != null) sky = skyObject.GetComponent<Image>();

            SetAlpha(body, 0f);
            monster.enabled = false;
            SetActive(lightsBroken, false);
            SetLit(true);

            ClearServedDish();

            if (recallButton != null)
            {
                recallButton.onClick.AddListener(RecallOrder);
                recallHover = recallButton.GetComponent<PointerHover>();
                if (recallHover == null) recallHover = recallButton.gameObject.AddComponent<PointerHover>();
                recallButton.gameObject.SetActive(false);
            }
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

            UpdateHover();
        }

        private void UpdateHover()
        {
            bool hovered = recallHover != null && recallHover.IsHovered && visible
                && gameManager.State == GameState.Prep && !runner.IsPlaying;

            if (hovered && !wasHovered) StartHop(hoverHop);
            wasHovered = hovered;

            float scale = Mathf.MoveTowards(body.rectTransform.localScale.x, hovered ? hoverScale : 1f, Time.deltaTime * 0.4f);
            body.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void StartHop(float height)
        {
            if (hop != null) StopCoroutine(hop);
            hop = StartCoroutine(Hop(height));
        }

        private void OnStateChanged(GameState state)
        {
            if (recallButton != null) recallButton.gameObject.SetActive(state == GameState.Prep);
            if (state == GameState.Ordering || state == GameState.Ending) ClearServedDish();

            if (state == GameState.Ordering)
            {
                if (gameManager.CurrentCustomer == current) return;

                SetWarm(false);
                if (visible) Exit();
                return;
            }

            if (state == GameState.Ending)
            {
                SetWarm(false);
                if (visible) Exit();
                if (dawnAtEnding && sky != null) StartCoroutine(Dawn());
                return;
            }

            if (state == GameState.Prep) Enter();
        }

        private void OnOrderServed(OrderResult result)
        {
            SetWarm(result == OrderResult.Good);

            if (servedDish == null) return;

            StopDishFade();
            dishTint = blackout ? blackoutDishTint : Color.white;
            servedDish.SetTint(dishTint);
            servedDish.DisplayDish(gameManager.LastServed);
        }

        private void ClearServedDish()
        {
            StopDishFade();
            if (servedDish != null) servedDish.ClearVisuals();
        }

        private void FadeOutServedDish()
        {
            if (servedDish == null) return;

            StopDishFade();

            if (runner.IsSkipping) servedDish.ClearVisuals();
            else dishFade = StartCoroutine(FadeDish());
        }

        private void StopDishFade()
        {
            if (dishFade != null) StopCoroutine(dishFade);
            dishFade = null;
        }

        private IEnumerator FadeDish()
        {
            Color tint = dishTint;

            for (float t = 0f; t < dishFadeSeconds; t += Time.deltaTime)
            {
                tint.a = 1f - t / dishFadeSeconds;
                servedDish.SetTint(tint);
                yield return null;
            }

            dishFade = null;
            servedDish.ClearVisuals();
        }

        private void RecallOrder()
        {
            if (gameManager.State != GameState.Prep || !runner.CanRecall) return;

            runner.Recall();
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
                    PlayVoice(value);
                    break;
                case "music":
                    PlayMusicCue(value.ToLowerInvariant());
                    break;
                case "anim":
                    PlayAnimCue(value.ToLowerInvariant());
                    break;
                case "jar":
                    TakeJar();
                    break;
                case "food":
                    FadeOutServedDish();
                    break;
            }
        }

        private void TakeJar()
        {
            if (jarGoneSprite == null || jarLayers == null) return;

            foreach (Image layer in jarLayers)
            {
                if (layer != null) layer.sprite = jarGoneSprite;
            }
        }

        private IEnumerator Dawn()
        {
            Color night = sky.color;

            for (float t = 0f; t < dawnSeconds; t += Time.deltaTime)
            {
                sky.color = Color.Lerp(night, dawnSkyColor, t / dawnSeconds);
                yield return null;
            }

            sky.color = dawnSkyColor;
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

            CustomerData customer = gameManager.CurrentCustomer;

            if (cue == "stop") AudioManager.Instance.StopMusic();
            else if (cue == "theme" && customer != null) AudioManager.Instance.PlayMusic(customer.theme);
            else if (cue != "theme") Debug.LogWarning($"[Customer] Unknown music cue '{cue}'.");
        }

        private void PlayVoice(string clipName)
        {
            if (runner.IsSkipping || AudioManager.Instance == null) return;

            if (clipName.Length > 0)
            {
                AudioManager.Instance.PlayVoice(clipName);
                return;
            }

            CustomerData customer = gameManager.CurrentCustomer;
            if (customer != null) AudioManager.Instance.PlayVoice(customer.voiceSfx);
        }

        private void Enter()
        {
            CustomerData customer = gameManager.CurrentCustomer;
            if (customer == null || (visible && customer == current)) return;

            CompleteSequence();

            current = customer;
            visible = true;
            shivering = false;
            StopMonster();
            SetAlpha(body, 0f);
            SetActive(lightsBroken, false);

            StartSequence(EnterRoutine(customer), () => FinishEnter(customer));
        }

        private void Exit()
        {
            if (!visible) return;

            CompleteSequence();

            visible = false;
            shivering = false;

            StartSequence(Flicker(HideCustomer, true), FinishExit);
        }

        private void Success()
        {
            if (!visible) return;

            CompleteSequence();

            CustomerData customer = current;
            shivering = false;
            StartHop(hopHeight);

            StartSequence(
                PlayFrames(body, customer.successFrames, customer.framesPerSecond, false),
                () => HoldLastFrame(customer.successFrames));
        }

        private void Fail()
        {
            if (!visible) return;

            CompleteSequence();

            CustomerData customer = current;
            shivering = false;

            if (customer.lightsOutOnFail)
            {
                StartSequence(Flicker(null, false), () => FinishLightsOut(customer));
            }
            else
            {
                StartSequence(
                    PlayFrames(body, customer.failFrames, customer.framesPerSecond, false),
                    () => HoldLastFrame(customer.failFrames));
            }
        }

        private IEnumerator EnterRoutine(CustomerData customer)
        {
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
        }

        private void FinishEnter(CustomerData customer)
        {
            SetLit(true);
            ShowBody(IdleEnd(customer));
            shivering = true;
        }

        private void FinishExit()
        {
            HideCustomer();
            SetActive(lightsBroken, false);
            SetLit(true);
        }

        private void FinishLightsOut(CustomerData customer)
        {
            blackout = true;
            SetLit(true);
            SetActive(lightsBroken, true);

            if (!HasFrames(customer.failFrames)) return;

            StopMonster();
            SetAlpha(body, 0f);
            monster.enabled = true;
            monsterLoop = StartCoroutine(PlayFrames(monster, customer.failFrames, customer.framesPerSecond, true));
        }

        private void HoldLastFrame(Sprite[] frames)
        {
            if (HasFrames(frames)) body.sprite = frames[frames.Length - 1];
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

        private IEnumerator Hop(float height)
        {
            const float duration = 0.35f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                hopOffset = Mathf.Sin(t / duration * Mathf.PI) * height;
                yield return null;
            }

            hopOffset = 0f;
        }

        private static Sprite IdleStart(CustomerData customer)
        {
            return HasFrames(customer.entranceFrames) ? customer.entranceFrames[0] : customer.sprite;
        }

        private static Sprite IdleEnd(CustomerData customer)
        {
            Sprite[] frames = customer.entranceFrames;

            return HasFrames(frames) ? frames[frames.Length - 1] : customer.sprite;
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

            if (blackout)
            {
                SetActive(lightsOn, false);
                if (fairyLight != null) fairyLight.SetActive(lit);
                SetAlpha(darkness, lit ? darkAlpha : BlackoutFlickerAlpha);
            }
            else
            {
                SetActive(lightsOn, lit);
                if (fairyLight != null) fairyLight.SetActive(false);
                SetAlpha(darkness, lit ? 0f : darkAlpha);
            }

            if (ambientLight != null) ambientLight.SetActive(lit && warm);
        }

        private static GameObject FindSibling(Transform of, string objectName)
        {
            Transform parent = of.parent;
            Transform found = parent != null ? parent.Find(objectName) : null;

            return found != null ? found.gameObject : null;
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

        private void StartSequence(IEnumerator routine, Action finish)
        {
            pendingFinish = finish;
            sequence = StartCoroutine(RunSequence(routine));
        }

        private IEnumerator RunSequence(IEnumerator routine)
        {
            yield return routine;

            sequence = null;
            CompleteSequence();
        }

        private void CompleteSequence()
        {
            if (sequence != null) StopCoroutine(sequence);
            sequence = null;

            if (pendingFinish == null) return;

            Action finish = pendingFinish;
            pendingFinish = null;
            finish();
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
