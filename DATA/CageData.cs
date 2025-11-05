using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CageData
{
    public int id;
    public int farmIndex;
    public int nestCapacity;
    public int nestsOccupied;
    public int normalChicks;
    public int champChicks;
    public bool hasEgg;           // Randomly assigned by system
    public float remainingTime;   // Randomly assigned by system
    public int upgradeLevel;
}

[System.Serializable]
public class FarmData
{
    // ⭐ CRITICAL: Make sure farmId is PUBLIC and has [SerializeField] or is just public
    public string farmId = "";        // e.g. "farm_001", "farm_002"
    public string farmName = "";      // e.g. "Farm 1", "Farm 2"
    public int farmIndex = 0;         // 0, 1, 2, etc.
    public int nestsOccupied = 0;
    public int normalChicks = 0;
    public int champChicks = 0;
    public List<CageData> cages = new List<CageData>();
    
    // Farm items (default values)
    public string farmKeyType = "normal";
    public string robotType = "none";        // "none", "robot"
    public string batteryType = "none";      // "none", "battery", "super_battery"
    
    // Check if this is a premium farm
    public bool IsPremiumFarm()
    {
        return farmKeyType == "premiumfarmkey" || farmKeyType == "key_farm";
    }
    
    // Check if item can be applied to this farm
    public bool CanApplyItem(string productId)
    {
        // Check farm key
        if (productId == "premiumfarmkey" || productId == "key_farm")
        {
            return farmKeyType == "normal";
        }
        
        // Check robot
        if (productId == "robot")
        {
            return robotType == "none";
        }
        
        // Check battery
        if (productId == "battery" || productId == "super_battery")
        {
            return batteryType == "none";
        }
        
        // Unknown item type
        return false;
    }
    
    // Apply item to farm
    public void ApplyItem(string productId)
    {
        // Apply farm key
        if (productId == "premiumfarmkey" || productId == "key_farm")
        {
            farmKeyType = productId;
            Debug.Log($"🔑 Applied farm key to {farmName}, new type: {farmKeyType}");
        }
        // Apply robot
        else if (productId == "robot")
        {
            robotType = productId;
            Debug.Log($"🤖 Applied robot to {farmName}");
        }
        // Apply battery
        else if (productId == "battery" || productId == "super_battery")
        {
            batteryType = productId;
            Debug.Log($"🔋 Applied {productId} to {farmName}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Unknown item type: {productId}");
        }
    }
}