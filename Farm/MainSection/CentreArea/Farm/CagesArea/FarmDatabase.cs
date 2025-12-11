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
    
    // ✅ NEW: Store nest details from backend for proper egg ready mapping
    private Dictionary<int, List<NestDetail>> farmNestDetails = new Dictionary<int, List<NestDetail>>();

    public void LoadFromJSON()
    {
        if (farmDataJSON == null)
        {
            Debug.LogError("❌ No JSON file assigned!");
            return;
        }

        try
        {
            FarmDatabaseWrapper wrapper = JsonUtility.FromJson<FarmDatabaseWrapper>(farmDataJSON.text);
            if (wrapper != null && wrapper.farms != null)
            {
                farms = wrapper.farms;
                
                for (int i = 0; i < farms.Count; i++)
                {
                    if (string.IsNullOrEmpty(farms[i].farmId))
                    {
                        farms[i].farmId = $"farm_{(i + 1):D3}";
                    }
                }
                
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
    /// ✅ UPDATED: Store nest details before generating cages
    /// </summary>
    public void SetFarmNestDetails(int farmIndex, List<NestDetail> nestDetails)
    {
        if (!farmNestDetails.ContainsKey(farmIndex))
        {
            farmNestDetails.Add(farmIndex, nestDetails);
        }
        else
        {
            farmNestDetails[farmIndex] = nestDetails;
        }
    }

    public void GenerateCagesFromFarmData()
    {
        foreach (var farm in farms)
        {
            farm.cages.Clear();
            
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
                    eggReady = false, // ✅ NEW: Initialize as false
                    remainingTime = 0f,
                    upgradeLevel = 0
                };
                farm.cages.Add(cage);
            }
            
            DistributeFarmDataToCages(farm);
        }
        
        Debug.Log($"✅ Generated cages for {farms.Count} farms");
    }

    private void DistributeFarmDataToCages(FarmData farm)
    {
        List<int> cagesWithNests = new List<int>();
        
        // Step 1: Distribute nests
        for (int i = 0; i < farm.nestsOccupied && i < 100; i++)
        {
            farm.cages[i].nestsOccupied = 1;
            cagesWithNests.Add(i);
        }

        // ⭐ STEP 1B — APPLY PREMIUM FLAG TO CAGES ⭐
        if (farmNestDetails.ContainsKey(farm.farmIndex))
        {
            var nests = farmNestDetails[farm.farmIndex];

            for (int i = 0; i < nests.Count && i < farm.cages.Count; i++)
            {
                farm.cages[i].isPremium = nests[i].isPremium;  
            }
        }

        // Step 2: Count eggs from backend
        int totalEggsReady = 0;
        
        if (farmNestDetails.ContainsKey(farm.farmIndex))
        {
            List<NestDetail> nestDetails = farmNestDetails[farm.farmIndex];

            foreach (var nest in nestDetails)
            {
                if (nest.hen != null && nest.hen.hasEggReady)
                {
                    totalEggsReady++;
                }
            }
        }
        else
        {
            totalEggsReady = Mathf.RoundToInt(cagesWithNests.Count * 0.5f);
        }

        int currentIndex = 0;

        // SuperLegend
        for (int i = 0; i < farm.superLegendChicks && currentIndex < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[currentIndex];
            farm.cages[cageIndex].superLegendChicks = 1;
            farm.cages[cageIndex].hasEgg = currentIndex < totalEggsReady;
            farm.cages[cageIndex].eggReady = currentIndex < totalEggsReady;
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            currentIndex++;
        }

        // Legend
        for (int i = 0; i < farm.legendChicks && currentIndex < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[currentIndex];
            farm.cages[cageIndex].legendChicks = 1;
            farm.cages[cageIndex].hasEgg = currentIndex < totalEggsReady;
            farm.cages[cageIndex].eggReady = currentIndex < totalEggsReady;
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            currentIndex++;
        }

        // Champ
        for (int i = 0; i < farm.champChicks && currentIndex < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[currentIndex];
            farm.cages[cageIndex].champChicks = 1;
            farm.cages[cageIndex].hasEgg = currentIndex < totalEggsReady;
            farm.cages[cageIndex].eggReady = currentIndex < totalEggsReady;
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            currentIndex++;
        }

        // Normal
        for (int i = 0; i < farm.normalChicks && currentIndex < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[currentIndex];
            farm.cages[cageIndex].normalChicks = 1;
            farm.cages[cageIndex].hasEgg = currentIndex < totalEggsReady;
            farm.cages[cageIndex].eggReady = currentIndex < totalEggsReady;
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            currentIndex++;
        }

        // Counts
        farm.premiumNests = farmNestDetails[farm.farmIndex].FindAll(n => n.isPremium).Count;
        farm.normalNests = farmNestDetails[farm.farmIndex].FindAll(n => !n.isPremium).Count;
    }

    #region BACKEND INTEGRATION

    /// <summary>
    /// Load farms from backend JSON string
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
    /// Update a single farm from backend data
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
                    eggReady = false,
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

    public void SwitchToFarm(int targetFarmIndex)
    {
        if (farms == null || farms.Count == 0)
        {
            Debug.LogError("❌ Farm list is empty!");
            return;
        }

        if (targetFarmIndex < 0 || targetFarmIndex >= farms.Count)
        {
            Debug.LogWarning("⚠️ Invalid farm index!");
            return;
        }

        currentFarmIndex = targetFarmIndex;
        Debug.Log($"✅ Switched to farm: {currentFarmIndex}");
    }

    [System.Serializable]
    private class FarmDatabaseWrapper
    {
        public List<FarmData> farms;
    }
}