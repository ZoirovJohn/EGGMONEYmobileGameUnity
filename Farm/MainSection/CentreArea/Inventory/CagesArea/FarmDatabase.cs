using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FarmDatabase", menuName = "Farm/FarmDatabase")]
public class FarmDatabase : ScriptableObject
{
    public int currentFarmIndex = 0;
    
    [Header("Load Data From JSON")]
    public TextAsset farmDataJSON;  // Assign your JSON file here in Inspector
    
    [HideInInspector]
    public List<FarmData> farms = new List<FarmData>();

    // Call this from Inspector or on Start to load from JSON
    [ContextMenu("Load Data from JSON")]
    public void LoadFromJSON()
    {
        if (farmDataJSON == null)
        {
            Debug.LogError("No JSON file assigned! Please assign farmDataJSON in Inspector.");
            return;
        }

        try
        {
            FarmDatabaseWrapper wrapper = JsonUtility.FromJson<FarmDatabaseWrapper>(farmDataJSON.text);
            if (wrapper != null && wrapper.farms != null)
            {
                farms = wrapper.farms;
                GenerateCagesFromFarmData();
                Debug.Log($"✓ Loaded {farms.Count} farms from JSON file");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse JSON: {e.Message}");
        }
    }

    // Generate cages based on farm data
    private void GenerateCagesFromFarmData()
    {
        foreach (var farm in farms)
        {
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
                    hasEgg = false,
                    remainingTime = 0f,
                    upgradeLevel = 0
                };
                farm.cages.Add(cage);
            }
            
            // Distribute the farm's data across cages
            DistributeFarmDataToCages(farm);
        }
    }

    private void DistributeFarmDataToCages(FarmData farm)
    {
        // Step 1: Distribute nests randomly across cages
        int remainingNests = farm.nestsOccupied;
        List<int> cagesWithNests = new List<int>();
        
        while (remainingNests > 0)
        {
            int randomCageIndex = Random.Range(0, farm.cages.Count);
            CageData cage = farm.cages[randomCageIndex];
            
            if (cage.nestsOccupied < cage.nestCapacity)
            {
                cage.nestsOccupied++;
                if (!cagesWithNests.Contains(randomCageIndex))
                    cagesWithNests.Add(randomCageIndex);
                remainingNests--;
            }
        }

        // Step 2: Distribute champion chicks (only in cages with nests)
        int remainingChampChicks = farm.champChicks;
        while (remainingChampChicks > 0 && cagesWithNests.Count > 0)
        {
            int randomIndex = Random.Range(0, cagesWithNests.Count);
            int cageIndex = cagesWithNests[randomIndex];
            CageData cage = farm.cages[cageIndex];
            
            if (cage.nestsOccupied > 0)
            {
                cage.champChicks++;
                
                // Randomly assign egg or clock (50/50 chance)
                cage.hasEgg = Random.value > 0.5f;
                
                // Random remaining time between 30-300 seconds
                cage.remainingTime = Random.Range(30f, 300f);
                
                remainingChampChicks--;
            }
        }

        // Step 3: Distribute normal chicks (only in cages with nests)
        int remainingNormalChicks = farm.normalChicks;
        while (remainingNormalChicks > 0 && cagesWithNests.Count > 0)
        {
            int randomIndex = Random.Range(0, cagesWithNests.Count);
            int cageIndex = cagesWithNests[randomIndex];
            CageData cage = farm.cages[cageIndex];
            
            if (cage.nestsOccupied > 0)
            {
                cage.normalChicks++;
                
                // Only set egg/clock if there's no champ chick already
                if (cage.champChicks == 0)
                {
                    // Randomly assign egg or clock (50/50 chance)
                    cage.hasEgg = Random.value > 0.5f;
                    
                    // Random remaining time between 30-300 seconds
                    cage.remainingTime = Random.Range(30f, 300f);
                }
                
                remainingNormalChicks--;
            }
        }
    }

    #region BACKEND INTEGRATION

    // Load from backend API (string JSON)
    public void LoadFromBackend(string jsonData)
    {
        try
        {
            FarmDatabaseWrapper wrapper = JsonUtility.FromJson<FarmDatabaseWrapper>(jsonData);
            if (wrapper != null && wrapper.farms != null)
            {
                farms = wrapper.farms;
                GenerateCagesFromFarmData();
                Debug.Log($"✓ Loaded {farms.Count} farms from backend");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load backend data: {e.Message}");
        }
    }

    // Update specific farm from backend
    public void UpdateFarmFromBackend(int farmIndex, int nests, int champChicks, int normalChicks)
    {
        if (farmIndex >= 0 && farmIndex < farms.Count)
        {
            farms[farmIndex].nestsOccupied = nests;
            farms[farmIndex].champChicks = champChicks;
            farms[farmIndex].normalChicks = normalChicks;
            
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
                    hasEgg = false,
                    remainingTime = 0f,
                    upgradeLevel = 0
                });
            }
            
            DistributeFarmDataToCages(farms[farmIndex]);
            
            Debug.Log($"✓ Updated Farm {farmIndex + 1} from backend");
        }
    }

    // Convert to JSON
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
            Debug.LogError("Farm list is empty! Load data first.");
            return;
        }

        if (targetFarmIndex < 0 || targetFarmIndex >= farms.Count)
        {
            Debug.LogWarning("Invalid farm index!");
            return;
        }

        currentFarmIndex = targetFarmIndex;
        Debug.Log($"✅ Successfully switched to farm: {currentFarmIndex}");
    }

    #endregion


    // Auto-load on first use
    public void GenerateDefaultData()
    {
        if (farmDataJSON != null)
        {
            LoadFromJSON();
        }
        else
        {
            Debug.LogWarning("No JSON file assigned. Please assign farmDataJSON and call 'Load Data from JSON'");
        }
    }
}