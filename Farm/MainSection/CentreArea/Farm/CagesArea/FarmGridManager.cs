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
    [Header("UI Managers")]
    public InventoryItemsBarChanger inventoryBarChanger;
    public InfoErrorChanger infoErrorChanger;

    private List<CageData> currentCages;

    void Start()
    {
        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        // Wait for canvas to fully initialize
        yield return null;
        Canvas.ForceUpdateCanvases();
        
        // Validate references
        if (!ValidateReferences())
        {
            yield break;
        }

        // ✅ FIXED: Check if backend data is already loaded
        if (farmDatabase.farms == null || farmDatabase.farms.Count == 0)
        {
            farmDatabase.LoadFromJSON();
            
            if (farmDatabase.farms == null || farmDatabase.farms.Count == 0)
            {
                yield break;
            }
        }
        else
        {
            Debug.Log($"✅ Backend data already loaded: {farmDatabase.farms.Count} farms available");
        }

        // Set to Farm 1 (index 0) by default
        farmDatabase.currentFarmIndex = 0;
        
        // Load Farm 1 cages
        LoadFarmCages(0);
        
        if (currentCages == null || currentCages.Count == 0)
        {
            yield break;
        }
        // Setup and build
        SetupGrid();
        
        BuildCages();
    }

    bool ValidateReferences()
    {
        if (grid == null)
        {
            return false;
        }
        
        if (viewport == null)
        {
            return false;
        }
        
        if (cagePrefab == null)
        {
            return false;
        }
        
        if (farmDatabase == null)
        {
            return false;
        }
        return true;
    }

    void SetupGrid()
    {
        // Force canvas update to ensure viewport has correct size
        Canvas.ForceUpdateCanvases();
        
        float viewportWidth = viewport.rect.width;
        
        // Fallback to screen width if viewport width is invalid
        if (viewportWidth <= 0)
        {
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
    }

    void BuildCages()
    {
        if (grid == null)
        {
            return;
        }
        
        if (cagePrefab == null)
        {
            return;
        }
        
        // Clear existing cages
        int childCount = grid.transform.childCount;
        
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(grid.transform.GetChild(i).gameObject);
        }

        if (currentCages == null || currentCages.Count == 0)
        {
            return;
        }

        // Count cages with nests for verification
        int cagesWithNests = 0;
        int totalChampChicks = 0;
        int totalNormalChicks = 0;

        // Build all cages
        for (int i = 0; i < currentCages.Count; i++)
        {
            CageData data = currentCages[i];

            GameObject cage = Instantiate(cagePrefab, grid.transform);
            
            if (cage == null)
            {
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

        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
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
        infoErrorChanger.CloseAllInfoErrorMethod();
        if (bigCage == null || currentCages == null) return;

        CageData data = currentCages[index];
        bigCage.SetActive(true);

        Transform inside = bigCage.transform.Find("BigCageInside");
        if (inside == null) return;

        ApplyCageDisplay(inside, data);
        
        // ✅ USE InventoryItemsBarChanger to open cage bar and close others
        if (inventoryBarChanger != null)
        {
            inventoryBarChanger.InventoryCageBarMethod();
        }
        else
        {
            Debug.LogWarning("⚠️ InventoryBarChanger not assigned!");
        }
    }

    // Load specific farm cages
    private void LoadFarmCages(int farmIndex)
    {
        FarmData farm = farmDatabase.GetFarmByIndex(farmIndex);
        if (farm != null && farm.cages != null && farm.cages.Count > 0)
        {
            currentCages = farm.cages;
        }
        else
        {
            currentCages = new List<CageData>();
        }
    }

    // PUBLIC: Called from FarmHeaderManager when farm clicked
    public void SwitchFarm(int farmIndex)
    {
        
        // ✅ Close any open info/error panels when switching farms
        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
        }
        else
        {
            Debug.LogWarning("⚠️ InfoErrorChanger not assigned - cannot close panels!");
        }
        
        farmDatabase.SwitchToFarm(farmIndex);
        LoadFarmCages(farmIndex);
        
        SetupGrid(); // Recalculate grid if needed
        BuildCages();
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

    #region FARM ITEM APPLICATION SUPPORT

    /// <summary>
    /// Get farm data by farmId (not index)
    /// Used by InventoryFarmItemApplier to find the target farm
    /// </summary>
    public FarmData GetFarmDataById(string farmId)
    {
        if (farmDatabase == null || farmDatabase.farms == null)
        {
            return null;
        }

        FarmData farm = farmDatabase.farms.Find(f => f.farmId == farmId);
        
        if (farm == null)
        {
            foreach (var f in farmDatabase.farms)
            {
                Debug.Log($"   - {f.farmId} ({f.farmName})");
            }
        }
        
        return farm;
    }

    /// <summary>
    /// Apply an item to a specific farm by farmId
    /// Returns true if successful
    /// </summary>
    public bool ApplyItemToFarm(string farmId, string itemId)
    {
        FarmData farm = GetFarmDataById(farmId);
        
        if (farm == null)
        {
            return false;
        }
        
        // Check if item can be applied
        if (!farm.CanApplyItem(itemId))
        {
            return false;
        }
        
        // Apply the item
        farm.ApplyItem(itemId);
        
        // If this is the currently displayed farm, refresh the display
        if (farmDatabase.currentFarmIndex == farm.farmIndex)
        {
            RefreshCurrentFarmDisplay();
        }
        
        return true;
    }

    /// <summary>
    /// Refresh the display for the currently selected farm
    /// Called after applying items to update the UI
    /// </summary>
    public void RefreshCurrentFarmDisplay()
    {
        int currentIndex = farmDatabase.currentFarmIndex;
        
        // Reload cages from database
        LoadFarmCages(currentIndex);
        
        // Rebuild the grid display
        BuildCages();
        
        // Notify FarmHeaderManager to update farm slot visuals
        FarmHeaderManager headerManager = FindAnyObjectByType<FarmHeaderManager>();
        if (headerManager != null)
        {
            headerManager.Refresh();
        }
    }

    /// <summary>
    /// Refresh display for a specific farm by index
    /// </summary>
    public void RefreshFarmDisplay(int farmIndex)
    {
        if (farmIndex < 0 || farmIndex >= farmDatabase.farms.Count)
        {
            return;
        }
        
        // If it's the current farm, refresh it
        if (farmDatabase.currentFarmIndex == farmIndex)
        {
            RefreshCurrentFarmDisplay();
        }
        else
        {
            Debug.Log($"ℹ️ Farm {farmIndex + 1} updated but not currently displayed");
        }
    }

    #endregion
}