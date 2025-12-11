using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;
using TMPro;

public class InventoryFarmItemApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FarmDatabase farmDatabase;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private InfoErrorChanger infoErrorChanger;
    [SerializeField] private InventoryItemsBarChanger inventoryBarChanger;
    [SerializeField] private APIConfig apiConfig;
    [SerializeField] private FarmAPIManager farmAPIManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private FarmGridManager farmGridManager;
    [SerializeField] private FarmHeaderManager farmHeaderManager;
    
    [Header("Buttons")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [Header("Auto Find")]
    [SerializeField] private bool autoFind = true;
    
    [Header("Loading")]
    [SerializeField] private GameObject loadingIndicator;
    
    [Header("Status Images - Robot")]
    [SerializeField] private GameObject robotExistImage;
    [SerializeField] private GameObject robotNotExistImage;

    [Header("Status Texts")]
    [SerializeField] private TMP_Text robotStatusText;
    [SerializeField] private TMP_Text batteryStatusText;

    
    [Header("Status Images - Battery (1-4)")]
    [SerializeField] private GameObject battery1NotExistImage;
    [SerializeField] private GameObject battery1ExistNormalImage;
    [SerializeField] private GameObject battery1ExistPremiumImage;
    [SerializeField] private GameObject battery2NotExistImage;
    [SerializeField] private GameObject battery2ExistNormalImage;
    [SerializeField] private GameObject battery2ExistPremiumImage;
    [SerializeField] private GameObject battery3NotExistImage;
    [SerializeField] private GameObject battery3ExistNormalImage;
    [SerializeField] private GameObject battery3ExistPremiumImage;
    [SerializeField] private GameObject battery4NotExistImage;
    [SerializeField] private GameObject battery4ExistNormalImage;
    [SerializeField] private GameObject battery4ExistPremiumImage;
    
    private string pendingProductId = "";
    private bool isProcessing = false;
    
    void Awake()
    {
        if (autoFind)
        {
            if (!farmDatabase)
                farmDatabase = FindAnyObjectByType<FarmDatabase>();
            
            if (!wallet)
                wallet = FindAnyObjectByType<PlayerWallet>();
            
            if (!infoErrorChanger)
                infoErrorChanger = FindAnyObjectByType<InfoErrorChanger>();
            
            if (!inventoryBarChanger)
                inventoryBarChanger = FindAnyObjectByType<InventoryItemsBarChanger>();
            
            if (!apiConfig)
                apiConfig = Resources.Load<APIConfig>("APIConfig");
            
            if (!farmAPIManager)
                farmAPIManager = FindAnyObjectByType<FarmAPIManager>();
            
            if (!inventoryManager)
                inventoryManager = FindAnyObjectByType<InventoryManager>();
            
            if (!farmGridManager)
                farmGridManager = FindAnyObjectByType<FarmGridManager>();
            
            if (!farmHeaderManager)
                farmHeaderManager = FindAnyObjectByType<FarmHeaderManager>();
            
            // ✅ Auto-find status images if not assigned
            AutoFindStatusImages();
        }
        
        // Hook up buttons
        if (yesButton)
            yesButton.onClick.AddListener(OnYesClicked);
        
        if (noButton)
            noButton.onClick.AddListener(OnNoClicked);
    }
    
    void AutoFindStatusImages()
    {
        // Try to find status images in the InfoSetItemToFarm panel
        GameObject infoPanel = GameObject.Find("InfoSetItemToFarm");
        if (infoPanel == null) infoPanel = GameObject.Find("infoSetItemToFarm");
        if (infoPanel == null) infoPanel = GameObject.Find("InfoSetItemtofarm");
        
        if (infoPanel != null)
        {
            // Find robot images
            if (!robotExistImage)
            {
                robotExistImage = FindChildByName(infoPanel.transform, "RobotExist");
                if (!robotExistImage) robotExistImage = FindChildByName(infoPanel.transform, "ImageRobotExist");
                if (!robotExistImage) robotExistImage = FindChildByName(infoPanel.transform, "Robot_Exist");
            }
            
            if (!robotNotExistImage)
            {
                robotNotExistImage = FindChildByName(infoPanel.transform, "RobotNotExist");
                if (!robotNotExistImage) robotNotExistImage = FindChildByName(infoPanel.transform, "ImageRobotNotExist");
                if (!robotNotExistImage) robotNotExistImage = FindChildByName(infoPanel.transform, "Robot_NotExist");
            }
            
            // Find battery images (1-4) - Normal and Premium variants
            string[] batteryNames = { "Battery1", "Battery2", "Battery3", "Battery4" };
            GameObject[] notExistImages = { battery1NotExistImage, battery2NotExistImage, battery3NotExistImage, battery4NotExistImage };
            GameObject[] existNormalImages = { battery1ExistNormalImage, battery2ExistNormalImage, battery3ExistNormalImage, battery4ExistNormalImage };
            GameObject[] existPremiumImages = { battery1ExistPremiumImage, battery2ExistPremiumImage, battery3ExistPremiumImage, battery4ExistPremiumImage };
            
            for (int i = 0; i < 4; i++)
            {
                // Not Exist
                if (notExistImages[i] == null)
                {
                    notExistImages[i] = FindChildByName(infoPanel.transform, $"Image{i + 1}NotExist");
                    if (!notExistImages[i]) notExistImages[i] = FindChildByName(infoPanel.transform, $"{batteryNames[i]}NotExist");
                    if (!notExistImages[i]) notExistImages[i] = FindChildByName(infoPanel.transform, $"Battery{i + 1}NotExist");
                }
                
                // Exist Normal
                if (existNormalImages[i] == null)
                {
                    existNormalImages[i] = FindChildByName(infoPanel.transform, $"Image{i + 1}ExistNormal");
                    if (!existNormalImages[i]) existNormalImages[i] = FindChildByName(infoPanel.transform, $"{batteryNames[i]}ExistNormal");
                    if (!existNormalImages[i]) existNormalImages[i] = FindChildByName(infoPanel.transform, $"Battery{i + 1}ExistNormal");
                }
                
                // Exist Premium
                if (existPremiumImages[i] == null)
                {
                    existPremiumImages[i] = FindChildByName(infoPanel.transform, $"Image{i + 1}ExistPremium");
                    if (!existPremiumImages[i]) existPremiumImages[i] = FindChildByName(infoPanel.transform, $"{batteryNames[i]}ExistPremium");
                    if (!existPremiumImages[i]) existPremiumImages[i] = FindChildByName(infoPanel.transform, $"Battery{i + 1}ExistPremium");
                }
            }
            
            battery1NotExistImage = notExistImages[0];
            battery2NotExistImage = notExistImages[1];
            battery3NotExistImage = notExistImages[2];
            battery4NotExistImage = notExistImages[3];
            
            battery1ExistNormalImage = existNormalImages[0];
            battery2ExistNormalImage = existNormalImages[1];
            battery3ExistNormalImage = existNormalImages[2];
            battery4ExistNormalImage = existNormalImages[3];
            
            battery1ExistPremiumImage = existPremiumImages[0];
            battery2ExistPremiumImage = existPremiumImages[1];
            battery3ExistPremiumImage = existPremiumImages[2];
            battery4ExistPremiumImage = existPremiumImages[3];
            
            Debug.Log($"✅ Status images found: Robot Exist={robotExistImage != null}, Robot NotExist={robotNotExistImage != null}");
            Debug.Log($"✅ Battery Normal images: B1={battery1ExistNormalImage != null}, B2={battery2ExistNormalImage != null}, B3={battery3ExistNormalImage != null}, B4={battery4ExistNormalImage != null}");
            Debug.Log($"✅ Battery Premium images: B1={battery1ExistPremiumImage != null}, B2={battery2ExistPremiumImage != null}, B3={battery3ExistPremiumImage != null}, B4={battery4ExistPremiumImage != null}");
        }
    }

   void UpdateStatusTexts(bool robotExists, int daysLeft)
    {
        // Robot text
        if (robotStatusText != null)
        {
            string format = LanguageManager.Instance.GetTranslation("Status_Robot");
            robotStatusText.text = string.Format(format, robotExists ? 1 : 0);
        }

        // Battery text
        if (batteryStatusText != null)
        {
            string format = LanguageManager.Instance.GetTranslation("Status_Battery");
            batteryStatusText.text = string.Format(format, Mathf.Max(daysLeft, 0));
        }
    }


    
    GameObject FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child.gameObject;
            
            GameObject result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
    
    /// <summary>
    /// ✅ PUBLIC method to update status images from backend data
    /// </summary>
    public void UpdateStatusImages()
    {
        if (farmDatabase == null || farmAPIManager == null)
        {
            Debug.LogWarning("⚠️ Missing references, cannot update status images");
            return;
        }
        
        int currentFarmIndex = farmDatabase.currentFarmIndex;
        int farmNumber = currentFarmIndex + 1;
        
        // ✅ Fetch farm summary from backend to get real robot/battery status
        farmAPIManager.GetFarmSummary(
            farmNumber,
            onSuccess: (summary) =>
            {
                UpdateStatusImagesFromSummary(summary);
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to fetch farm summary: {error}");
                // Fallback to local data
                UpdateStatusImagesFromLocal();
            }
        );
    }
    
    /// <summary>
    /// ✅ Update images based on backend farm summary
    /// </summary>
    void UpdateStatusImagesFromSummary(FarmSummary summary)
    {
        // Check if robot exists
        bool hasRobot = summary.robot != null && !string.IsNullOrEmpty(summary.robot.id);
        
        // Update robot images
        if (robotExistImage != null)
            robotExistImage.SetActive(hasRobot);
        
        if (robotNotExistImage != null)
            robotNotExistImage.SetActive(!hasRobot);
        
        Debug.Log($"🤖 Robot: {(hasRobot ? "EXISTS" : "NOT EXISTS")}");

        // --- UPDATE TEXTS (Robot: X / Batteries: Y days) ---
        int daysLeft = hasRobot ? Mathf.Max(summary.robot.daysLeftToNextCharge, 0) : 0;
        UpdateStatusTexts(hasRobot, daysLeft);
        // ---------------------------------------------------

        // Get battery level + type
        int batteryLevel = 0;
        string batteryType = "normal";

        if (hasRobot && summary.robot.isActive)
        {
            batteryLevel = CalculateBatteryLevelFromDays(summary.robot.daysLeftToNextCharge);
            batteryType = summary.robot.batteryType ?? "normal";

            Debug.Log($"🔋 Battery Level: {batteryLevel}/4 (Days left: {summary.robot.daysLeftToNextCharge}, Type: {batteryType})");
        }

        // Update battery images visually
        UpdateBatteryImages(batteryLevel, batteryType);
    }
    
    /// <summary>
    /// ✅ Calculate battery level (0-4) from days remaining (provided by backend)
    /// </summary>
    int CalculateBatteryLevelFromDays(int daysRemaining)
    {
        // 7 days max = 4 batteries
        // Each battery represents ~1.75 days
        if (daysRemaining <= 0)
            return 0;
        else if (daysRemaining == 1)
            return 1;
        else if (daysRemaining >= 2 && daysRemaining <= 3)
            return 2;
        else if (daysRemaining >= 4 && daysRemaining <= 5)
            return 3;
        else // 6-7 days
            return 4;
    }
    
    /// <summary>
    /// ✅ Update battery images based on level (0-4) and type (normal/premium)
    /// </summary>
    void UpdateBatteryImages(int level, string batteryType)
    {
        bool isPremium = batteryType == "premium";
        
        // Battery 1
        if (battery1NotExistImage != null)
            battery1NotExistImage.SetActive(level < 1);
        if (battery1ExistNormalImage != null)
            battery1ExistNormalImage.SetActive(level >= 1 && !isPremium);
        if (battery1ExistPremiumImage != null)
            battery1ExistPremiumImage.SetActive(level >= 1 && isPremium);
        
        // Battery 2
        if (battery2NotExistImage != null)
            battery2NotExistImage.SetActive(level < 2);
        if (battery2ExistNormalImage != null)
            battery2ExistNormalImage.SetActive(level >= 2 && !isPremium);
        if (battery2ExistPremiumImage != null)
            battery2ExistPremiumImage.SetActive(level >= 2 && isPremium);
        
        // Battery 3
        if (battery3NotExistImage != null)
            battery3NotExistImage.SetActive(level < 3);
        if (battery3ExistNormalImage != null)
            battery3ExistNormalImage.SetActive(level >= 3 && !isPremium);
        if (battery3ExistPremiumImage != null)
            battery3ExistPremiumImage.SetActive(level >= 3 && isPremium);
        
        // Battery 4
        if (battery4NotExistImage != null)
            battery4NotExistImage.SetActive(level < 4);
        if (battery4ExistNormalImage != null)
            battery4ExistNormalImage.SetActive(level >= 4 && !isPremium);
        if (battery4ExistPremiumImage != null)
            battery4ExistPremiumImage.SetActive(level >= 4 && isPremium);
        
        Debug.Log($"✅ Battery images updated: Level {level}/4, Type: {batteryType}");
    }
    
    /// <summary>
    /// ✅ Fallback: Update from local FarmDatabase
    /// </summary>
    void UpdateStatusImagesFromLocal()
    {
        if (farmDatabase == null)
        {
            Debug.LogWarning("⚠️ FarmDatabase not found");
            return;
        }
        
        int currentFarmIndex = farmDatabase.currentFarmIndex;
        FarmData currentFarm = farmDatabase.GetFarmByIndex(currentFarmIndex);
        
        if (currentFarm == null)
        {
            Debug.LogWarning($"⚠️ Current farm is null (index: {currentFarmIndex})");
            return;
        }
        
        // Check if farm has robot (local data)
        bool hasRobot = !string.IsNullOrEmpty(currentFarm.robotType) && currentFarm.robotType != "none";
        
        // Update robot images
        if (robotExistImage != null)
            robotExistImage.SetActive(hasRobot);
        
        if (robotNotExistImage != null)
            robotNotExistImage.SetActive(!hasRobot);
        
        // For local fallback, show 0 batteries (we don't have accurate data)
        UpdateBatteryImages(0, "normal");
        
        Debug.Log($"✅ Status images updated from local - Robot: {hasRobot} (No battery data available locally)");
    }
    
    public void SetPendingItem(string productId)
    {
        pendingProductId = productId;
        
        // ✅ Check if it's a battery - if so, hide Yes/No buttons
        if (productId == "battery" || productId == "super_battery")
        {
            // Hide Yes/No buttons for batteries (robot handles them automatically)
            if (yesButton != null)
                yesButton.gameObject.SetActive(false);
            
            if (noButton != null)
                noButton.gameObject.SetActive(false);
            
            Debug.Log("🔋 Battery detected - hiding Yes/No buttons");
        }
        else
        {
            // Show Yes/No buttons for robot
            if (yesButton != null)
                yesButton.gameObject.SetActive(true);
            
            if (noButton != null)
                noButton.gameObject.SetActive(true);
            
            // ✅ Update status images when setting pending item (when panel opens)
            UpdateStatusImages();
        }
    }
    
    void OnNoClicked()
    {
        pendingProductId = "";
        
        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
        }
    }
    
    void OnYesClicked()
    {
        if (isProcessing)
        {
            Debug.LogWarning("⚠️ Already processing a request");
            return;
        }
        
        if (string.IsNullOrEmpty(pendingProductId))
        {
            Debug.LogWarning("⚠️ No pending product ID!");
            return;
        }
        
        // Validate references
        if (!ValidateReferences())
        {
            Debug.LogError("❌ Missing required references!");
            return;
        }
        
        // ✅ Get current farm number from FarmDatabase (same as vitamin logic)
        if (farmDatabase == null)
        {
            Debug.LogError("❌ FarmDatabase not found!");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("System error: FarmDatabase not found");
            }
            return;
        }
        
        int currentFarmIndex = farmDatabase.currentFarmIndex;
        int targetFarmNumber = currentFarmIndex + 1; // Convert 0-based to 1-based
        
        Debug.Log($"🎯 Applying {pendingProductId} to Farm {targetFarmNumber} (index: {currentFarmIndex})");
        
        // Check if player has the item
        if (wallet == null || wallet.GetItemCount(pendingProductId) <= 0)
        {
            Debug.LogWarning($"⚠️ Player doesn't have {pendingProductId}");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("You don't have this item.");
            }
            return;
        }

        // ✅ Call backend API to place the item
        PlaceItemInFarm(targetFarmNumber);
    }
    
    void PlaceItemInFarm(int farmNumber)
    {
        StartCoroutine(PlaceItemCoroutine(farmNumber));
    }
    
    IEnumerator PlaceItemCoroutine(int farmNumber)
    {
        isProcessing = true;
        ShowLoading(true);
        
        // Map product ID to backend format
        string itemType = MapProductIdToItemType(pendingProductId);
        string tier = MapProductIdToTier(pendingProductId);
        
        if (string.IsNullOrEmpty(itemType))
        {
            Debug.LogError($"❌ Invalid item type: {pendingProductId}");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault($"Invalid item type: {pendingProductId}");
            }
            isProcessing = false;
            ShowLoading(false);
            yield break;
        }
        
        // Create request body
        var requestBody = new PlaceFarmRequest
        {
            itemType = itemType,
            tier = tier,
            quantity = 1, // Robot is always 1
            farmNumber = farmNumber
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log($"📤 Sending request: {jsonBody}");
        
        // Get access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("❌ Not authenticated - no access token!");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("Authentication error");
            }
            isProcessing = false;
            ShowLoading(false);
            yield break;
        }
        
        // Prepare request
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        string url = apiConfig.baseUrl + "/farm/place";
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        
        // Send request
        yield return request.SendWebRequest();
        
        ShowLoading(false);
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log($"✅ Success response: {responseText}");
            
            HandlePlacementSuccess(responseText, farmNumber);
        }
        else
        {
            // Check if it's a 400 error (item already exists)
            if (request.responseCode == 400)
            {
                string errorResponse = request.downloadHandler.text;
                Debug.LogWarning($"⚠️ Item already exists: {errorResponse}");
                
                // Try to parse error message
                try
                {
                    var errorObj = JsonUtility.FromJson<ErrorResponse>(errorResponse);
                    if (errorObj != null && !string.IsNullOrEmpty(errorObj.message))
                    {
                        if (infoErrorChanger != null)
                        {
                            infoErrorChanger.OpenErrorDefault(errorObj.message);
                        }
                    }
                    else
                    {
                        if (infoErrorChanger != null)
                        {
                            infoErrorChanger.OpenErrorDefault("This item is already applied to that farm.");
                        }
                    }
                }
                catch
                {
                    if (infoErrorChanger != null)
                    {
                        infoErrorChanger.OpenErrorDefault("This item is already applied to that farm.");
                    }
                }
            }
            else
            {
                string errorMsg = request.error;
                Debug.LogError($"❌ Request failed: {errorMsg}");
                Debug.LogError($"❌ Response code: {request.responseCode}");
                
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenErrorDefault($"Failed to apply item: {errorMsg}");
                }
            }
        }
        
        isProcessing = false;
    }
    
    void HandlePlacementSuccess(string responseJson, int farmNumber)
    {
        try
        {
            var response = JsonUtility.FromJson<PlaceFarmResponse>(responseJson);
            
            if (response != null && response.placed > 0)
            {
                Debug.Log($"✅ Successfully placed {response.placed} {response.type}(s) in farm {response.farmNumber}");
                
                // Update local wallet (optimistic update)
                if (wallet != null)
                {
                    wallet.TryConsumeItem(pendingProductId, 1);
                }
                
                // Update local FarmDatabase
                UpdateLocalFarmDatabase(farmNumber, pendingProductId);
                
                // ✅ Update status images AFTER successful placement
                UpdateStatusImages();
                
                // Show success message
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenInfoSetItemToFarm("Item successfully added to the farm! You want to add more?");
                }
                
                // Refresh data from backend
                StartCoroutine(RefreshFarmData(farmNumber));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to parse response: {e.Message}");
            Debug.LogError($"Response was: {responseJson}");
        }
    }
    
    void UpdateLocalFarmDatabase(int farmNumber, string productId)
    {
        if (farmDatabase == null) return;
        
        int farmIndex = farmNumber - 1;
        FarmData farm = farmDatabase.GetFarmByIndex(farmIndex);
        
        if (farm != null)
        {
            // Apply item locally
            farm.ApplyItem(productId);
            Debug.Log($"✅ Updated local FarmDatabase: {productId} applied to farm {farmNumber}");
        }
    }
    
    IEnumerator RefreshFarmData(int farmNumber)
    {
        // Step 1: Refresh inventory
        if (inventoryManager != null)
        {
            bool inventoryRefreshed = false;
            
            inventoryManager.GetInventory(
                onSuccess: (response) => {
                    inventoryRefreshed = true;
                    Debug.Log("✅ Inventory refreshed");
                },
                onError: (error) => {
                    inventoryRefreshed = true;
                    Debug.LogWarning($"⚠️ Inventory refresh failed: {error}");
                }
            );
            
            // Wait for inventory refresh (with timeout)
            float timeout = 3f;
            float elapsed = 0f;
            while (!inventoryRefreshed && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
        }
        
        // Step 2: Fetch farm summary from backend
        if (farmAPIManager != null && farmDatabase != null)
        {
            bool farmRefreshed = false;
            
            farmAPIManager.GetFarmSummary(
                farmNumber,
                onSuccess: (summary) => {
                    int farmIndex = farmNumber - 1;
                    FarmData farm = farmDatabase.GetFarmByIndex(farmIndex);
                    
                    if (farm != null)
                    {
                        // Update farm with backend data including robot info
                        farm.hasRobot = summary.robot != null && !string.IsNullOrEmpty(summary.robot.id);
                        farm.robotActive = summary.robot != null && summary.robot.isActive;
                        farm.robotPoweredUntil = summary.robot != null ? summary.robot.poweredUntil : "";
                        
                        farmDatabase.UpdateFarmFromBackend(
                            farmIndex: farmIndex,
                            nests: summary.nests.total,
                            champChicks: summary.henStats.byKind.Champ,
                            normalChicks: summary.henStats.byKind.Normal,
                            legendChicks: summary.henStats.byKind.Legend,
                            superLegendChicks: summary.henStats.byKind.SuperLegend
                        );
                    }
                    
                    farmRefreshed = true;
                    Debug.Log("✅ Farm data refreshed from backend");
                },
                onError: (error) => {
                    farmRefreshed = true;
                    Debug.LogError($"❌ Failed to fetch farm summary: {error}");
                }
            );
            
            // Wait for farm refresh (with timeout)
            float timeout = 3f;
            float elapsed = 0f;
            while (!farmRefreshed && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
        }
        
        // Step 3: Refresh UI displays
        if (farmGridManager != null)
        {
            int farmIndex = farmNumber - 1;
            farmGridManager.RefreshFarmDisplay(farmIndex);
        }
        
        if (farmHeaderManager != null)
        {
            farmHeaderManager.UpdateAllFarmSlotVisuals();
        }
        
        // Step 4: Update status images one more time after refresh
        UpdateStatusImages();
    }
    
    bool ValidateReferences()
    {
        if (wallet == null)
        {
            Debug.LogError("❌ PlayerWallet not assigned!");
            return false;
        }
        
        if (apiConfig == null)
        {
            Debug.LogError("❌ APIConfig not assigned!");
            return false;
        }
        
        if (farmDatabase == null)
        {
            Debug.LogError("❌ FarmDatabase not assigned!");
            return false;
        }
        
        return true;
    }
    
    string MapProductIdToItemType(string productId)
    {
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace(" ", "");
        
        // ROBOT ONLY
        if (normalized.Contains("robot"))
        {
            return "robot";
        }
        
        // BATTERY - Not supported via this flow (only robot can be placed)
        if (normalized.Contains("battery"))
        {
            Debug.LogWarning($"⚠️ Battery placement not supported through this interface");
            return null;
        }
        
        Debug.LogWarning($"⚠️ Unknown product ID: {productId}");
        return null;
    }
    
    string MapProductIdToTier(string productId)
    {
        // ROBOT - always normal tier
        return "normal";
    }
    
    void ShowLoading(bool show)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(show);
        }
        
        if (yesButton != null)
        {
            yesButton.interactable = !show;
        }
        
        if (noButton != null)
        {
            noButton.interactable = !show;
        }
    }
    
    // ===========================
    // Request/Response Classes
    // ===========================
    
    [Serializable]
    private class PlaceFarmRequest
    {
        public string itemType;
        public string tier;
        public int quantity;
        public int farmNumber;
    }

    [Serializable]
    private class PlaceFarmResponse
    {
        public int placed;
        public string type;
        public int farmNumber;
    }
    
    [Serializable]
    private class ErrorResponse
    {
        public string message;
        public string error;
        public int statusCode;
    }
}