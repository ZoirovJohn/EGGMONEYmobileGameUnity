using UnityEngine;
using System.Collections;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;   // BGM AudioSource
    public AudioSource sfxSource;     // SFX AudioSource

    [Header("Global Sounds")]
    public AudioClip buttonClickClip; // Main button click sound

    [Header("BGM Playlist")]
    public AudioClip[] bgmClips;      // Assign your 4 music clips in inspector
    private int currentTrackIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (bgmClips.Length > 0)
        {
            PlayNextTrack();
        }
    }

    private void Update()
    {
        // Check if current track finished
        if (musicSource != null && !musicSource.isPlaying && bgmClips.Length > 0)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        musicSource.clip = bgmClips[currentTrackIndex];
        musicSource.loop = false; // we handle looping manually across tracks
        musicSource.Play();

        // Advance index for next track
        currentTrackIndex = (currentTrackIndex + 1) % bgmClips.Length;
    }

    // Play any SFX clip
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip);
    }
}
