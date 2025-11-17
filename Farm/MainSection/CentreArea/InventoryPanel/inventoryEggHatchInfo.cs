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
        
        // Get button reference
        button = GetComponent<Button>();
        if (button)
        {
            // Add listener at the BEGINNING (before inventoryFarmInfo)
            button.onClick.AddListener(OnEggCellClicked);
        }
        
        Debug.Log($"🔧 inventoryEggHatchInfo initialized on {gameObject.name}");
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
        Debug.Log($"🖱️ Egg handler clicked! GameObject: {gameObject.name}");
        
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            Debug.LogWarning($"Cell ID or product ID is missing! cellId: {cellId}, productId: {(cellId != null ? cellId.productId : "null")}");
            // Not an egg, let other handlers process
            return;
        }
        
        Debug.Log($"🔍 Cell product ID: {cellId.productId}");
        
        // Check if it's an egg item - ONLY handle eggs
        if (cellId.productId != "silver_egg" && cellId.productId != "gold_egg")
        {
            Debug.Log($"Item '{cellId.productId}' is not an egg, letting other handlers process");
            // Not an egg, let other handlers process
            return;
        }
        
        // ✅ IT'S AN EGG - STOP OTHER HANDLERS FROM EXECUTING
        Debug.Log($"✅✅✅ It's an egg! Type: {cellId.productId} - BLOCKING other handlers");
        
        if (!wallet)
        {
            Debug.LogWarning("Wallet reference is missing!");
            return;
        }
        
        // Remove ALL other listeners temporarily to prevent inventoryFarmInfo from executing
        if (button != null)
        {
            // Store current listener count
            int listenerCount = button.onClick.GetPersistentEventCount();
            Debug.Log($"🔧 Button has {listenerCount} persistent listeners");
            
            // The onClick is a UnityEvent, we can't easily remove runtime listeners
            // So we'll rely on inventoryFarmInfo's check instead
        }
        
        eggType = cellId.productId;
        
        // Check egg availability
        CheckEggAvailability();
    }

    void CheckEggAvailability()
    {
        // Get egg count from wallet
        int availableEggs = wallet.GetItemCount(eggType);
        
        Debug.Log($"🥚 Checking '{eggType}': {availableEggs} available locally");
        
        if (availableEggs == 0)
        {
            Debug.Log("⚠️ No eggs available locally, syncing with backend...");
            
            // Sync with backend to ensure accuracy
            if (inventoryManager != null)
            {
                inventoryManager.GetInventory(
                    onSuccess: (response) =>
                    {
                        // Recheck after sync
                        availableEggs = wallet.GetItemCount(eggType);
                        
                        Debug.Log($"📥 After sync: {availableEggs} eggs available");
                        
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
            Debug.Log($"✅ Has {availableEggs} eggs locally, opening hatch panel...");
            
            // Has eggs locally, open hatch panel
            OpenHatchPanel(availableEggs);
            
            // Background sync (don't block user)
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
        Debug.Log($"🚪 OpenHatchPanel called with {availableEggs} eggs");
        
        if (!infoHatchManager)
        {
            Debug.LogError("❌ InfoHatchManager reference is missing! Trying to find it...");
            
            infoHatchManager = FindAnyObjectByType<InfoHatchManager>();
            
            if (!infoHatchManager)
            {
                Debug.LogError("❌ InfoHatchManager not found in scene! Make sure InfoHatchManager script is attached to InfoHatch panel.");
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
        
        // Let InfoHatchManager handle everything
        Debug.Log($"📞 Calling InitializeHatchPanel({eggType}, {availableEggs})");
        infoHatchManager.InitializeHatchPanel(eggType, availableEggs);
    }
}