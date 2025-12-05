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
    
    [Header("Error Message")]
    [SerializeField] TMP_Text errorMessageText;
    
    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;
    
    [Header("Loading State")]
    [SerializeField] GameObject loadingIndicator;

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
        
        // ✅ SKIP EGGS - they should open InfoHatch, not InfoSetItemToFarm
        if (cellId.productId == "silver_egg" || 
            cellId.productId == "gold_egg" || 
            cellId.productId == "super_red_egg" || 
            cellId.productId == "super_blue_egg")
        {
            return;
        }
        
        // ✅ Sync with backend first, then check
        CheckItemAvailability();
    }

    void CheckItemAvailability()
    {
        // Get item count from wallet (may be stale)
        int itemCount = wallet.GetItemCount(cellId.productId);
        
        if (itemCount == 0)
        {
            // ✅ Sync with backend to ensure accuracy
            if (inventoryManager != null)
            {
                ShowLoading(true);
                
                inventoryManager.GetInventory(
                    onSuccess: (response) =>
                    {
                        ShowLoading(false);
                        
                        // Recheck after sync
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
            // Has items locally, show correct panel
            ShowCorrectPanel();
            
            // Background sync (don't block user)
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
        // ✅ Check if item is vitamin or super_vitamin
        if (cellId.productId == "vitamin" || cellId.productId == "super_vitamin")
        {
            // Show cage panel for vitamins
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenInfoSetItemToCage();
            }
        }
        else
        {
            // Show farm panel for other items (robot, battery, etc.)
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
}