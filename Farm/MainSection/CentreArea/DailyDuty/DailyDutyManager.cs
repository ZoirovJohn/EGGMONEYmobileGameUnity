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
        // Assign main panel button listeners
        simonButton.onClick.AddListener(OpenSimon);
        collectButton.onClick.AddListener(OpenCollect);
        matchingButton.onClick.AddListener(OpenMatching);

        // Load full summary data
        LoadFullSummary();

        // Start with only the main panel visible
        OpenMainPanel();
    }

    private void OnEnable()
    {
        // Subscribe to profile changes to update images
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged += UpdateStatusImages;
            playerWallet.OnItemChanged += OnInventoryItemChanged;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged -= UpdateStatusImages;
            playerWallet.OnItemChanged -= OnInventoryItemChanged;
        }
    }

    private void OnInventoryItemChanged(string itemId, int newValue)
    {
        // Check if the changed item is food or superFood
        if (itemId == "food" || itemId == "super_food")
        {
            CheckFoodInventory();
            Debug.Log($"🔄 Food inventory changed - {itemId}: {newValue}");
        }
    }

    private void CheckFoodInventory()
    {
        if (playerWallet == null || warningMessage == null) return;

        // Check if both food and superFood are 0
        bool noFood = (playerWallet.Food == 0 && playerWallet.SuperFood == 0);
        
        // Turn warning ON if no food, OFF if there is food
        warningMessage.SetActive(noFood);
        
        Debug.Log($"🍗 Food Check - Food: {playerWallet.Food}, SuperFood: {playerWallet.SuperFood}, Warning: {noFood}");
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

        // ✅ NEW: Check food inventory
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