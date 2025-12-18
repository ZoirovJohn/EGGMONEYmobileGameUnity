using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class MainMenuPanelPhone : MonoBehaviour
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
        // Check screen aspect ratio directly
        float aspectRatio = (float)Screen.width / Screen.height;
        Debug.Log($"📱 MainMenuPanelPhone: Screen {Screen.width}x{Screen.height}, Aspect: {aspectRatio:F2}");
        
        // Phone detection: Exclude tablet aspect ratios
        bool isPhone = (aspectRatio < 0.6f || aspectRatio > 1.7f);
        
        if (isPhone)
        {
            Debug.Log("✅ Phone panel activated - setting up videos");
            gameObject.SetActive(true);
            SetupVideos();
            isInitialized = true;
        }
        else
        {
            Debug.Log("❌ Phone panel deactivated (device is tablet)");
            gameObject.SetActive(false);
        }
    }

    // ✅ NEW: Handle re-enabling
    private void OnEnable()
    {
        // Only restart videos if we've already initialized
        // (Skip on first enable since Start() handles it)
        if (isInitialized)
        {
            Debug.Log("🔄 MainMenuPanelPhone re-enabled - restarting videos");
            RestartAllVideos();
        }
    }

    // ✅ NEW: Stop videos when disabled
    private void OnDisable()
    {
        if (isInitialized)
        {
            Debug.Log("⏸️ MainMenuPanelPhone disabled - stopping videos");
            StopAllVideos();
        }
    }

    private void SetupVideos()
    {
        SetupMainScreenVideo();
        SetupTopButtonVideos();
    }

    // ✅ NEW: Restart all videos
    private void RestartAllVideos()
    {
        // Restart main screen video
        if (mainScreenVideoPlayer != null && mainScreenVideoPlayer.clip != null)
        {
            if (!mainScreenVideoPlayer.isPlaying)
            {
                mainScreenVideoPlayer.Play();
                Debug.Log("▶️ Restarted main screen video");
            }
        }

        // Restart button videos
        for (int i = 0; i < buttonVideoPlayers.Length; i++)
        {
            if (buttonVideoPlayers[i] != null && buttonVideoPlayers[i].clip != null)
            {
                if (!buttonVideoPlayers[i].isPlaying)
                {
                    buttonVideoPlayers[i].Play();
                    Debug.Log($"▶️ Restarted button {i + 1} video");
                }
            }
        }
    }

    // ✅ NEW: Stop all videos
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
            Debug.LogError("[Phone] Main screen video player or image not assigned!");
            return;
        }

        if (mainScreenVideo == null)
        {
            Debug.LogError("[Phone] Main screen video clip not assigned!");
            return;
        }

        mainScreenVideoPlayer.enabled = true;

        int width = 1080;
        int height = 1080;
        
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


        Debug.Log($"✅ [Phone] Main screen video loaded ({width}x{height})");
    }

    private void SetupTopButtonVideos()
    {
        int width = 512;
        int height = 682;

        for (int i = 0; i < 6; i++)
        {
            if (buttonVideoPlayers[i] == null || buttonRawImages[i] == null)
            {
                Debug.LogWarning($"[Phone] Button {i + 1} video player or raw image not assigned!");
                continue;
            }

            if (i >= buttonVideoClips.Length || buttonVideoClips[i] == null)
            {
                Debug.LogWarning($"[Phone] Button {i + 1} video clip not assigned!");
                continue;
            }

            if (buttonVideoPlayers[i].isPlaying)
            {
                buttonVideoPlayers[i].Stop();
            }
            
            buttonVideoPlayers[i].enabled = true;

            // ✅ Check if RenderTexture already exists
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


            Debug.Log($"✅ [Phone] Button {i + 1} video setup: {buttonVideoClips[i].name} ({width}x{height})");
        }
    }

    private System.Collections.IEnumerator PlayVideoAfterFrame(VideoPlayer videoPlayer)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("❌ VideoPlayer is null in coroutine!");
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
            Debug.LogError($"❌ Video failed to prepare: {videoPlayer.clip?.name}");
            yield break;
        }

        videoPlayer.Play();
        Debug.Log($"▶️ Playing video: {videoPlayer.clip.name}");
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