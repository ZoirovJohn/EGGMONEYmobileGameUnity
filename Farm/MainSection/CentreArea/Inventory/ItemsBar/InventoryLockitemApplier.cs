using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryLockItemApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FarmDatabase farmDatabase;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private InfoErrorChanger infoErrorChanger;
    [SerializeField] private FarmGridManager farmGridManager;
    [SerializeField] private FarmHeaderManager farmHeaderManager;
    
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
        Debug.Log($"🔑 Pending key to use: {keyId}");
    }
    
    void OnNoClicked()
    {
        Debug.Log("❌ User clicked NO - canceling farm unlock");
        pendingKeyId = "";
        
        // Hide Yes/No buttons
        if (yesButton)
        {
            yesButton.gameObject.SetActive(false);
            Debug.Log("🙈 Hidden Yes button");
        }
        
        if (noButton)
        {
            noButton.gameObject.SetActive(false);
            Debug.Log("🙈 Hidden No button");
        }
        
        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
        }
        
        Debug.Log("🔄 Reset to initial state - buttons hidden");
    }
    
    void OnYesClicked()
    {
        Debug.Log("✅ User clicked YES - attempting to unlock farm");
        
        if (string.IsNullOrEmpty(pendingKeyId))
        {
            Debug.LogWarning("⚠️ No pending key ID!");
            return;
        }
        
        // 1. Check if player has the key
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
        
        // All checks passed - unlock the farm!
        
        // 4. Remove key from wallet
        if (!wallet.TryConsumeItem(pendingKeyId, 1))
        {
            Debug.LogError($"❌ Failed to consume key: {pendingKeyId}");
            return;
        }
        Debug.Log($"🔑 Removed 1x {pendingKeyId} from wallet");
        
        // 5. Create and add new farm data with default values
        FarmData newFarm = CreateNewFarmData(farmIdToUnlock, pendingKeyId);
        
        if (farmDatabase != null)
        {
            farmDatabase.farms.Add(newFarm);
            // Save to persistent storage if you have a save system
            // farmDatabase.SaveFarmsData();
            Debug.Log($"✅ Created new farm: {newFarm.farmName} ({newFarm.farmId})");
        }
        else
        {
            Debug.LogError("❌ FarmDatabase is null!");
            return;
        }
        
        // 6. Update farm counts in FarmHeaderManager
        if (farmHeaderManager != null)
        {
            farmHeaderManager.farmCount++;
            farmHeaderManager.lockCount--;
            Debug.Log($"📊 Updated counts: {farmHeaderManager.farmCount} farms, {farmHeaderManager.lockCount} locks");
        }
        
        // 7. Start unlock sequence (visual feedback + open farm)
        StartCoroutine(UnlockFarmSequence(farmIdToUnlock));
    }
    
    IEnumerator UnlockFarmSequence(string farmId)
    {
        // Get the NEW farm index (it's the last one since we just added it)
        int newFarmIndex = -1;
        if (farmDatabase != null && farmDatabase.farms != null)
        {
            newFarmIndex = farmDatabase.farms.Count - 1; // Last farm in the list
            Debug.Log($"🔓 Unlocking farm {farmId} at index {newFarmIndex}");
        }
        
        // Toggle lock visuals (adjust GameObject names based on your hierarchy)
        GameObject lockClose = GameObject.Find($"ImageLockClose_{farmId}");
        GameObject lockOpen = GameObject.Find($"ImageLockOpen_{farmId}");
        
        // Alternative naming patterns
        if (!lockClose) lockClose = GameObject.Find($"LockClose_{newFarmIndex}");
        if (!lockOpen) lockOpen = GameObject.Find($"LockOpen_{newFarmIndex}");
        
        if (lockClose)
        {
            lockClose.SetActive(false);
            Debug.Log($"🔒 Disabled ImageLockClose for {farmId}");
        }
        
        if (lockOpen)
        {
            lockOpen.SetActive(true);
            Debug.Log($"🔓 Enabled ImageLockOpen for {farmId}");
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
            Debug.Log("🚪 Closed all info/error panels");
        }
        
        // Refresh FarmHeaderManager to show the new farm slot
        if (farmHeaderManager != null)
        {
            farmHeaderManager.Refresh();
            Debug.Log("🔄 Refreshed FarmHeaderManager");
        }
        
        // Switch to the newly unlocked farm
        if (newFarmIndex >= 0)
        {
            // Call DefaultBannerMethod from InventoryItemsBarChanger
            InventoryItemsBarChanger barChanger = FindAnyObjectByType<InventoryItemsBarChanger>();
            if (barChanger != null)
            {
                barChanger.DefaultBannerMethod();
                Debug.Log("🎌 Called DefaultBannerMethod");
            }
            else
            {
                Debug.LogWarning("⚠️ InventoryItemsBarChanger not found!");
            }
            
            if (farmGridManager != null)
            {
                farmGridManager.SwitchFarm(newFarmIndex);
                Debug.Log($"🏡 Switched to newly unlocked farm: {farmId} (index {newFarmIndex})");
            }
            else
            {
                Debug.LogWarning("⚠️ FarmGridManager is null - cannot switch farm");
            }
            
            if (farmDatabase != null)
            {
                farmDatabase.SwitchToFarm(newFarmIndex);
                Debug.Log($"📂 FarmDatabase switched to index {newFarmIndex}");
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
        
        Debug.Log($"📦 Created farm data: {newFarm.farmName} (Index: {newFarm.farmIndex}, KeyType: {newFarm.farmKeyType}) with 100 empty cages");
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
                Debug.Log($"✅ Next available farm: {farmId}");
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