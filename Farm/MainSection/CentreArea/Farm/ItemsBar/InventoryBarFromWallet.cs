using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryBarFromWallet : MonoBehaviour
{
    [Header("Where the ItemCells live")]
    [SerializeField] Transform content;    // Viewport/Content container for item cells

    [Header("Source")]
    [SerializeField] PlayerWallet wallet;

    [Header("Scrolling Layout")]
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] RectTransform viewport;

    InventoryCellId[] cells;

    void Awake()
    {
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (!content) content = transform;
    }

    void OnEnable()
    {
        CacheCells();
        ApplySizing();
        RefreshAll();

        if (wallet != null)
        {
            wallet.OnItemChanged    += OnItemChanged;
            wallet.OnProfileChanged += RefreshAll;
        }
    }

    void OnDisable()
    {
        if (wallet != null)
        {
            wallet.OnItemChanged    -= OnItemChanged;
            wallet.OnProfileChanged -= RefreshAll;
        }
    }

    void CacheCells() => cells = content.GetComponentsInChildren<InventoryCellId>(true);

    //---------------------------------------------------------------------
    // ✅ AUTO-SIZING TO ALWAYS FIT WHOLE CELLS INSIDE VIEWPORT HEIGHT
    //---------------------------------------------------------------------
     void ApplySizing()
    {
        if (!grid || !viewport || cells == null || cells.Length == 0)
            return;

        float viewportW = viewport.rect.width;
        float viewportH = viewport.rect.height;

        float spacing = grid.spacing.x;
        float padL = grid.padding.left;
        float padR = grid.padding.right;
        float padT = grid.padding.top;
        float padB = grid.padding.bottom;

        float availW = viewportW - padL - padR;
        float availH = viewportH - padT - padB;

        int count = cells.Length;

        // Step 1: Calculate how many cells should fit based on viewport ratio
        // Use ROUND to nearest whole number for best fit
        // 3.9 → 4, 3.6 → 4, 3.4 → 3, 3.1 → 3
        int visibleCells = Mathf.RoundToInt(availW / availH);
        
        // Ensure at least 1 cell
        if (visibleCells < 1) visibleCells = 1;
        
        // Step 2: Calculate cell size to fit exactly this many cells
        float cellSize = (availW - (spacing * (visibleCells - 1))) / visibleCells;
        
        // Step 3: Cell should be at most 5px smaller than viewport height
        float maxCellSize = availH - 5f;
        if (cellSize > maxCellSize) cellSize = maxCellSize;

        Debug.Log($"📐 Inventory: Viewport {viewportW:F1}x{viewportH:F1}, {visibleCells} cells fit, Cell: {cellSize:F1}x{cellSize:F1}");

        // ✅ APPLY cell size (square)
        grid.cellSize = new Vector2(cellSize, cellSize);
        
        // ✅ ALIGN content to middle-left
        grid.childAlignment = TextAnchor.MiddleLeft;

        // ✅ SCROLLABLE CONTENT WIDTH (all items)
        float totalWidth =
            padL +
            (cellSize * count) +
            (spacing * (count - 1)) +
            padR;

        RectTransform contentRT = content as RectTransform;
        Vector2 size = contentRT.sizeDelta;
        size.x = totalWidth;
        contentRT.sizeDelta = size;

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
    }

    void OnRectTransformDimensionsChange() => ApplySizing();

    //---------------------------------------------------------------------
    // ✅ DATA DISPLAY
    //---------------------------------------------------------------------
    void OnItemChanged(string id, int newCount)
    {
        string norm = Norm(id);
        foreach (var c in cells)
        {
            if (!c || string.IsNullOrEmpty(c.productId)) continue;
            if (Norm(c.productId) != norm) continue;

            var txt = EnsureCountRef(c);
            if (txt) txt.text = newCount.ToString("N0");
            ApplyZeroVisual(c, newCount == 0);
            break;
        }
    }

    public void RefreshAll()
    {
        if (cells == null) CacheCells();
        if (cells == null || wallet == null) return;

        foreach (var c in cells)
        {
            if (!c || string.IsNullOrEmpty(c.productId)) continue;
            int count = wallet.GetItemCount(c.productId);
            var txt = EnsureCountRef(c);
            if (txt) txt.text = count.ToString("N0");
            ApplyZeroVisual(c, count == 0);
        }
    }

    void ApplyZeroVisual(InventoryCellId c, bool isZero)
    {
        if (c.icon)
        {
            Color col = c.icon.color;
            col.a = isZero ? 0.58f : 1f;
            c.icon.color = col;
        }
    }

    //---------------------------------------------------------------------
    // ✅ HELPERS
    //---------------------------------------------------------------------
    static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
    }

    static TMP_Text EnsureCountRef(InventoryCellId c)
    {
        if (c.countText) return c.countText;

        Transform t = c.transform.Find("Count");
        if (!t) t = FindByNameRecursive(c.transform, "Count");

        if (t)
        {
            var tmp = t.GetComponent<TMP_Text>();
            if (!tmp) tmp = t.GetComponentInChildren<TMP_Text>(true);
            c.countText = tmp;
        }

        return c.countText;
    }

    static Transform FindByNameRecursive(Transform root, string name)
    {
        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            if (tr.name == name) return tr;
        return null;
    }
}