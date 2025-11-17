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
            Destroy(gameObject);
            return;
        }
    }
    
    private void OnEnable()
    {
        // Resume music when scene loads if Instance exists
        if (Instance == this && musicSource != null && !musicSource.isPlaying)
        {
            if (bgmClips != null && bgmClips.Length > 0)
            {
                Debug.Log("Resuming music on scene load");
                PlayNextTrack();
            }
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

        if (musicSource.volume == 0) musicSource.volume = 0.7f;
        if (sfxSource.volume == 0) sfxSource.volume = 1f;
    }

    private void Start()
    {
        // Start music immediately when AudioManager is created
        StartCoroutine(StartMusicDelayed());
    }

    private System.Collections.IEnumerator StartMusicDelayed()
    {
        // Wait a frame to ensure everything is initialized
        yield return null;

        if (Instance == this && bgmClips != null && bgmClips.Length > 0)
        {
            if (!musicSource.isPlaying)
            {
                Debug.Log("🎵 Starting BGM");
                PlayNextTrack();
            }
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