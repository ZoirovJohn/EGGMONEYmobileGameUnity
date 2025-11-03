using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmGridManager : MonoBehaviour
{
    public GridLayoutGroup grid;
    public RectTransform viewport;
    public GameObject cagePrefab;
    public GameObject bigCage;          // assign root BigCage
    public FarmDatabase farmDatabase;   // assign ScriptableObject

    private List<CageData> currentCages;

    void Start()
    {
        if (farmDatabase == null)
        {
            Debug.LogError("FarmDatabase not assigned!");
            return;
        }

        // Auto-load from JSON if not already loaded
        if (farmDatabase.farms.Count == 0)
        {
            farmDatabase.LoadFromJSON();
        }

        currentCages = farmDatabase.GetCurrentFarmCages()?.cages;

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
    }

    void BuildCages()
    {
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        if (currentCages == null) return;

        for (int i = 0; i < currentCages.Count; i++)
        {
            CageData data = currentCages[i];

            GameObject cage = Instantiate(cagePrefab, grid.transform);
            cage.name = $"Cage_{i+1}";
            
            // Apply ordered display logic
            ApplyCageDisplay(cage.transform, data);

            Button btn = cage.GetComponent<Button>() ?? cage.AddComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() => ShowBigCage(index));
        }
    }

    void ApplyCageDisplay(Transform cageRoot, CageData data)
    {
        // Turn everything off first
        SetAllOff(cageRoot.gameObject);

        bool hasAnyChick = data.normalChicks > 0 || data.champChicks > 0;

        // ORDER 1: Nest (ALWAYS show as base - the green grass)
        Transform nest = cageRoot.Find("Nest");
        if (nest != null)
        {
            // Show nest always (it's the grass base), but can check nestsOccupied if needed
            nest.gameObject.SetActive(true);
            nest.SetAsFirstSibling(); // Bottom layer
        }

        // ORDER 2: ChampChick (priority chick - golden one)
        Transform champChick = cageRoot.Find("ChampChick");
        if (champChick != null && data.champChicks > 0)
        {
            champChick.gameObject.SetActive(true);
            champChick.SetAsLastSibling();
        }

        // ORDER 3: NormalChick (white chick)
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

        // ORDER 5: LifeTime (top layer - progress bar, only if there are chicks)
        Transform lifeTime = cageRoot.Find("LifeTime");
        if (lifeTime != null && hasAnyChick)
        {
            lifeTime.gameObject.SetActive(true);
            lifeTime.SetAsLastSibling(); // Top layer
        }
    }

    void SetAllOff(GameObject cage)
    {
        string[] names = { "LifeTime", "Nest", "Egg", "Clock", "WhiteChick", "ChampChick" };
        foreach (var n in names)
            cage.transform.Find(n)?.gameObject.SetActive(false);
    }

    void ShowBigCage(int index)
    {
        if (bigCage == null || currentCages == null) return;

        CageData data = currentCages[index];
        bigCage.SetActive(true);

        Transform inside = bigCage.transform.Find("BigCageInside");
        if (inside == null) return;

        // Apply the same ordered display logic to BigCage
        ApplyCageDisplay(inside, data);
    }

    // Optional: call this to switch farm dynamically
    public void SwitchFarm(int farmIndex)
    {
        farmDatabase.currentFarmIndex = farmIndex;
        currentCages = farmDatabase.GetCurrentFarmCages()?.cages;
        BuildCages();
    }

    // Call this to refresh cages after backend data loads
    public void RefreshCages()
    {
        currentCages = farmDatabase.GetCurrentFarmCages()?.cages;
        BuildCages();
    }
}