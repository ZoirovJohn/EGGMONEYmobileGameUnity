using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FarmDatabase", menuName = "Farm/FarmDatabase")]
public class FarmDatabase : ScriptableObject
{
    public int currentFarmIndex = 0;
    
    [Header("Load Data From JSON")]
    public TextAsset farmDataJSON;
    
    [HideInInspector]
    public List<FarmData> farms = new List<FarmData>();

    [ContextMenu("Load Data from JSON")]
    // Add this to your FarmDatabase.cs LoadFromJSON() method

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
                
                // ⭐ AUTO-GENERATE FARM IDs IF MISSING ⭐
                for (int i = 0; i < farms.Count; i++)
                {
                    if (string.IsNullOrEmpty(farms[i].farmId))
                    {
                        farms[i].farmId = $"farm_{(i + 1):D3}"; // farm_001, farm_002, etc.
                        Debug.Log($"🔧 Auto-generated farmId: {farms[i].farmId} for {farms[i].farmName}");
                    }
                }
                
                // ⭐ RESET ALL APPLIED ITEMS ON LOAD ⭐
                foreach (var farm in farms)
                {
                    farm.ResetAppliedItems();
                }
                Debug.Log($"✅ Reset applied items for all {farms.Count} farms");
                
                GenerateCagesFromFarmData();
                Debug.Log($"✓ Loaded {farms.Count} farms from JSON file");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse JSON: {e.Message}");
        }
    }

    public void GenerateCagesFromFarmData()
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
        Debug.Log($"🔧 Distributing data for {farm.farmName}:");
        Debug.Log($"   Total Nests: {farm.nestsOccupied}, Champ: {farm.champChicks}, Normal: {farm.normalChicks}");

        // Create a list to track which cages have nests
        List<int> cagesWithNests = new List<int>();
        
        // Step 1: Distribute nests - ONLY nestsOccupied amount (not all 100)
        for (int i = 0; i < farm.nestsOccupied && i < 100; i++)
        {
            farm.cages[i].nestsOccupied = 1; // Each cage gets 1 nest
            cagesWithNests.Add(i);
        }

        Debug.Log($"   ✓ Distributed {farm.nestsOccupied} nests to first {farm.nestsOccupied} cages");

        // Step 2: Distribute CHAMP chicks first (priority)
        int champDistributed = 0;
        for (int i = 0; i < farm.champChicks && i < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[i];
            farm.cages[cageIndex].champChicks = 1;
            
            // Randomly assign egg or clock (50/50 chance)
            farm.cages[cageIndex].hasEgg = Random.value > 0.5f;
            
            // Random remaining time between 30-300 seconds
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            
            champDistributed++;
        }

        Debug.Log($"   ✓ Distributed {champDistributed} champ chicks");

        // Step 3: Distribute NORMAL chicks (after champs)
        int normalDistributed = 0;
        int startIndex = farm.champChicks; // Start after champ chicks
        
        for (int i = 0; i < farm.normalChicks && (startIndex + i) < cagesWithNests.Count; i++)
        {
            int cageIndex = cagesWithNests[startIndex + i];
            farm.cages[cageIndex].normalChicks = 1;
            
            // Randomly assign egg or clock (50/50 chance)
            farm.cages[cageIndex].hasEgg = Random.value > 0.5f;
            
            // Random remaining time between 30-300 seconds
            farm.cages[cageIndex].remainingTime = Random.Range(30f, 300f);
            
            normalDistributed++;
        }

        Debug.Log($"   ✓ Distributed {normalDistributed} normal chicks");
        
        // Verify distribution
        int actualNests = 0;
        int actualChamps = 0;
        int actualNormals = 0;
        
        foreach (var cage in farm.cages)
        {
            if (cage.nestsOccupied > 0) actualNests++;
            if (cage.champChicks > 0) actualChamps++;
            if (cage.normalChicks > 0) actualNormals++;
        }
        
        Debug.Log($"   ✅ Verification - Nests: {actualNests}, Champs: {actualChamps}, Normals: {actualNormals}");
    }

    #region BACKEND INTEGRATION

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