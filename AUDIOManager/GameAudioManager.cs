using UnityEngine;
using System.Collections;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Global Sounds")]
    public AudioClip buttonClickClip;

    [Header("BGM Playlist")]
    public AudioClip[] bgmClips;

    private int currentTrackIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            Debug.Log("GameAudioManager initialized and persisted");
        }
        else
        {
            // IMPORTANT: Copy clips from the new scene instance before destroying
            if (bgmClips != null && bgmClips.Length > 0)
            {
                Instance.bgmClips = bgmClips;
                Instance.buttonClickClip = buttonClickClip;
                
                // Restart music with new clips if not playing
                if (!Instance.musicSource.isPlaying)
                {
                    Instance.currentTrackIndex = 0;
                    Instance.StartCoroutine(Instance.StartMusicDelayed());
                }
            }
            
            Destroy(gameObject);
            return;
        }
    }
    
    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("Music AudioSource was missing - created automatically");
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("SFX AudioSource was missing - created automatically");
        }

        musicSource.volume = musicSource.volume == 0 ? 0.7f : musicSource.volume;
        sfxSource.volume = sfxSource.volume == 0 ? 1f : sfxSource.volume;
    }

    private void Start()
    {
        // Only start if this is the singleton instance
        if (Instance == this)
        {
            StartCoroutine(StartMusicDelayed());
        }
    }

    private IEnumerator StartMusicDelayed()
    {
        yield return null;

        if (bgmClips != null && bgmClips.Length > 0 && !musicSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    private void Update()
    {
        if (musicSource != null && !musicSource.isPlaying && bgmClips != null && bgmClips.Length > 0)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        if (bgmClips == null || bgmClips.Length == 0) return;

        musicSource.clip = bgmClips[currentTrackIndex];
        musicSource.loop = false;
        musicSource.Play();
        currentTrackIndex = (currentTrackIndex + 1) % bgmClips.Length;
    }

    // Rest of your methods remain the same...
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip);
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }

    public float GetMusicVolume()
    {
        return musicSource != null ? musicSource.volume : 0.7f;
    }

    public float GetSFXVolume()
    {
        return sfxSource != null ? sfxSource.volume : 1f;
    }
}