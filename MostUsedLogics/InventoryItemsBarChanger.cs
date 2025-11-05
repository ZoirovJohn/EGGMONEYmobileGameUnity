using UnityEngine;

public class InventoryItemsBarChanger : MonoBehaviour
{
    [Header("Inventory Bar References")]
    [SerializeField] private GameObject inventoryCageBar;
    [SerializeField] private GameObject inventoryFarmBar;
    [SerializeField] private GameObject defaultBanner;

    private string currentFarmId = ""; // Store the current farm ID

    private void Start()
    {
        // Initialize - show default banner on start
        DefaultBannerMethod();
    }

    public void InventoryFarmBarMethod(string farmId)
    {
        // Store the farm ID
        currentFarmId = farmId;
        Debug.Log($"📦 InventoryItemsBarChanger: Stored farmId = {farmId}");
        
        // Close all others
        CloseAllBars();
        
        // Open farm bar
        if (inventoryFarmBar != null)
        {
            inventoryFarmBar.SetActive(true);
            Debug.Log($"✅ Opened InventoryFarmBar for farmId: {farmId}");
        }
        else
        {
            Debug.LogWarning("InventoryFarmBar is not assigned!");
        }
    }

    public void InventoryCageBarMethod()
    {
        // Close all others
        CloseAllBars();
        
        // Open cage bar
        if (inventoryCageBar != null)
        {
            inventoryCageBar.SetActive(true);
        }
        else
        {
            Debug.LogWarning("InventoryCageBar is not assigned!");
        }
    }

    public void DefaultBannerMethod()
    {
        // Close all others
        CloseAllBars();
        
        // Open default banner
        if (defaultBanner != null)
        {
            defaultBanner.SetActive(true);
        }
        else
        {
            Debug.LogWarning("DefaultBanner is not assigned!");
        }
    }

    private void CloseAllBars()
    {
        if (inventoryCageBar != null)
            inventoryCageBar.SetActive(false);
            
        if (inventoryFarmBar != null)
            inventoryFarmBar.SetActive(false);
            
        if (defaultBanner != null)
            defaultBanner.SetActive(false);
    }

    // Optional: Method to close all bars without opening any
    public void CloseAll()
    {
        CloseAllBars();
    }

    // Get current farm ID - CRITICAL METHOD FOR ITEM APPLICATION
    public string GetCurrentFarmId()
    {
        Debug.Log($"📍 InventoryItemsBarChanger: GetCurrentFarmId() returning: {currentFarmId}");
        return currentFarmId;
    }
}