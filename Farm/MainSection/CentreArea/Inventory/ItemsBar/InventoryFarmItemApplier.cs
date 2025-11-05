using UnityEngine;
using UnityEngine.UI;

public class InventoryFarmItemApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FarmDatabase farmDatabase;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private InfoErrorChanger infoErrorChanger;
    [SerializeField] private InventoryItemsBarChanger inventoryBarChanger;
    
    [Header("Buttons")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [Header("Auto Find")]
    [SerializeField] private bool autoFind = true;
    
    private string pendingProductId = "";
    
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
            
            if (!inventoryBarChanger)
                inventoryBarChanger = FindAnyObjectByType<InventoryItemsBarChanger>();
        }
        
        // Hook up buttons
        if (yesButton)
            yesButton.onClick.AddListener(OnYesClicked);
        
        if (noButton)
            noButton.onClick.AddListener(OnNoClicked);
    }
    
    public void SetPendingItem(string productId)
    {
        pendingProductId = productId;
        Debug.Log($"📦 Pending item to apply: {productId}");
    }
    
    void OnNoClicked()
    {
        Debug.Log("❌ User clicked NO - canceling item application");
        pendingProductId = "";
        
        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
        }
    }
    
    void OnYesClicked()
    {
        Debug.Log("✅ User clicked YES - attempting to apply item");
        
        if (string.IsNullOrEmpty(pendingProductId))
        {
            Debug.LogWarning("⚠️ No pending product ID!");
            return;
        }
        
        // Get current farm ID from inventory bar changer
        string currentFarmId = "";
        if (inventoryBarChanger != null)
        {
            currentFarmId = inventoryBarChanger.GetCurrentFarmId();
            Debug.Log($"📍 Current Farm ID from InventoryBarChanger: {currentFarmId}");
        }
        
        if (string.IsNullOrEmpty(currentFarmId))
        {
            Debug.LogError("❌ No farm selected!");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("Please select a farm first.");
            }
            return;
        }
        
        // Find the farm by farmId
        FarmData targetFarm = null;
        if (farmDatabase != null && farmDatabase.farms != null)
        {
            targetFarm = farmDatabase.farms.Find(f => f.farmId == currentFarmId);
            
            if (targetFarm == null)
            {
                Debug.LogError($"❌ Could not find farm with ID: {currentFarmId}");
                Debug.Log($"📋 Available farms in database:");
                foreach (var farm in farmDatabase.farms)
                {
                    Debug.Log($"   - {farm.farmId} ({farm.farmName})");
                }
            }
            else
            {
                Debug.Log($"✅ Found farm: {targetFarm.farmName} ({targetFarm.farmId})");
            }
        }
        else
        {
            Debug.LogError("❌ FarmDatabase or farms list is null!");
        }
        
        if (targetFarm == null)
        {
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("Farm not found.");
            }
            return;
        }
        
        // Check if item can be applied to this farm
        if (!targetFarm.CanApplyItem(pendingProductId))
        {
            Debug.LogWarning($"⚠️ {pendingProductId} already applied to {targetFarm.farmName}");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("This item is already applied to that farm.");
            }
            return;
        }
        
        // Check if player has the item
        if (wallet == null || wallet.GetItemCount(pendingProductId) <= 0)
        {
            Debug.LogWarning($"⚠️ Player doesn't have {pendingProductId}");
            if (infoErrorChanger != null)
            {
                infoErrorChanger.OpenErrorDefault("You don't have this item.");
            }
            return;
        }

        // All checks passed - apply the item!

        // 1. Remove item from wallet
        if (!wallet.TryConsumeItem(pendingProductId, 1))
        {
            Debug.LogError($"❌ Failed to consume {pendingProductId}");
            return;
        }
        Debug.Log($"💰 Removed 1x {pendingProductId} from wallet");

        // 2. Apply item to farm
        targetFarm.ApplyItem(pendingProductId);
        Debug.Log($"✅ Successfully applied {pendingProductId} to {targetFarm.farmName}!");

        // 3. Refresh farm display to show the new item
        RefreshFarmDisplay(targetFarm.farmIndex);

        // 4. Clear pending item
        pendingProductId = "";

        // 5. Close the panel
        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
        }
    }
    
    private void RefreshFarmDisplay(int farmIndex)
    {
        // Find FarmGridManager to refresh the display
        FarmGridManager farmGridManager = FindAnyObjectByType<FarmGridManager>();
        if (farmGridManager != null)
        {
            farmGridManager.RefreshFarmDisplay(farmIndex);
            Debug.Log($"🔄 Refreshed display for farm {farmIndex}");
        }
    }
}