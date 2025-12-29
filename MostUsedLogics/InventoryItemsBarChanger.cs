using UnityEngine;

public class InventoryItemsBarChanger : MonoBehaviour
{
    [Header("Inventory Bar References")]
    [SerializeField] private GameObject inventoryCageBar;
    [SerializeField] private GameObject inventoryFarmBar;
    [SerializeField] private GameObject inventoryLockBar;
    [SerializeField] private GameObject defaultBanner;

    private string currentFarmId = "";

    private void Start()
    {
        DefaultBannerMethod();
    }

    public void InventoryFarmBarMethod(string farmId)
    {
        currentFarmId = farmId;
        CloseAllBars();
        
        if (inventoryFarmBar != null)
        {
            inventoryFarmBar.SetActive(true);
        }
        else
        {
            Debug.LogWarning("InventoryFarmBar is not assigned!");
        }
    }

    public void InventoryCageBarMethod()
    {
        CloseAllBars();
        if (inventoryCageBar != null)
        {
            inventoryCageBar.SetActive(true);
        }
        else
        {
            Debug.LogWarning("InventoryCageBar is not assigned!");
        }
    }

    public void InventoryLockBarMethod()
    {
        CloseAllBars();
        if (inventoryLockBar != null)
        {
            inventoryLockBar.SetActive(true);
        }
        else
        {
            Debug.LogWarning("InventoryLockBar is not assigned!");
        }
    }

    public void DefaultBannerMethod()
    {
        CloseAllBars();
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
            
        if (inventoryLockBar != null)
            inventoryLockBar.SetActive(false);
            
        if (defaultBanner != null)
            defaultBanner.SetActive(false);
    }
    public void CloseAll()
    {
        CloseAllBars();
    }

    public string GetCurrentFarmId()
    {
        return currentFarmId;
    }
}