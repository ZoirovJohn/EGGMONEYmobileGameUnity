using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class inventoryCageInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InventoryCellId cellId;
    [SerializeField] PlayerWallet wallet;
    [SerializeField] InfoErrorChanger infoErrorChanger;
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
            
            if (!inventoryManager)
                inventoryManager = FindAnyObjectByType<InventoryManager>();
            
            if (!errorMessageText)
            {
                // Try to find error message text in the scene
                GameObject errorPanel = GameObject.Find("ErrorGoToStore");
                if (!errorPanel) errorPanel = GameObject.Find("errorGoStore");
                if (!errorPanel) errorPanel = GameObject.Find("ErrorGoStore");
                
                if (errorPanel)
                {
                    errorMessageText = errorPanel.GetComponentInChildren<TMP_Text>(true);
                }
            }
        }
        
        // Hook up button click
        Button btn = GetComponent<Button>();
        if (btn)
        {
            btn.onClick.AddListener(inventoryCageInfoMethod);
        }
    }

    public void inventoryCageInfoMethod()
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
        
        // ✅ First check local wallet (fast feedback)
        CheckItemAvailability();
    }

    void CheckItemAvailability()
    {
        // Get item count from wallet
        int itemCount = wallet.GetItemCount(cellId.productId);
        
        Debug.Log($"📦 Checking inventory for '{cellId.productId}': {itemCount} available");
        
        if (itemCount == 0)
        {
            // ✅ Sync with backend to ensure accuracy before showing error
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
                        
                        // Show error anyway if sync failed
                        ShowNotEnoughItemsError();
                    }
                );
            }
            else
            {
                // No inventory manager, just show error
                ShowNotEnoughItemsError();
            }
        }
        else
        {
            // Item count > 0, show SetItemToFarm panel
            ShowSetItemPanel();
        }
    }

    void ShowNotEnoughItemsError()
    {
        // Get item name for better error message
        string itemName = GetItemDisplayName(cellId.productId);
        
        if (errorMessageText)
        {
            errorMessageText.text = $"You don't have any {itemName}.\nPurchase it from the store.";
        }
        
        // Use InfoErrorChanger to show ErrorGoStore panel
        if (infoErrorChanger != null)
        {
            infoErrorChanger.OpenErrorGoStore();
            Debug.Log("🚨 Opened ErrorGoStore - Item not available");
        }
        else
        {
            Debug.LogWarning("⚠️ InfoErrorChanger is not assigned!");
        }
    }

    void ShowSetItemPanel()
    {
        if (infoErrorChanger != null)
        {
            infoErrorChanger.OpenInfoSetItemToCage();
            Debug.Log($"✅ Opened InfoSetItemToCage for '{cellId.productId}'");
        }
        else
        {
            Debug.LogWarning("⚠️ InfoErrorChanger is not assigned!");
        }
    }

    void ShowLoading(bool show)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(show);
        }
    }

    // ✅ Get user-friendly item names
    string GetItemDisplayName(string productId)
    {
        switch (productId)
        {
            case "nest":
                return "Nest";
            case "silver_egg":
                return "Silver Egg";
            case "food":
                return "Food";
            case "gold_egg":
                return "Gold Egg";
            case "booster":
                return "Vitamin Booster";
            case "battery":
                return "Battery";
            case "robot":
                return "Robot";
            case "super_blue_egg":
                return "Super Blue Egg";
            case "super_red_egg":
                return "Super Red Egg";
            case "farmKey":
                return "Farm Key";
            default:
                return "this item";
        }
    }

    // ✅ PUBLIC: Reload inventory after using an item
    public void RefreshInventoryAfterUse()
    {
        if (inventoryManager != null)
        {
            inventoryManager.GetInventory(
                onSuccess: (response) =>
                {
                    Debug.Log("✅ Inventory refreshed after item use");
                },
                onError: (err) =>
                {
                    Debug.LogError($"Failed to refresh inventory: {err}");
                }
            );
        }
    }
}