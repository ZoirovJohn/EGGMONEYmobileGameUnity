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
        
        // ✅ Get target farm from currently opened/selected farm
        if (farmDatabase != null)
        {
            targetFarmNumber = farmDatabase.currentFarmIndex + 1; // Convert 0-based to 1-based
            Debug.Log($"🎯 Target farm auto-set to: Farm {targetFarmNumber}");
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
        
        // ✅ Log what we mapped to
        Debug.Log($"📋 Mapped to → itemType: '{itemType}', tier: '{tier}'");
        
        if (string.IsNullOrEmpty(itemType))
        {
            Debug.LogError($"❌ Invalid item type: {cellId.productId}");
            onError?.Invoke($"Invalid item type: {cellId.productId}");
            isProcessing = false;
            ShowLoading(false);
            yield break;
        }
        
        Debug.Log($"📦 Placing {currentQuantity}x {itemType} (tier: {tier}) in Farm {targetFarmNumber}");
        
        // Create request body
        var requestBody = new PlaceFarmRequest
        {
            itemType = itemType,
            tier = tier,
            quantity = currentQuantity,
            farmNumber = targetFarmNumber
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log($"📤 Full JSON payload: {jsonBody}");
        
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
        Debug.Log($"🌐 POST to: {url}");
        
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
            Debug.Log($"✅ Response: {responseText}");
            
            HandlePlacementSuccess(responseText);
            onSuccess?.Invoke(responseText);
            
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
            Debug.Log($"📥 Raw response: {responseJson}");
            
            var response = JsonUtility.FromJson<PlaceFarmResponse>(responseJson);
            
            if (response != null)
            {
                Debug.Log($"✅ Successfully placed {response.placed}x {response.type} in Farm {response.farmNumber}");
                
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

    IEnumerator RefreshFarmData()
    {
        Debug.Log("🔄 Refreshing farm data from backend...");
        
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
                targetFarmNumber,
                onSuccess: (summary) => {
                    // Update FarmDatabase with backend data
                    int farmIndex = targetFarmNumber - 1;
                    
                    farmDatabase.UpdateFarmFromBackend(
                        farmIndex: farmIndex,
                        nests: summary.nests.total, // ✅ Use total nests
                        champChicks: summary.henStats.byKind.Champ,
                        normalChicks: summary.henStats.byKind.Normal,
                        legendChicks: summary.henStats.byKind.Legend,
                        superLegendChicks: summary.henStats.byKind.SuperLegend
                    );
                    
                    farmRefreshed = true;
                    Debug.Log($"✅ Farm {targetFarmNumber} data updated from backend");
                    Debug.Log($"   Total Nests: {summary.nests.total}");
                    Debug.Log($"   Occupied: {summary.nests.occupied}");
                    Debug.Log($"   Hens: Normal={summary.henStats.byKind.Normal}, Champ={summary.henStats.byKind.Champ}, Legend={summary.henStats.byKind.Legend}, SuperLegend={summary.henStats.byKind.SuperLegend}");
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
            Debug.Log($"🔄 Refreshed Farm {targetFarmNumber} grid display");
        }
        
        if (farmHeaderManager != null)
        {
            farmHeaderManager.UpdateAllFarmSlotVisuals();
            Debug.Log("🔄 Refreshed farm header visuals");
        }
        
        Debug.Log("✅ Farm data refresh complete");
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

    /// <summary>
    /// Map Unity product IDs to backend item types
    /// Backend accepts: "nest", "hen", or "robot"
    /// </summary>
    string MapProductIdToItemType(string productId)
    {
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace(" ", "");
        
        Debug.Log($"🔍 MapProductIdToItemType: '{productId}' → normalized: '{normalized}'");
        
        // HEN items (chicks that are already hatched hens)
        if (normalized.Contains("chick") || normalized.Contains("hen"))
        {
            Debug.Log($"   ✅ Mapped to: 'hen'");
            return "hen";
        }
        
        // NEST items
        if (normalized.Contains("nest"))
        {
            Debug.Log($"   ✅ Mapped to: 'nest'");
            return "nest";
        }
        
        // ROBOT
        if (normalized.Contains("robot"))
        {
            Debug.Log($"   ✅ Mapped to: 'robot'");
            return "robot";
        }
        
        Debug.LogWarning($"⚠️ Unknown product ID for farm placement: {productId}");
        return null;
    }

    /// <summary>
    /// Map Unity product IDs to backend tiers
    /// For HENS: "normal", "gold", "legend", "superlegend" (lowercase!)
    /// For NESTS: "normal", "premium" (lowercase)
    /// For ROBOT: null (no tier)
    /// 
    /// IMPORTANT: Backend stores champ hens as "gold" tier!
    /// </summary>
    string MapProductIdToTier(string productId)
    {
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace(" ", "");
        
        Debug.Log($"🔍 MapProductIdToTier input: '{productId}'");
        Debug.Log($"🔍 Normalized: '{normalized}'");
        
        // ✅ HEN TIERS (lowercase - backend expects these exact values!)
        if (normalized.Contains("chick") || normalized.Contains("hen"))
        {
            // Check in order of specificity (most specific first)
            if (normalized.Contains("superlegend") || normalized.Contains("super"))
            {
                Debug.Log($"   ✅ Returning hen tier: 'superlegend'");
                return "superlegend";
            }
            else if (normalized.Contains("legend"))
            {
                Debug.Log($"   ✅ Returning hen tier: 'legend'");
                return "legend";
            }
            // ✅ FIXED: Map "champ" to "gold" for backend (backend stores it as "gold")
            else if (normalized.Contains("champ") || normalized.Contains("gold"))
            {
                Debug.Log($"   ✅ Returning hen tier: 'gold' (backend expects 'gold' not 'champ')");
                return "gold";
            }
            else if (normalized.Contains("normal") || normalized.Contains("white") || normalized.Contains("basic"))
            {
                Debug.Log($"   ✅ Returning hen tier: 'normal'");
                return "normal";
            }
            else
            {
                Debug.LogWarning($"⚠️ Unknown hen tier for: {productId}, defaulting to 'normal'");
                return "normal";
            }
        }
        
        // ✅ NEST TIERS (lowercase)
        if (normalized.Contains("nest"))
        {
            if (normalized.Contains("premium") || normalized.Contains("super"))
            {
                Debug.Log($"   ✅ Returning nest tier: 'premium'");
                return "premium";
            }
            // Don't map "gold" nests - backend might expect "gold" for nests
            else if (normalized.Contains("gold"))
            {
                Debug.Log($"   ⚠️ Gold nest detected - returning 'premium' (verify if backend expects 'gold' or 'premium')");
                return "premium"; // Change to "gold" if backend expects it
            }
            else
            {
                Debug.Log($"   ✅ Returning nest tier: 'normal'");
                return "normal";
            }
        }
        
        // ✅ ROBOT - no tier needed
        if (normalized.Contains("robot"))
        {
            Debug.Log($"   ✅ Robot item: returning null (no tier)");
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

    /// <summary>
    /// PUBLIC: Call this when the panel opens to set the target farm
    /// </summary>
    public void SetTargetFarm(int farmNumber)
    {
        targetFarmNumber = farmNumber;
        Debug.Log($"🎯 Target farm set to: Farm {targetFarmNumber}");
    }

    /// <summary>
    /// PUBLIC: Call this when the panel opens to set the item
    /// </summary>
    public void SetItem(InventoryCellId item)
    {
        cellId = item;
        
        if (cellId != null)
        {
            Debug.Log($"📦 Item set to: {cellId.productId}");
            SetQuantity(1); // Reset to 1 when new item selected
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
        public int currentNestCount;
        public int maxCapacity;
    }
}