using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class inventoryFarmInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InventoryCellId cellId;
    [SerializeField] PlayerWallet wallet;
    [SerializeField] InfoErrorChanger infoErrorChanger;
    [SerializeField] InventoryFarmItemApplier farmItemApplier; // ⭐ ADD THIS
    
    [Header("Error Message")]
    [SerializeField] TMP_Text errorMessageText;
    
    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;

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
            
            // ⭐ ADD THIS - Auto find farmItemApplier
            if (!farmItemApplier)
                farmItemApplier = FindAnyObjectByType<InventoryFarmItemApplier>();
            
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
        
        // Get item count from wallet
        int itemCount = wallet.GetItemCount(cellId.productId);
        
        if (itemCount == 0)
        {
            // Show error: not enough items
            if (errorMessageText)
            {
                errorMessageText.text = "You don't have enough of the item.\nPurchase it from the store.";
            }
            
            // Use InfoErrorChanger to show ErrorGoStore panel
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorGoStore();
                Debug.Log("🚨 Opened ErrorGoStore via InfoErrorChanger");
            }
            else
            {
                Debug.LogWarning("⚠️ InfoErrorChanger is not assigned!");
            }
        }
        else
        {
            // Item count > 0, show SetItemToFarm panel
            
            // ⭐ ADD THIS - Set the pending item BEFORE opening the panel
            if (farmItemApplier != null)
            {
                farmItemApplier.SetPendingItem(cellId.productId);
                Debug.Log($"📦 Set pending item: {cellId.productId}");
            }
            else
            {
                Debug.LogWarning("⚠️ InventoryFarmItemApplier is not assigned!");
            }
            
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenInfoSetItemToFarm();
                Debug.Log("✅ Opened InfoSetItemToFarm via InfoErrorChanger");
            }
            else
            {
                Debug.LogWarning("⚠️ InfoErrorChanger is not assigned!");
            }
        }
    }
}