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

    [Header("Lifetime Sprites")]
    public Sprite[] lifetimeSprites = new Sprite[15];


    void Start()
    {
        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        
        if (!ValidateReferences())
        {
            yield break;
        }

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

        farmDatabase.currentFarmIndex = 0;
        LoadFarmCages(0);
        if (currentCages == null || currentCages.Count == 0)
        {
            yield break;
        }
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
        Canvas.ForceUpdateCanvases();
        float viewportWidth = viewport.rect.width;
        if (viewportWidth <= 0)
        {
            viewportWidth = Screen.width;
        }
        
        float spacing = grid.spacing.x;
        int columns = (viewportWidth < 700) ? 4 : (viewportWidth < 1100) ? 5 : 6;
        float totalSpacing = spacing * (columns - 1);
        float cellSize = (viewportWidth - totalSpacing) / columns;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = new Vector2(cellSize, cellSize);

        float totalUsed = columns * cellSize + spacing * (columns - 1);
        float sidePadding = Mathf.Max(0, (viewportWidth - totalUsed) / 2f);
        grid.padding.left = Mathf.RoundToInt(sidePadding);
        grid.padding.right = Mathf.RoundToInt(sidePadding);

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
        
        int childCount = grid.transform.childCount;
        
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(grid.transform.GetChild(i).gameObject);
        }

        if (currentCages == null || currentCages.Count == 0)
        {
            return;
        }

        int cagesWithNests = 0;
        int totalChampChicks = 0;
        int totalNormalChicks = 0;

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

            if (data.nestsOccupied > 0) cagesWithNests++;
            totalChampChicks += data.champChicks;
            totalNormalChicks += data.normalChicks;
            
            if ((i + 1) % 25 == 0)
            {
                Debug.Log($"   📦 Created {i + 1}/{currentCages.Count} cages...");
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    }

    void ApplyCageDisplay(Transform cageRoot, CageData data)
    {
        if (cageRoot == null) return;
        
        SetAllOff(cageRoot.gameObject);

        bool hasAnyChick = data.normalChicks > 0 || data.champChicks > 0;

        Transform premiumNest = cageRoot.Find("PremiumNest");
        Transform normalNest = cageRoot.Find("Nest");

        if (data.nestsOccupied > 0)
        {
            if (data.isPremium && premiumNest != null)
            {
                premiumNest.gameObject.SetActive(true);
                premiumNest.SetAsFirstSibling();
            }
            else if (normalNest != null)
            {
                normalNest.gameObject.SetActive(true);
                normalNest.SetAsFirstSibling();
            }
        }

        Transform champChick = cageRoot.Find("ChampChick");
        Transform normalChick = cageRoot.Find("WhiteChick");

        if (data.champChicks > 0 && champChick != null)
        {
            champChick.gameObject.SetActive(true);
            champChick.SetAsLastSibling();
        }

        if (data.normalChicks > 0 && normalChick != null)
        {
            normalChick.gameObject.SetActive(true);
            normalChick.SetAsLastSibling();
        }

        // 3️⃣ EGG / CLOCK
        if (hasAnyChick)
        {
            Transform egg = cageRoot.Find("Egg");
            Transform clock = cageRoot.Find("Clock");

            if (data.hasEgg && egg != null)
            {
                egg.gameObject.SetActive(true);
                egg.SetAsLastSibling();
            }
            else if (clock != null)
            {
                clock.gameObject.SetActive(true);
                clock.SetAsLastSibling();
            }
        }

        Transform lifeTime = cageRoot.Find("LifeTime");
        if (lifeTime != null && hasAnyChick)
        {
            Image img = lifeTime.GetComponent<Image>();
            if (img != null && data.lifetimeDaysRemaining > 0)
            {
                int day = Mathf.Clamp(data.lifetimeDaysRemaining, 1, 15);

                int[] order = { 3,2,5,7,9,1,4,6,8,10,11,12,13,14,15 };
                int index = System.Array.IndexOf(order, day);

                if (index >= 0)
                {
                    img.sprite = lifetimeSprites[index];
                    lifeTime.gameObject.SetActive(true);
                }
            }
        }
    }


    void SetAllOff(GameObject cage)
    {
        string[] names = 
        { 
            "LifeTime", 
            "Nest", 
            "PremiumNest", 
            "Egg", 
            "Clock", 
            "WhiteChick", 
            "ChampChick" 
        };

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
        
        if (inventoryBarChanger != null)
        {
            inventoryBarChanger.InventoryCageBarMethod();
        }
        else
        {
            Debug.LogWarning("⚠️ InventoryBarChanger not assigned!");
        }
    }

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

    public void SwitchFarm(int farmIndex)
    {
        
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
        
        SetupGrid(); 
        BuildCages();
    }

    public void RefreshCages()
    {
        LoadFarmCages(farmDatabase.currentFarmIndex);
        BuildCages();
    }

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
        
        if (!farm.CanApplyItem(itemId))
        {
            return false;
        }
        farm.ApplyItem(itemId);
        
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
        LoadFarmCages(currentIndex);
        BuildCages();
        
        FarmHeaderManager headerManager = FindAnyObjectByType<FarmHeaderManager>();
        if (headerManager != null)
        {
            headerManager.UpdateAllFarmSlotVisuals();
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