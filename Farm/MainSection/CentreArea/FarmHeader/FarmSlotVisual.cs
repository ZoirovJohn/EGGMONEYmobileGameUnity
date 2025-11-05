using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to each Farm Slot prefab to show visual indicators 
/// for premium farm key, robot, and battery
/// </summary>
public class FarmSlotVisual : MonoBehaviour
{
    [Header("Visual Indicators (Optional)")]
    [SerializeField] private GameObject premiumKeyIcon;   // Show when farm has premium key
    [SerializeField] private GameObject robotIcon;        // Show when farm has robot
    [SerializeField] private GameObject batteryIcon;      // Show when farm has battery
    [SerializeField] private GameObject superBatteryIcon; // Show when farm has super battery
    
    [Header("Visual Effects (Optional)")]
    [SerializeField] private Image backgroundGlow;        // Premium farms get a gold glow
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color premiumColor = new Color(1f, 0.84f, 0f); // Gold
    
    private FarmData farmData;
    
    /// <summary>
    /// Call this to update the visual based on farm data
    /// </summary>
    public void UpdateVisual(FarmData farm)
    {
        if (farm == null)
        {
            Debug.LogWarning("⚠️ FarmSlotVisual: FarmData is null!");
            return;
        }
        
        farmData = farm;
        
        // Update premium key indicator
        if (premiumKeyIcon != null)
        {
            bool hasPremiumKey = farm.IsPremiumFarm();
            premiumKeyIcon.SetActive(hasPremiumKey);
            
            if (hasPremiumKey)
            {
                Debug.Log($"🔑 {farm.farmName} has premium key - showing icon");
            }
        }
        
        // Update robot indicator
        if (robotIcon != null)
        {
            bool hasRobot = farm.robotType == "robot";
            robotIcon.SetActive(hasRobot);
            
            if (hasRobot)
            {
                Debug.Log($"🤖 {farm.farmName} has robot - showing icon");
            }
        }
        
        // Update battery indicators
        if (batteryIcon != null)
        {
            bool hasBattery = farm.batteryType == "battery";
            batteryIcon.SetActive(hasBattery);
        }
        
        if (superBatteryIcon != null)
        {
            bool hasSuperBattery = farm.batteryType == "super_battery";
            superBatteryIcon.SetActive(hasSuperBattery);
        }
        
        // Update background glow for premium farms
        if (backgroundGlow != null)
        {
            Color targetColor = farm.IsPremiumFarm() ? premiumColor : normalColor;
            backgroundGlow.color = targetColor;
        }
    }
    
    /// <summary>
    /// Get the current farm data
    /// </summary>
    public FarmData GetFarmData()
    {
        return farmData;
    }
}