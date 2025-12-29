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
            
            AutoFindStatusImages();
        }
        
        if (yesButton)
            yesButton.onClick.AddListener(OnYesClicked);
        
        if (noButton)
            noButton.onClick.AddListener(OnNoClicked);
    }
    
    void AutoFindStatusImages()
    {
        GameObject infoPanel = GameObject.Find("InfoSetItemToFarm");
        if (infoPanel == null) infoPanel = GameObject.Find("infoSetItemToFarm");
        if (infoPanel == null) infoPanel = GameObject.Find("InfoSetItemtofarm");
        
        if (infoPanel != null)
        {
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
        }
    }

   void UpdateStatusTexts(bool robotExists, int daysLeft)
    {
        if (robotStatusText != null)
        {
            string format = LanguageManager.Instance.GetTranslation("Status_Robot");
            robotStatusText.text = string.Format(format, robotExists ? 1 : 0);
        }

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
            return;
        }
        
        int currentFarmIndex = farmDatabase.currentFarmIndex;
        int farmNumber = currentFarmIndex + 1;
        
        farmAPIManager.GetFarmSummary(
            farmNumber,
            onSuccess: (summary) =>
            {
                UpdateStatusImagesFromSummary(summary);
            },
            onError: (error) =>
            {
                UpdateStatusImagesFromLocal();
            }
        );
    }
    
    /// <summary>
    /// ✅ Update images based on backend farm summary
    /// </summary>
    void UpdateStatusImagesFromSummary(FarmSummary summary)
    {
        bool hasRobot = summary.robot != null && !string.IsNullOrEmpty(summary.robot.id);
        if (robotExistImage != null)
            robotExistImage.SetActive(hasRobot);
        
        if (robotNotExistImage != null)
            robotNotExistImage.SetActive(!hasRobot);
        
        int daysLeft = hasRobot ? Mathf.Max(summary.robot.daysLeftToNextCharge, 0) : 0;
        UpdateStatusTexts(hasRobot, daysLeft);
        int batteryLevel = 0;
        string batteryType = "normal";

        if (hasRobot && summary.robot.isActive)
        {
            batteryLevel = CalculateBatteryLevelFromDays(
                summary.robot.daysLeftToNextCharge,
                summary.robot.poweredUntil
            );

            batteryType = summary.robot.batteryType ?? "normal";
        }

        UpdateBatteryImages(batteryLevel, batteryType);
    }
    
    /// <summary>
    /// ✅ Calculate battery level (0-4) from days remaining (provided by backend)
    /// </summary>
    int CalculateBatteryLevelFromDays(int daysRemaining, string poweredUntil)
    {
        // 🔧 FIX: backend rounds down (<1 day → 0)
        if (daysRemaining <= 0 && !string.IsNullOrEmpty(poweredUntil))
        {
            if (DateTime.TryParse(poweredUntil, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime poweredUntilUtc))
            {
                if (poweredUntilUtc > DateTime.UtcNow)
                {
                    // Less than 1 day left → show 1 battery
                    return 1;
                }
            }
        }

        if (daysRemaining <= 0)
            return 0;
        else if (daysRemaining == 1)
            return 1;
        else if (daysRemaining <= 3)
            return 2;
        else if (daysRemaining <= 5)
            return 3;
        else
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
    }
    
    /// <summary>
    /// ✅ Fallback: Update from local FarmDatabase
    /// </summary>
    void UpdateStatusImagesFromLocal()
    {
        if (farmDatabase == null)
        {
            return;
        }
        
        int currentFarmIndex = farmDatabase.currentFarmIndex;
        FarmData currentFarm = farmDatabase.GetFarmByIndex(currentFarmIndex);
        
        if (currentFarm == null)
        {
            return;
        }
        
        // Check if farm has robot (local data)
        bool hasRobot = !string.IsNullOrEmpty(currentFarm.robotType) && currentFarm.robotType != "none";
        
        // Update robot images
        if (robotExistImage != null)
            robotExistImage.SetActive(hasRobot);
        
        if (robotNotExistImage != null)
            robotNotExistImage.SetActive(!hasRobot);
        
        UpdateBatteryImages(0, "normal");
    }
    
    public void SetPendingItem(string productId)
    {
        pendingProductId = productId;
        
        if (productId == "battery" || productId == "super_battery")
        {
            // Hide Yes/No buttons for batteries (robot handles them automatically)
            if (yesButton != null)
                yesButton.gameObject.SetActive(false);
            
            if (noButton != null)
                noButton.gameObject.SetActive(false);
        }
        else
        {
            if (yesButton != null)
                yesButton.gameObject.SetActive(true);
            
            if (noButton != null)
                noButton.gameObject.SetActive(true);
            
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
            return;
        }
        
        if (string.IsNullOrEmpty(pendingProductId))
        {
            return;
        }
        
        if (!ValidateReferences())
        {
            return;
        }
        
        if (farmDatabase == null)
        {
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("System error: FarmDatabase not found");
            }
            return;
        }
        
        int currentFarmIndex = farmDatabase.currentFarmIndex;
        int targetFarmNumber = currentFarmIndex + 1; // Convert 0-based to 1-based
        
        if (wallet == null || wallet.GetItemCount(pendingProductId) <= 0)
        {
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("You don't have this item.");
            }
            return;
        }

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
        
        string itemType = MapProductIdToItemType(pendingProductId);
        string tier = MapProductIdToTier(pendingProductId);
        
        if (string.IsNullOrEmpty(itemType))
        {
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault($"Invalid item type: {pendingProductId}");
            }
            isProcessing = false;
            ShowLoading(false);
            yield break;
        }
        
        var requestBody = new PlaceFarmRequest
        {
            itemType = itemType,
            tier = tier,
            quantity = 1, // Robot is always 1
            farmNumber = farmNumber
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("Authentication error");
            }
            isProcessing = false;
            ShowLoading(false);
            yield break;
        }
        
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        string url = apiConfig.baseUrl + "/farm/place";
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        
        yield return request.SendWebRequest();
        
        ShowLoading(false);
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            
            HandlePlacementSuccess(responseText, farmNumber);
        }
        else
        {
            if (request.responseCode == 400)
            {
                string errorResponse = request.downloadHandler.text;
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
                if (wallet != null)
                {
                    wallet.TryConsumeItem(pendingProductId, 1);
                }
                
                UpdateLocalFarmDatabase(farmNumber, pendingProductId);
                UpdateStatusImages();
                
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenInfoSetItemToFarm("Item successfully added to the farm! You want to add more?");
                }
                
                StartCoroutine(RefreshFarmData(farmNumber));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to parse response: {e.Message}");
        }
    }
    
    void UpdateLocalFarmDatabase(int farmNumber, string productId)
    {
        if (farmDatabase == null) return;
        
        int farmIndex = farmNumber - 1;
        FarmData farm = farmDatabase.GetFarmByIndex(farmIndex);
        
        if (farm != null)
        {
            farm.ApplyItem(productId);
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
                },
                onError: (error) => {
                    inventoryRefreshed = true;
                }
            );
            
            float timeout = 3f;
            float elapsed = 0f;
            while (!inventoryRefreshed && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
        }
        
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
                },
                onError: (error) => {
                    farmRefreshed = true;
                }
            );
            
            float timeout = 3f;
            float elapsed = 0f;
            while (!farmRefreshed && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
        }
        
        if (farmGridManager != null)
        {
            int farmIndex = farmNumber - 1;
            farmGridManager.RefreshFarmDisplay(farmIndex);
        }
        
        if (farmHeaderManager != null)
        {
            farmHeaderManager.UpdateAllFarmSlotVisuals();
        }
        
        UpdateStatusImages();
    }
    
    bool ValidateReferences()
    {
        if (wallet == null)
        {
            return false;
        }
        
        if (apiConfig == null)
        {
            return false;
        }
        
        if (farmDatabase == null)
        {
            return false;
        }
        
        return true;
    }
    
    string MapProductIdToItemType(string productId)
    {
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace(" ", "");
        
        if (normalized.Contains("robot"))
        {
            return "robot";
        }
        if (normalized.Contains("battery"))
        {
            return null;
        }
        
        return null;
    }
    
    string MapProductIdToTier(string productId)
    {
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