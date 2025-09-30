using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PopulateStoreUIFromDB : MonoBehaviour
{
    [Header("Assign these")]
    [SerializeField] StoreDB store;                 // drag StorePanel (with StoreDB)
    [SerializeField] Transform content;             // StorePanel/ItemsScroll/Viewport/Content
    [SerializeField] string playerInfoNode = "PlayerInfo_Left";

    // exact names expected under PlayerInfo_Left
    static readonly string[] FieldOrder = 
        { "Name", "Price", "HowToGet", "GiftFunction", "LifeTime", "Performance" };

    void Start() => Apply();

    [ContextMenu("Apply Now")]
    public void Apply()
    {
        if (!store) store = StoreDB.Instance;
        if (!store || content == null || store.items == null || store.items.Count == 0)
        {
            Debug.LogWarning("[PopulateStoreUIFromDB] Missing store/content or no items.");
            return;
        }

        // collect ItemCells under Content IN HIERARCHY ORDER that contain PlayerInfo_Left
        var cells = new List<Transform>();
        for (int i = 0; i < content.childCount; i++)
        {
            var cell = content.GetChild(i);
            if (cell.Find(playerInfoNode) != null) cells.Add(cell);
        }

        int count = Mathf.Min(store.items.Count, cells.Count);
        if (cells.Count != store.items.Count)
            Debug.Log($"[PopulateStoreUIFromDB] DB items: {store.items.Count}, cells: {cells.Count}. Filling first {count}.");

        for (int i = 0; i < count; i++)
        {
            var item = store.items[i];
            var infoRoot = cells[i].Find(playerInfoNode);
            if (!infoRoot) continue;

            // build plain "Label: value" strings (your bolding script will style labels)
            string nameText        = $"Name: {item.name}";
            string priceText       = $"Price: {(item.canBuy && item.priceFP > 0 ? $"{item.priceFP:N0} FP" : "None")}";
            string howToGetText    = $"HowToGet: {item.howToGet}";
            string giftText        = $"GiftFunction: {(item.giftPermitted ? "Permitted" : "Not permitted")}";
            string lifeTimeText    = $"LifeTime: {item.lifetime}";
            string performanceText = $"Performance: {(string.IsNullOrWhiteSpace(item.performance) ? "—" : item.performance)}";

            // try exact names first; if missing, fallback to index mapping
            SetByNameOrIndex(infoRoot, "Name",        0, nameText);
            SetByNameOrIndex(infoRoot, "Price",       1, priceText);
            SetByNameOrIndex(infoRoot, "HowToGet",    2, howToGetText);
            SetByNameOrIndex(infoRoot, "GiftFunction",3, giftText);
            SetByNameOrIndex(infoRoot, "LifeTime",    4, lifeTimeText);
            SetByNameOrIndex(infoRoot, "Performance", 5, performanceText);
        }
    }

    // --- helpers ---

    void SetByNameOrIndex(Transform root, string exactName, int fallbackIndex, string text)
    {
        // 1) exact child (any depth) named EXACTLY 'exactName'
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == exactName && TrySetText(t, text)) return;
        }

        // 2) fallback by order of text components under PlayerInfo_Left
        var orderedTexts = GetOrderedTextComponents(root);
        if (fallbackIndex >= 0 && fallbackIndex < orderedTexts.Count)
        {
            SetComponentText(orderedTexts[fallbackIndex], text);
            return;
        }

        Debug.LogWarning($"[PopulateStoreUIFromDB] Could not set '{exactName}' (no exact match, index {fallbackIndex} out of range).");
    }

    List<Component> GetOrderedTextComponents(Transform root)
    {
        var list = new List<Component>();
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            var tmp = t.GetComponent<TMP_Text>();
            if (tmp) { list.Add(tmp); continue; }
            var ui = t.GetComponent<Text>();
            if (ui) { list.Add(ui); }
        }
        return list;
    }

    bool TrySetText(Transform t, string text)
    {
        var tmp = t.GetComponent<TMP_Text>();
        if (tmp) { tmp.text = text; return true; }

        var ui = t.GetComponent<Text>();
        if (ui) { ui.supportRichText = true; ui.text = text; return true; }

        // common case: label object with child Text
        tmp = t.GetComponentInChildren<TMP_Text>(true);
        if (tmp) { tmp.text = text; return true; }
        ui = t.GetComponentInChildren<Text>(true);
        if (ui) { ui.supportRichText = true; ui.text = text; return true; }

        return false;
    }

    void SetComponentText(Component c, string text)
    {
        if (c is TMP_Text tmp) { tmp.text = text; }
        else if (c is Text ui) { ui.supportRichText = true; ui.text = text; }
    }
}
