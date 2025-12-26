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
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public RawImage rawImage;
    public UnityEngine.Video.VideoClip afterFeedingVideo;
    public UnityEngine.Video.VideoClip afterCollectingVideo;
    public UnityEngine.Video.VideoClip afterCleanupVideo;

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
        Debug.Log("🎮 DailyDutyManager Start()");
        
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
            Debug.Log("✅ Subscribed to PlayerWallet events");
            
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
            Debug.Log("🎬 Video interrupted — force finishing");

            isPlayingSuccessVideo = false;

            if (videoPlayer != null)
                videoPlayer.Stop();

            if (panelAnim != null)
                panelAnim.SetActive(false);

            if (mainPanel != null)
                mainPanel.SetActive(true);
        }
        
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged -= UpdateStatusImages;
            playerWallet.OnItemChanged -= OnInventoryItemChanged;
            Debug.Log("✅ Unsubscribed from PlayerWallet events");
        }
    }



    private void OnInventoryItemChanged(string itemId, int newValue)
    {
        Debug.Log($"🔔 OnInventoryItemChanged called - itemId: '{itemId}', newValue: {newValue}");
        
        if (itemId == "food" || itemId == "super_food")
        {
            Debug.Log($"🍗 FOOD ITEM CHANGED! Calling CheckFoodInventory()");
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
            Debug.LogError("❌ CheckFoodInventory: PlayerWallet is NULL!");
            return;
        }
        
        if (warningMessage == null)
        {
            Debug.LogError("❌ CheckFoodInventory: warningMessage GameObject is NULL!");
            return;
        }

        int foodCount = playerWallet.Food;
        int superFoodCount = playerWallet.SuperFood;
        
        Debug.Log($"🍗 CheckFoodInventory - Food: {foodCount}, SuperFood: {superFoodCount}");

        bool noFood = (foodCount == 0 && superFoodCount == 0);
        
        Debug.Log($"🚨 No Food Status: {noFood} (should show warning: {noFood})");
        
        warningMessage.SetActive(noFood);

        if (goShopButton != null)
            goShopButton.gameObject.SetActive(noFood);

        simonButton.gameObject.SetActive(!noFood);
                
        Debug.Log($"✅ Warning message SetActive({noFood}) - GameObject active: {warningMessage.activeSelf}");
    }

    // =========================
    // Load Full Summary
    // =========================
    private void LoadFullSummary()
    {
        if (fullSummaryManager == null)
        {
            Debug.LogWarning("⚠️ FullSummaryManager not assigned!");
            return;
        }

        fullSummaryManager.GetFullSummary(
            onSuccess: (response) =>
            {
                UpdateStatusImages();
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to load full summary: {error}");
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

                // update DB
                farmDatabase.UpdateFarmFromBackend(
                    farmIndex: farmIndex,
                    nests: summary.nests.total,
                    champChicks: summary.henStats.byKind.Champ,
                    normalChicks: summary.henStats.byKind.Normal,
                    legendChicks: summary.henStats.byKind.Legend,
                    superLegendChicks: summary.henStats.byKind.SuperLegend
                );

                // refresh visuals
                farmGridManager?.RefreshFarmDisplay(farmIndex);
                farmHeaderManager?.UpdateAllFarmSlotVisuals();

                done = true;
            },
            onError: (err) =>
            {
                Debug.LogError($"❌ Farm refresh failed after collect: {err}");
                done = true;
            }
        );

        while (!done)
            yield return null;
    }


    // =========================
    // Update Status Images
    // =========================
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

        Debug.Log($"📊 Status Updated - Egg Ready: {playerWallet.HensWithEggReady}, Food: {playerWallet.HensNeedingFood}, Clean: {playerWallet.HensNeedingClean}");
    }

    // =========================
    // Button Click Handlers
    // =========================
    
    // 🍗 FEED BUTTON (Simon Button)
    private void OnFeedButtonClicked()
    {
        Debug.Log("🍗 Feed button clicked");

        if (feedingManager == null)
        {
            Debug.LogError("❌ FeedingManager is not assigned!");
            return;
        }

        if (playerWallet == null)
        {
            Debug.LogError("❌ PlayerWallet is NULL!");
            return;
        }

        bool noFood = playerWallet.Food == 0 && playerWallet.SuperFood == 0;
        if (noFood)
        {
            Debug.Log("🚫 Cannot feed: no food available");

            if (warningMessage != null)
                warningMessage.SetActive(true);
            return;
        }

        simonButton.interactable = false;

        feedingManager.FeedAll(
            onSuccess: (response) => 
            {
                Debug.Log("✅ Feeding successful!");
                
                if (inventoryManager != null)
                {
                    inventoryManager.GetInventory(
                        onSuccess: (inventoryResponse) =>
                        {
                            Debug.Log("✅ Inventory refreshed after feeding");
                            
                            RefreshSummaryAndPlayAnimation(simonButton, afterFeedingVideo);
                        },
                        onError: (inventoryError) =>
                        {
                            Debug.LogError($"❌ Failed to refresh inventory: {inventoryError}");
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
                Debug.LogError($"❌ Feeding failed: {error}");
                simonButton.interactable = true;
            }
        );
    }

    private IEnumerator AfterCollectFlow(Button button, VideoClip clip)
    {
        // refresh wallet / summary
        yield return new WaitForSeconds(0.1f);

        // 🔥 THIS fixes the egg still showing
        yield return StartCoroutine(RefreshCurrentFarmData());

        PlayAnimationVideo(clip);
        button.interactable = true;
    }


    private void OnGoShopClicked()
    {
        Debug.Log("🛒 GoShop button clicked");

        if (storeButton != null)
        {
            storeButton.onClick.Invoke(); // 👈 THIS is the key
            Debug.Log("✅ Store button invoked programmatically");
        }
        else
        {
            Debug.LogError("❌ StoreButton is not assigned!");
        }
    }

    // 🥚 COLLECT BUTTON
    private void OnCollectButtonClicked()
    {
        Debug.Log("🥚 Collect button clicked");

        if (collectingManager == null)
        {
            Debug.LogError("❌ CollectingManager is not assigned!");
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
                Debug.LogError($"❌ Collection failed: {error}");
                collectButton.interactable = true;
            }
        );


    }

    // 🧹 CLEAN BUTTON (Matching Button)
    private void OnCleanButtonClicked()
    {
        Debug.Log("🧹 Clean button clicked");

        if (cleanupManager == null)
        {
            Debug.LogError("❌ CleanupManager is not assigned!");
            return;
        }

        matchingButton.interactable = false;

        cleanupManager.CleanAll(
            onSuccess: (response) => 
            {
                Debug.Log("✅ Cleanup successful!");
                
                RefreshSummaryAndPlayAnimation(matchingButton, afterCleanupVideo);
            },
            onError: (error) => 
            {
                Debug.LogError($"❌ Cleanup failed: {error}");
                matchingButton.interactable = true;
            }
        );
    }

    // =========================
    // Helper Methods
    // =========================
    private void RefreshSummaryAndPlayAnimation(Button button, UnityEngine.Video.VideoClip videoClip)
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
    
    private void PlayAnimationVideo(UnityEngine.Video.VideoClip videoClip)
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning("⚠️ VideoPlayer is not assigned! Skipping animation.");
            return;
        }
        
        if (videoClip == null)
        {
            Debug.LogWarning("⚠️ VideoClip is not assigned! Skipping animation.");
            return;
        }
        
        isPlayingSuccessVideo = true;
        
        mainPanel.SetActive(false);
        panelAnim.SetActive(true);
        
        videoPlayer.Stop();
        
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
    }
    
    private void OnVideoFinished(UnityEngine.Video.VideoPlayer vp)
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
    
    // =========================
    // Setup RenderTexture
    // =========================
    private void SetupVideoPlayerRenderTexture()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("❌ VideoPlayer is not assigned!");
            return;
        }
        
        if (rawImage == null)
        {
            Debug.LogError("❌ RawImage is not assigned!");
            return;
        }
        
        if (videoPlayer.targetTexture == null)
        {
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            renderTexture.name = "VideoRenderTexture";
            
            videoPlayer.targetTexture = renderTexture;
            
            Debug.Log("✅ Created RenderTexture for VideoPlayer: 1920x1080");
        }
        
        rawImage.texture = videoPlayer.targetTexture;
        
        Debug.Log("✅ VideoPlayer and RawImage connected via RenderTexture");
    }
}