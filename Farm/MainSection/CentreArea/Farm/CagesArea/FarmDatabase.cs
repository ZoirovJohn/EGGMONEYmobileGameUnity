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

    /// <summary>
    /// ✅ UPDATED: Only load from JSON if no backend data exists (fallback)
    /// </summary>
    [ContextMenu("Load Data from JSON")]
    public void LoadFromJSON()
    {
        if (farmDataJSON == null)
        {
            Debug.LogError("❌ No JSON file assigned! Please assign farmDataJSON in Inspector.");
            return;
        }

        try
        {
            FarmDatabaseWrapper wrapper = JsonUtility.FromJson<FarmDatabaseWrapper>(farmDataJSON.text);
            if (wrapper != null && wrapper.farms != null)
            {
                farms = wrapper.farms;
                
                // ⭐ AUTO-GENERATE FARM IDs IF MISSING ⭐
                for (int i = 0; i < farms.Count; i++)
                {
                    if (string.IsNullOrEmpty(farms[i].farmId))
                    {
                        farms[i].farmId = $"farm_{(i + 1):D3}"; // farm_001, farm_002, etc.
                    }
                }
                
                // ⭐ RESET APPLIED ITEMS ONLY FOR JSON DATA ⭐
                foreach (var farm in farms)
                {
                    farm.ResetAppliedItems();
                }
                
                GenerateCagesFromFarmData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to parse JSON: {e.Message}");
        }
    }

    /// <summary>
    /// ✅ NEW: Generate cages WITHOUT resetting applied items (for backend data)
    /// </summary>
    public void GenerateCagesFromFarmData()
    {
        foreach (var farm in farms)
        {
            // ✅ Only clear and regenerate cages, don't touch applied items
            farm.cages.Clear();
            
            // Create 100 empty cages
            for (int i = 0; i < 100; i++)
            {
                CageData cage = new CageData
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
                    remainingTime = 0f,
                    upgradeLevel = 0
                };
                farm.cages.Add(cage);
            }
            
            // Distribute the farm's data across cages
            DistributeFarmDataToCages(farm);
        }
        
        Debug.Log($"✅ Generated cages for {farms.Count} farms");
    }

    private void DistributeFarmDataToCages(FarmData farm)
    {
        // Create a list to track which cages have nests
        List<int> cagesWithNests = new List<int>();
        
        // Step 1: Distribute nests - ONLY nestsOccupied amount (not all 100)
        for (int i = 0; i < farm.nestsOccupied && i < 100; i++)
        {
            farm.cages[i].nestsOccupied = 1; // Each cage gets 1 nest
            cagesWithNests.Add(i);
        }

        // Step 2: Distribute chicks by priority (SuperLegend > Legend > Champ > Normal)
        int currentIndex = 0;
        
        // SuperLegend chicks first
        for (int i = 0; i < farm.superLegendChicks && currentIndex < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[currentIndex];
            farm.cages[cageIndex].superLegendChicks = 1;
            farm.cages[cageIndex].hasEgg = Random.value > 0.5f;
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            currentIndex++;
        }
        
        // Legend chicks
        for (int i = 0; i < farm.legendChicks && currentIndex < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[currentIndex];
            farm.cages[cageIndex].legendChicks = 1;
            farm.cages[cageIndex].hasEgg = Random.value > 0.5f;
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            currentIndex++;
        }
        
        // Champ chicks
        for (int i = 0; i < farm.champChicks && currentIndex < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[currentIndex];
            farm.cages[cageIndex].champChicks = 1;
            farm.cages[cageIndex].hasEgg = Random.value > 0.5f;
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            currentIndex++;
        }
        
        // Normal chicks last
        for (int i = 0; i < farm.normalChicks && currentIndex < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[currentIndex];
            farm.cages[cageIndex].normalChicks = 1;
            farm.cages[cageIndex].hasEgg = Random.value > 0.5f;
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            currentIndex++;
        }
        
        // Verify distribution
        int actualNests = 0;
        int actualSuperLegends = 0;
        int actualLegends = 0;
        int actualChamps = 0;
        int actualNormals = 0;
        
        foreach (var cage in farm.cages)
        {
            if (cage.nestsOccupied > 0) actualNests++;
            if (cage.superLegendChicks > 0) actualSuperLegends++;
            if (cage.legendChicks > 0) actualLegends++;
            if (cage.champChicks > 0) actualChamps++;
            if (cage.normalChicks > 0) actualNormals++;
        }
        
    }

    #region BACKEND INTEGRATION

    /// <summary>
    /// ✅ Load farms from backend JSON string
    /// </summary>
    public void LoadFromBackend(string jsonData)
    {
        try
        {
            FarmDatabaseWrapper wrapper = JsonUtility.FromJson<FarmDatabaseWrapper>(jsonData);
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

    /// <summary>
    /// ✅ Update a single farm from backend data
    /// </summary>
    public void UpdateFarmFromBackend(int farmIndex, int nests, int champChicks, int normalChicks, int legendChicks = 0, int superLegendChicks = 0)
    {
        if (farmIndex >= 0 && farmIndex < farms.Count)
        {
            farms[farmIndex].nestsOccupied = nests;
            farms[farmIndex].champChicks = champChicks;
            farms[farmIndex].normalChicks = normalChicks;
            farms[farmIndex].legendChicks = legendChicks;
            farms[farmIndex].superLegendChicks = superLegendChicks;
            
            // Regenerate cages for this farm
            farms[farmIndex].cages.Clear();
            for (int i = 0; i < 100; i++)
            {
                farms[farmIndex].cages.Add(new CageData
                {
                    id = i + 1,
                    farmIndex = farmIndex,
                    nestCapacity = 16,
                    nestsOccupied = 0,
                    normalChicks = 0,
                    champChicks = 0,
                    legendChicks = 0,
                    superLegendChicks = 0,
                    hasEgg = false,
                    remainingTime = 0f,
                    upgradeLevel = 0
                });
            }
            
            DistributeFarmDataToCages(farms[farmIndex]);
        }
    }

    /// <summary>
    /// Export current farm data as JSON
    /// </summary>
    public string ToJson()
    {
        FarmDatabaseWrapper wrapper = new FarmDatabaseWrapper { farms = farms };
        return JsonUtility.ToJson(wrapper, true);
    }

    [System.Serializable]
    private class FarmDatabaseWrapper
    {
        public List<FarmData> farms;
    }

    #endregion

    #region GETTERS

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
        {
            return farms[farmIndex].cages.Find(c => c.id == cageId);
        }
        return null;
    }

    #endregion

    #region FARM SWITCHING

    public void SwitchToFarm(int targetFarmIndex)
    {
        if (farms == null || farms.Count == 0)
        {
            Debug.LogError("❌ Farm list is empty! Load data first.");
            return;
        }

        if (targetFarmIndex < 0 || targetFarmIndex >= farms.Count)
        {
            Debug.LogWarning("⚠️ Invalid farm index!");
            return;
        }

        currentFarmIndex = targetFarmIndex;
        Debug.Log($"✅ Successfully switched to farm: {currentFarmIndex}");
    }

    #endregion

    /// <summary>
    /// Generate default data - only used as fallback
    /// </summary>
    public void GenerateDefaultData()
    {
        if (farmDataJSON != null)
        {
            LoadFromJSON();
        }
        else
        {
            Debug.LogWarning("⚠️ No JSON file assigned. Please assign farmDataJSON and call 'Load Data from JSON'");
        }
    }
}