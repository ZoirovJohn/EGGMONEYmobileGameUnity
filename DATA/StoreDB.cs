using UnityEngine;
using System.Collections.Generic;

public class StoreDB : MonoBehaviour
{
    public static StoreDB Instance { get; private set; }

    [System.Serializable]
    public class Item
    {
        public string id;            
        public int priceFP;

        public bool canBuy = true;

        // 🔹 Localized name
        public string Title =>
            LanguageManager.Instance.GetTranslation($"Store_{id}_Title");

        // 🔹 Localized description
        public string Description =>
            LanguageManager.Instance.GetTranslation($"Store_{id}_Desc");
    }

    public List<Item> items = new();

    Dictionary<string, Item> _byId;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (items == null || items.Count == 0)
            SeedItems();

        RebuildIndex();
    }

    // ---------------------------------------
    //  Seed 10 items
    // ---------------------------------------
    [ContextMenu("Seed Items")]
    public void SeedItems()
    {
        items = new()
        {
            new Item { id="nest",            priceFP=500 },
            new Item { id="battery",         priceFP=3500 },
            new Item { id="vitamin",         priceFP=3000 },
            new Item { id="food",            priceFP=10 },
            new Item { id="robot",           priceFP=100000 },
            new Item { id="silver_egg",      priceFP=10000 },
            new Item { id="gold_egg",        priceFP=30000 },
            new Item { id="super_blue_egg",  priceFP=6000 },
            new Item { id="super_red_egg",   priceFP=20000 },
            new Item { id="farmKey",         priceFP=100000 },
        };
    }

    public void RebuildIndex()
    {
        _byId = new Dictionary<string, Item>();
        foreach (var it in items)
            if (!string.IsNullOrEmpty(it.id))
                _byId[it.id] = it;
    }

    public Item Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _byId.TryGetValue(id, out var it) ? it : null;
    }

    public int GetPrice(string id) => Get(id)?.priceFP ?? -1;
}
