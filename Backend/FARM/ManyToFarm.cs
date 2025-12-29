using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System;

public class ManyToFarm : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] Button putButton;
    [SerializeField] TMP_InputField quantityInput;
    [SerializeField] Button cancelButton;
    [SerializeField] Button btnPlus1;
    [SerializeField] Button btnPlus5;
    [SerializeField] Button btnPlus10;

    [Header("Runtime Data - Auto Set")]
    private InventoryCellId cellId;
    private int targetFarmNumber = 1;
    
    [Header("References")]
    [SerializeField] APIConfig apiConfig;
    [SerializeField] PlayerWallet wallet;
    [SerializeField] FarmAPIManager farmAPIManager;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] FarmDatabase farmDatabase;
    [SerializeField] FarmGridManager farmGridManager;
    [SerializeField] FarmHeaderManager farmHeaderManager;
    [SerializeField] InfoErrorChanger infoErrorChanger;
    
    [Header("Cage Panels")]
    [SerializeField] GameObject bigCageInside2;
    
    [Header("Auto Find")]
    [SerializeField] bool autoFind = true;
    
    private int currentQuantity = 0;
    private bool isProcessing = false;
    private bool justPlacedPremiumNest = false;

    void Awake()
    {
        if (autoFind)
        {
            if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>();
            if (!farmAPIManager) farmAPIManager = FindAnyObjectByType<FarmAPIManager>();
            if (!inventoryManager) inventoryManager = FindAnyObjectByType<InventoryManager>();
            if (!farmDatabase) farmDatabase = FindAnyObjectByType<FarmDatabase>();
            if (!farmGridManager) farmGridManager = FindAnyObjectByType<FarmGridManager>();
            if (!farmHeaderManager) farmHeaderManager = FindAnyObjectByType<FarmHeaderManager>();
            if (!infoErrorChanger) infoErrorChanger = FindAnyObjectByType<InfoErrorChanger>();
            
            if (!apiConfig)
            {
                apiConfig = Resources.Load<APIConfig>("APIConfig");
                if (!apiConfig)
                {
                    Debug.LogError("❌ APIConfig not found! Create one in Resources folder.");
                }
            }
        }
        
        if (cancelButton)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        if (putButton)
        {
            putButton.onClick.AddListener(OnPutButtonClicked);
        }
        
        if (quantityInput)
        {
            quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);
        }

        if (btnPlus1)
            btnPlus1.onClick.AddListener(() => AddQuantity(1));

        if (btnPlus5)
            btnPlus5.onClick.AddListener(() => AddQuantity(5));

        if (btnPlus10)
            btnPlus10.onClick.AddListener(() => AddQuantity(10));
        }

    void OnEnable()
    {
        SetQuantity(0);

        if (cellId == null)
        {
            cellId = FindAnyObjectByType<InventoryCellId>();
            if (cellId != null)
                Debug.Log($"📦 Auto-found selected item: {cellId.productId}");
        }
    }

    void AddQuantity(int amount)
    {
        SetQuantity(currentQuantity + amount);
    }

    void OnCancelClicked()
    {
        if (currentQuantity > 0)
        {
            SetQuantity(0); 
        }
        else
        {
            gameObject.SetActive(false); 
        }
    }

    void SetQuantity(int value)
    {
        int maxAvailable = GetMaxAvailableQuantity();
        currentQuantity = Mathf.Clamp(value, 0, maxAvailable);

        if (quantityInput)
        {
            quantityInput.SetTextWithoutNotify(currentQuantity.ToString());
        }

        UpdateButtonStates();
    }

    void OnQuantityInputChanged(string value)
    {
        if (int.TryParse(value, out int quantity))
            SetQuantity(quantity);
        else
            SetQuantity(0);
    }

    void UpdateButtonStates()
    {
        int maxAvailable = GetMaxAvailableQuantity();

        if (putButton)
        {
            putButton.interactable =
                !isProcessing &&
                currentQuantity > 0 &&
                currentQuantity <= maxAvailable;
        }

        if (btnPlus1)  btnPlus1.interactable  = currentQuantity + 1  <= maxAvailable;
        if (btnPlus5)  btnPlus5.interactable  = currentQuantity + 5  <= maxAvailable;
        if (btnPlus10) btnPlus10.interactable = currentQuantity + 10 <= maxAvailable;
    }

    int GetMaxAvailableQuantity()
    {
        if (!wallet || !cellId || string.IsNullOrEmpty(cellId.productId))
        {
            return 0;
        }
        
        return wallet.GetItemCount(cellId.productId);
    }

    public void OnPutButtonClicked()
    {
        if (isProcessing)
        {
            return;
        }
        
        if (!ValidateReferences())
        {
            return;
        }
        
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            return;
        }
        
        string productId = cellId.productId.ToLower();
        bool isTryingToPlaceHen = productId.Contains("chick") || productId.Contains("hen");
        bool isTryingToPlaceNest = productId.Contains("nest");
        
        if (isTryingToPlaceHen)
        {
            if (IsHenAlreadyInCage())
            {
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenErrorDefault("Cage already has a hen! Select another cage.");
                }
                
                return;
            }
        }
        
        if (isTryingToPlaceNest)
        {
            if (IsNestAlreadyInCage())
            {
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenErrorDefault("Cage already has a nest! Select another cage.");
                }
                
                return;
            }
        }
        
        if (farmDatabase != null)
        {
            targetFarmNumber = farmDatabase.currentFarmIndex + 1;
        }
        else
        {
            Debug.LogError("❌ FarmDatabase not found!");
            return;
        }
        
        int available = wallet.GetItemCount(cellId.productId);
        if (available < currentQuantity)
        {
            return;
        }
        
        PlaceItemsInFarm(
            onSuccess: (response) => {
                Debug.Log("✅ Items placed successfully!");
            },
            onError: (error) => {
                Debug.LogError($"❌ Failed to place items: {error}");
            }
        );
    }
    
    bool IsHenAlreadyInCage()
    {
        if (bigCageInside2 == null)
            return false;
        
        Transform champChick = bigCageInside2.transform.Find("ChampChick");
        if (champChick != null && champChick.gameObject.activeSelf)
        {
            return true;
        }
        
        Transform whiteChick = bigCageInside2.transform.Find("WhiteChick");
        if (whiteChick != null && whiteChick.gameObject.activeSelf)
        {
            return true;
        }
        
        return false;
    }
    
    bool IsNestAlreadyInCage()
    {
        if (bigCageInside2 == null)
            return false;

        Transform normalNest = bigCageInside2.transform.Find("Nest");
        if (normalNest != null && normalNest.gameObject.activeSelf)
            return true;

        Transform premiumNest = bigCageInside2.transform.Find("PremiumNest");
        if (premiumNest != null && premiumNest.gameObject.activeSelf)
            return true;

        return false;
    }

    public void PlaceItemsInFarm(Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(PlaceItemsCoroutine(onSuccess, onError));
    }

    private IEnumerator PlaceItemsCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        isProcessing = true;
        UpdateButtonStates();
        ShowLoading(true);
        
        string itemType = MapProductIdToItemType(cellId.productId);
        string tier = MapProductIdToTier(cellId.productId);
        
        justPlacedPremiumNest = (itemType == "nest" && tier == "premium");
        
        if (string.IsNullOrEmpty(itemType))
        {
            onError?.Invoke($"Invalid item type: {cellId.productId}");
            isProcessing = false;
            ShowLoading(false);
            yield break;
        }
        
        var requestBody = new PlaceFarmRequest
        {
            itemType = itemType,
            tier = tier,
            quantity = currentQuantity,
            farmNumber = targetFarmNumber
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
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
            
            HandlePlacementSuccess(responseText);
            onSuccess?.Invoke(responseText);
            UpdateBigCageInside2Visual();
            yield return null;
            yield return RefreshFarmData();
            justPlacedPremiumNest = false;
            
            if (infoErrorChanger != null)
            {
                infoErrorChanger.CloseAllInfoErrorMethod();
            }
        }
        else
        {
            string errorMsg = request.error;
            
            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogError($"❌ Response body: {request.downloadHandler.text}");
            }
            
            HandlePlacementError(errorMsg);
            onError?.Invoke(errorMsg);
        }
        
        isProcessing = false;
        UpdateButtonStates();
    }

    void HandlePlacementSuccess(string responseJson)
    {
        try
        {
            var response = JsonUtility.FromJson<PlaceFarmResponse>(responseJson);
            
            if (response != null)
            {
                if (response.currentNestCount > 0 || response.maxCapacity > 0)
                {
                    Debug.Log($"📊 Current nest count: {response.currentNestCount}/{response.maxCapacity}");
                }
                
                if (wallet != null)
                {
                    wallet.TryConsumeItem(cellId.productId, currentQuantity);
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Response parsed as null");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Response was: {responseJson}");
        }
    }

    void HandlePlacementError(string error)
    {
        Debug.LogError($"❌ Failed to place items: {error}");
    }

    void UpdateBigCageInside2Visual()
    {
        if (bigCageInside2 == null || cellId == null)
            return;

        string id = cellId.productId.ToLowerInvariant();

        // PREMIUM NEST
        if (id == "super_nest" || id.Contains("super"))
        {
            Transform premiumNest = bigCageInside2.transform.Find("PremiumNest");
            if (premiumNest != null)
            {
                premiumNest.gameObject.SetActive(true);
            }

            Transform normalNest = bigCageInside2.transform.Find("Nest");
            if (normalNest != null)
                normalNest.gameObject.SetActive(false);

            return;
        }

        // NORMAL NEST
        if (id == "nest")
        {
            Transform normalNest = bigCageInside2.transform.Find("Nest");
            if (normalNest != null)
            {
                normalNest.gameObject.SetActive(true);
            }

            Transform premiumNest = bigCageInside2.transform.Find("PremiumNest");
            if (premiumNest != null)
                premiumNest.gameObject.SetActive(false);

            return;
        }

        // HENS
        if (id.Contains("chick") || id.Contains("hen"))
        {
            string chickType = (id.Contains("champ") || id.Contains("gold"))
                ? "ChampChick"
                : "WhiteChick";

            Transform chick = bigCageInside2.transform.Find(chickType);
            if (chick != null)
            {
                chick.gameObject.SetActive(true);
            }

            Transform clock = bigCageInside2.transform.Find("Clock");
            if (clock != null)
                clock.gameObject.SetActive(true);

            Transform lifeTime = bigCageInside2.transform.Find("LifeTime");
            if (lifeTime != null)
                lifeTime.gameObject.SetActive(true);

            return;
        }
    }

    IEnumerator RefreshFarmData()
    {
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
                targetFarmNumber,
                onSuccess: (summary) => {
                    int farmIndex = targetFarmNumber - 1;
                    
                    if (summary.nests != null && summary.nests.details != null)
                    {
                        var nestList = new System.Collections.Generic.List<NestDetail>(summary.nests.details);

                        farmDatabase.SetFarmNestDetails(farmIndex, nestList);
                    }

                    
                    farmDatabase.UpdateFarmFromBackend(
                        farmIndex: farmIndex,
                        nests: summary.nests.total,
                        champChicks: summary.henStats.byKind.Champ,
                        normalChicks: summary.henStats.byKind.Normal,
                        legendChicks: summary.henStats.byKind.Legend,
                        superLegendChicks: summary.henStats.byKind.SuperLegend
                    );
                    
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
            int farmIndex = targetFarmNumber - 1;
            farmGridManager.RefreshFarmDisplay(farmIndex);
        }
        
        if (farmHeaderManager != null)
        {
            farmHeaderManager.UpdateAllFarmSlotVisuals();
        }
        
        if (justPlacedPremiumNest && bigCageInside2 != null)
        {
            Transform premiumNest = bigCageInside2.transform.Find("PremiumNest");
            Transform normalNest = bigCageInside2.transform.Find("Nest");
            
            if (premiumNest != null && !premiumNest.gameObject.activeSelf)
            {
                premiumNest.gameObject.SetActive(true);
            }
            
            if (normalNest != null && normalNest.gameObject.activeSelf)
            {
                normalNest.gameObject.SetActive(false);
            }
        }
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
        
        if (farmAPIManager == null)
        {
            return false;
        }
        
        return true;
    }

    string MapProductIdToItemType(string productId)
    {
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace(" ", "");
        
        if (normalized.Contains("chick") || normalized.Contains("hen"))
        {
            return "hen";
        }
        
        if (normalized.Contains("nest"))
        {
            return "nest";
        }
        
        if (normalized.Contains("robot"))
        {
            return "robot";
        }
        
        return null;
    }

    string MapProductIdToTier(string productId)
    {
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace(" ", "");
        
        if (normalized.Contains("chick") || normalized.Contains("hen"))
        {
            if (normalized.Contains("superlegend"))
                return "superlegend";

            if (normalized.Contains("legend"))
                return "legend";

            if (normalized.Contains("champ") || normalized.Contains("gold"))
                return "gold";

            return "normal";
        }

        if (normalized.Contains("nest"))
        {
            if (normalized.Contains("super"))
                return "premium";

            return "normal";
        }

        if (normalized.Contains("robot"))
        {
            return null;
        }

        return null;
    }

    void ShowLoading(bool show)
    {
        if (putButton)
            putButton.interactable = !show;

        if (quantityInput)
            quantityInput.interactable = !show;

        if (cancelButton)
            cancelButton.interactable = !show;
    }

    public void SetTargetFarm(int farmNumber)
    {
        targetFarmNumber = farmNumber;
    }

    public void SetItem(InventoryCellId item)
    {
        cellId = item;

        if (cellId != null)
        {
            SetQuantity(0);
        }
    }

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
        public int currentNestCount;
        public int maxCapacity;
    }
}