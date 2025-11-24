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
    }
    
    void OnNoClicked()
    {
        pendingProductId = "";
        
        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
        }
    }
    
    void OnYesClicked()
    {
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
        // 2. Apply item to farm
        targetFarm.ApplyItem(pendingProductId);

        // 3. ✅ Show success message (keep pendingProductId for now)
        if (infoErrorChanger != null)
        {
            infoErrorChanger.OpenInfoSetItemToFarm("Item successfully added to the farm! You want to add more?");
        }
        
        // 4. ✅ DON'T clear pendingProductId yet - keep it until user closes the panel
        // pendingProductId will be cleared when user clicks No or closes the panel
    }
}