using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TabletCharacterVideoController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject centerVideoSection;
    [SerializeField] private VideoPlayer centerVideoPlayer;
    [SerializeField] private RawImage centerVideoRawImage;
    
    [Header("Character Buttons")]
    [SerializeField] private Button button1; // Yellow chick
    [SerializeField] private Button button2; // White chicken
    [SerializeField] private Button button3; // Captain America chick 1
    [SerializeField] private Button button4; // Captain America chick 2
    [SerializeField] private Button button5; // Gold armor chick
    [SerializeField] private Button button6; // Robot
    
    [Header("Video Clips")]
    [SerializeField] private VideoClip videoClip1;
    [SerializeField] private VideoClip videoClip2;
    [SerializeField] private VideoClip videoClip3;
    [SerializeField] private VideoClip videoClip4;
    [SerializeField] private VideoClip videoClip5;
    [SerializeField] private VideoClip videoClip6;
    
    [Header("Close Button")]
    [SerializeField] private Button closeBtn;
    
    private int currentActiveButton = -1; // Track which button is active
    private RenderTexture renderTexture;
    
    private void Awake()
    {
        // Create a RenderTexture for the video (4:3 aspect ratio)
        renderTexture = new RenderTexture(1440, 1080, 24);
        centerVideoPlayer.targetTexture = renderTexture;
        centerVideoRawImage.texture = renderTexture;
        
        // Set VideoPlayer aspect ratio to fit inside
        centerVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
    }

    private void OnEnable()
    {
        if (VideoLoadingManager.Instance != null)
        {
            VideoLoadingManager.Instance.RegisterVideo(centerVideoPlayer);
        }
    }

    private void Start()
    {
        // Initially hide the video section
        centerVideoSection.SetActive(false);
        
        // Add listeners to all buttons
        button1.onClick.AddListener(() => OnButtonClick(1));
        button2.onClick.AddListener(() => OnButtonClick(2));
        button3.onClick.AddListener(() => OnButtonClick(3));
        button4.onClick.AddListener(() => OnButtonClick(4));
        button5.onClick.AddListener(() => OnButtonClick(5));
        button6.onClick.AddListener(() => OnButtonClick(6));
        
        // Add listener to close button
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(CloseVideo);
        }
    }
    
    private void OnButtonClick(int buttonIndex)
    {
        // If clicking the same button, close the video
        if (currentActiveButton == buttonIndex)
        {
            CloseVideo();
            return;
        }
        
        // Open video section and play the corresponding video
        centerVideoSection.SetActive(true);
        currentActiveButton = buttonIndex;
        
        // Get the appropriate video clip
        VideoClip selectedClip = GetVideoClip(buttonIndex);
        
        if (selectedClip != null)
        {
            centerVideoPlayer.clip = selectedClip;
            centerVideoPlayer.Play();
        }
        else
        {
            Debug.LogWarning($"Video clip {buttonIndex} is not assigned!");
        }
    }
    
    private VideoClip GetVideoClip(int index)
    {
        switch (index)
        {
            case 1: return videoClip1;
            case 2: return videoClip2;
            case 3: return videoClip3;
            case 4: return videoClip4;
            case 5: return videoClip5;
            case 6: return videoClip6;
            default: return null;
        }
    }
    
    private void CloseVideo()
    {
        centerVideoSection.SetActive(false);
        centerVideoPlayer.Stop();
        currentActiveButton = -1;
    }
    
    private void OnDestroy()
    {
        // Clean up listeners
        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();
        button3.onClick.RemoveAllListeners();
        button4.onClick.RemoveAllListeners();
        button5.onClick.RemoveAllListeners();
        button6.onClick.RemoveAllListeners();
        
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
        }
        
        // Clean up RenderTexture
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}