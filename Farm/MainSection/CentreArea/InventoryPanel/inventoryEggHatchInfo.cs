using UnityEngine;
using UnityEngine.UI;

public class inventoryEggHatchInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InventoryCellId cellId;
    [SerializeField] PlayerWallet wallet;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] InfoHatchManager infoHatchManager;
    
    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;

    // Runtime
    private string eggType = "";
    private Button button;

    void Awake()
    {
        if (!cellId)
            cellId = GetComponent<InventoryCellId>();
            
        if (autoFind)
        {
            if (!wallet)
                wallet = FindAnyObjectByType<PlayerWallet>();
            
            if (!inventoryManager)
                inventoryManager = FindAnyObjectByType<InventoryManager>();
            
            if (!infoHatchManager)
                infoHatchManager = FindAnyObjectByType<InfoHatchManager>();
        }
        
        button = GetComponent<Button>();
        if (button)
        {
            button.onClick.AddListener(OnEggCellClicked);
        }
    }

    void OnDestroy()
    {
        if (button)
        {
            button.onClick.RemoveListener(OnEggCellClicked);
        }
    }

    public void OnEggCellClicked()
    {
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            return;
        }
        
        if (cellId.productId != "silver_egg" && 
            cellId.productId != "gold_egg" && 
            cellId.productId != "super_red_egg" && 
            cellId.productId != "super_blue_egg")
        {
            return;
        }
        
        if (!wallet)
        {
            return;
        }
        
        eggType = cellId.productId;
        CheckEggAvailability();
    }

    void CheckEggAvailability()
    {
        int availableEggs = wallet.GetItemCount(eggType);
        
        if (availableEggs == 0)
        {
            if (inventoryManager != null)
            {
                inventoryManager.GetInventory(
                    onSuccess: (response) =>
                    {
                        availableEggs = wallet.GetItemCount(eggType);
                        
                        if (availableEggs > 0)
                        {
                            OpenHatchPanel(availableEggs);
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ No {eggType} available after sync");
                        }
                    },
                    onError: (err) =>
                    {
                        Debug.LogError($"Failed to sync inventory: {err}");
                    }
                );
            }
            else
            {
                Debug.LogWarning($"⚠️ No {eggType} available and no inventory manager to sync");
            }
        }
        else
        {
            OpenHatchPanel(availableEggs);
            if (inventoryManager != null)
            {
                inventoryManager.GetInventory(
                    onSuccess: (response) => 
                    {
                        Debug.Log("✅ Background sync complete");
                    },
                    onError: (err) => Debug.LogWarning($"⚠️ Background sync failed: {err}")
                );
            }
        }
    }

    void OpenHatchPanel(int availableEggs)
    {
        if (!infoHatchManager)
        {
            infoHatchManager = FindAnyObjectByType<InfoHatchManager>();
            
            if (!infoHatchManager)
            {
                return;
            }
            else
            {
                Debug.Log($"✅ Found InfoHatchManager on: {infoHatchManager.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"✅ InfoHatchManager already assigned on: {infoHatchManager.gameObject.name}");
        }
        
        infoHatchManager.InitializeHatchPanel(eggType, availableEggs);
    }
}