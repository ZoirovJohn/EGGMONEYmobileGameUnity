using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class MainMenuPanelTablet : MonoBehaviour
{
    [Header("Main Screen Video")]
    public VideoClip mainScreenVideo;
    public RawImage mainScreenImage;
    public VideoPlayer mainScreenVideoPlayer;

    [Header("Top Button Videos (6 buttons)")]
    public VideoClip[] buttonVideoClips = new VideoClip[6];

    [Header("Button References")]
    public RawImage[] buttonRawImages = new RawImage[6];
    public VideoPlayer[] buttonVideoPlayers = new VideoPlayer[6];
    
    private bool isInitialized = false;

    private void Start()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        bool isTablet = (aspectRatio >= 1.3f && aspectRatio <= 1.7f) || 
                        (aspectRatio >= 0.6f && aspectRatio <= 0.8f);
        
        if (isTablet)
        {
            gameObject.SetActive(true);
            SetupVideos();
            isInitialized = true;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (isInitialized)
        {
            RestartAllVideos();
        }
    }

    private void OnDisable()
    {
        if (isInitialized)
        {
            StopAllVideos();
        }
    }

    private void SetupVideos()
    {
        SetupMainScreenVideo();
        SetupTopButtonVideos();
    }

    private void RestartAllVideos()
    {
        if (mainScreenVideoPlayer != null && mainScreenVideoPlayer.clip != null)
        {
            if (!mainScreenVideoPlayer.isPlaying)
            {
                mainScreenVideoPlayer.Play();
            }
        }

        for (int i = 0; i < buttonVideoPlayers.Length; i++)
        {
            if (buttonVideoPlayers[i] != null && buttonVideoPlayers[i].clip != null)
            {
                if (!buttonVideoPlayers[i].isPlaying)
                {
                    buttonVideoPlayers[i].Play();
                }
            }
        }
    }

    private void StopAllVideos()
    {
        if (mainScreenVideoPlayer != null && mainScreenVideoPlayer.isPlaying)
        {
            mainScreenVideoPlayer.Pause();
        }

        foreach (var videoPlayer in buttonVideoPlayers)
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
        }
    }

    private void SetupMainScreenVideo()
    {
        if (mainScreenVideoPlayer == null || mainScreenImage == null)
        {
            return;
        }

        if (mainScreenVideo == null)
        {
            return;
        }

        mainScreenVideoPlayer.enabled = true;

        int width = 1440;
        int height = 960;
        if (mainScreenVideoPlayer.targetTexture == null)
        {
            RenderTexture renderTexture = new RenderTexture(width, height, 0);
            mainScreenVideoPlayer.targetTexture = renderTexture;
            mainScreenImage.texture = renderTexture;
        }

        mainScreenVideoPlayer.clip = mainScreenVideo;
        mainScreenVideoPlayer.isLooping = true;
        mainScreenVideoPlayer.playOnAwake = false;
        mainScreenVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        mainScreenVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        mainScreenVideoPlayer.Play();

        if (VideoLoadingManager.Instance != null)
        {
            VideoLoadingManager.Instance.RegisterVideo(mainScreenVideoPlayer);
        }
    }

    private void SetupTopButtonVideos()
    {
        int width = 512;
        int height = 512;

        for (int i = 0; i < 6; i++)
        {
            if (buttonVideoPlayers[i] == null || buttonRawImages[i] == null)
            {
                continue;
            }

            if (i >= buttonVideoClips.Length || buttonVideoClips[i] == null)
            {
                continue;
            }

            if (buttonVideoPlayers[i].isPlaying)
            {
                buttonVideoPlayers[i].Stop();
            }
            
            buttonVideoPlayers[i].enabled = true;

            if (buttonVideoPlayers[i].targetTexture == null)
            {
                RenderTexture renderTexture = new RenderTexture(width, height, 0);
                buttonVideoPlayers[i].targetTexture = renderTexture;
                buttonRawImages[i].texture = renderTexture;
            }

            buttonVideoPlayers[i].source = VideoSource.VideoClip;
            buttonVideoPlayers[i].isLooping = true;
            buttonVideoPlayers[i].playOnAwake = false;
            buttonVideoPlayers[i].renderMode = VideoRenderMode.RenderTexture;
            buttonVideoPlayers[i].aspectRatio = VideoAspectRatio.FitInside;
            buttonVideoPlayers[i].skipOnDrop = true;
            buttonVideoPlayers[i].playbackSpeed = 1f;
            
            buttonVideoPlayers[i].clip = buttonVideoClips[i];
            
            int index = i;
            StartCoroutine(PlayVideoAfterFrame(buttonVideoPlayers[index]));

            if (VideoLoadingManager.Instance != null)
            {
                VideoLoadingManager.Instance.RegisterVideo(buttonVideoPlayers[i]);
            }
        }
    }

    private System.Collections.IEnumerator PlayVideoAfterFrame(VideoPlayer videoPlayer)
    {
        if (videoPlayer == null)
        {
            yield break;
        }

        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
        }

        float timeout = 5f;
        float elapsed = 0f;
        
        while (!videoPlayer.isPrepared && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            yield break;
        }

        videoPlayer.Play();
    }

    private void OnDestroy()
    {
        if (mainScreenVideoPlayer != null && mainScreenVideoPlayer.targetTexture != null)
        {
            mainScreenVideoPlayer.targetTexture.Release();
            Destroy(mainScreenVideoPlayer.targetTexture);
        }

        foreach (var videoPlayer in buttonVideoPlayers)
        {
            if (videoPlayer != null && videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.Release();
                Destroy(videoPlayer.targetTexture);
            }
        }
    }
}
