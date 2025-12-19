using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FarmDatabase", menuName = "Farm/FarmDatabase")]
public class FarmDatabase : ScriptableObject
{
    public int currentFarmIndex = 0;

    [Header("Load Data From JSON (Fallback Only)")]
    public TextAsset farmDataJSON;

    [HideInInspector]
    public List<FarmData> farms = new List<FarmData>();

    // ==================================================
    // BACKEND STORAGE
    // ==================================================

    // Store nest details per farm (index -> nests)
    private Dictionary<int, List<NestDetail>> farmNestDetails =
        new Dictionary<int, List<NestDetail>>();

    // Store hen lifetime summary (for debug / charts)
    private Dictionary<int, Dictionary<int, int>> farmHenLifetimeMap =
        new Dictionary<int, Dictionary<int, int>>();

    // ==================================================
    // JSON FALLBACK
    // ==================================================

    public void LoadFromJSON()
    {
        if (farmDataJSON == null)
        {
            Debug.LogError("❌ No JSON file assigned!");
            return;
        }

        try
        {
            FarmDatabaseWrapper wrapper =
                JsonUtility.FromJson<FarmDatabaseWrapper>(farmDataJSON.text);

            if (wrapper != null && wrapper.farms != null)
            {
                farms = wrapper.farms;

                for (int i = 0; i < farms.Count; i++)
                {
                    if (string.IsNullOrEmpty(farms[i].farmId))
                        farms[i].farmId = $"farm_{(i + 1):D3}";
                }

                foreach (var farm in farms)
                    farm.ResetAppliedItems();

                GenerateCagesFromFarmData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to parse JSON: {e.Message}");
        }
    }

    // ==================================================
    // BACKEND SETTERS
    // ==================================================

    /// <summary>
    /// Store raw nest details from backend
    /// </summary>
    public void SetFarmNestDetails(int farmIndex, List<NestDetail> nestDetails)
    {
        if (nestDetails == null) return;
        farmNestDetails[farmIndex] = nestDetails;
    }

    /// <summary>
    /// Store hen lifetime distribution (for debug view)
    /// </summary>
    public void SetHenLifetimeSummary(int farmIndex, List<NestDetail> nestDetails)
    {
        if (nestDetails == null) return;

        Dictionary<int, int> lifetimeCounts = new Dictionary<int, int>();

        for (int d = 1; d <= 15; d++)
            lifetimeCounts[d] = 0;

        foreach (var nest in nestDetails)
        {
            if (nest.hen != null && nest.hen.alive)
            {
                int days = Mathf.Clamp(nest.hen.lifetimeDaysRemaining, 1, 15);
                lifetimeCounts[days]++;
            }
        }

        farmHenLifetimeMap[farmIndex] = lifetimeCounts;
    }

    // ==================================================
    // GETTERS
    // ==================================================

    public Dictionary<int, int> GetHenLifetimeSummary(int farmIndex)
    {
        if (farmHenLifetimeMap.TryGetValue(farmIndex, out var map))
            return map;

        return null;
    }

    public FarmData GetCurrentFarmCages()
    {
        if (currentFarmIndex < 0 || currentFarmIndex >= farms.Count)
            return null;
        return farms[currentFarmIndex];
    }

    public FarmData GetFarmByIndex(int index)
    {
        if (index >= 0 && index < farms.Count)
            return farms[index];
        return null;
    }

    public CageData GetCageData(int farmIndex, int cageId)
    {
        if (farmIndex >= 0 && farmIndex < farms.Count)
            return farms[farmIndex].cages.Find(c => c.id == cageId);
        return null;
    }

    // ==================================================
    // CAGE GENERATION
    // ==================================================

    public void GenerateCagesFromFarmData()
    {
        foreach (var farm in farms)
        {
            farm.cages.Clear();

            for (int i = 0; i < 100; i++)
            {
                farm.cages.Add(new CageData
                {
                    id = i + 1,
                    farmIndex = farm.farmIndex,
                    nestCapacity = 16,
                    nestsOccupied = 0,
                    normalChicks = 0,
                    champChicks = 0,
                    legendChicks = 0,
                    superLegendChicks = 0,
                    hasEgg = false,
                    eggReady = false,
                    remainingTime = 0f,
                    lifetimeDaysRemaining = 0,
                    upgradeLevel = 0,
                    isPremium = false
                });
            }

            DistributeFarmDataToCages(farm);
        }

        Debug.Log($"✅ Generated cages for {farms.Count} farms");
    }

    private void DistributeFarmDataToCages(FarmData farm)
    {
        List<int> cagesWithNests = new List<int>();

        // Step 1: distribute nests
        for (int i = 0; i < farm.nestsOccupied && i < 100; i++)
        {
            farm.cages[i].nestsOccupied = 1;
            cagesWithNests.Add(i);
        }

        // Step 2: apply premium flags
        if (farmNestDetails.TryGetValue(farm.farmIndex, out var nests))
        {
            int premium = 0;
            int normal = 0;

            for (int i = 0; i < nests.Count && i < farm.cages.Count; i++)
            {
                farm.cages[i].isPremium = nests[i].isPremium;
                if (nests[i].isPremium) premium++;
                else normal++;
            }

            farm.premiumNests = premium;
            farm.normalNests = normal;
        }

        // Step 3: count eggs
        int totalEggsReady = 0;
        if (farmNestDetails.TryGetValue(farm.farmIndex, out var nestDetails))
        {
            foreach (var nest in nestDetails)
            {
                if (nest.hen != null && nest.hen.hasEggReady)
                    totalEggsReady++;
            }
        }

        // ✅ NEW: Group hens by lifetime in custom display order
        int[] lifetimeDisplayOrder = { 3, 2, 5, 7, 9, 1, 4, 6, 8, 10, 11, 12, 13, 14, 15 };
        List<NestDetail> sortedNests = new List<NestDetail>();

        if (farmNestDetails.TryGetValue(farm.farmIndex, out var nestsForSorting))
        {
            // Group nests by lifetime day
            Dictionary<int, List<NestDetail>> nestsByLifetime = new Dictionary<int, List<NestDetail>>();
            
            foreach (var nest in nestsForSorting)
            {
                if (nest.hen != null && nest.hen.alive)
                {
                    int day = Mathf.Clamp(nest.hen.lifetimeDaysRemaining, 1, 15);
                    
                    if (!nestsByLifetime.ContainsKey(day))
                        nestsByLifetime[day] = new List<NestDetail>();
                    
                    nestsByLifetime[day].Add(nest);
                }
            }

            // Add nests to sortedNests in display order
            Debug.Log($"🐔 Farm {farm.farmIndex + 1} - Sorting hens by lifetime:");
            foreach (int day in lifetimeDisplayOrder)
            {
                if (nestsByLifetime.ContainsKey(day))
                {
                    int count = nestsByLifetime[day].Count;
                    Debug.Log($"   📅 Day {day}: {count} hens");
                    sortedNests.AddRange(nestsByLifetime[day]);
                }
            }

            // Add any remaining nests that don't have hens or have invalid lifetime
            foreach (var nest in nestsForSorting)
            {
                if (nest.hen == null || !nest.hen.alive)
                {
                    sortedNests.Add(nest);
                }
            }
        }

        int index = 0;

        void Assign(int count, System.Action<CageData> setter)
        {
            for (int i = 0; i < count && index < cagesWithNests.Count; i++)
            {
                int cageIndex = cagesWithNests[index];
                CageData cage = farm.cages[cageIndex];

                setter(cage);

                // ✅ Use sorted nests list for lifetime assignment
                if (sortedNests.Count > 0 && index < sortedNests.Count && sortedNests[index].hen != null)
                {
                    cage.lifetimeDaysRemaining = Mathf.Clamp(sortedNests[index].hen.lifetimeDaysRemaining, 1, 15);
                    Debug.Log($"🐔 Cage {cageIndex + 1}: Assigned lifetime = {cage.lifetimeDaysRemaining} days");
                }

                cage.hasEgg = index < totalEggsReady;
                cage.eggReady = cage.hasEgg;

                index++;
            }
        }

        Assign(farm.superLegendChicks, c => c.superLegendChicks = 1);
        Assign(farm.legendChicks, c => c.legendChicks = 1);
        Assign(farm.champChicks, c => c.champChicks = 1);
        Assign(farm.normalChicks, c => c.normalChicks = 1);
    }

    // ==================================================
    // BACKEND SUPPORT
    // ==================================================

    public void LoadFromBackend(string jsonData)
    {
        try
        {
            FarmDatabaseWrapper wrapper =
                JsonUtility.FromJson<FarmDatabaseWrapper>(jsonData);

            if (wrapper != null && wrapper.farms != null)
            {
                farms = wrapper.farms;
                GenerateCagesFromFarmData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to load backend data: {e.Message}");
        }
    }

    public void UpdateFarmFromBackend(
        int farmIndex,
        int nests,
        int champChicks,
        int normalChicks,
        int legendChicks = 0,
        int superLegendChicks = 0)
    {
        if (farmIndex < 0 || farmIndex >= farms.Count)
            return;

        FarmData farm = farms[farmIndex];
        farm.nestsOccupied = nests;
        farm.champChicks = champChicks;
        farm.normalChicks = normalChicks;
        farm.legendChicks = legendChicks;
        farm.superLegendChicks = superLegendChicks;

        farm.cages.Clear();

        for (int i = 0; i < 100; i++)
        {
            farm.cages.Add(new CageData
            {
                id = i + 1,
                farmIndex = farmIndex,
                nestCapacity = 16,
                nestsOccupied = 0,
                hasEgg = false,
                eggReady = false,
                remainingTime = 0f,
                lifetimeDaysRemaining = 0,
                upgradeLevel = 0
            });
        }

        DistributeFarmDataToCages(farm);
    }

    // ==================================================
    // FARM SWITCH
    // ==================================================

    public void SwitchToFarm(int targetFarmIndex)
    {
        if (targetFarmIndex < 0 || targetFarmIndex >= farms.Count)
        {
            Debug.LogWarning("⚠️ Invalid farm index!");
            return;
        }

        currentFarmIndex = targetFarmIndex;
        Debug.Log($"✅ Switched to farm: {currentFarmIndex}");
    }

    // ==================================================

    [System.Serializable]
    private class FarmDatabaseWrapper
    {
        public List<FarmData> farms;
    }
}
