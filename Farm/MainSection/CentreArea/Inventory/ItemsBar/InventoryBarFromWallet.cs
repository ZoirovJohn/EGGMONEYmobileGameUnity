using UnityEngine;
using TMPro;

public class InventoryBarFromWallet : MonoBehaviour
{
    [Header("Where the ItemCells live")]
    [SerializeField] Transform content;       // InventoryitemsBarItemScroll/Viewport/Content

    [Header("Source")]
    [SerializeField] PlayerWallet wallet;     // auto-find if null

    InventoryCellId[] cells;

    void Awake()
    {
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (!content) content = transform;
    }

    void OnEnable()
    {
        CacheCells();
        RefreshAll();

        if (wallet != null)
        {
            wallet.OnItemChanged    += OnItemChanged;   // (id, newCount)
            wallet.OnProfileChanged += RefreshAll;      // fallback for bulk changes
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

    void OnItemChanged(string id, int newCount)
    {
        if (cells == null) return;
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

    // --- visuals when count == 0 ---
    void ApplyZeroVisual(InventoryCellId c, bool isZero)
    {
        if (c.icon)
        {
            var col = c.icon.color;                 // preserve existing tint
            col.a = isZero ? (150f / 255f) : 1f;    // 150/255 alpha when zero
            c.icon.color = col;
        }
    }


    // --- helpers ---
    static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.ToLowerInvariant();
        s = s.Replace(" ", "").Replace("_", "").Replace("-", "");
        return s;
    }

    static TMP_Text EnsureCountRef(InventoryCellId c)
    {
        if (c.countText) return c.countText;
        var t = c.transform.Find("Count");
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
