using UnityEngine;
using UnityEngine.UI;
using System.Collections;        
using UnityEngine.Video;

public class DailyDutyManager : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject mainPanel;
    public GameObject panelAnim;

    [SerializeField] FarmAPIManager farmAPIManager;
    [SerializeField] FarmDatabase farmDatabase;
    [SerializeField] FarmGridManager farmGridManager;
    [SerializeField] FarmHeaderManager farmHeaderManager;

    [Header("Single Video Player Setup")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public VideoClip afterFeedingVideo;
    public VideoClip afterCollectingVideo;
    public VideoClip afterCleanupVideo;

    [Header("Buttons (on Main Panel)")]
    public Button simonButton;
    public Button collectButton;
    public Button matchingButton;

    [Header("API Managers")]
    public FeedingManager feedingManager;
    public CleanupManager cleanupManager;
    public CollectingManager collectingManager;

    [Header("Full Summary")]
    public FullSummaryManager fullSummaryManager;
    public PlayerWallet playerWallet;

    [Header("Inventory Manager")]
    public InventoryManager inventoryManager;

    [Header("Status Images")]
    public GameObject hensWithEggReadyImage;
    public GameObject hensNeedingFoodImage;
    public GameObject hensNeedingCleanImage;

    [Header("Warning Message")]
    public GameObject warningMessage;

    [Header("Go Shop UI")]
    public Button goShopButton; 
    public Button storeButton; 

    private bool isPlayingSuccessVideo = false;

    private void Start()
    {
        simonButton.onClick.AddListener(OnFeedButtonClicked);
        collectButton.onClick.AddListener(OnCollectButtonClicked);
        matchingButton.onClick.AddListener(OnCleanButtonClicked);

        if (goShopButton != null)
        {
            goShopButton.onClick.AddListener(OnGoShopClicked);
            goShopButton.gameObject.SetActive(false);
        }

        LoadFullSummary();

        mainPanel.SetActive(true);
        panelAnim.SetActive(false);
        
        CheckFoodInventory();
        
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            SetupVideoPlayerRenderTexture();
        }
    }

    private void OnEnable()
    {
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged += UpdateStatusImages;
            playerWallet.OnItemChanged += OnInventoryItemChanged;
            CheckFoodInventory();
        }
        else
        {
            Debug.LogError("❌ PlayerWallet is NULL in OnEnable!");
        }
    }

    private void OnDisable()
    {
        if (isPlayingSuccessVideo)
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            
            isPlayingSuccessVideo = false;
        }
        
        if (panelAnim != null && panelAnim.activeSelf)
        {
            panelAnim.SetActive(false);
        }
        
        if (mainPanel != null && !mainPanel.activeSelf)
        {
            mainPanel.SetActive(true);
        }
        
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged -= UpdateStatusImages;
            playerWallet.OnItemChanged -= OnInventoryItemChanged;
        }
    }

    private void OnInventoryItemChanged(string itemId, int newValue)
    {
        if (itemId == "food" || itemId == "super_food")
        {
            CheckFoodInventory();
        }
        else
        {
            Debug.Log($"⚪ Item '{itemId}' is not food-related, skipping check");
        }
    }

    private void CheckFoodInventory()
    {
        if (playerWallet == null)
        {
            return;
        }
        
        if (warningMessage == null)
        {
            return;
        }

        int foodCount = playerWallet.Food;
        int superFoodCount = playerWallet.SuperFood;
        
        bool noFood = (foodCount == 0 && superFoodCount == 0);
        
        warningMessage.SetActive(noFood);

        if (goShopButton != null)
            goShopButton.gameObject.SetActive(noFood);

        simonButton.gameObject.SetActive(!noFood);
    }

    private void LoadFullSummary()
    {
        if (fullSummaryManager == null)
        {
            return;
        }

        fullSummaryManager.GetFullSummary(
            onSuccess: (response) =>
            {
                UpdateStatusImages();
            },
            onError: (error) =>
            {
                if (hensWithEggReadyImage != null) hensWithEggReadyImage.SetActive(false);
                if (hensNeedingFoodImage != null) hensNeedingFoodImage.SetActive(false);
                if (hensNeedingCleanImage != null) hensNeedingCleanImage.SetActive(false);
            }
        );
    }

    private IEnumerator RefreshCurrentFarmData()
    {
        if (farmAPIManager == null || farmDatabase == null)
            yield break;

        int farmNumber = farmDatabase.currentFarmIndex + 1;
        bool done = false;

        farmAPIManager.GetFarmSummary(
            farmNumber,
            onSuccess: (summary) =>
            {
                int farmIndex = farmNumber - 1;

                farmDatabase.UpdateFarmFromBackend(
                    farmIndex: farmIndex,
                    nests: summary.nests.total,
                    champChicks: summary.henStats.byKind.Champ,
                    normalChicks: summary.henStats.byKind.Normal,
                    legendChicks: summary.henStats.byKind.Legend,
                    superLegendChicks: summary.henStats.byKind.SuperLegend
                );

                farmGridManager?.RefreshFarmDisplay(farmIndex);
                farmHeaderManager?.UpdateAllFarmSlotVisuals();

                done = true;
            },
            onError: (err) =>
            {
                done = true;
            }
        );

        while (!done)
            yield return null;
    }

    private void UpdateStatusImages()
    {
        if (playerWallet == null) return;

        if (hensWithEggReadyImage != null)
        {
            hensWithEggReadyImage.SetActive(playerWallet.HensWithEggReady == 0);
        }

        if (hensNeedingFoodImage != null)
        {
            hensNeedingFoodImage.SetActive(playerWallet.HensNeedingFood == 0);
        }

        if (hensNeedingCleanImage != null)
        {
            hensNeedingCleanImage.SetActive(playerWallet.HensNeedingClean == 0);
        }

        CheckFoodInventory();
    }

    private void OnFeedButtonClicked()
    {
        if (feedingManager == null)
        {
            return;
        }

        if (playerWallet == null)
        {
            return;
        }

        bool noFood = playerWallet.Food == 0 && playerWallet.SuperFood == 0;
        if (noFood)
        {
            if (warningMessage != null)
                warningMessage.SetActive(true);
            return;
        }

        simonButton.interactable = false;

        feedingManager.FeedAll(
            onSuccess: (response) => 
            {
                if (inventoryManager != null)
                {
                    inventoryManager.GetInventory(
                        onSuccess: (inventoryResponse) =>
                        {
                            RefreshSummaryAndPlayAnimation(simonButton, afterFeedingVideo);
                        },
                        onError: (inventoryError) =>
                        {
                            RefreshSummaryAndPlayAnimation(simonButton, afterFeedingVideo);
                        }
                    );
                }
                else
                {
                    RefreshSummaryAndPlayAnimation(simonButton, afterFeedingVideo);
                }
            },
            onError: (error) => 
            {
                simonButton.interactable = true;
            }
        );
    }

    private IEnumerator AfterCollectFlow(Button button, VideoClip clip)
    {
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(RefreshAllFarmsData());
        
        if (fullSummaryManager != null)
        {
            bool summaryDone = false;
            
            fullSummaryManager.GetFullSummary(
                onSuccess: (response) =>
                {
                    summaryDone = true;
                },
                onError: (error) =>
                {
                    summaryDone = true;
                }
            );
            
            while (!summaryDone)
                yield return null;
        }

        PlayAnimationVideo(clip);
        button.interactable = true;
    }

    private void OnGoShopClicked()
    {
        if (storeButton != null)
        {
            storeButton.onClick.Invoke();
        }
        else
        {
            Debug.LogError("❌ StoreButton is not assigned!");
        }
    }

    private void OnCollectButtonClicked()
    {
        if (collectingManager == null)
        {
            return;
        }

        collectButton.interactable = false;

        collectingManager.CollectAll(
            onSuccess: (response) =>
            {
                StartCoroutine(AfterCollectFlow(collectButton, afterCollectingVideo));
            },
            onError: (error) =>
            {
                collectButton.interactable = true;
            }
        );
    }

    private void OnCleanButtonClicked()
    {
        if (cleanupManager == null)
        {
            return;
        }

        matchingButton.interactable = false;

        cleanupManager.CleanAll(
            onSuccess: (response) => 
            {
                RefreshSummaryAndPlayAnimation(matchingButton, afterCleanupVideo);
            },
            onError: (error) => 
            {
                matchingButton.interactable = true;
            }
        );
    }

    private void RefreshSummaryAndPlayAnimation(Button button, VideoClip videoClip)
    {
        if (fullSummaryManager != null)
        {
            fullSummaryManager.GetFullSummary(
                onSuccess: (summaryResponse) => 
                {
                    PlayAnimationVideo(videoClip);
                    button.interactable = true;
                },
                onError: (summaryError) => 
                {
                    PlayAnimationVideo(videoClip);
                    button.interactable = true;
                }
            );
        }
        else
        {
            PlayAnimationVideo(videoClip);
            button.interactable = true;
        }
    }
    
    private void PlayAnimationVideo(VideoClip videoClip)
    {
        if (videoPlayer == null)
        {
            return;
        }
        
        if (videoClip == null)
        {
            return;
        }
        
        isPlayingSuccessVideo = true;
        
        mainPanel.SetActive(false);
        panelAnim.SetActive(true);
        
        videoPlayer.Stop();
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
    }
    
    private void OnVideoFinished(VideoPlayer vp)
    {
        isPlayingSuccessVideo = false;
        
        panelAnim.SetActive(false);
        mainPanel.SetActive(true);
        
        vp.Stop();

        if (FXManager.Instance != null)
        {
            FXManager.Instance.PlayGameFX_Center_4Times();
        }
    }

    private IEnumerator RefreshAllFarmsData()
    {
        if (farmAPIManager == null || farmDatabase == null)
            yield break;

        int farmCount = farmDatabase.farms.Count;
        bool done = false;

        farmAPIManager.LoadAllFarmSummaries(
            farmCount,
            onAllLoaded: (summaries) =>
            {
                int currentFarmIndex = farmDatabase.currentFarmIndex;
                farmGridManager?.RefreshFarmDisplay(currentFarmIndex);
                farmHeaderManager?.UpdateAllFarmSlotVisuals();

                done = true;
            },
            onError: (error) =>
            {
                done = true;
            }
        );

        while (!done)
            yield return null;
    }

    private void SetupVideoPlayerRenderTexture()
    {
        if (videoPlayer == null)
        {
            return;
        }
        
        if (rawImage == null)
        {
            return;
        }
        
        if (videoPlayer.targetTexture == null)
        {
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            renderTexture.name = "VideoRenderTexture";
            
            videoPlayer.targetTexture = renderTexture;
        }
        
        rawImage.texture = videoPlayer.targetTexture;
    }
}