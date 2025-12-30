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
    public int legendChicks = 0;
    public int superLegendChicks = 0;
    public bool hasEgg = false;
    public bool eggReady = false;
    public float remainingTime = 0f;
    public int lifetimeDaysRemaining = 0;
    public int upgradeLevel = 0;
    public bool isPremium = false;
}

[System.Serializable]
public class FarmData
{
    public string farmId = "";
    public string farmName = "";
    public int farmIndex = 0;
    
    public int nestsOccupied = 0;
    public int normalChicks = 0;
    public int champChicks = 0;
    public int legendChicks = 0;
    public int superLegendChicks = 0;
    public int premiumNests = 0;
    public int normalNests = 0;
    
    public List<CageData> cages = new List<CageData>();
    
    public string farmKeyType = "normal";
    public string robotType = "none";
    public string batteryType = "none";
    
    public bool isPremium = false;
    public int maxCapacity = 100;
    public bool hasRobot = false;
    public bool robotActive = false;
    public string robotPoweredUntil = "";
    
    #region FARM KEY CHECKS
    
    public bool IsPremiumFarm()
    {
        return farmKeyType == "premiumfarmkey" || farmKeyType == "key_farm" || isPremium;
    }
    
    #endregion
    
    #region APPLIED ITEMS MANAGEMENT
    
    public bool CanApplyItem(string productId)
    {
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace("-", "");
        
        if (normalized.Contains("farmkey") || normalized == "keyfarm")
        {
            return false;
        }
        
        if (normalized == "robot" || normalized == "farmmanagementrobot")
        {
            return robotType == "none" && !hasRobot;
        }
        
        if (normalized == "battery")
        {
            return batteryType == "none";
        }
        
        if (normalized == "superbattery")
        {
            return batteryType == "none";
        }
        return false;
    }

    public int GetFreeNestSlotsInFarm()
    {
        return Mathf.Max(0, maxCapacity - nestsOccupied);
    }

    public int GetFreeNestCount()
    {
        int freeNests = 0;

        foreach (var cage in cages)
        {
            if (cage == null) continue;

            int freeInCage = cage.nestCapacity - cage.nestsOccupied;
            if (freeInCage > 0)
                freeNests += freeInCage;
        }

        return freeNests;
    }
    
    public void ApplyItem(string productId)
    {
        if (!CanApplyItem(productId))
        {
            return;
        }
        
        string normalized = productId.ToLowerInvariant().Replace("_", "").Replace("-", "");
        
        if (normalized.Contains("farmkey") || normalized == "keyfarm")
        {
            farmKeyType = productId;
            isPremium = normalized.Contains("premium");
        }
        else if (normalized == "robot" || normalized == "farmmanagementrobot")
        {
            robotType = "robot";
            hasRobot = true;
            robotActive = true;
        }
        else if (normalized == "battery" || normalized == "superbattery")
        {
            batteryType = productId;
        }
        else
        {
            Debug.LogWarning($"⚠️ Unknown item type: {productId}");
        }
    }

    public void ResetAppliedItems()
    {
        farmKeyType = "normal";
        robotType = "none";
        batteryType = "none";
    }
    
    #endregion
}