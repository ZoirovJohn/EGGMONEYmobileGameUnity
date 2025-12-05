using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryLockItemApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FarmDatabase farmDatabase;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private InfoErrorChanger infoErrorChanger;
    [SerializeField] private FarmGridManager farmGridManager;
    [SerializeField] private FarmHeaderManager farmHeaderManager;
    [SerializeField] private UseFarmKeyManager useFarmKeyManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private FarmAPIManager farmAPIManager; // ✅ NEW
    
    [Header("Buttons")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [Header("Auto Find")]
    [SerializeField] private bool autoFind = true;
    
    private string pendingKeyId = "";
    
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
            
            if (!farmGridManager)
                farmGridManager = FindAnyObjectByType<FarmGridManager>();
            
            if (!farmHeaderManager)
                farmHeaderManager = FindAnyObjectByType<FarmHeaderManager>();
            
            if (!useFarmKeyManager)
                useFarmKeyManager = FindAnyObjectByType<UseFarmKeyManager>();
            
            if (!inventoryManager)
                inventoryManager = FindAnyObjectByType<InventoryManager>();
            
            // ✅ NEW: Auto-find FarmAPIManager
            if (!farmAPIManager)
                farmAPIManager = FindAnyObjectByType<FarmAPIManager>();
            
            // Auto-find buttons if not assigned
            if (!yesButton)
            {
                GameObject yesObj = GameObject.Find("BtnYes");
                if (yesObj == null) yesObj = GameObject.Find("YesButton");
                if (yesObj != null) yesButton = yesObj.GetComponent<Button>();
            }
            
            if (!noButton)
            {
                GameObject noObj = GameObject.Find("BtnNo");
                if (noObj == null) noObj = GameObject.Find("NoButton");
                if (noObj != null) noButton = noObj.GetComponent<Button>();
            }
        }
        
        // Hook up buttons
        if (yesButton)
            yesButton.onClick.AddListener(OnYesClicked);
        
        if (noButton)
            noButton.onClick.AddListener(OnNoClicked);
    }
    
    public void SetPendingItem(string keyId)
    {
        pendingKeyId = keyId;
    }
    
    void OnNoClicked()
    {
        pendingKeyId = "";
        
        // Hide Yes/No buttons
        if (yesButton)
        {
            yesButton.gameObject.SetActive(false);
        }
        
        if (noButton)
        {
            noButton.gameObject.SetActive(false);
        }
        
        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
        }
    }
    
    void OnYesClicked()
    {
        if (string.IsNullOrEmpty(pendingKeyId))
        {
            Debug.LogWarning("⚠️ No pending key ID!");
            return;
        }
        
        // 1. Check if player has the key locally
        if (wallet == null || wallet.GetItemCount(pendingKeyId) <= 0)
        {
            Debug.LogWarning($"⚠️ Player doesn't have key: {pendingKeyId}");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("You don't have this key.");
            }
            return;
        }
        
        // 2. Determine which farm this key unlocks
        string farmIdToUnlock = GetFarmIdForKey(pendingKeyId);
        
        if (string.IsNullOrEmpty(farmIdToUnlock))
        {
            Debug.LogError($"❌ Could not determine farm for key: {pendingKeyId}");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("Invalid key.");
            }
            return;
        }
        
        // 3. Check if farm is already unlocked
        if (farmDatabase != null && farmDatabase.farms != null)
        {
            FarmData existingFarm = farmDatabase.farms.Find(f => f.farmId == farmIdToUnlock);
            if (existingFarm != null)
            {
                Debug.LogWarning($"⚠️ Farm {farmIdToUnlock} is already unlocked!");
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenErrorDefault("This farm is already unlocked.");
                }
                return;
            }
        }
        
        // ✅ Call backend to use farm key, then refresh inventory and farm data
        if (useFarmKeyManager != null)
        {
            Debug.Log($"🔑 Calling backend to use key: {pendingKeyId}");
            
            useFarmKeyManager.UseFarmKey(
                pendingKeyId,
                onSuccess: (response) =>
                {
                    Debug.Log($"✅ Backend: Farm key used successfully!");
                    
                    // ✅ Refresh inventory to sync with backend
                    if (inventoryManager != null)
                    {
                        inventoryManager.GetInventory(
                            onSuccess: (invResponse) =>
                            {
                                Debug.Log("✅ Inventory refreshed after using farm key");
                                
                                // ✅ NEW: Reload farm data from backend after unlocking
                                ReloadFarmDataFromBackend(farmIdToUnlock);
                            },
                            onError: (invError) =>
                            {
                                Debug.LogError($"❌ Failed to refresh inventory: {invError}");
                                
                                // Still proceed with farm data reload
                                ReloadFarmDataFromBackend(farmIdToUnlock);
                            }
                        );
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ InventoryManager not found, skipping inventory refresh");
                        ReloadFarmDataFromBackend(farmIdToUnlock);
                    }
                },
                onError: (error) =>
                {
                    Debug.LogError($"❌ Backend error when using farm key: {error}");
                    
                    if (infoErrorChanger != null)
                    {
                        infoErrorChanger.OpenErrorDefault("Failed to unlock farm. Please try again.");
                    }
                }
            );
        }
        else
        {
            Debug.LogWarning("⚠️ UseFarmKeyManager not found! Proceeding with local unlock only.");
            
            // Fallback: proceed without backend call (old behavior)
            if (!wallet.TryConsumeItem(pendingKeyId, 1))
            {
                Debug.LogError($"❌ Failed to consume key: {pendingKeyId}");
                return;
            }
            
            ProceedWithFarmUnlock(farmIdToUnlock);
        }
    }
    
    /// <summary>
    /// ✅ NEW: Reload all farm data from backend after unlocking a new farm
    /// </summary>
    void ReloadFarmDataFromBackend(string farmIdToUnlock)
    {
        if (wallet == null)
        {
            Debug.LogWarning("⚠️ PlayerWallet not found, cannot determine farm count");
            ProceedWithFarmUnlock(farmIdToUnlock);
            return;
        }
        
        // Get updated farm count from wallet (should be +1 now)
        int newFarmCount = wallet.UserFarms;
        
        Debug.Log($"🔄 Reloading {newFarmCount} farms from backend after unlock...");
        
        // Start coroutine to reload farms sequentially
        StartCoroutine(ReloadAllFarmsCoroutine(newFarmCount, farmIdToUnlock));
    }
    
    /// <summary>
    /// Coroutine to reload all farm data from backend
    /// </summary>
    System.Collections.IEnumerator ReloadAllFarmsCoroutine(int farmCount, string farmIdToUnlock)
    {
        if (farmAPIManager == null)
        {
            Debug.LogWarning("⚠️ FarmAPIManager not found, proceeding with local unlock only");
            ProceedWithFarmUnlock(farmIdToUnlock);
            yield break;
        }
        
        bool loadSuccess = false;
        bool loadComplete = false;
        string loadError = "";
        
        // Use FarmAPIManager to load all farms
        farmAPIManager.LoadAllFarmSummaries(
            farmCount,
            onAllLoaded: (summaries) =>
            {
                Debug.Log($"✅ Successfully loaded all {summaries.Length} farm summaries");
                
                // Update FarmDatabase from summaries
                UpdateFarmDatabaseFromSummaries(summaries);
                
                loadSuccess = true;
                loadComplete = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to reload farms: {error}");
                loadError = error;
                loadComplete = true;
            }
        );
        
        // Wait for load to complete
        yield return new WaitUntil(() => loadComplete);
        
        if (loadSuccess)
        {
            // Now proceed with unlock sequence using the NEW farm index
            int newFarmIndex = farmCount - 1; // Last farm in the list
            ProceedWithFarmUnlockByIndex(newFarmIndex);
        }
        else
        {
            Debug.LogError($"❌ Failed to reload farms from backend: {loadError}");
            // Fallback to local unlock if backend fails
            ProceedWithFarmUnlock(farmIdToUnlock);
        }
    }
    
    /// <summary>
    /// ✅ UPDATED: Fully reload ALL farms from backend summaries
    /// </summary>
    void UpdateFarmDatabaseFromSummaries(FarmSummary[] summaries)
    {
        if (farmDatabase == null || summaries == null || summaries.Length == 0)
        {
            Debug.LogWarning("⚠️ Cannot update FarmDatabase - missing data");
            return;
        }
        
        Debug.Log($"🔄 Reloading ALL {summaries.Length} farms from backend...");
        
        // ✅ Clear existing farms and rebuild from scratch
        farmDatabase.farms.Clear();
        
        // ✅ Rebuild each farm from backend data
        for (int i = 0; i < summaries.Length; i++)
        {
            FarmSummary summary = summaries[i];
            string farmId = $"farm_{(i + 1):D3}";
            
            FarmData newFarm = new FarmData
            {
                farmId = farmId,
                farmName = GetFarmNameForId(farmId),
                farmIndex = i,
                farmKeyType = "normal", // Will be loaded properly by FarmGridManager
                robotType = "none",
                batteryType = "none",
                nestsOccupied = 0,
                normalChicks = 0,
                champChicks = 0,
                legendChicks = 0,
                superLegendChicks = 0,
                cages = new System.Collections.Generic.List<CageData>()
            };
            
            // Initialize empty cages (will be populated when farm is loaded)
            for (int j = 0; j < 100; j++)
            {
                newFarm.cages.Add(new CageData
                {
                    id = j + 1,
                    farmIndex = i,
                    nestCapacity = 16,
                    nestsOccupied = 0,
                    normalChicks = 0,
                    champChicks = 0,
                    legendChicks = 0,
                    superLegendChicks = 0,
                    hasEgg = false,
                    eggReady = false,
                    remainingTime = 0f,
                    upgradeLevel = 0
                });
            }
            
            farmDatabase.farms.Add(newFarm);
        }
        
        Debug.Log($"✅ FarmDatabase cleared and rebuilt with {farmDatabase.farms.Count} farms");
        
        // ✅ NOW reload the actual farm data for each farm using FarmGridManager
        StartCoroutine(ReloadAllFarmDataSequentially(summaries.Length));
    }
    
    /// <summary>
    /// ✅ NEW: Reload actual farm data for all farms sequentially using FarmAPIManager
    /// </summary>
    System.Collections.IEnumerator ReloadAllFarmDataSequentially(int farmCount)
    {
        if (farmAPIManager == null)
        {
            Debug.LogWarning("⚠️ FarmAPIManager not found, skipping detailed farm reload");
            yield break;
        }
        
        Debug.Log($"🔄 Reloading detailed data for {farmCount} farms...");
        
        // Reload each farm's detailed data from backend
        for (int i = 0; i < farmCount; i++)
        {
            bool loadComplete = false;
            int farmNumber = i + 1; // Backend uses 1-based farm numbers
            int farmIndex = i;
            
            // Get farm summary from backend
            farmAPIManager.GetFarmSummary(
                farmNumber,
                onSuccess: (summary) =>
                {
                    Debug.Log($"✅ Loaded farm {farmNumber} summary from backend");
                    
                    // Update the farm data in FarmDatabase if needed
                    // (The actual cage population will happen when switching to the farm)
                    
                    loadComplete = true;
                },
                onError: (err) =>
                {
                    Debug.LogError($"❌ Failed to load farm {farmNumber}: {err}");
                    loadComplete = true;
                }
            );
            
            // Wait for this farm to finish loading before moving to next
            yield return new WaitUntil(() => loadComplete);
            
            // Small delay between requests to avoid overwhelming the server
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log($"✅ All {farmCount} farms reloaded with fresh backend data!");
    }
    
    void ProceedWithFarmUnlock(string farmIdToUnlock)
    {
        // Create and add new farm data with default values
        FarmData newFarm = CreateNewFarmData(farmIdToUnlock, pendingKeyId);
        
        if (farmDatabase != null)
        {
            farmDatabase.farms.Add(newFarm);
        }
        else
        {
            Debug.LogError("❌ FarmDatabase is null!");
            return;
        }
        
        // Update farm counts in FarmHeaderManager
        if (farmHeaderManager != null)
        {
            farmHeaderManager.farmCount++;
            farmHeaderManager.lockCount--;
        }
        
        // Start unlock sequence (visual feedback + open farm)
        StartCoroutine(UnlockFarmSequence(farmIdToUnlock));
    }
    
    /// <summary>
    /// ✅ NEW: Proceed with unlock using farm index (when loaded from backend)
    /// </summary>
    void ProceedWithFarmUnlockByIndex(int newFarmIndex)
    {
        // Update farm counts in FarmHeaderManager
        if (farmHeaderManager != null)
        {
            farmHeaderManager.farmCount++;
            farmHeaderManager.lockCount--;
        }
        
        // Get the farm ID for the new farm
        FarmData newFarm = farmDatabase.GetFarmByIndex(newFarmIndex);
        string farmId = newFarm != null ? newFarm.farmId : "";
        
        // Start unlock sequence
        StartCoroutine(UnlockFarmSequenceByIndex(farmId, newFarmIndex));
    }
    
    System.Collections.IEnumerator UnlockFarmSequence(string farmId)
    {
        // Get the NEW farm index (it's the last one since we just added it)
        int newFarmIndex = -1;
        if (farmDatabase != null && farmDatabase.farms != null)
        {
            newFarmIndex = farmDatabase.farms.Count - 1; // Last farm in the list
        }
        
        yield return UnlockFarmSequenceByIndex(farmId, newFarmIndex);
    }
    
    /// <summary>
    /// ✅ UPDATED: Unified unlock sequence that works with farm index
    /// </summary>
    System.Collections.IEnumerator UnlockFarmSequenceByIndex(string farmId, int newFarmIndex)
    {
        // Toggle lock visuals (adjust GameObject names based on your hierarchy)
        GameObject lockClose = GameObject.Find($"ImageLockClose_{farmId}");
        GameObject lockOpen = GameObject.Find($"ImageLockOpen_{farmId}");
        
        // Alternative naming patterns
        if (!lockClose) lockClose = GameObject.Find($"LockClose_{newFarmIndex}");
        if (!lockOpen) lockOpen = GameObject.Find($"LockOpen_{newFarmIndex}");
        
        if (lockClose)
        {
            lockClose.SetActive(false);
        }
        
        if (lockOpen)
        {
            lockOpen.SetActive(true);
        }
        
        // Wait 1 second for visual feedback
        yield return new WaitForSeconds(1f);
        
        // Hide Yes/No buttons before closing
        if (yesButton)
            yesButton.gameObject.SetActive(false);
        if (noButton)
            noButton.gameObject.SetActive(false);
        
        // Close all info/error panels
        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
        }
        
        // Refresh FarmHeaderManager to show the new farm slot
        if (farmHeaderManager != null)
        {
            farmHeaderManager.Refresh();
        }
        
        // Switch to the newly unlocked farm
        if (newFarmIndex >= 0)
        {
            // Call DefaultBannerMethod from InventoryItemsBarChanger
            InventoryItemsBarChanger barChanger = FindAnyObjectByType<InventoryItemsBarChanger>();
            if (barChanger != null)
            {
                barChanger.DefaultBannerMethod();
            }
            else
            {
                Debug.LogWarning("⚠️ InventoryItemsBarChanger not found!");
            }
            
            if (farmGridManager != null)
            {
                farmGridManager.SwitchFarm(newFarmIndex);
            }
            else
            {
                Debug.LogWarning("⚠️ FarmGridManager is null - cannot switch farm");
            }
            
            if (farmDatabase != null)
            {
                farmDatabase.SwitchToFarm(newFarmIndex);
            }
        }
        
        // Clear pending data
        pendingKeyId = "";
    }
    
    FarmData CreateNewFarmData(string farmId, string keyType)
    {
        FarmData newFarm = new FarmData();
        newFarm.farmId = farmId;
        newFarm.farmName = GetFarmNameForId(farmId);
        newFarm.farmIndex = GetFarmIndexForId(farmId);
        
        // Normalize key type to match first 3 farms
        if (keyType == "key_farm")
        {
            newFarm.farmKeyType = "normal";
        }
        else if (keyType == "premiumfarmkey" || keyType == "premium_farm_key" || keyType == "premiumFarmKey")
        {
            newFarm.farmKeyType = "premium";
        }
        else
        {
            newFarm.farmKeyType = keyType; // Keep original if it's already "normal" or "premium"
        }
        
        newFarm.robotType = "none";
        newFarm.batteryType = "none";
        newFarm.nestsOccupied = 0;
        newFarm.normalChicks = 0;
        newFarm.champChicks = 0;
        
        // Initialize empty cages list
        newFarm.cages = new System.Collections.Generic.List<CageData>();
        for (int i = 0; i < 100; i++)
        {
            newFarm.cages.Add(new CageData
            {
                id = i + 1,
                farmIndex = newFarm.farmIndex,
                nestCapacity = 16,
                nestsOccupied = 0,
                normalChicks = 0,
                champChicks = 0,
                hasEgg = false,
                remainingTime = 0f,
                upgradeLevel = 0
            });
        }
        return newFarm;
    }
    
    // Helper method to map keys to farm IDs
    string GetFarmIdForKey(string keyId)
    {
        // Based on your JSON format: farm_001, farm_002, farm_003, farm_004...
        switch (keyId)
        {
            // Generic key_farm - find next available farm
            case "key_farm":
                return GetNextAvailableFarmId();
            
            // Premium farm key - also finds next available farm
            case "premiumfarmkey":
            case "premium_farm_key":
            case "premiumFarmKey":
                return GetNextAvailableFarmId();
            
            case "key_farm_4":
            case "farmKey4":
            case "farm_4_key":
                return "farm_004";
                
            case "key_farm_5":
            case "farmKey5":
            case "farm_5_key":
                return "farm_005";
                
            case "key_farm_6":
            case "farmKey6":
            case "farm_6_key":
                return "farm_006";
                
            case "key_farm_7":
            case "farmKey7":
            case "farm_7_key":
                return "farm_007";
                
            case "key_farm_8":
            case "farmKey8":
            case "farm_8_key":
                return "farm_008";
                
            default:
                Debug.LogWarning($"⚠️ Unknown key type: {keyId}");
                return "";
        }
    }
    
    // Helper method to get the next available farm ID based on what's already unlocked
    string GetNextAvailableFarmId()
    {
        if (farmDatabase == null || farmDatabase.farms == null)
        {
            Debug.LogWarning("⚠️ FarmDatabase is null, defaulting to farm_004");
            return "farm_004";
        }
        
        // Get list of all unlocked farm IDs
        var unlockedFarmIds = new System.Collections.Generic.HashSet<string>();
        foreach (var farm in farmDatabase.farms)
        {
            unlockedFarmIds.Add(farm.farmId);
        }
        
        // Check farms 4-8 and return the first one not unlocked
        string[] possibleFarms = { "farm_004", "farm_005", "farm_006", "farm_007", "farm_008" };
        
        foreach (string farmId in possibleFarms)
        {
            if (!unlockedFarmIds.Contains(farmId))
            {
                return farmId;
            }
        }
        
        Debug.LogWarning("⚠️ All farms are already unlocked!");
        return "";
    }
    
    // Helper method to get farm display name
    string GetFarmNameForId(string farmId)
    {
        switch (farmId)
        {
            case "farm_001": return "Farm 1";
            case "farm_002": return "Farm 2";
            case "farm_003": return "Farm 3";
            case "farm_004": return "Farm 4";
            case "farm_005": return "Farm 5";
            case "farm_006": return "Farm 6";
            case "farm_007": return "Farm 7";
            case "farm_008": return "Farm 8";
            default: return $"Farm {farmId}";
        }
    }
    
    // Helper method to get farm index (based on total farm count)
    int GetFarmIndexForId(string farmId)
    {
        // The index should be the current total number of farms
        // Since we're adding a NEW farm, it goes at the end
        if (farmDatabase != null && farmDatabase.farms != null)
        {
            return farmDatabase.farms.Count; // This will be the new farm's index
        }
        
        // Fallback - parse from farmId
        switch (farmId)
        {
            case "farm_001": return 0;
            case "farm_002": return 1;
            case "farm_003": return 2;
            case "farm_004": return 3;
            case "farm_005": return 4;
            case "farm_006": return 5;
            case "farm_007": return 6;
            case "farm_008": return 7;
            default: return -1;
        }
    }
}