using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class inventoryCageInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InventoryCellId cellId;
    [SerializeField] PlayerWallet wallet;
    
    [Header("Panels")]
    [SerializeField] GameObject errorGoToStorePanel;
    [SerializeField] GameObject setItemToFarmPanel;
    
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
            
            if (!errorGoToStorePanel)
            {
                errorGoToStorePanel = GameObject.Find("ErrorGoToStore");
                if (!errorGoToStorePanel) errorGoToStorePanel = GameObject.Find("errorGoStore");
                if (!errorGoToStorePanel) errorGoToStorePanel = GameObject.Find("ErrorGoStore");
            }
            
            if (!setItemToFarmPanel)
            {
                setItemToFarmPanel = GameObject.Find("SetItemToFarm");
                if (!setItemToFarmPanel) setItemToFarmPanel = GameObject.Find("SetItemToFarmPanel");
            }
            
            if (!errorMessageText && errorGoToStorePanel)
            {
                // Try to find text field in error panel
                errorMessageText = errorGoToStorePanel.GetComponentInChildren<TMP_Text>(true);
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
        
        // Get item count from wallet
        int itemCount = wallet.GetItemCount(cellId.productId);
        
        if (itemCount == 0)
        {
            // Show error: not enough items
            if (errorMessageText)
            {
                errorMessageText.text = "You don't have enough of the item.\nPurchase it from the store.";
            }
            
            if (errorGoToStorePanel)
            {
                errorGoToStorePanel.SetActive(true);
            }
        }
        else
        {
            // Item count > 0, show SetItemToFarm panel
            if (setItemToFarmPanel)
            {
                setItemToFarmPanel.SetActive(true);
            }
        }
    }
}