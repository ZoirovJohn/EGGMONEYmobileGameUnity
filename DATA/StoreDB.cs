using UnityEngine;
using System.Collections.Generic;

public class StoreDB : MonoBehaviour
{
    public static StoreDB Instance { get; private set; }

    [System.Serializable]
    public class Item
    {
        public string id;             // unique key you’ll reference from popups
        public string name;           // display
        public int priceFP;           // price (set 0 if not for sale)
        public bool canBuy = true;    // if false → not sold (event/drop only)
        public bool canSell = false;  // true if “sell-only”

        [TextArea] public string howToGet;
        public bool giftPermitted;
        [TextArea] public string giftFunction;
        [TextArea] public string lifetime;
        [TextArea] public string performance;
    }

    public List<Item> items = new List<Item>();

    Dictionary<string, Item> _byId;

    void Awake()
    {
        if (Instance == null) Instance = this; else if (Instance != this) Destroy(gameObject);
        if (items == null || items.Count == 0) SeedFromChat();    // auto-fill once
        RebuildIndex();
    }

    [ContextMenu("Rebuild Index")]
    public void RebuildIndex()
    {
        _byId = new Dictionary<string, Item>();
        foreach (var it in items) if (it != null && !string.IsNullOrEmpty(it.id)) _byId[it.id] = it;
    }

    public Item Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_byId != null && _byId.TryGetValue(id, out var it)) return it;
        foreach (var i in items) if (i != null && i.id == id) return i; // fallback
        return null;
    }

    // ---- PRE-FILLED DATA FROM YOUR MESSAGE ----
    [ContextMenu("Seed From Chat")]
    public void SeedFromChat()
    {
        items = new List<Item>
        {
            // Group 1
            new Item{
                id="nest", name="Nest", priceFP=500, canBuy=true, canSell=false,
                howToGet="Store",
                giftPermitted=true, giftFunction="Permitted",
                lifetime="No change after one use",
                performance="Chickens cannot lay eggs without a nest."
            },
            new Item{
                id="premium_nest", name="Premium Nest", priceFP=0, canBuy=false, canSell=false,
                howToGet="Blue Event Egg [Bgg]",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="No change after one use",
                performance="Birth speed +0.01"
            },
            new Item{
                id="chick", name="Chick", priceFP=0, canBuy=false, canSell=false,
                howToGet="From Ggg or Sgg",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="24h or 48h",
                performance="Tap gold or silver egg to get a chick. It becomes Soondong or Champ later."
            },
            new Item{
                id="egg", name="Egg", priceFP=400, canBuy=false, canSell=true,
                howToGet="From Soondong or Champ",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="Unlimited",
                performance="Convert to FP or request delivery to home."
            },

            // Group 2
            new Item{
                id="battery", name="Battery", priceFP=3500, canBuy=true, canSell=false,
                howToGet="Store",
                giftPermitted=true, giftFunction="Permitted",
                lifetime="7 days",
                performance="When installed on a farm management robot, it runs for 7 days."
            },
            new Item{
                id="premium_battery", name="Premium Battery", priceFP=0, canBuy=false, canSell=false,
                howToGet="Blue Event Egg [Bgg]",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="14 days",
                performance="Runs robot for 14 days."
            },
            new Item{
                id="soondong", name="Soondong", priceFP=0, canBuy=false, canSell=false,
                howToGet="From Silver Egg (Sgg)",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="150 days",
                performance="Birth speed +0.34. If you don’t clean/feed for a day, lifespan -1 day."
            },
            new Item{
                id="silver_egg", name="Silver Egg (Sgg)", priceFP=10000, canBuy=true, canSell=false,
                howToGet="Store",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="Unlimited",
                performance="Tap to turn into a chick; becomes Soondong after 48 hours."
            },

            // Group 3
            new Item{
                id="vitamin_booster", name="Vitamin Booster", priceFP=3000, canBuy=true, canSell=false,
                howToGet="Store",
                giftPermitted=true, giftFunction="Permitted",
                lifetime="During the chicken’s lifetime",
                performance="One chicken only. Birth speed +0.0545"
            },
            new Item{
                id="premium_vitamin_booster", name="Premium Vitamin Booster", priceFP=0, canBuy=false, canSell=false,
                howToGet="Blue Event Egg [Bgg]",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="During the chicken’s lifetime",
                performance="One chicken only. Birth speed +0.16"
            },
            new Item{
                id="champ", name="Champ", priceFP=0, canBuy=false, canSell=false,
                howToGet="From Gold Egg (Ggg)",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="150 days",
                performance="Birth speed +1.04. If you don’t clean/feed for a day, lifespan -1 day."
            },
            new Item{
                id="gold_egg", name="Gold Egg (Ggg)", priceFP=30000, canBuy=true, canSell=false,
                howToGet="Store",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="Unlimited",
                performance="Tap to turn into a chick; becomes Champ after 24 hours."
            },

            // Group 4
            new Item{
                id="prey", name="Prey", priceFP=10, canBuy=true, canSell=false,
                howToGet="Store / Daily check",
                giftPermitted=true, giftFunction="Permitted",
                lifetime="During the chicken’s lifetime",
                performance="Daily food amount a chicken can eat."
            },
            new Item{
                id="premium_prey", name="Premium Prey", priceFP=0, canBuy=false, canSell=false,
                howToGet="Blue Event Egg [Bgg]",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="During the chicken’s lifetime",
                performance="Daily food amount + Birth speed +0.02"
            },
            new Item{
                id="legend_coco", name="Legend Coco", priceFP=0, canBuy=false, canSell=false,
                howToGet="Red Event Egg [Rgg]",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="30 days",
                performance="Your farm: Birth speed +0.3; friends’ farm: +0.05"
            },
            new Item{
                id="event_blue_egg", name="Event Blue Egg (Bgg)", priceFP=6000, canBuy=true, canSell=false,
                howToGet="Store",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="Unlimited",
                performance="Tap to get premium items."
            },

            // Group 5
            new Item{
                id="farm_robot", name="Farm Management Robot", priceFP=100000, canBuy=true, canSell=false,
                howToGet="Store",
                giftPermitted=true, giftFunction="Permitted",
                lifetime="No change after one use",
                performance="Once set, cannot be moved. Operates while battery lasts. Performs daily duties automatically."
            },
            new Item{
                id="premium_farm_open_key", name="Premium Farm Open Key", priceFP=0, canBuy=false, canSell=false,
                howToGet="Blue Event Egg [Bgg]",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="No change after one use",
                performance="Opens a farm with 100 premium nests on 100 tiles."
            },
            new Item{
                id="super_legend_coco", name="Super Legend Coco", priceFP=0, canBuy=false, canSell=false,
                howToGet="Red Event Egg [Rgg]",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="30 days",
                performance="Your farm: Birth speed +1.0; friends’ farm: +0.1"
            },
            new Item{
                id="event_red_egg", name="Event Red Egg (Rgg)", priceFP=20000, canBuy=true, canSell=false,
                howToGet="Store",
                giftPermitted=false, giftFunction="Not permitted",
                lifetime="Unlimited",
                performance="Tap to get Soondong, Champ, Legend Coco, and Super Legend Coco."
            },
        };

        RebuildIndex();
    }
}
