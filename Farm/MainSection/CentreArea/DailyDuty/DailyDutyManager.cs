using UnityEngine;
using UnityEngine.UI;

public class DailyDutyManager : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject mainPanel;
    public GameObject gamesPanel;

    [Header("Buttons (on Main Panel)")]
    public Button simonButton;
    public Button collectButton;
    public Button matchingButton;

    [Header("Mini Game Manager Reference")]
    public MiniGamePanelManager miniGameManager;

    [Header("Full Summary")]
    public FullSummaryManager fullSummaryManager;
    public PlayerWallet playerWallet;

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
        simonButton.onClick.AddListener(OpenSimon);
        collectButton.onClick.AddListener(OpenCollect);
        matchingButton.onClick.AddListener(OpenMatching);

        // Load full summary data
        LoadFullSummary();

        // Start with only the main panel visible
        OpenMainPanel();
        
        // 🔍 Initial check
        CheckFoodInventory();
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
    // Open Methods
    // =========================
    public void OpenSimon()
    {
        mainPanel.SetActive(false);
        gamesPanel.SetActive(true);
        miniGameManager.OpenSimon();
    }

    public void OpenCollect()
    {
        mainPanel.SetActive(false);
        gamesPanel.SetActive(true);
        miniGameManager.OpenCollect();
    }

    public void OpenMatching()
    {
        mainPanel.SetActive(false);
        gamesPanel.SetActive(true);
        miniGameManager.OpenMatching();
    }

    // =========================
    // Open Main Panel
    // =========================
    public void OpenMainPanel()
    {
        gamesPanel.SetActive(false);
        mainPanel.SetActive(true);
        
        // Refresh summary when returning to main panel
        LoadFullSummary();
    }
}