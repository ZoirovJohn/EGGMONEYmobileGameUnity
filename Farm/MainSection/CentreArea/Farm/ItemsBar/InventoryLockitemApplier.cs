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
        if (string.IsNullOrEmpty(pendingKeyId))
        {
            return;
        }
        
        if (wallet == null || wallet.GetItemCount(pendingKeyId) <= 0)
        {
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault("You don't have this key.");
            return;
        }
        
        string farmIdToUnlock = GetFarmIdForKey(pendingKeyId);
        
        if (string.IsNullOrEmpty(farmIdToUnlock))
        {
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault("Invalid key.");
            return;
        }
        
        if (wallet != null)
        {
            int currentFarms = wallet.UserFarms;
            int maxFarms = 99999; 
            if (farmHeaderManager != null)
            {
                maxFarms = farmHeaderManager.maxTotalFarms;
                if (maxFarms < 100)
                {
                    maxFarms = 99999;
                }
            }
            
            if (currentFarms >= maxFarms)
            {
                if (infoErrorChanger != null)
                    infoErrorChanger.OpenErrorDefault("All farms are already unlocked.");
                return;
            }
        }
        
        if (useFarmKeyManager != null)
        {
            useFarmKeyManager.UseFarmKey(
                pendingKeyId,
                onSuccess: (response) =>
                {
                    RefreshUserProfile(farmIdToUnlock);
                },
                onError: (error) =>
                {
                    if (infoErrorChanger != null)
                        infoErrorChanger.OpenErrorDefault("Failed to unlock farm. Please try again.");
                }
            );
        }
        else
        {
            if (!wallet.TryConsumeItem(pendingKeyId, 1))
            {
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
            RefreshInventory(farmIdToUnlock);
            return;
        }
        
        authManager.GetUserProfile(
            onSuccess: (profile) =>
            {
                RefreshInventory(farmIdToUnlock);
            },
            onError: (error) =>
            {
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
            ReloadFarmDataFromBackend(farmIdToUnlock);
            return;
        }
        
        inventoryManager.GetInventory(
            onSuccess: (invResponse) =>
            {
                ReloadFarmDataFromBackend(farmIdToUnlock);
            },
            onError: (invError) =>
            {
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
            ProceedWithFarmUnlock(farmIdToUnlock);
            return;
        }
        
        int newFarmCount = wallet.UserFarms;
        int newFarmNumber = newFarmCount; 
        int newFarmIndex = newFarmCount - 1; 
        
        if (farmAPIManager == null)
        {
            ProceedWithFarmUnlock(farmIdToUnlock);
            return;
        }
        
        farmAPIManager.GetFarmSummary(
            newFarmNumber,
            onSuccess: (summary) =>
            {
                AddNewFarmToDatabase(summary, newFarmIndex);
                ProceedWithFarmUnlockByIndex(newFarmIndex);
            },
            onError: (error) =>
            {
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
            return;
        }
        
        string farmId = $"farm_{(newFarmIndex + 1):D3}";
        string farmKeyType = (summary.farm != null && summary.farm.isPremium) ? "premium" : "normal";
        string robotType = "none";
        if (summary.robot != null && !string.IsNullOrEmpty(summary.robot.id))
        {
            robotType = summary.robot.id;
        }
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
        
        if (summary.nests != null && summary.nests.details != null)
        {
            var nestList = new System.Collections.Generic.List<NestDetail>(summary.nests.details);
            farmDatabase.SetFarmNestDetails(newFarmIndex, nestList);
        }
        
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
        farmDatabase.GenerateCagesFromFarmData();
    }
    
    void ProceedWithFarmUnlock(string farmIdToUnlock)
    {
        FarmData newFarm = CreateNewFarmData(farmIdToUnlock, pendingKeyId);
        
        if (farmDatabase != null)
        {
            farmDatabase.farms.Add(newFarm);
        }
        else
        {
            return;
        }
        
        int newFarmIndex = farmDatabase.farms.Count - 1;
        StartCoroutine(UnlockFarmSequenceByIndex(farmIdToUnlock, newFarmIndex));
    }
    
    /// <summary>
    /// ✅ Proceed with unlock using farm index (when loaded from backend)
    /// </summary>
    void ProceedWithFarmUnlockByIndex(int newFarmIndex)
    {
        FarmData newFarm = farmDatabase.GetFarmByIndex(newFarmIndex);
        string farmId = newFarm != null ? newFarm.farmId : "";
        StartCoroutine(UnlockFarmSequenceByIndex(farmId, newFarmIndex));
    }
    
    /// <summary>
    /// ✅ Unified unlock sequence that works with farm index
    /// </summary>
    System.Collections.IEnumerator UnlockFarmSequenceByIndex(string farmId, int newFarmIndex)
    {
        GameObject lockClose = GameObject.Find($"ImageLockClose_{farmId}");
        GameObject lockOpen = GameObject.Find($"ImageLockOpen_{farmId}");
        if (!lockClose) lockClose = GameObject.Find($"LockClose_{newFarmIndex}");
        if (!lockOpen) lockOpen = GameObject.Find($"LockOpen_{newFarmIndex}");
        
        if (lockClose)
            lockClose.SetActive(false);
        
        if (lockOpen)
            lockOpen.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        
        if (yesButton)
            yesButton.gameObject.SetActive(false);
        if (noButton)
            noButton.gameObject.SetActive(false);
        
        if (infoErrorChanger != null)
            infoErrorChanger.CloseAllInfoErrorMethod();
        
        if (farmHeaderManager != null)
        {
            farmHeaderManager.Refresh();
        }
        
        if (newFarmIndex >= 0)
        {
            InventoryItemsBarChanger barChanger = FindAnyObjectByType<InventoryItemsBarChanger>();
            if (barChanger != null)
            {
                barChanger.DefaultBannerMethod();
            }
            
            if (farmGridManager != null)
            {
                farmGridManager.SwitchFarm(newFarmIndex);
            }
            
            if (farmDatabase != null)
            {
                farmDatabase.SwitchToFarm(newFarmIndex);
            }
            
            if (farmHeaderManager != null)
            {
                farmHeaderManager.SelectFarm(newFarmIndex);
            }
        }
        
        if (FXManager.Instance != null)
        {
            FXManager.Instance.PlayGameFX_Center_4Times();
        }

        pendingKeyId = "";
    }
    
    FarmData CreateNewFarmData(string farmId, string keyType)
    {
        FarmData newFarm = new FarmData();
        newFarm.farmId = farmId;
        newFarm.farmName = GetFarmNameForId(farmId);
        newFarm.farmIndex = GetFarmIndexForId(farmId);
        
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
    
    string GetFarmIdForKey(string keyId)
    {
        switch (keyId)
        {
            case "key_farm":
            case "keyFarm":        
            case "farmKey":       
            case "FarmKey":       
                return GetNextAvailableFarmId();
            
            case "premiumfarmkey":  
            case "premiumFarmKey": 
            case "premium_farm_key":
            case "PremiumFarmKey": 
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
                return "";
        }
    }
    
    // Helper method to get the next available farm ID based on what's already unlocked
    string GetNextAvailableFarmId()
    {
        if (wallet == null)
        {
            return "farm_004";
        }
        
        int currentUnlockedCount = wallet.UserFarms;
        int nextFarmNumber = currentUnlockedCount + 1; 
        
        int maxFarms = 99999;
        if (farmHeaderManager != null)
        {
            maxFarms = farmHeaderManager.maxTotalFarms;
            
            if (maxFarms < 100)
            {
                maxFarms = 99999;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ FarmHeaderManager is null, using default max: 99999");
        }
        
        if (nextFarmNumber > maxFarms)
        {
            return "";
        }
        
        string nextFarmId = $"farm_{nextFarmNumber:D3}";
        return nextFarmId;
    }
    
    string GetFarmNameForId(string farmId)
    {
        string numberPart = farmId.Replace("farm_", "");
        if (int.TryParse(numberPart, out int farmNumber))
        {
            return $"Farm {farmNumber}";
        }
        
        return $"Farm {farmId}";
    }
    
    int GetFarmIndexForId(string farmId)
    {
        if (farmDatabase != null && farmDatabase.farms != null)
        {
            return farmDatabase.farms.Count; 
        }
        
        string numberPart = farmId.Replace("farm_", "");
        if (int.TryParse(numberPart, out int farmNumber))
        {
            return farmNumber - 1;
        }
        
        return -1;
    }
}