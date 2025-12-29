using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class inventoryFarmInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InventoryCellId cellId;
    [SerializeField] PlayerWallet wallet;
    [SerializeField] InfoErrorChanger infoErrorChanger;
    [SerializeField] InventoryFarmItemApplier farmItemApplier;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] VitaminAllManager vitaminAllManager;
    
    [Header("Error Message")]
    [SerializeField] TMP_Text errorMessageText;
    
    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;
    
    [Header("Loading State")]
    [SerializeField] GameObject loadingIndicator;

    [Header("Vitamin Panel References")]
    [SerializeField] Button putVitaminButton;
    [SerializeField] TMP_Text vitaminCountText;

    private static string currentVitaminType = "";
    private static inventoryFarmInfo activeInstance = null;

    void Awake()
    {
        if (!cellId)
            cellId = GetComponent<InventoryCellId>();
            
        if (autoFind)
        {
            if (!wallet)
                wallet = FindAnyObjectByType<PlayerWallet>();
            
            if (!infoErrorChanger)
                infoErrorChanger = FindAnyObjectByType<InfoErrorChanger>();
            
            if (!farmItemApplier)
                farmItemApplier = FindAnyObjectByType<InventoryFarmItemApplier>();
            
            if (!inventoryManager)
                inventoryManager = FindAnyObjectByType<InventoryManager>();
            
            if (!vitaminAllManager)
                vitaminAllManager = FindAnyObjectByType<VitaminAllManager>();
            
            if (!errorMessageText)
            {
                GameObject errorPanel = GameObject.Find("ErrorGoToStore");
                if (!errorPanel) errorPanel = GameObject.Find("errorGoStore");
                if (!errorPanel) errorPanel = GameObject.Find("ErrorGoStore");
                
                if (errorPanel)
                {
                    errorMessageText = errorPanel.GetComponentInChildren<TMP_Text>(true);
                }
            }
        }
        
        Button btn = GetComponent<Button>();
        if (btn)
        {
            btn.onClick.AddListener(inventoryFarmInfoMethod);
        }

        if (putVitaminButton != null)
        {
            putVitaminButton.onClick.RemoveAllListeners();
            putVitaminButton.onClick.AddListener(OnPutVitaminButtonClicked);
        }
    }

    string T(string key)
    {
        return LanguageManager.Instance != null
            ? LanguageManager.Instance.GetTranslation(key)
            : key;
    }

    public void inventoryFarmInfoMethod()
    {
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            return;
        }
        
        if (!wallet)
        {
            return;
        }
        
        if (cellId.productId == "silver_egg" || 
            cellId.productId == "gold_egg" || 
            cellId.productId == "super_red_egg" || 
            cellId.productId == "super_blue_egg")
        {
            return;
        }
        
        CheckItemAvailability();
    }

    void CheckItemAvailability()
    {
        int itemCount = wallet.GetItemCount(cellId.productId);
        
        if (itemCount == 0)
        {
            if (inventoryManager != null)
            {
                ShowLoading(true);
                
                inventoryManager.GetInventory(
                    onSuccess: (response) =>
                    {
                        ShowLoading(false);
                        
                        int updatedCount = wallet.GetItemCount(cellId.productId);
                        
                        if (updatedCount == 0)
                        {
                            ShowNotEnoughItemsError();
                        }
                        else
                        {
                            ShowCorrectPanel();
                        }
                    },
                    onError: (err) =>
                    {
                        ShowLoading(false);
                        ShowNotEnoughItemsError();
                    }
                );
            }
            else
            {
                ShowNotEnoughItemsError();
            }
        }
        else
        {
            ShowCorrectPanel();
            
            if (inventoryManager != null)
            {
                inventoryManager.GetInventory(
                    onSuccess: (response) =>
                    {
                        Debug.Log("✅ Background inventory sync complete");
                    },
                    onError: (err) =>
                    {
                        Debug.LogWarning($"⚠️ Background sync failed: {err}");
                    }
                );
            }
        }
    }

    void ShowNotEnoughItemsError()
    {
        string itemName = GetItemDisplayName(cellId.productId);
        
        if (errorMessageText)
        {
            errorMessageText.text = string.Format(
                T("ItemNotEnough"),
                itemName
            );
        }
        
        if (infoErrorChanger != null)
        {
            infoErrorChanger.OpenErrorGoStore();
        }
    }

    void ShowCorrectPanel()
    {
        if (cellId.productId == "vitamin" || cellId.productId == "super_vitamin")
        {
            currentVitaminType = cellId.productId;
            activeInstance = this;
            
            if (infoErrorChanger != null)
            {
                string vitaminNameKey = cellId.productId == "vitamin"
                    ? "Item_Vitamin"
                    : "Item_SuperVitamin";

                string vitaminName = T(vitaminNameKey);

                string message = string.Format(
                    T("HowManyToPutCage"),
                    vitaminName
                );

                infoErrorChanger.OpenInfoSetVitaminToFarm(message);

                if (vitaminCountText != null && wallet != null)
                {
                    int vitaminCount = wallet.GetItemCount(cellId.productId);
                    vitaminCountText.text = vitaminCount.ToString();
                }
            }
        }
        else if (cellId.productId == "battery" || cellId.productId == "super_battery")
        {
            bool hasRobot = CheckIfRobotExists();
            
            if (hasRobot)
            {
                if (farmItemApplier != null)
                {
                    farmItemApplier.SetPendingItem(cellId.productId);
                }
                
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenInfoSetItemToFarm(
                        T("RobotTakesBattery")
                    );
                }
            }
            else
            {
                if (infoErrorChanger != null)
                {
                    infoErrorChanger.OpenErrorDefault(
                        T("BatteryNeedRobot")
                    );
                }
            }
        }
        else
        {
            if (farmItemApplier != null)
            {
                farmItemApplier.SetPendingItem(cellId.productId);
            }
            
            if (infoErrorChanger != null)
            {
                string itemName = GetItemDisplayName(cellId.productId);
                string message = string.Format(
                    T("ConfirmPlaceItem"),
                    itemName
                );

                infoErrorChanger.OpenInfoSetItemToFarm(message);

            }
        }
    }
    
    /// <summary>
    /// ✅ Check if current farm has a robot
    /// </summary>
    bool CheckIfRobotExists()
    {
        FarmDatabase farmDatabase = null;
        
        if (farmItemApplier != null)
        {
            var field = farmItemApplier.GetType().GetField("farmDatabase", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                farmDatabase = field.GetValue(farmItemApplier) as FarmDatabase;
            }
        }
        
        if (farmDatabase == null)
        {
            return false;
        }
        
        int currentFarmIndex = farmDatabase.currentFarmIndex;
        FarmData currentFarm = farmDatabase.GetFarmByIndex(currentFarmIndex);
        
        if (currentFarm == null)
        {
            return false;
        }
        
        bool hasRobot = !string.IsNullOrEmpty(currentFarm.robotType) && currentFarm.robotType != "none";
        
        return hasRobot;
    }

    void OnPutVitaminButtonClicked()
    {
        if (string.IsNullOrEmpty(currentVitaminType))
        {
            return;
        }

        if (currentVitaminType != "vitamin" && currentVitaminType != "super_vitamin")
        {
            return;
        }

        if (activeInstance == null)
        {
            return;
        }

        if (activeInstance.vitaminAllManager == null)
        {
            if (activeInstance.infoErrorChanger != null)
            {
                activeInstance.infoErrorChanger.OpenErrorDefault("System error: VitaminAllManager not found");
            }
            return;
        }

        FarmDatabase farmDatabase = null;
        
        if (activeInstance.farmItemApplier != null)
        {
            var field = activeInstance.farmItemApplier.GetType().GetField("farmDatabase", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                farmDatabase = field.GetValue(activeInstance.farmItemApplier) as FarmDatabase;
            }
        }
        
        if (farmDatabase == null)
        {
            if (activeInstance.infoErrorChanger != null)
            {
                activeInstance.infoErrorChanger.OpenErrorDefault("System error: FarmDatabase not found");
            }
            return;
        }

        int currentFarmIndex = farmDatabase.currentFarmIndex;
        
        activeInstance.ShowLoading(true);

        activeInstance.vitaminAllManager.ApplyVitaminToAllByIndex(
            farmIndex: currentFarmIndex,
            vitaminType: currentVitaminType,
            onSuccess: (response) =>
            {
                activeInstance.ShowLoading(false);
                
                VitaminAllManager.VitaminAllResponse data = JsonUtility.FromJson<VitaminAllManager.VitaminAllResponse>(response);
                
                if (activeInstance.infoErrorChanger != null)
                {
                    activeInstance.infoErrorChanger.CloseAllInfoErrorMethod();
                }
                
                activeInstance.RefreshInventoryAfterUse();
                
                currentVitaminType = "";
                activeInstance = null;
            },
            onError: (error) =>
            {
                activeInstance.ShowLoading(false);
                
                if (activeInstance.infoErrorChanger != null)
                {
                    activeInstance.infoErrorChanger.OpenErrorDefault($"Failed to apply vitamins: {error}");
                }
            }
        );
    }

    void ShowLoading(bool show)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(show);
        }
    }

    string GetItemDisplayName(string productId)
    {
        switch (productId)
        {
            case "robot": return T("Item_Robot");
            case "battery": return T("Item_Battery");
            case "super_battery": return T("Item_SuperBattery");
            case "vitamin": return T("Item_Vitamin");
            case "super_vitamin": return T("Item_SuperVitamin");
            default: return T("Item_Unknown");
        }
    }

    void RefreshInventoryAfterUse()
    {
        if (inventoryManager != null)
        {
            inventoryManager.GetInventory(
                onSuccess: (response) =>
                {
                    Debug.Log("✅ Inventory refreshed after vitamin use");
                },
                onError: (err) =>
                {
                    Debug.LogError($"Failed to refresh inventory: {err}");
                }
            );
        }
    }
}