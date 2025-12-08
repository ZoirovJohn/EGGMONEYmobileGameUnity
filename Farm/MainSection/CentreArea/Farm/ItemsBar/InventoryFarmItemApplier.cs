using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;

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
    
    [Header("Status Images in Info Panel")]
    [SerializeField] private GameObject robotExistImage;
    [SerializeField] private GameObject robotNotExistImage;
    [SerializeField] private GameObject batteryExistImage;
    [SerializeField] private GameObject batteryNotExistImage;
    
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
            
            // Find battery images
            if (!batteryExistImage)
            {
                batteryExistImage = FindChildByName(infoPanel.transform, "BatteryExist");
                if (!batteryExistImage) batteryExistImage = FindChildByName(infoPanel.transform, "ImageBatteryExist");
                if (!batteryExistImage) batteryExistImage = FindChildByName(infoPanel.transform, "Battery_Exist");
            }
            
            if (!batteryNotExistImage)
            {
                batteryNotExistImage = FindChildByName(infoPanel.transform, "BatteryNotExist");
                if (!batteryNotExistImage) batteryNotExistImage = FindChildByName(infoPanel.transform, "ImageBatteryNotExist");
                if (!batteryNotExistImage) batteryNotExistImage = FindChildByName(infoPanel.transform, "Battery_NotExist");
            }
            
            Debug.Log($"✅ Status images found: Robot Exist={robotExistImage != null}, Robot NotExist={robotNotExistImage != null}, Battery Exist={batteryExistImage != null}, Battery NotExist={batteryNotExistImage != null}");
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
    /// ✅ PUBLIC method so inventoryFarmInfo can call it when opening the panel
    /// </summary>
    public void UpdateStatusImages()
    {
        if (farmDatabase == null)
        {
            Debug.LogWarning("⚠️ FarmDatabase not found, cannot update status images");
            return;
        }
        
        // Get current farm data using index
        int currentFarmIndex = farmDatabase.currentFarmIndex;
        FarmData currentFarm = farmDatabase.GetFarmByIndex(currentFarmIndex);
        
        if (currentFarm == null)
        {
            Debug.LogWarning($"⚠️ Current farm is null (index: {currentFarmIndex})");
            return;
        }
        
        // ✅ Check if farm has robot
        bool hasRobot = !string.IsNullOrEmpty(currentFarm.robotType) && currentFarm.robotType != "none";
        
        // ✅ Check if farm has battery (any type)
        bool hasBattery = !string.IsNullOrEmpty(currentFarm.batteryType) && currentFarm.batteryType != "none";
        
        Debug.Log($"🔍 Farm {currentFarm.farmName}: Robot={hasRobot} ({currentFarm.robotType}), Battery={hasBattery} ({currentFarm.batteryType})");
        
        // ✅ Update robot images - SHOW exist if has robot, show not-exist if doesn't have robot
        if (robotExistImage != null)
            robotExistImage.SetActive(hasRobot);
        
        if (robotNotExistImage != null)
            robotNotExistImage.SetActive(!hasRobot);
        
        // ✅ Update battery images - SHOW exist if has battery, show not-exist if doesn't have battery
        if (batteryExistImage != null)
            batteryExistImage.SetActive(hasBattery);
        
        if (batteryNotExistImage != null)
            batteryNotExistImage.SetActive(!hasBattery);
        
        Debug.Log($"✅ Status images updated - Robot Exist: {hasRobot}, Battery Exist: {hasBattery}");
    }
    
    public void SetPendingItem(string productId)
    {
        pendingProductId = productId;
        
        // ✅ Update status images when setting pending item (when panel opens)
        UpdateStatusImages();
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
            quantity = 1, // Robot/Battery is always 1
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
                    
                    farmDatabase.UpdateFarmFromBackend(
                        farmIndex: farmIndex,
                        nests: summary.nests.total,
                        champChicks: summary.henStats.byKind.Champ,
                        normalChicks: summary.henStats.byKind.Normal,
                        legendChicks: summary.henStats.byKind.Legend,
                        superLegendChicks: summary.henStats.byKind.SuperLegend
                    );
                    
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