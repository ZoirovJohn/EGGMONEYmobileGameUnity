using UnityEngine;
using System.Collections.Generic;

public class StoreDB : MonoBehaviour
{
    public static StoreDB Instance { get; private set; }

    [System.Serializable]
    public class Item
    {
        public string id;            // ex) "nest", "silver_egg", ...
        public string name;          // display name
        public int priceFP;          // price (0 if not sold)
        public bool canBuy = true;   // set false if not purchasable
        public bool canSell = false; // set true if sell-only

        [TextArea] public string howToGet;
        public bool giftPermitted;
        [TextArea] public string giftFunction;
        [TextArea] public string lifetime;
        [TextArea] public string performance;

        public Sprite icon;          // optional
    }

    public List<Item> items = new List<Item>();

    // quick lookup by id
    Dictionary<string, Item> _byId;

    void Awake()
    {
        if (Instance == null) Instance = this; else if (Instance != this) Destroy(gameObject);

        // Seed automatically if empty
        if (items == null || items.Count == 0) SeedInOrder();
        RebuildIndex();
    }

    [ContextMenu("Seed Items (in this order)")]
    public void SeedInOrder()
    {
        items = new List<Item>
        {
            // 1) nest
            new Item{
                id="nest", name="Nest", priceFP=500, canBuy=true,
                howToGet="Store",
                giftPermitted=true,
                lifetime="No change after one use",
                performance="Chickens cannot lay eggs without a nest."
            },

            // 2) SilverEgg (Sgg)
            new Item{
                id="silver_egg", name="Silver Egg", priceFP=10000, canBuy=true,
                howToGet="Store",
                giftPermitted=false,
                lifetime="Unlimited",
                performance="Tap to turn into a chick; it becomes Soondong after 48 hours."
            },

            // 3) Food (Prey)
            new Item{
                id="food", name="Food", priceFP=10, canBuy=true,
                howToGet="Store / Daily check",
                giftPermitted=true,
                lifetime="For one chicken for one day",
                performance="Daily food amount a chicken can eat."
            },

            // 4) GoldEgg (Ggg)
            new Item{
                id="gold_egg", name="Gold Egg", priceFP=30000, canBuy=true,
                howToGet="Store",
                giftPermitted=false,
                lifetime="Unlimited",
                performance="Tap to turn into a chick; it becomes Champ after 24 hours."
            },

            // 5) Booster (Vitamin Booster)
            new Item{
                id="booster", name="Booster", priceFP=3000, canBuy=true,
                howToGet="Store",
                giftPermitted=true,
                lifetime="During the chicken's lifetime",
                performance="Use on one chicken. Birth speed +0.0545."
            },

            // 6) battery
            new Item{
                id="battery", name="Battery", priceFP=3500, canBuy=true,
                howToGet="Store",
                giftPermitted=true,
                lifetime="7 days",
                performance="Powers the farm management robot for 7 days."
            },

            // 7) Robot (Farm management robot)
            new Item{
                id="robot", name="Robot", priceFP=100000, canBuy=true,
                howToGet="Store",
                giftPermitted=true,
                lifetime="No change after one use",
                performance="Once placed, cannot be moved; works while battery lasts; auto-performs daily duties."
            },

            // 8) SuperBlueEgg (Event Blue egg / Bgg)
            new Item{
                id="super_blue_egg", name="Super Blue Egg", priceFP=6000, canBuy=true,
                howToGet="Store",
                giftPermitted=false,
                lifetime="Unlimited",
                performance="Tap to get premium items."
            },

            // 9) SuperRedEgg (Event Red egg / Rgg)
            new Item{
                id="super_red_egg", name="Super Red Egg", priceFP=20000, canBuy=true,
                howToGet="Store",
                giftPermitted=false,
                lifetime="Unlimited",
                performance="Tap to get Soondong, Champ, Legend coco, and Super Legend coco."
            },
        };
    }

    [ContextMenu("Rebuild Index")]
    public void RebuildIndex()
    {
        _byId = new Dictionary<string, Item>(items.Count);
        foreach (var it in items) if (it != null && !string.IsNullOrEmpty(it.id)) _byId[it.id] = it;
    }

    public Item Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_byId != null && _byId.TryGetValue(id, out var it)) return it;
        foreach (var i in items) if (i != null && i.id == id) return i; // fallback if index stale
        return null;
    }

    public int GetPrice(string id) => Get(id)?.priceFP ?? -1;
}
