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

    // ✅ NEW: Static variable shared across all instances
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

        // ✅ Only ONE instance should hook up the button
        if (putVitaminButton != null)
        {
            putVitaminButton.onClick.RemoveAllListeners();
            putVitaminButton.onClick.AddListener(OnPutVitaminButtonClicked);
        }
    }

    public void inventoryFarmInfoMethod()
    {
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            Debug.LogWarning("Cell ID or product ID is missing!");
            return;
        }
        
        if (!wallet)
        {
            Debug.LogWarning("Wallet reference is missing!");
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
                        Debug.LogError($"Failed to sync inventory: {err}");
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
            errorMessageText.text = $"You don't have any {itemName}.\nPurchase it from the store.";
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
            // ✅ STORE the vitamin type (static, shared across all instances)
            currentVitaminType = cellId.productId;
            activeInstance = this;
            
            Debug.Log($"✅ Stored vitamin type: {currentVitaminType}");
            
            if (infoErrorChanger != null)
            {
                string vitaminName = cellId.productId == "vitamin" ? "Vitamin" : "Super Vitamin";
                string message = $"How many {vitaminName} do you want to put for hens?";
                
                infoErrorChanger.OpenInfoSetVitaminToFarm(message);
                
                if (vitaminCountText != null && wallet != null)
                {
                    int vitaminCount = wallet.GetItemCount(cellId.productId);
                    vitaminCountText.text = vitaminCount.ToString();
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
                infoErrorChanger.OpenInfoSetItemToFarm();
            }
        }
    }

    void OnPutVitaminButtonClicked()
    {
        Debug.Log($"🔵 Button clicked! Current vitamin type: {currentVitaminType}");
        
        if (string.IsNullOrEmpty(currentVitaminType))
        {
            Debug.LogWarning("⚠️ No vitamin type stored!");
            return;
        }

        if (currentVitaminType != "vitamin" && currentVitaminType != "super_vitamin")
        {
            Debug.LogWarning("⚠️ Stored item is not a vitamin!");
            return;
        }

        if (activeInstance == null)
        {
            Debug.LogError("❌ No active instance!");
            return;
        }

        if (activeInstance.vitaminAllManager == null)
        {
            Debug.LogError("❌ VitaminAllManager not found!");
            if (activeInstance.infoErrorChanger != null)
            {
                activeInstance.infoErrorChanger.OpenErrorDefault("System error: VitaminAllManager not found");
            }
            return;
        }

        // ✅ FIXED: Get FarmDatabase from VitaminAllManager's public field
        FarmDatabase farmDatabase = activeInstance.vitaminAllManager.GetComponent<VitaminAllManager>()?.GetFarmDatabase();
        
        // ✅ If that doesn't work, try finding it
        if (farmDatabase == null)
        {
            farmDatabase = FindAnyObjectByType<FarmDatabase>(FindObjectsInactive.Include);
        }
        
        if (farmDatabase == null)
        {
            Debug.LogError("❌ FarmDatabase not found in scene!");
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
                
                Debug.Log($"✅ Applied to {data.hensAffected} hens! Used {data.vitaminsUsed} vitamins");
                
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
                
                Debug.LogError($"❌ Failed to apply vitamins: {error}");
                
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
            case "robot": return "Robot";
            case "battery": return "Battery";
            case "super_battery": return "Super Battery";
            case "vitamin": return "Vitamin";
            case "super_vitamin": return "Super Vitamin";
            default: return "this item";
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