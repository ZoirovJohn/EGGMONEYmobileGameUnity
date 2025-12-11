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
    [SerializeField] Button plusButton;
    [SerializeField] Button minusButton;
    
    [Header("Runtime Data - Auto Set")]
    private InventoryCellId cellId; // Gets productId from selected item
    private int targetFarmNumber = 1; // Which farm to place items in
    
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
    
    private int currentQuantity = 1;
    private bool isProcessing = false;

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
            
            // Find APIConfig if not assigned
            if (!apiConfig)
            {
                apiConfig = Resources.Load<APIConfig>("APIConfig");
                if (!apiConfig)
                {
                    Debug.LogError("❌ APIConfig not found! Create one in Resources folder.");
                }
            }
        }
        
        // Setup button listeners
        if (putButton)
        {
            putButton.onClick.AddListener(OnPutButtonClicked);
        }
        
        if (plusButton)
        {
            plusButton.onClick.AddListener(() => AdjustQuantity(1));
        }
        
        if (minusButton)
        {
            minusButton.onClick.AddListener(() => AdjustQuantity(-1));
        }
        
        if (quantityInput)
        {
            quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);
        }
    }

    void OnEnable()
    {
        // Reset to default quantity when panel opens
        SetQuantity(1);
        
        // ✅ Get cellId from scene (selected inventory item)
        if (cellId == null)
        {
            cellId = FindAnyObjectByType<InventoryCellId>();
            if (cellId != null)
            {
                Debug.Log($"📦 Auto-found selected item: {cellId.productId}");
            }
        }
    }

    void AdjustQuantity(int delta)
    {
        SetQuantity(currentQuantity + delta);
    }

    void SetQuantity(int value)
    {
        // Get max available from wallet
        int maxAvailable = GetMaxAvailableQuantity();
        
        // Clamp between 1 and max available
        currentQuantity = Mathf.Clamp(value, 1, Mathf.Max(1, maxAvailable));
        
        if (quantityInput)
        {
            quantityInput.text = currentQuantity.ToString();
        }
        
        // Update button states
        UpdateButtonStates();
    }

    void OnQuantityInputChanged(string value)
    {
        if (int.TryParse(value, out int quantity))
        {
            SetQuantity(quantity);
        }
        else
        {
            // Reset to 1 if invalid input
            SetQuantity(1);
        }
    }

    void UpdateButtonStates()
    {
        int maxAvailable = GetMaxAvailableQuantity();
        
        if (minusButton)
        {
            minusButton.interactable = currentQuantity > 1;
        }
        
        if (plusButton)
        {
            plusButton.interactable = currentQuantity < maxAvailable;
        }
        
        if (putButton)
        {
            putButton.interactable = !isProcessing && currentQuantity > 0 && currentQuantity <= maxAvailable;
        }
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
            Debug.LogWarning("⚠️ Already processing a request");
            return;
        }
        
        if (!ValidateReferences())
        {
            Debug.LogError("❌ Missing required references!");
            return;
        }
        
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            Debug.LogError("❌ No item selected!");
            return;
        }
        
        // ✅ CHECK IF TRYING TO PLACE A HEN OR NEST
        string productId = cellId.productId.ToLower();
        bool isTryingToPlaceHen = productId.Contains("chick") || productId.Contains("hen");
        bool isTryingToPlaceNest = productId.Contains("nest");
        
        if (isTryingToPlaceHen)
        {
            // ✅ CHECK IF THERE'S ALREADY A HEN IN THE CAGE
            if (IsHenAlreadyInCage())
            {
                Debug.LogWarning("⚠️ Cage already has a hen! Cannot place another hen.");
                
                // Show error message to user
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenErrorDefault("Cage already has a hen! Select another cage.");
                }
                
                return; // ❌ STOP - Don't call backend
            }
        }
        
        if (isTryingToPlaceNest)
        {
            // ✅ CHECK IF THERE'S ALREADY A NEST IN THE CAGE
            if (IsNestAlreadyInCage())
            {
                Debug.LogWarning("⚠️ Cage already has a nest! Cannot place another nest.");
                
                // Show error message to user
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenErrorDefault("Cage already has a nest! Select another cage.");
                }
                
                return; // ❌ STOP - Don't call backend
            }
        }
        
        // ✅ GET CURRENT FARM DYNAMICALLY (always up-to-date)
        if (farmDatabase != null)
        {
            targetFarmNumber = farmDatabase.currentFarmIndex + 1; // Convert 0-based to 1-based
            Debug.Log($"🎯 Placing items in Farm {targetFarmNumber}");
        }
        else
        {
            Debug.LogError("❌ FarmDatabase not found!");
            return;
        }
        
        // Check if user has enough items
        int available = wallet.GetItemCount(cellId.productId);
        if (available < currentQuantity)
        {
            Debug.LogWarning($"⚠️ Not enough items! You have {available}, need {currentQuantity}.");
            return;
        }
        
        // Start placement process
        PlaceItemsInFarm(
            onSuccess: (response) => {
                Debug.Log("✅ Items placed successfully!");
            },
            onError: (error) => {
                Debug.LogError($"❌ Failed to place items: {error}");
            }
        );
    }
    
    // ✅ NEW METHOD: Check if there's already a hen in bigCageInside2
    bool IsHenAlreadyInCage()
    {
        if (bigCageInside2 == null)
            return false;
        
        // Check for ChampChick
        Transform champChick = bigCageInside2.transform.Find("ChampChick");
        if (champChick != null && champChick.gameObject.activeSelf)
        {
            Debug.Log("🐔 ChampChick already active in cage");
            return true;
        }
        
        // Check for WhiteChick
        Transform whiteChick = bigCageInside2.transform.Find("WhiteChick");
        if (whiteChick != null && whiteChick.gameObject.activeSelf)
        {
            Debug.Log("🐔 WhiteChick already active in cage");
            return true;
        }
        
        return false;
    }
    
    // ✅ NEW METHOD: Check if there's already a nest in bigCageInside2
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



    // =====================
    // PLACE ITEMS (POST /farm/place)
    // =====================
    public void PlaceItemsInFarm(Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(PlaceItemsCoroutine(onSuccess, onError));
    }

    private IEnumerator PlaceItemsCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        isProcessing = true;
        UpdateButtonStates();
        ShowLoading(true);
        
        // ✅ Log the original product ID
        Debug.Log($"🔍 Original productId from cellId: '{cellId.productId}'");
        
        // Map product ID to backend format
        string itemType = MapProductIdToItemType(cellId.productId);
        string tier = MapProductIdToTier(cellId.productId);
        
        if (string.IsNullOrEmpty(itemType))
        {
            Debug.LogError($"❌ Invalid item type: {cellId.productId}");
            onError?.Invoke($"Invalid item type: {cellId.productId}");
            isProcessing = false;
            ShowLoading(false);
            yield break;
        }
        
        // Create request body
        var requestBody = new PlaceFarmRequest
        {
            itemType = itemType,
            tier = tier,
            quantity = currentQuantity,
            farmNumber = targetFarmNumber
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        
        // Get access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("❌ Not authenticated - no access token!");
            onError?.Invoke("No access token found");
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
            
            HandlePlacementSuccess(responseText);
            onSuccess?.Invoke(responseText);
            
            // ✅ Update visual in BigCageInside2
            UpdateBigCageInside2Visual();
            
            // Wait a frame before refreshing
            yield return null;
            
            // Refresh farm data from backend
            yield return RefreshFarmData();
            
            // Close the placement panel
            if (infoErrorChanger != null)
            {
                infoErrorChanger.CloseAllInfoErrorMethod();
            }
        }
        else
        {
            string errorMsg = request.error;
            Debug.LogError($"❌ Request failed: {errorMsg}");
            Debug.LogError($"❌ Response code: {request.responseCode}");
            
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
                // Only log nest count if it exists in response
                if (response.currentNestCount > 0 || response.maxCapacity > 0)
                {
                    Debug.Log($"📊 Current nest count: {response.currentNestCount}/{response.maxCapacity}");
                }
                
                // Update local wallet (optimistic update)
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
            Debug.LogError($"❌ Failed to parse response: {e.Message}");
            Debug.LogError($"Response was: {responseJson}");
        }
    }

    void HandlePlacementError(string error)
    {
        Debug.LogError($"❌ Failed to place items: {error}");
    }

    // ✅ UPDATED: Update BigCageInside2 visual to show the added item
    void UpdateBigCageInside2Visual()
    {
        if (bigCageInside2 == null || cellId == null)
            return;

        string id = cellId.productId.ToLowerInvariant();

        // ==========================
        // PREMIUM NEST ("super_nest")
        // ==========================
        if (id == "super_nest" || id.Contains("super"))
        {
            // Turn ON PremiumNest
            Transform premiumNest = bigCageInside2.transform.Find("PremiumNest");
            if (premiumNest != null)
            {
                premiumNest.gameObject.SetActive(true);
                Debug.Log("✅ Activated PremiumNest in BigCageInside2");
            }

            // Turn OFF normal nest if needed
            Transform normalNest = bigCageInside2.transform.Find("Nest");
            if (normalNest != null)
                normalNest.gameObject.SetActive(false);

            return;
        }

        // ======================
        // NORMAL NEST ("nest")
        // ======================
        if (id == "nest")
        {
            // Turn ON normal nest
            Transform normalNest = bigCageInside2.transform.Find("Nest");
            if (normalNest != null)
            {
                normalNest.gameObject.SetActive(true);
                Debug.Log("✅ Activated Normal Nest in BigCageInside2");
            }

            // Turn OFF premium nest if needed
            Transform premiumNest = bigCageInside2.transform.Find("PremiumNest");
            if (premiumNest != null)
                premiumNest.gameObject.SetActive(false);

            return;
        }

        // ======================
        // HENS
        // ======================
        if (id.Contains("chick") || id.Contains("hen"))
        {
            string chickType = (id.Contains("champ") || id.Contains("gold"))
                ? "ChampChick"
                : "WhiteChick";

            Transform chick = bigCageInside2.transform.Find(chickType);
            if (chick != null)
            {
                chick.gameObject.SetActive(true);
                Debug.Log($"✅ Activated {chickType} in BigCageInside2");
            }

            // Clock UI
            Transform clock = bigCageInside2.transform.Find("Clock");
            if (clock != null)
                clock.gameObject.SetActive(true);

            // Lifetime UI
            Transform lifeTime = bigCageInside2.transform.Find("LifeTime");
            if (lifeTime != null)
                lifeTime.gameObject.SetActive(true);

            return;
        }
    }

    IEnumerator RefreshFarmData()
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
                targetFarmNumber,
                onSuccess: (summary) => {
                    // Update FarmDatabase with backend data
                    int farmIndex = targetFarmNumber - 1;
                    
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
            int farmIndex = targetFarmNumber - 1;
            farmGridManager.RefreshFarmDisplay(farmIndex);
        }
        
        if (farmHeaderManager != null)
        {
            farmHeaderManager.UpdateAllFarmSlotVisuals();
        }
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
        
        if (farmAPIManager == null)
        {
            Debug.LogError("❌ FarmAPIManager not assigned!");
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
        
        Debug.LogWarning($"⚠️ Unknown product ID for farm placement: {productId}");
        return null;
    }

    string MapProductIdToTier(string productId)
    {
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace(" ", "");
        
        // 🐔 HENS
        if (normalized.Contains("chick") || normalized.Contains("hen"))
        {
            if (normalized.Contains("superlegend"))
                return "superlegend";

            if (normalized.Contains("legend"))
                return "legend";

            if (normalized.Contains("champ") || normalized.Contains("gold"))
                return "gold";

            // default white / normal / basic chickens
            return "normal";
        }

        // 🪺 NESTS
        if (normalized.Contains("nest"))
        {
            // PREMIUM NEST ("super_nest")
            if (normalized.Contains("super"))
                return "premium";

            // NORMAL NEST ("nest")
            return "normal";
        }

        // 🤖 Robot
        if (normalized.Contains("robot"))
        {
            return null;
        }

        Debug.LogWarning($"⚠️ Unknown item type for: {productId}, returning null");
        return null;
    }


    void ShowLoading(bool show)
    {
        if (putButton != null)
        {
            putButton.interactable = !show;
        }
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
            SetQuantity(1);
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