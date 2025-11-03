using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmGridManager : MonoBehaviour
{
    public GridLayoutGroup grid;
    public RectTransform viewport;
    public GameObject cagePrefab;
    public GameObject bigCage;
    public FarmDatabase farmDatabase;

    private List<CageData> currentCages;

    void Start()
    {
        Debug.Log("🚀 FarmGridManager Start()");
        
        if (farmDatabase == null)
        {
            Debug.LogError("❌ FarmDatabase not assigned!");
            return;
        }

        // Force load from JSON
        Debug.Log("📂 Loading data from JSON...");
        farmDatabase.LoadFromJSON();
        
        // Check if data loaded
        if (farmDatabase.farms == null || farmDatabase.farms.Count == 0)
        {
            Debug.LogError("❌ No farms loaded! Check JSON file assignment.");
            return;
        }

        Debug.Log($"✅ Loaded {farmDatabase.farms.Count} farms");

        // Set to Farm 1 (index 0) by default
        farmDatabase.currentFarmIndex = 0;
        
        // Load Farm 1 cages
        LoadFarmCages(0);
        
        if (currentCages == null || currentCages.Count == 0)
        {
            Debug.LogError("❌ No cages generated!");
            return;
        }

        Debug.Log($"✅ Farm 1 has {currentCages.Count} cages");
        
        SetupGrid();
        BuildCages();
    }

    void SetupGrid()
    {
        float screenWidth = viewport.rect.width;
        float spacing = grid.spacing.x;

        int columns = (screenWidth < 700) ? 4 : (screenWidth < 1100) ? 5 : 6;

        float totalSpacing = spacing * (columns - 1);
        float cellSize = (screenWidth - totalSpacing) / columns;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = new Vector2(cellSize, cellSize);

        float totalUsed = columns * cellSize + spacing * (columns - 1);
        float sidePadding = Mathf.Max(0, (screenWidth - totalUsed) / 2f);
        grid.padding.left = Mathf.RoundToInt(sidePadding);
        grid.padding.right = Mathf.RoundToInt(sidePadding);

        grid.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenWidth);
        
        Debug.Log($"📐 Grid setup: {columns} columns, cell size: {cellSize}");
    }

    void BuildCages()
    {
        Debug.Log($"🔨 Building cages... Current cage count: {currentCages?.Count ?? 0}");
        
        // Clear existing cages
        int childCount = grid.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(grid.transform.GetChild(i).gameObject);
        }

        if (currentCages == null || currentCages.Count == 0)
        {
            Debug.LogWarning("⚠️ No cages to display!");
            return;
        }

        if (cagePrefab == null)
        {
            Debug.LogError("❌ CagePrefab not assigned!");
            return;
        }

        // Count cages with nests for verification
        int cagesWithNests = 0;
        int totalChampChicks = 0;
        int totalNormalChicks = 0;

        // Build all 100 cages
        for (int i = 0; i < currentCages.Count; i++)
        {
            CageData data = currentCages[i];

            GameObject cage = Instantiate(cagePrefab, grid.transform);
            cage.name = $"Cage_{i+1}";
            
            ApplyCageDisplay(cage.transform, data);

            Button btn = cage.GetComponent<Button>() ?? cage.AddComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() => ShowBigCage(index));

            // Count for debug
            if (data.nestsOccupied > 0) cagesWithNests++;
            totalChampChicks += data.champChicks;
            totalNormalChicks += data.normalChicks;
        }

        Debug.Log($"✅ Built {currentCages.Count} cages for Farm {farmDatabase.currentFarmIndex + 1}");
        Debug.Log($"   📊 Cages with nests: {cagesWithNests}, Champ: {totalChampChicks}, Normal: {totalNormalChicks}");
        
        // Force layout refresh
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    }

    void ApplyCageDisplay(Transform cageRoot, CageData data)
    {
        SetAllOff(cageRoot.gameObject);

        bool hasAnyChick = data.normalChicks > 0 || data.champChicks > 0;

        // ORDER 1: Nest (ONLY show if nestsOccupied > 0)
        Transform nest = cageRoot.Find("Nest");
        if (nest != null && data.nestsOccupied > 0)
        {
            nest.gameObject.SetActive(true);
            nest.SetAsFirstSibling();
        }

        // ORDER 2: ChampChick (priority chick)
        Transform champChick = cageRoot.Find("ChampChick");
        if (champChick != null && data.champChicks > 0)
        {
            champChick.gameObject.SetActive(true);
            champChick.SetAsLastSibling();
        }

        // ORDER 3: NormalChick
        Transform normalChick = cageRoot.Find("WhiteChick");
        if (normalChick != null && data.normalChicks > 0)
        {
            normalChick.gameObject.SetActive(true);
            normalChick.SetAsLastSibling();
        }

        // ORDER 4: Egg or Clock (only if there are chicks)
        if (hasAnyChick)
        {
            Transform egg = cageRoot.Find("Egg");
            Transform clock = cageRoot.Find("Clock");
            
            if (data.hasEgg && egg != null)
            {
                egg.gameObject.SetActive(true);
                egg.SetAsLastSibling();
            }
            else if (!data.hasEgg && clock != null)
            {
                clock.gameObject.SetActive(true);
                clock.SetAsLastSibling();
            }
        }

        // ORDER 5: LifeTime (top layer - only if there are chicks)
        Transform lifeTime = cageRoot.Find("LifeTime");
        if (lifeTime != null && hasAnyChick)
        {
            lifeTime.gameObject.SetActive(true);
            lifeTime.SetAsLastSibling();
        }
    }

    void SetAllOff(GameObject cage)
    {
        string[] names = { "LifeTime", "Nest", "Egg", "Clock", "WhiteChick", "ChampChick" };
        foreach (var n in names)
        {
            Transform t = cage.transform.Find(n);
            if (t != null) t.gameObject.SetActive(false);
        }
    }

    void ShowBigCage(int index)
    {
        if (bigCage == null || currentCages == null) return;

        CageData data = currentCages[index];
        bigCage.SetActive(true);

        Transform inside = bigCage.transform.Find("BigCageInside");
        if (inside == null) return;

        ApplyCageDisplay(inside, data);
        
        Debug.Log($"📦 Opened cage {index + 1}: Nests={data.nestsOccupied}, Normal={data.normalChicks}, Champ={data.champChicks}");
    }

    // Load specific farm cages
    private void LoadFarmCages(int farmIndex)
    {
        Debug.Log($"📥 Loading cages for Farm {farmIndex + 1}...");
        
        FarmData farm = farmDatabase.GetFarmByIndex(farmIndex);
        if (farm != null && farm.cages != null && farm.cages.Count > 0)
        {
            currentCages = farm.cages;
            Debug.Log($"✅ Loaded {currentCages.Count} cages from Farm {farmIndex + 1}");
            Debug.Log($"   - Nests: {farm.nestsOccupied}, Normal: {farm.normalChicks}, Champ: {farm.champChicks}");
        }
        else
        {
            Debug.LogError($"❌ Failed to load Farm {farmIndex + 1} or farm has no cages!");
            currentCages = new List<CageData>();
        }
    }

    // PUBLIC: Called from FarmHeaderManager when farm clicked
    public void SwitchFarm(int farmIndex)
    {
        Debug.Log($"🔄 Switching to Farm {farmIndex + 1}...");
        
        farmDatabase.SwitchToFarm(farmIndex);
        LoadFarmCages(farmIndex);
        BuildCages();
        
        Debug.Log($"✅ Successfully switched to Farm {farmIndex + 1}");
    }

    // PUBLIC: Refresh current farm (for backend updates)
    public void RefreshCages()
    {
        LoadFarmCages(farmDatabase.currentFarmIndex);
        BuildCages();
    }
}