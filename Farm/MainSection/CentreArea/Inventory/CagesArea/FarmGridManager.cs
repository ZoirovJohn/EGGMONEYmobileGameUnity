using System.Collections;
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
        Debug.Log("🚀 FarmGridManager Start() - Beginning initialization");
        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        Debug.Log("⏳ Waiting one frame for canvas initialization...");
        
        // Wait for canvas to fully initialize
        yield return null;
        Canvas.ForceUpdateCanvases();
        
        Debug.Log("✅ Canvas updated, starting validation...");
        
        // Validate references
        if (!ValidateReferences())
        {
            Debug.LogError("❌ Reference validation failed!");
            yield break;
        }

        // Load farm data
        Debug.Log("📂 Loading data from JSON...");
        farmDatabase.LoadFromJSON();
        
        if (farmDatabase.farms == null || farmDatabase.farms.Count == 0)
        {
            Debug.LogError("❌ No farms loaded! Check JSON file assignment.");
            yield break;
        }

        Debug.Log($"✅ Loaded {farmDatabase.farms.Count} farms");

        // Set to Farm 1 (index 0) by default
        farmDatabase.currentFarmIndex = 0;
        
        // Load Farm 1 cages
        LoadFarmCages(0);
        
        if (currentCages == null || currentCages.Count == 0)
        {
            Debug.LogError("❌ No cages generated!");
            yield break;
        }

        Debug.Log($"✅ Farm 1 has {currentCages.Count} cages - About to setup grid");
        
        // Setup and build
        SetupGrid();
        
        Debug.Log("📦 Grid setup complete - About to build cages");
        
        BuildCages();
        
        Debug.Log("🎉 Initialization complete!");
    }

    bool ValidateReferences()
    {
        if (grid == null)
        {
            Debug.LogError("❌ GridLayoutGroup is NULL! Assign it in Inspector!");
            return false;
        }
        
        if (viewport == null)
        {
            Debug.LogError("❌ Viewport is NULL! Assign it in Inspector!");
            return false;
        }
        
        if (cagePrefab == null)
        {
            Debug.LogError("❌ CagePrefab is NULL! Assign it in Inspector!");
            return false;
        }
        
        if (farmDatabase == null)
        {
            Debug.LogError("❌ FarmDatabase not assigned!");
            return false;
        }

        Debug.Log($"✅ All references assigned");
        return true;
    }

    void SetupGrid()
    {
        // Force canvas update to ensure viewport has correct size
        Canvas.ForceUpdateCanvases();
        
        float viewportWidth = viewport.rect.width;
        
        Debug.Log($"🔍 Viewport width: {viewportWidth}, Screen.width: {Screen.width}");
        
        // Fallback to screen width if viewport width is invalid
        if (viewportWidth <= 0)
        {
            Debug.LogWarning("⚠️ Viewport width invalid, using Screen.width as fallback");
            viewportWidth = Screen.width;
        }
        
        float spacing = grid.spacing.x;

        // Responsive column count
        int columns = (viewportWidth < 700) ? 4 : (viewportWidth < 1100) ? 5 : 6;

        // Calculate cell size
        float totalSpacing = spacing * (columns - 1);
        float cellSize = (viewportWidth - totalSpacing) / columns;

        // Apply to grid
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = new Vector2(cellSize, cellSize);

        // Center alignment with padding
        float totalUsed = columns * cellSize + spacing * (columns - 1);
        float sidePadding = Mathf.Max(0, (viewportWidth - totalUsed) / 2f);
        grid.padding.left = Mathf.RoundToInt(sidePadding);
        grid.padding.right = Mathf.RoundToInt(sidePadding);

        // Set grid width
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportWidth);
        
        Debug.Log($"📐 Grid setup: {columns} columns, cell size: {cellSize}x{cellSize}, spacing: {spacing}");
    }

    void BuildCages()
    {
        Debug.Log($"🔨 BuildCages() START - Current cage count: {currentCages?.Count ?? 0}");
        
        if (grid == null)
        {
            Debug.LogError("❌ Grid is NULL in BuildCages!");
            return;
        }
        
        if (cagePrefab == null)
        {
            Debug.LogError("❌ CagePrefab is NULL in BuildCages!");
            return;
        }
        
        Debug.Log($"✅ Grid and prefab references valid");
        
        // Clear existing cages
        int childCount = grid.transform.childCount;
        Debug.Log($"🧹 Clearing {childCount} existing children from grid...");
        
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(grid.transform.GetChild(i).gameObject);
        }

        Debug.Log($"✅ Grid cleared, child count now: {grid.transform.childCount}");

        if (currentCages == null || currentCages.Count == 0)
        {
            Debug.LogWarning("⚠️ No cages to display!");
            return;
        }

        // Count cages with nests for verification
        int cagesWithNests = 0;
        int totalChampChicks = 0;
        int totalNormalChicks = 0;

        Debug.Log($"🏗️ Starting to instantiate {currentCages.Count} cages...");

        // Build all cages
        for (int i = 0; i < currentCages.Count; i++)
        {
            CageData data = currentCages[i];

            GameObject cage = Instantiate(cagePrefab, grid.transform);
            
            if (cage == null)
            {
                Debug.LogError($"❌ Failed to instantiate cage {i}!");
                continue;
            }
            
            cage.name = $"Cage_{i+1}";
            
            ApplyCageDisplay(cage.transform, data);

            Button btn = cage.GetComponent<Button>() ?? cage.AddComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() => ShowBigCage(index));

            // Count for debug
            if (data.nestsOccupied > 0) cagesWithNests++;
            totalChampChicks += data.champChicks;
            totalNormalChicks += data.normalChicks;
            
            // Log progress every 25 cages
            if ((i + 1) % 25 == 0)
            {
                Debug.Log($"   📦 Created {i + 1}/{currentCages.Count} cages...");
            }
        }

        Debug.Log($"✅ Built {currentCages.Count} cages for Farm {farmDatabase.currentFarmIndex + 1}");
        Debug.Log($"   📊 Cages with nests: {cagesWithNests}, Champ: {totalChampChicks}, Normal: {totalNormalChicks}");
        Debug.Log($"   📦 Grid now has {grid.transform.childCount} children");
        
        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
        
        Debug.Log($"🎉 BuildCages() COMPLETE");
    }

    void ApplyCageDisplay(Transform cageRoot, CageData data)
    {
        if (cageRoot == null) return;
        
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
        
        SetupGrid(); // Recalculate grid if needed
        BuildCages();
        
        Debug.Log($"✅ Successfully switched to Farm {farmIndex + 1}");
    }

    // PUBLIC: Refresh current farm (for backend updates)
    public void RefreshCages()
    {
        LoadFarmCages(farmDatabase.currentFarmIndex);
        BuildCages();
    }

    // PUBLIC: Handle screen rotation or resize
    public void OnScreenSizeChanged()
    {
        SetupGrid();
        BuildCages();
    }
}