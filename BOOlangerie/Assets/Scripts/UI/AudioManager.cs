using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings Keys")]
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

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

        ApplyMusicVolume(MusicVolume);
        ApplySFXVolume(SFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume;
        ApplyMusicVolume(volume);
        PlayerPrefs.SetFloat(MUSIC_KEY, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
        ApplySFXVolume(volume);
        PlayerPrefs.SetFloat(SFX_KEY, volume);
        PlayerPrefs.Save();
    }

    private void ApplyMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    private void ApplySFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, SFXVolume);
        }
    }
}