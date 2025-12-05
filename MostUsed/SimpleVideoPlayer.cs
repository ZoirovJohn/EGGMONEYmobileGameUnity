using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SimpleVideoPlayer : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public VideoPlayer videoPlayer;      // VideoPlayer component
    public RawImage rawImage;            // RawImage UI
    public AudioSource audioSource;      // Optional (for audio)

    void Start()
    {
        // Connect audio if needed
        if (audioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        // When video is prepared, show frame
        videoPlayer.prepareCompleted += OnVideoPrepared;

        // Prepare video (important for RawImage)
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        // Set texture to RawImage
        rawImage.texture = videoPlayer.texture;

        // Play video
        videoPlayer.Play();

        if (audioSource != null)
            audioSource.Play();
    }
}
