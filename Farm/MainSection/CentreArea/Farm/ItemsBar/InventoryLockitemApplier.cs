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
    [SerializeField] private FarmAPIManager farmAPIManager;
    [SerializeField] private AuthManager authManager;
    
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
            {
                farmHeaderManager = FindAnyObjectByType<FarmHeaderManager>();
                if (farmHeaderManager != null)
                {
                    Debug.Log($"✅ Auto-found FarmHeaderManager, maxTotalFarms = {farmHeaderManager.maxTotalFarms}");
                }
            }
            
            if (!useFarmKeyManager)
                useFarmKeyManager = FindAnyObjectByType<UseFarmKeyManager>();
            
            if (!inventoryManager)
                inventoryManager = FindAnyObjectByType<InventoryManager>();
            
            if (!farmAPIManager)
                farmAPIManager = FindAnyObjectByType<FarmAPIManager>();
            
            if (!authManager)
                authManager = FindAnyObjectByType<AuthManager>();
            
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
        
        if (yesButton)
            yesButton.gameObject.SetActive(false);
        
        if (noButton)
            noButton.gameObject.SetActive(false);
        
        if (infoErrorChanger != null)
            infoErrorChanger.CloseAllInfoErrorMethod();
    }
    
    void OnYesClicked()
    {
        Debug.Log("=== FARM KEY DEBUG ===");
        Debug.Log($"Pending Key ID: '{pendingKeyId}'");
        Debug.Log($"User Farms: {wallet?.UserFarms}");
        Debug.Log($"Max Farms: {farmHeaderManager?.maxTotalFarms ?? 99999}");
        
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
                infoErrorChanger.OpenErrorDefault("You don't have this key.");
            return;
        }
        
        // 2. Determine which farm this key unlocks
        string farmIdToUnlock = GetFarmIdForKey(pendingKeyId);
        
        if (string.IsNullOrEmpty(farmIdToUnlock))
        {
            Debug.LogError($"❌ Could not determine farm for key: {pendingKeyId}");
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault("Invalid key.");
            return;
        }
        
        // 3. Check if we've reached max farms
        if (wallet != null)
        {
            int currentFarms = wallet.UserFarms;
            int maxFarms = 99999; // Default max
            if (farmHeaderManager != null)
            {
                maxFarms = farmHeaderManager.maxTotalFarms;
                
                // ✅ CRITICAL FIX: If maxTotalFarms is unreasonably low (like 8), use 99999 instead
                if (maxFarms < 100)
                {
                    Debug.LogWarning($"⚠️ FarmHeaderManager.maxTotalFarms is too low ({maxFarms}), using 99999 instead");
                    maxFarms = 99999;
                }
            }
            
            if (currentFarms >= maxFarms)
            {
                Debug.LogWarning($"⚠️ All farms unlocked! (User has {currentFarms}/{maxFarms} farms)");
                if (infoErrorChanger != null)
                    infoErrorChanger.OpenErrorDefault("All farms are already unlocked.");
                return;
            }
            
            Debug.Log($"✅ Can unlock next farm. Current: {currentFarms}/{maxFarms}");
        }
        
        // ✅ Call backend to use farm key
        if (useFarmKeyManager != null)
        {
            Debug.Log($"🔑 Calling backend to use key: {pendingKeyId}");
            
            useFarmKeyManager.UseFarmKey(
                pendingKeyId,
                onSuccess: (response) =>
                {
                    Debug.Log($"✅ Backend: Farm key used successfully! Message: {response.message}");
                    
                    // ✅ Step 1: Refresh user profile to get updated userFarms count
                    RefreshUserProfile(farmIdToUnlock);
                },
                onError: (error) =>
                {
                    Debug.LogError($"❌ Backend error when using farm key: {error}");
                    
                    if (infoErrorChanger != null)
                        infoErrorChanger.OpenErrorDefault("Failed to unlock farm. Please try again.");
                }
            );
        }
        else
        {
            Debug.LogWarning("⚠️ UseFarmKeyManager not found! Falling back to local unlock.");
            
            // Fallback: proceed without backend call
            if (!wallet.TryConsumeItem(pendingKeyId, 1))
            {
                Debug.LogError($"❌ Failed to consume key: {pendingKeyId}");
                return;
            }
            
            ProceedWithFarmUnlock(farmIdToUnlock);
        }
    }
    
    /// <summary>
    /// ✅ Step 1: Refresh user profile from /auth/me to get updated userFarms count
    /// </summary>
    void RefreshUserProfile(string farmIdToUnlock)
    {
        if (authManager == null)
        {
            Debug.LogWarning("⚠️ AuthManager not found, skipping profile refresh");
            RefreshInventory(farmIdToUnlock);
            return;
        }
        
        Debug.Log("🔄 Refreshing user profile from /auth/me...");
        
        authManager.GetUserProfile(
            onSuccess: (profile) =>
            {
                Debug.Log($"✅ User profile refreshed! UserFarms: {wallet.UserFarms}");
                
                // ✅ Step 2: Refresh inventory
                RefreshInventory(farmIdToUnlock);
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to refresh profile: {error}");
                
                // Continue anyway - inventory refresh might still work
                RefreshInventory(farmIdToUnlock);
            }
        );
    }
    
    /// <summary>
    /// ✅ Step 2: Refresh inventory to sync with backend
    /// </summary>
    void RefreshInventory(string farmIdToUnlock)
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("⚠️ InventoryManager not found, skipping inventory refresh");
            ReloadFarmDataFromBackend(farmIdToUnlock);
            return;
        }
        
        Debug.Log("🔄 Refreshing inventory...");
        
        inventoryManager.GetInventory(
            onSuccess: (invResponse) =>
            {
                Debug.Log("✅ Inventory refreshed after using farm key");
                
                // ✅ Step 3: Reload farm data from backend
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
    
    /// <summary>
    /// ✅ Step 3: Load the newly unlocked farm from backend
    /// </summary>
    void ReloadFarmDataFromBackend(string farmIdToUnlock)
    {
        if (wallet == null)
        {
            Debug.LogWarning("⚠️ PlayerWallet not found");
            ProceedWithFarmUnlock(farmIdToUnlock);
            return;
        }
        
        // Get updated farm count (should be +1 after backend unlock)
        int newFarmCount = wallet.UserFarms;
        int newFarmNumber = newFarmCount; // Backend uses 1-based numbering
        int newFarmIndex = newFarmCount - 1; // Unity uses 0-based indexing
        
        Debug.Log($"🔄 Loading newly unlocked farm {newFarmNumber} from backend (index {newFarmIndex})...");
        
        if (farmAPIManager == null)
        {
            Debug.LogWarning("⚠️ FarmAPIManager not found, using local unlock only");
            ProceedWithFarmUnlock(farmIdToUnlock);
            return;
        }
        
        // Load ONLY the new farm from backend
        farmAPIManager.GetFarmSummary(
            newFarmNumber,
            onSuccess: (summary) =>
            {
                Debug.Log($"✅ Successfully loaded new farm {newFarmNumber} from backend");
                
                // Add the new farm to FarmDatabase
                AddNewFarmToDatabase(summary, newFarmIndex);
                
                // Now proceed with unlock sequence
                ProceedWithFarmUnlockByIndex(newFarmIndex);
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to load new farm: {error}");
                
                // Fallback: proceed with local unlock if backend fails
                ProceedWithFarmUnlock(farmIdToUnlock);
            }
        );
    }
    
    /// <summary>
    /// ✅ Add newly unlocked farm from backend to FarmDatabase
    /// </summary>
    void AddNewFarmToDatabase(FarmSummary summary, int newFarmIndex)
    {
        if (farmDatabase == null || summary == null)
        {
            Debug.LogWarning("⚠️ Cannot add farm - missing FarmDatabase or summary");
            return;
        }
        
        string farmId = $"farm_{(newFarmIndex + 1):D3}";
        
        Debug.Log($"➕ Adding new farm to database: {farmId} at index {newFarmIndex}");
        
        // Determine farm key type from backend
        string farmKeyType = (summary.farm != null && summary.farm.isPremium) ? "premium" : "normal";
        
        // Determine robot type from backend
        string robotType = "none";
        if (summary.robot != null && !string.IsNullOrEmpty(summary.robot.id))
        {
            robotType = summary.robot.id;
        }
        
        // Count chicks by kind from backend
        int normalChicks = 0;
        int champChicks = 0;
        int legendChicks = 0;
        int superLegendChicks = 0;
        
        if (summary.henStats != null && summary.henStats.byKind != null)
        {
            normalChicks = summary.henStats.byKind.Normal;
            champChicks = summary.henStats.byKind.Champ;
            legendChicks = summary.henStats.byKind.Legend;
            superLegendChicks = summary.henStats.byKind.SuperLegend;
        }
        
        // Get nests occupied
        int nestsOccupied = (summary.nests != null) ? summary.nests.occupied : 0;
        
        FarmData newFarm = new FarmData
        {
            farmId = farmId,
            farmName = GetFarmNameForId(farmId),
            farmIndex = newFarmIndex,
            farmKeyType = farmKeyType,
            robotType = robotType,
            batteryType = "none",
            nestsOccupied = nestsOccupied,
            normalChicks = normalChicks,
            champChicks = champChicks,
            legendChicks = legendChicks,
            superLegendChicks = superLegendChicks,
            cages = new System.Collections.Generic.List<CageData>()
        };
        
        // ✅ Store nest details in FarmDatabase for proper egg mapping
        if (summary.nests != null && summary.nests.details != null)
        {
            farmDatabase.SetFarmNestDetails(newFarmIndex, summary.nests.details);
        }
        
        // Initialize empty cages (will be populated by FarmDatabase.DistributeFarmDataToCages)
        for (int j = 0; j < 100; j++)
        {
            newFarm.cages.Add(new CageData
            {
                id = j + 1,
                farmIndex = newFarmIndex,
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
        
        // ✅ Generate cages from farm data (distributes chicks and eggs)
        farmDatabase.GenerateCagesFromFarmData();
        
        Debug.Log($"✅ Successfully added farm {farmId} to database (Total farms: {farmDatabase.farms.Count})");
        Debug.Log($"   - Farm Type: {farmKeyType}");
        Debug.Log($"   - Nests: {nestsOccupied}");
        Debug.Log($"   - Normal: {normalChicks}, Champ: {champChicks}, Legend: {legendChicks}, SuperLegend: {superLegendChicks}");
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
        
        // Get the NEW farm index (it's the last one since we just added it)
        int newFarmIndex = farmDatabase.farms.Count - 1;
        
        // Start unlock sequence (visual feedback + open farm)
        StartCoroutine(UnlockFarmSequenceByIndex(farmIdToUnlock, newFarmIndex));
    }
    
    /// <summary>
    /// ✅ Proceed with unlock using farm index (when loaded from backend)
    /// </summary>
    void ProceedWithFarmUnlockByIndex(int newFarmIndex)
    {
        // Get the farm ID for the new farm
        FarmData newFarm = farmDatabase.GetFarmByIndex(newFarmIndex);
        string farmId = newFarm != null ? newFarm.farmId : "";
        
        Debug.Log($"🎉 Proceeding with unlock for farm index {newFarmIndex} (ID: {farmId})");
        
        // Start unlock sequence
        StartCoroutine(UnlockFarmSequenceByIndex(farmId, newFarmIndex));
    }
    
    /// <summary>
    /// ✅ Unified unlock sequence that works with farm index
    /// </summary>
    System.Collections.IEnumerator UnlockFarmSequenceByIndex(string farmId, int newFarmIndex)
    {
        Debug.Log($"🔓 Starting unlock sequence for farm {farmId} at index {newFarmIndex}");
        
        // Toggle lock visuals (adjust GameObject names based on your hierarchy)
        GameObject lockClose = GameObject.Find($"ImageLockClose_{farmId}");
        GameObject lockOpen = GameObject.Find($"ImageLockOpen_{farmId}");
        
        // Alternative naming patterns
        if (!lockClose) lockClose = GameObject.Find($"LockClose_{newFarmIndex}");
        if (!lockOpen) lockOpen = GameObject.Find($"LockOpen_{newFarmIndex}");
        
        if (lockClose)
            lockClose.SetActive(false);
        
        if (lockOpen)
            lockOpen.SetActive(true);
        
        // Wait 1 second for visual feedback
        yield return new WaitForSeconds(1f);
        
        // Hide Yes/No buttons before closing
        if (yesButton)
            yesButton.gameObject.SetActive(false);
        if (noButton)
            noButton.gameObject.SetActive(false);
        
        // Close all info/error panels
        if (infoErrorChanger != null)
            infoErrorChanger.CloseAllInfoErrorMethod();
        
        // ✅ Refresh FarmHeaderManager to rebuild UI with new farm
        if (farmHeaderManager != null)
        {
            Debug.Log("🔄 Refreshing FarmHeaderManager to show new farm...");
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
            
            if (farmGridManager != null)
            {
                Debug.Log($"🔄 Switching to farm index {newFarmIndex}...");
                farmGridManager.SwitchFarm(newFarmIndex);
            }
            
            if (farmDatabase != null)
            {
                farmDatabase.SwitchToFarm(newFarmIndex);
            }
            
            // ✅ CRITICAL FIX: Update farm header selection to show yellow on new farm
            if (farmHeaderManager != null)
            {
                farmHeaderManager.SelectFarm(newFarmIndex);
                Debug.Log($"✅ Updated farm header yellow highlight to farm {newFarmIndex + 1}");
            }
        }
        
        // Clear pending data
        pendingKeyId = "";
        
        Debug.Log($"✅ Farm unlock sequence complete!");
    }
    
    FarmData CreateNewFarmData(string farmId, string keyType)
    {
        FarmData newFarm = new FarmData();
        newFarm.farmId = farmId;
        newFarm.farmName = GetFarmNameForId(farmId);
        newFarm.farmIndex = GetFarmIndexForId(farmId);
        
        // Normalize key type to match first 3 farms
        if (keyType == "key_farm" || keyType == "keyFarm" || keyType == "farmKey" || keyType == "FarmKey")
        {
            newFarm.farmKeyType = "normal";
        }
        else if (keyType == "premiumfarmkey" || keyType == "premium_farm_key" || keyType == "premiumFarmKey" || keyType == "PremiumFarmKey")
        {
            newFarm.farmKeyType = "premium";
        }
        else
        {
            newFarm.farmKeyType = keyType;
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
            // Generic farm keys - find next available farm
            case "key_farm":
            case "keyFarm":        // ✅ Reversed version
            case "farmKey":        // ✅ ACTUAL ID from inventory
            case "FarmKey":        // ✅ Capital F variant
                return GetNextAvailableFarmId();
            
            // Premium farm key - also finds next available farm
            case "premiumfarmkey":  // ✅ From your inventory cell
            case "premiumFarmKey":  // ✅ Capital F variant
            case "premium_farm_key":
            case "PremiumFarmKey":  // ✅ Capital P variant
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
        if (wallet == null)
        {
            Debug.LogWarning("⚠️ PlayerWallet is null, defaulting to farm_004");
            return "farm_004";
        }
        
        // ✅ Use PlayerWallet.UserFarms to determine next farm
        int currentUnlockedCount = wallet.UserFarms;
        int nextFarmNumber = currentUnlockedCount + 1; // Next farm to unlock
        
        // ✅ Get max farms from FarmHeaderManager (default 99999 if not set)
        int maxFarms = 99999;
        if (farmHeaderManager != null)
        {
            maxFarms = farmHeaderManager.maxTotalFarms;
            
            // ✅ CRITICAL FIX: If maxTotalFarms is unreasonably low (like 8), use 99999 instead
            if (maxFarms < 100)
            {
                Debug.LogWarning($"⚠️ FarmHeaderManager.maxTotalFarms is too low ({maxFarms}), using 99999 instead");
                maxFarms = 99999;
            }
            
            Debug.Log($"📊 Max farms from FarmHeaderManager: {maxFarms}");
        }
        else
        {
            Debug.LogWarning("⚠️ FarmHeaderManager is null, using default max: 99999");
        }
        
        Debug.Log($"🔍 Current unlocked: {currentUnlockedCount}, Next: {nextFarmNumber}, Max: {maxFarms}");
        
        // Make sure we don't exceed maximum farms
        if (nextFarmNumber > maxFarms)
        {
            Debug.LogWarning($"⚠️ All farms are already unlocked! (User has {currentUnlockedCount}/{maxFarms})");
            return "";
        }
        
        // Return the farm ID for the next farm
        string nextFarmId = $"farm_{nextFarmNumber:D3}";
        Debug.Log($"🔑 Next farm to unlock: {nextFarmId} (user currently has {currentUnlockedCount}/{maxFarms} farms)");
        
        return nextFarmId;
    }
    
    // Helper method to get farm display name
    string GetFarmNameForId(string farmId)
    {
        // ✅ Extract farm number from ID (e.g., "farm_004" → 4)
        string numberPart = farmId.Replace("farm_", "");
        if (int.TryParse(numberPart, out int farmNumber))
        {
            return $"Farm {farmNumber}";
        }
        
        // Fallback
        return $"Farm {farmId}";
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
        
        // Fallback - parse from farmId (e.g., "farm_004" → index 3)
        string numberPart = farmId.Replace("farm_", "");
        if (int.TryParse(numberPart, out int farmNumber))
        {
            return farmNumber - 1; // Convert 1-based to 0-based index
        }
        
        return -1;
    }
}