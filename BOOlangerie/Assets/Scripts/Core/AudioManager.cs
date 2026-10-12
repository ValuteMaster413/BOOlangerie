using System;
using System.Collections;
using System.Collections.Generic;
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

        [Header("Sounds Played By Name")]
        [SerializeField] private AudioClip[] sfxLibrary;

        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SFXVolume";

        private AudioSource voiceSource;
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

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            if (sfxSource != null) voiceSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;

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

        public void PlaySFX(string clipName)
        {
            PlaySFX(FindClip(clipName));
        }

        public void PlayVoice(AudioClip clip)
        {
            if (voiceSource == null || clip == null) return;

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        public void PlayVoice(string clipName)
        {
            PlayVoice(FindClip(clipName));
        }

        private AudioClip FindClip(string clipName)
        {
            if (sfxLibrary != null)
            {
                foreach (AudioClip clip in sfxLibrary)
                {
                    if (clip != null && string.Equals(clip.name, clipName, StringComparison.OrdinalIgnoreCase)) return clip;
                }
            }

            Debug.LogWarning($"[Audio] No sound named '{clipName}' in the Sfx Library.");
            return null;
        }

        public void PlayRandomSFX(string namePrefix)
        {
            if (sfxLibrary == null) return;

            List<AudioClip> matches = new List<AudioClip>();
            foreach (AudioClip clip in sfxLibrary)
            {
                if (clip != null && clip.name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase)) matches.Add(clip);
            }

            if (matches.Count == 0)
            {
                Debug.LogWarning($"[Audio] No sounds starting with '{namePrefix}' in the Sfx Library.");
                return;
            }

            PlaySFX(matches[UnityEngine.Random.Range(0, matches.Count)]);
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

            PlaySFX(clips[UnityEngine.Random.Range(0, clips.Length)]);
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

            if (voiceSource != null)
                voiceSource.volume = SFXVolume;
        }
    }
}