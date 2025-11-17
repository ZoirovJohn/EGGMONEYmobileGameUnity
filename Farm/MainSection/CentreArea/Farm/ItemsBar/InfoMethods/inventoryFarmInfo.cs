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
        
        // ✅ Sync with backend first, then check
        CheckItemAvailability();
    }

    void CheckItemAvailability()
    {
        // Get item count from wallet (may be stale)
        int itemCount = wallet.GetItemCount(cellId.productId);
        
        Debug.Log($"📦 Checking '{cellId.productId}': {itemCount} available locally");
        
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
                            ShowSetItemPanel();
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
            // Has items locally, but still sync in background
            ShowSetItemPanel();
            
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
            Debug.Log("🚨 Opened ErrorGoStore - Item not available");
        }
    }

    void ShowSetItemPanel()
    {
        if (farmItemApplier != null)
        {
            farmItemApplier.SetPendingItem(cellId.productId);
            Debug.Log($"📦 Set pending item: {cellId.productId}");
        }
        
        if (infoErrorChanger != null)
        {
            infoErrorChanger.OpenInfoSetItemToFarm();
            Debug.Log("✅ Opened InfoSetItemToFarm");
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
            default: return "this item";
        }
    }
}