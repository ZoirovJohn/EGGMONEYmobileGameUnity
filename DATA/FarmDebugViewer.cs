using UnityEngine;
using System.Text;

/// <summary>
/// Debug viewer to see farm data in real-time during gameplay
/// Shows in Unity Inspector while game is running
/// </summary>
public class FarmDebugViewer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FarmDatabase farmDatabase;

    [Header("Auto Find")]
    [SerializeField] private bool autoFind = true;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("View Options")]
    [SerializeField] private bool showCageDetails = false;
    [SerializeField] private int maxCagesToShow = 10;

    [Header("Runtime Farm Data - READ ONLY")]
    [TextArea(20, 50)]
    [SerializeField] private string farmDataDisplay = "Run the game to see farm data...";

    private float lastUpdateTime;

    void Awake()
    {
        if (autoFind && farmDatabase == null)
        {
            farmDatabase = FindAnyObjectByType<FarmDatabase>();
        }
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateFarmDisplay();
            lastUpdateTime = Time.time;
        }
    }

    void UpdateFarmDisplay()
    {
        if (farmDatabase == null || farmDatabase.farms == null)
        {
            farmDataDisplay = "❌ FarmDatabase not found or farms list is null!";
            return;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════╗");
        sb.AppendLine("║      FARM DATABASE - RUNTIME DATA     ║");
        sb.AppendLine("╚══════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"📊 Total Farms: {farmDatabase.farms.Count}");
        sb.AppendLine($"🏡 Current Farm Index: {farmDatabase.currentFarmIndex}");
        sb.AppendLine();

        if (farmDatabase.farms.Count == 0)
        {
            sb.AppendLine("⚠️ No farms in database!");
            farmDataDisplay = sb.ToString();
            return;
        }

        for (int i = 0; i < farmDatabase.farms.Count; i++)
        {
            FarmData farm = farmDatabase.farms[i];

            sb.AppendLine("┌─────────────────────────────────────┐");
            sb.AppendLine($"│ {(i == farmDatabase.currentFarmIndex ? "★" : "☆")} FARM {i + 1}: {farm.farmName}");
            sb.AppendLine("└─────────────────────────────────────┘");
            sb.AppendLine($"  🆔 Farm ID: {farm.farmId}");
            sb.AppendLine($"  📍 Farm Index: {farm.farmIndex}");
            sb.AppendLine($"  🔑 Key Type: {farm.farmKeyType}");
            sb.AppendLine($"  🤖 Robot: {farm.robotType}");
            sb.AppendLine($"  🔋 Battery: {farm.batteryType}");
            sb.AppendLine($"  🪺 Nests Occupied: {farm.nestsOccupied}");
            sb.AppendLine($"  🐣 Normal Chicks: {farm.normalChicks}");
            sb.AppendLine($"  🏆 Champ Chicks: {farm.champChicks}");
            sb.AppendLine($"  💎 Premium Nests: {farm.premiumNests}");
            sb.AppendLine($"  🪺 Normal Nests: {farm.normalNests}");

            AppendHenLifetimeInfo(sb, i);

            if (farm.cages != null)
            {
                sb.AppendLine($"  📦 Total Cages: {farm.cages.Count}");

                int occupiedCages = 0;
                int cagesWithEggs = 0;
                int totalNormalChicks = 0;
                int totalChampChicks = 0;

                foreach (var cage in farm.cages)
                {
                    if (cage.nestsOccupied > 0) occupiedCages++;
                    if (cage.hasEgg) cagesWithEggs++;
                    totalNormalChicks += cage.normalChicks;
                    totalChampChicks += cage.champChicks;
                }

                sb.AppendLine($"  📊 Occupied Cages: {occupiedCages}/{farm.cages.Count}");
                sb.AppendLine($"  🥚 Cages with Eggs: {cagesWithEggs}");
                sb.AppendLine($"  🐣 Total Normal Chicks in Cages: {totalNormalChicks}");
                sb.AppendLine($"  🏆 Total Champ Chicks in Cages: {totalChampChicks}");

                if (showCageDetails)
                {
                    sb.AppendLine($"  ┌─ Cage Details (first {maxCagesToShow}):");

                    int cagesToShow = Mathf.Min(maxCagesToShow, farm.cages.Count);
                    for (int c = 0; c < cagesToShow; c++)
                    {
                        CageData cage = farm.cages[c];
                        if (cage.nestsOccupied > 0 || cage.hasEgg)
                        {
                            sb.AppendLine(
                                $"  │ Cage {cage.id}: Nests {cage.nestsOccupied}/{cage.nestCapacity}, " +
                                $"Egg: {(cage.hasEgg ? "Yes" : "No")}, " +
                                $"Normal: {cage.normalChicks}, Champ: {cage.champChicks}, " +
                                $"Time: {cage.remainingTime:F1}s, Level: {cage.upgradeLevel}"
                            );
                        }
                    }
                    sb.AppendLine("  └─");
                }
            }
            else
            {
                sb.AppendLine("  ⚠️ Cages list is null!");
            }

            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine($"Last Update: {System.DateTime.Now:HH:mm:ss}");

        farmDataDisplay = sb.ToString();
    }

    void AppendHenLifetimeInfo(StringBuilder sb, int farmIndex)
    {
        var lifetimeMap = farmDatabase.GetHenLifetimeSummary(farmIndex);

        if (lifetimeMap == null)
        {
            sb.AppendLine("  🐔 Hen Lifetime: ❌ Not available");
            return;
        }

        sb.AppendLine("  🐔 Hen Lifetime (Days Remaining):");

        for (int day = 15; day >= 1; day--)
        {
            int count = lifetimeMap.ContainsKey(day) ? lifetimeMap[day] : 0;
            sb.AppendLine($"    ⏳ {day,2} days : {count}");
        }
    }

    [ContextMenu("Force Update Display")]
    public void ForceUpdate()
    {
        UpdateFarmDisplay();
    }

    [ContextMenu("Toggle Cage Details")]
    public void ToggleCageDetails()
    {
        showCageDetails = !showCageDetails;
        UpdateFarmDisplay();
    }

    [ContextMenu("Print Farm Data to Console")]
    public void PrintToConsole()
    {
        Debug.Log(farmDataDisplay);
    }
}
