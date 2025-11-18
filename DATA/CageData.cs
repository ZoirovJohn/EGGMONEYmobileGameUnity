using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CageData
{
    public int id;
    public int farmIndex;
    public int nestCapacity = 16;
    public int nestsOccupied = 0;
    public int normalChicks = 0;
    public int champChicks = 0;
    public int legendChicks = 0;      // ✅ Added for backend support
    public int superLegendChicks = 0; // ✅ Added for backend support
    public bool hasEgg = false;
    public float remainingTime = 0f;
    public int upgradeLevel = 0;
}

[System.Serializable]
public class FarmData
{
    // ⭐ CRITICAL: farmId must be public for JSON serialization
    public string farmId = "";        // e.g. "farm_001", "farm_002"
    public string farmName = "";      // e.g. "Farm 1", "Farm 2"
    public int farmIndex = 0;         // 0, 1, 2, etc.
    
    // Farm Stats
    public int nestsOccupied = 0;
    public int normalChicks = 0;
    public int champChicks = 0;
    public int legendChicks = 0;      // ✅ Added for backend support
    public int superLegendChicks = 0; // ✅ Added for backend support
    
    // Cage list
    public List<CageData> cages = new List<CageData>();
    
    // Farm items (default values)
    public string farmKeyType = "normal";  // "normal", "premiumfarmkey", "key_farm"
    public string robotType = "none";      // "none", "robot"
    public string batteryType = "none";    // "none", "battery", "super_battery"
    
    // ✅ Backend integration fields
    public bool isPremium = false;         // From backend farm.isPremium
    public int maxCapacity = 100;          // From backend farm.maxCapacity
    public bool hasRobot = false;          // From backend robot.id
    public bool robotActive = false;       // From backend robot.isActive
    public string robotPoweredUntil = "";  // From backend robot.poweredUntil
    
    #region FARM KEY CHECKS
    
    // Check if this is a premium farm
    public bool IsPremiumFarm()
    {
        return farmKeyType == "premiumfarmkey" || farmKeyType == "key_farm" || isPremium;
    }
    
    #endregion
    
    #region APPLIED ITEMS MANAGEMENT
    
    // Check if item can be applied to this farm
    public bool CanApplyItem(string productId)
    {
        // Normalize product ID
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace("-", "");
        
        // Check farm key
        if (normalized.Contains("farmkey") || normalized == "keyfarm")
        {
            // Cannot apply key to farm that already has one
            // farmKeyType = "normal" means unlocked with normal key
            // farmKeyType = "premiumfarmkey" means unlocked with premium key
            return false; // Farm already unlocked, cannot apply another key
        }
        
        // Check robot
        if (normalized == "robot" || normalized == "farmmanagementrobot")
        {
            return robotType == "none" && !hasRobot;
        }
        
        // Check battery
        if (normalized == "battery")
        {
            return batteryType == "none";
        }
        
        if (normalized == "superbattery")
        {
            return batteryType == "none";
        }
        
        // Unknown item type
        Debug.LogWarning($"⚠️ Unknown item type: {productId}");
        return false;
    }
    
    // Apply item to farm
    public void ApplyItem(string productId)
    {
        if (!CanApplyItem(productId))
        {
            Debug.LogWarning($"⚠️ Cannot apply {productId} to {farmName}");
            return;
        }
        
        // Normalize product ID
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace("-", "");
        
        // Apply farm key
        if (normalized.Contains("farmkey") || normalized == "keyfarm")
        {
            farmKeyType = productId;
            isPremium = normalized.Contains("premium");
            Debug.Log($"🔑 Applied farm key to {farmName}, new type: {farmKeyType}");
        }
        // Apply robot
        else if (normalized == "robot" || normalized == "farmmanagementrobot")
        {
            robotType = "robot";
            hasRobot = true;
            robotActive = true;
            Debug.Log($"🤖 Applied robot to {farmName}");
        }
        // Apply battery
        else if (normalized == "battery" || normalized == "superbattery")
        {
            batteryType = productId;
            Debug.Log($"🔋 Applied {productId} to {farmName}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Unknown item type: {productId}");
        }
    }

    // Reset all applied items (called on data load)
    public void ResetAppliedItems()
    {
        // Reset to defaults
        farmKeyType = "normal";
        robotType = "none";
        batteryType = "none";
        
        // Keep backend data intact
        // (isPremium, hasRobot, etc. come from backend)
        
        Debug.Log($"🔄 Reset applied items for {farmName}");
    }
    
    #endregion
}