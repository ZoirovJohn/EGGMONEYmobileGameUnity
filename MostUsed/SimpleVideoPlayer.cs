using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SimpleVideoPlayer : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public AudioSource audioSource;

    private bool isPrepared = false;

    private void Awake()
    {
        if (videoPlayer == null)
            return;

        // DO NOT prepare here
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnEnable()
    {
        // Do nothing here on purpose
        // VideoPlayer may still be disabled
    }

    /// <summary>
    /// Call this ONLY when you actually want to play the video
    /// (button click, page open, panel open)
    /// </summary>
    public void PlayVideoSafe()
    {
        if (videoPlayer == null)
            return;

        // 🔒 HARD SAFETY CHECK
        if (!videoPlayer.enabled)
        {
            Debug.LogWarning($"⏸ VideoPlayer is disabled, skipping prepare: {name}");
            return;
        }

        if (!isPrepared)
        {
            videoPlayer.Prepare();
        }
        else
        {
            StartPlayback();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        isPrepared = true;
        StartPlayback();
    }

    private void StartPlayback()
    {
        if (rawImage != null)
            rawImage.texture = videoPlayer.texture;

        videoPlayer.Play();

        if (audioSource != null)
            audioSource.Play();
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}
