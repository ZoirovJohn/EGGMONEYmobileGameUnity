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

    private void Start()
    {
        // Check screen aspect ratio directly
        float aspectRatio = (float)Screen.width / Screen.height;
        Debug.Log($"📱 MainMenuPanelPhone: Screen {Screen.width}x{Screen.height}, Aspect: {aspectRatio:F2}");
        
        // Phone detection: Exclude tablet aspect ratios
        // Phones are either very tall (< 0.6) or very wide (> 1.7)
        bool isPhone = (aspectRatio < 0.6f || aspectRatio > 1.7f);
        
        if (isPhone)
        {
            Debug.Log("✅ Phone panel activated - setting up videos");
            gameObject.SetActive(true);
            SetupVideos();
        }
        else
        {
            Debug.Log("❌ Phone panel deactivated (device is tablet)");
            gameObject.SetActive(false);
        }
    }

    private void SetupVideos()
    {
        SetupMainScreenVideo();
        SetupTopButtonVideos();
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

        // Phone: 1080x1080
        int width = 1080;
        int height = 1080;
        
        RenderTexture renderTexture = new RenderTexture(width, height, 0);
        mainScreenVideoPlayer.targetTexture = renderTexture;
        mainScreenImage.texture = renderTexture;

        mainScreenVideoPlayer.clip = mainScreenVideo;
        mainScreenVideoPlayer.isLooping = true;
        mainScreenVideoPlayer.playOnAwake = false;
        mainScreenVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        mainScreenVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        mainScreenVideoPlayer.Play();

        Debug.Log($"✅ [Phone] Main screen video loaded ({width}x{height})");
    }

    private void SetupTopButtonVideos()
    {
        // Phone: 512x682
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

            // Stop any existing playback first
            if (buttonVideoPlayers[i].isPlaying)
            {
                buttonVideoPlayers[i].Stop();
            }
            
            buttonVideoPlayers[i].enabled = true;

            RenderTexture renderTexture = new RenderTexture(width, height, 0);
            buttonVideoPlayers[i].targetTexture = renderTexture;
            buttonRawImages[i].texture = renderTexture;

            // Configure video player settings BEFORE assigning clip
            buttonVideoPlayers[i].source = VideoSource.VideoClip;
            buttonVideoPlayers[i].isLooping = true;
            buttonVideoPlayers[i].playOnAwake = false;
            buttonVideoPlayers[i].renderMode = VideoRenderMode.RenderTexture;
            buttonVideoPlayers[i].aspectRatio = VideoAspectRatio.FitInside;
            buttonVideoPlayers[i].skipOnDrop = true;
            buttonVideoPlayers[i].playbackSpeed = 1f;
            
            // Assign the specific clip for this button
            buttonVideoPlayers[i].clip = buttonVideoClips[i];
            
            // Capture index for coroutine
            int index = i;
            StartCoroutine(PlayVideoAfterFrame(buttonVideoPlayers[index]));

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

        // Prepare the video if not already prepared
        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
        }

        // Wait until the video is prepared
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

        // Now play the video
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