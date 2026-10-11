using System.Collections;
using UnityEngine;

namespace BOO.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music")]
        [SerializeField] private float musicFadeTime = 0.6f;

        [Header("UI Sounds")]
        [SerializeField] private AudioClip[] clickSfx;
        [SerializeField] private AudioClip[] hoverSfx;

        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SFXVolume";

        private AudioClip currentMusic;
        private float musicFade = 1f;
        private Coroutine musicSwap;

        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadVolumeSettings();
        }

        private void LoadVolumeSettings()
        {
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
            SFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);

            ApplyMusicVolume();
            ApplySFXVolume();
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
            ApplyMusicVolume();
            PlayerPrefs.SetFloat(MUSIC_KEY, volume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = volume;
            ApplySFXVolume();
            PlayerPrefs.SetFloat(SFX_KEY, volume);
            PlayerPrefs.Save();
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || clip == currentMusic || musicSource == null) return;

            currentMusic = clip;
            if (musicSwap != null) StopCoroutine(musicSwap);
            musicSwap = StartCoroutine(SwapMusic(clip));
        }

        public void StopMusic()
        {
            if (currentMusic == null || musicSource == null) return;

            currentMusic = null;
            if (musicSwap != null) StopCoroutine(musicSwap);
            musicSwap = StartCoroutine(SwapMusic(null));
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
                sfxSource.PlayOneShot(clip);
        }

        public void PlayClick()
        {
            PlayRandom(clickSfx);
        }

        public void PlayHover()
        {
            PlayRandom(hoverSfx);
        }

        private void PlayRandom(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return;

            PlaySFX(clips[Random.Range(0, clips.Length)]);
        }

        private IEnumerator SwapMusic(AudioClip next)
        {
            if (musicSource.isPlaying) yield return FadeMusic(0f);

            musicFade = 0f;
            ApplyMusicVolume();
            musicSource.clip = next;

            if (next == null)
            {
                musicSource.Stop();
            }
            else
            {
                musicSource.Play();
                yield return FadeMusic(1f);
            }

            musicSwap = null;
        }

        private IEnumerator FadeMusic(float target)
        {
            float start = musicFade;
            for (float t = 0f; t < musicFadeTime; t += Time.unscaledDeltaTime)
            {
                musicFade = Mathf.Lerp(start, target, t / musicFadeTime);
                ApplyMusicVolume();
                yield return null;
            }

            musicFade = target;
            ApplyMusicVolume();
        }

        private void ApplyMusicVolume()
        {
            if (musicSource != null)
                musicSource.volume = MusicVolume * musicFade;
        }

        private void ApplySFXVolume()
        {
            if (sfxSource != null)
                sfxSource.volume = SFXVolume;
        }
    }
}