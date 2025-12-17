using UnityEngine;
using UnityEngine.UI;

public class DailyDutyManager : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject mainPanel;
    public GameObject panelAnim;

    [Header("Single Video Player Setup")]
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public RawImage rawImage; // ← Add RawImage reference
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

    private void Start()
    {
        Debug.Log("🎮 DailyDutyManager Start()");
        
        // Assign main panel button listeners
        simonButton.onClick.AddListener(OnFeedButtonClicked);
        collectButton.onClick.AddListener(OnCollectButtonClicked);
        matchingButton.onClick.AddListener(OnCleanButtonClicked);

        // Load full summary data
        LoadFullSummary();

        // Start with only the main panel visible
        mainPanel.SetActive(true);
        panelAnim.SetActive(false);
        
        // 🔍 Initial check
        CheckFoodInventory();
        
        // Setup video player listener
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            
            // ✅ Create RenderTexture dynamically
            SetupVideoPlayerRenderTexture();
        }
    }

    private void OnEnable()
    {
        Debug.Log("🎮 DailyDutyManager OnEnable()");
        
        // Subscribe to profile changes to update images
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged += UpdateStatusImages;
            playerWallet.OnItemChanged += OnInventoryItemChanged;
            Debug.Log("✅ Subscribed to PlayerWallet events");
            
            // ✅ CHECK FOOD STATUS WHEN PANEL OPENS (in case user bought food while panel was closed)
            CheckFoodInventory();
        }
        else
        {
            Debug.LogError("❌ PlayerWallet is NULL in OnEnable!");
        }
    }

    private void OnDisable()
    {
        Debug.Log("🎮 DailyDutyManager OnDisable()");
        
        // Unsubscribe when disabled
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
        
        // Check if the changed item is food or superFood
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

        // Check if both food and superFood are 0
        bool noFood = (foodCount == 0 && superFoodCount == 0);
        
        Debug.Log($"🚨 No Food Status: {noFood} (should show warning: {noFood})");
        
        // Turn warning ON if no food, OFF if there is food
        warningMessage.SetActive(noFood);
        
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
                Debug.Log("✅ Full Summary loaded successfully!");
                UpdateStatusImages();
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to load full summary: {error}");
                // Hide all images on error
                if (hensWithEggReadyImage != null) hensWithEggReadyImage.SetActive(false);
                if (hensNeedingFoodImage != null) hensNeedingFoodImage.SetActive(false);
                if (hensNeedingCleanImage != null) hensNeedingCleanImage.SetActive(false);
            }
        );
    }

    // =========================
    // Update Status Images
    // =========================
    private void UpdateStatusImages()
    {
        if (playerWallet == null) return;

        // Show image when count is 0 (nothing to collect/do)
        // Hide image when count > 0 (work available)
        
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

        // ✅ Check food inventory
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

        // Disable button to prevent multiple clicks
        simonButton.interactable = false;

        feedingManager.FeedAll(
            onSuccess: (response) => 
            {
                Debug.Log("✅ Feeding successful!");
                
                // ✅ STEP 1: Refresh Inventory first (updates food counts)
                if (inventoryManager != null)
                {
                    inventoryManager.GetInventory(
                        onSuccess: (inventoryResponse) =>
                        {
                            Debug.Log("✅ Inventory refreshed after feeding");
                            
                            // ✅ STEP 2: Then refresh FullSummary
                            RefreshSummaryAndPlayAnimation(simonButton, afterFeedingVideo);
                        },
                        onError: (inventoryError) =>
                        {
                            Debug.LogError($"❌ Failed to refresh inventory: {inventoryError}");
                            // Continue to summary refresh anyway
                            RefreshSummaryAndPlayAnimation(simonButton, afterFeedingVideo);
                        }
                    );
                }
                else
                {
                    // No inventory manager, just do summary
                    RefreshSummaryAndPlayAnimation(simonButton, afterFeedingVideo);
                }
            },
            onError: (error) => 
            {
                Debug.LogError($"❌ Feeding failed: {error}");
                // Re-enable button on error
                simonButton.interactable = true;
            }
        );
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

        // Disable button to prevent multiple clicks
        collectButton.interactable = false;

        collectingManager.CollectAll(
            onSuccess: (response) => 
            {
                Debug.Log($"✅ Collection successful! Collected: {response.collected} eggs, Basket total: {response.basketEggCount}");
                
                // Refresh FullSummary to update UI and play animation
                RefreshSummaryAndPlayAnimation(collectButton, afterCollectingVideo);
            },
            onError: (error) => 
            {
                Debug.LogError($"❌ Collection failed: {error}");
                // Re-enable button on error
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

        // Disable button to prevent multiple clicks
        matchingButton.interactable = false;

        cleanupManager.CleanAll(
            onSuccess: (response) => 
            {
                Debug.Log("✅ Cleanup successful!");
                
                // Refresh FullSummary to update UI and play animation
                RefreshSummaryAndPlayAnimation(matchingButton, afterCleanupVideo);
            },
            onError: (error) => 
            {
                Debug.LogError($"❌ Cleanup failed: {error}");
                // Re-enable button on error
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
                    Debug.Log("✅ Full Summary refreshed");
                    // Play animation after successful refresh
                    PlayAnimationVideo(videoClip);
                    // Re-enable button
                    button.interactable = true;
                },
                onError: (summaryError) => 
                {
                    Debug.LogError($"❌ Failed to refresh summary: {summaryError}");
                    // Play animation even on error
                    PlayAnimationVideo(videoClip);
                    // Re-enable button
                    button.interactable = true;
                }
            );
        }
        else
        {
            Debug.LogWarning("⚠️ FullSummaryManager not assigned!");
            // Play animation anyway
            PlayAnimationVideo(videoClip);
            // Re-enable button
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
        
        Debug.Log($"🎬 Playing video: {videoClip.name}");
        
        // Hide main panel, show animation panel
        mainPanel.SetActive(false);
        panelAnim.SetActive(true);
        
        // Stop any currently playing video
        videoPlayer.Stop();
        
        // Set the new video clip and play
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
    }
    
    private void OnVideoFinished(UnityEngine.Video.VideoPlayer vp)
    {
        Debug.Log($"🎬 Video finished");
        
        // Hide animation panel, show main panel
        panelAnim.SetActive(false);
        mainPanel.SetActive(true);
        
        // Stop the video
        vp.Stop();
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
        
        // Check if RenderTexture already exists
        if (videoPlayer.targetTexture == null)
        {
            // Create a new RenderTexture
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            renderTexture.name = "VideoRenderTexture";
            
            // Assign to VideoPlayer
            videoPlayer.targetTexture = renderTexture;
            
            Debug.Log("✅ Created RenderTexture for VideoPlayer: 1920x1080");
        }
        
        // Assign the same RenderTexture to RawImage
        rawImage.texture = videoPlayer.targetTexture;
        
        Debug.Log("✅ VideoPlayer and RawImage connected via RenderTexture");
    }
}