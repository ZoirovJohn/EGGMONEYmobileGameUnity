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
    [SerializeField] private Button button1; 
    [SerializeField] private Button button2; 
    [SerializeField] private Button button3; 
    [SerializeField] private Button button4; 
    [SerializeField] private Button button5; 
    [SerializeField] private Button button6; 
    
    [Header("Video Clips")]
    [SerializeField] private VideoClip videoClip1;
    [SerializeField] private VideoClip videoClip2;
    [SerializeField] private VideoClip videoClip3;
    [SerializeField] private VideoClip videoClip4;
    [SerializeField] private VideoClip videoClip5;
    [SerializeField] private VideoClip videoClip6;
    
    [Header("Close Button")]
    [SerializeField] private Button closeBtn;
    
    private int currentActiveButton = -1; 
    private RenderTexture renderTexture;
    
    private void Awake()
    {
        renderTexture = new RenderTexture(1440, 1080, 24);
        centerVideoPlayer.targetTexture = renderTexture;
        centerVideoRawImage.texture = renderTexture;
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
        centerVideoSection.SetActive(false);
        button1.onClick.AddListener(() => OnButtonClick(1));
        button2.onClick.AddListener(() => OnButtonClick(2));
        button3.onClick.AddListener(() => OnButtonClick(3));
        button4.onClick.AddListener(() => OnButtonClick(4));
        button5.onClick.AddListener(() => OnButtonClick(5));
        button6.onClick.AddListener(() => OnButtonClick(6));
        
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(CloseVideo);
        }
    }
    
    private void OnButtonClick(int buttonIndex)
    {
        if (currentActiveButton == buttonIndex)
        {
            CloseVideo();
            return;
        }
        
        centerVideoSection.SetActive(true);
        currentActiveButton = buttonIndex;
        
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
        
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}