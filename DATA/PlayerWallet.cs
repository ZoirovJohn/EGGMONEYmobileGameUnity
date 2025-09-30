using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] string playerName = "Edwin";
    [SerializeField, Min(1)] int level = 1;
    [SerializeField, Min(0)] int farms = 5;
    [SerializeField, Min(0)] int friends = 30;
    [SerializeField] string location = "Kor";
    [SerializeField, Min(1)] int ranking = 1;
    [SerializeField, Min(0)] int eggs = 1_000;

    [Header("Wallet")]
    [SerializeField, Min(0)] int fp = 1_000_000; // 100,000,000

    [Header("Inventory (counts)")]
    [SerializeField, Min(0)] int nest = 1;
    [SerializeField, Min(0)] int silverEgg = 1;
    [SerializeField, Min(0)] int food = 1;
    [SerializeField, Min(0)] int goldEgg = 0;
    [SerializeField, Min(0)] int booster = 0;
    [SerializeField, Min(0)] int battery = 0;
    [SerializeField, Min(0)] int keyFarm = 0;
    [SerializeField, Min(0)] int robot = 0;
    [SerializeField, Min(0)] int superFood = 0;
    [SerializeField, Min(0)] int superBooster = 0;
    [SerializeField, Min(0)] int superBattery = 0;
    [SerializeField, Min(0)] int superBlueEgg = 0;
    [SerializeField, Min(0)] int superRedEgg = 0;

    // Public getters
    public string Name => playerName;
    public int Level => level;
    public int Farms => farms;
    public int Friends => friends;
    public string Location => location;
    public int Ranking => ranking;
    public int Eggs => eggs;
    public int FP => fp;

    public int Nest => nest;
    public int SilverEgg => silverEgg;
    public int Food => food;
    public int GoldEgg => goldEgg;
    public int Booster => booster;
    public int Battery => battery;
    public int KeyFarm => keyFarm;
    public int Robot => robot;
    public int SuperFood => superFood;
    public int SuperBooster => superBooster;
    public int SuperBattery => superBattery;
    public int SuperBlueEgg => superBlueEgg;
    public int SuperRedEgg => superRedEgg;

    // Events
    public event Action<int> OnFPChanged;
    public event Action OnProfileChanged;
    public event Action<string,int> OnItemChanged; // (id, newCount)

    // --- FP ops ---
    public bool Has(int amount) => amount <= fp;

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || amount > fp) return false;
        fp -= amount;
        OnFPChanged?.Invoke(fp);
        return true;
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        fp += amount;
        OnFPChanged?.Invoke(fp);
    }

    // --- Eggs / Level helpers ---
    public bool TrySpendEggs(int amount)
    {
        if (amount <= 0 || amount > eggs) return false;
        eggs -= amount; OnProfileChanged?.Invoke(); return true;
    }

    public void AddEggs(int amount)
    {
        if (amount <= 0) return;
        eggs += amount; OnProfileChanged?.Invoke();
    }

    public void LevelUp(int by = 1)
    {
        level = Mathf.Max(1, level + Mathf.Max(1, by));
        OnProfileChanged?.Invoke();
    }

    // --- Inventory API (string id friendly) ---
    public int GetItemCount(string id)
    {
        switch (MapId(id))
        {
            case Item.Nest:           return nest;
            case Item.SilverEgg:      return silverEgg;
            case Item.Food:           return food;
            case Item.GoldEgg:        return goldEgg;
            case Item.Booster:        return booster;
            case Item.Battery:        return battery;
            case Item.KeyFarm:        return keyFarm;
            case Item.Robot:          return robot;
            case Item.SuperFood:      return superFood;
            case Item.SuperBooster:   return superBooster;
            case Item.SuperBattery:   return superBattery;
            case Item.SuperBlueEgg:   return superBlueEgg;
            case Item.SuperRedEgg:    return superRedEgg;
            default: return 0;
        }
    }

    public void AddItem(string id, int amount)
    {
        if (amount <= 0) return;
        var key = MapId(id);
        int v = Mathf.Max(0, GetItemCount(id) + amount);
        SetItemCount(key, v);
    }

    public bool TryConsumeItem(string id, int amount)
    {
        if (amount <= 0) return false;
        var key = MapId(id);
        int cur = GetItemCount(id);
        if (cur < amount) return false;
        SetItemCount(key, cur - amount);
        return true;
    }

    // --- Internal: storage & mapping ---
    enum Item
    {
        Unknown,
        Nest, SilverEgg, Food, GoldEgg, Booster, Battery, KeyFarm, Robot,
        SuperFood, SuperBooster, SuperBattery, SuperBlueEgg, SuperRedEgg
    }

    static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.ToLowerInvariant();
        s = s.Replace(" ", "").Replace("_", "").Replace("-", "");
        return s;
    }

    Item MapId(string id)
    {
        string n = Norm(id);
        // accept both DB ids (e.g. "silver_egg") and human names (e.g. "SilverEgg")
        if (n == "nest") return Item.Nest;
        if (n == "silveregg" || n == "silvere") return Item.SilverEgg;
        if (n == "food" || n == "prey") return Item.Food;
        if (n == "goldegg" || n == "goldenegg" || n == "ggg") return Item.GoldEgg;
        if (n == "booster" || n == "vitaminbooster") return Item.Booster;
        if (n == "battery") return Item.Battery;
        if (n == "keyfarm" || n == "farmopenkey" || n == "premiumfarmopenkey") return Item.KeyFarm;
        if (n == "robot" || n == "farmmanagementrobot") return Item.Robot;
        if (n == "superfood") return Item.SuperFood;
        if (n == "superbooster") return Item.SuperBooster;
        if (n == "superbattery") return Item.SuperBattery;
        if (n == "superblueegg" || n == "bgg" || n == "eventblueegg") return Item.SuperBlueEgg;
        if (n == "superredegg" || n == "rgg" || n == "eventredegg") return Item.SuperRedEgg;
        return Item.Unknown;
    }

    void SetItemCount(Item key, int value)
    {
        switch (key)
        {
            case Item.Nest:           nest = value;          RaiseItem("nest", value); break;
            case Item.SilverEgg:      silverEgg = value;     RaiseItem("silver_egg", value); break;
            case Item.Food:           food = value;          RaiseItem("food", value); break;
            case Item.GoldEgg:        goldEgg = value;       RaiseItem("gold_egg", value); break;
            case Item.Booster:        booster = value;       RaiseItem("booster", value); break;
            case Item.Battery:        battery = value;       RaiseItem("battery", value); break;
            case Item.KeyFarm:        keyFarm = value;       RaiseItem("key_farm", value); break;
            case Item.Robot:          robot = value;         RaiseItem("robot", value); break;
            case Item.SuperFood:      superFood = value;     RaiseItem("super_food", value); break;
            case Item.SuperBooster:   superBooster = value;  RaiseItem("super_booster", value); break;
            case Item.SuperBattery:   superBattery = value;  RaiseItem("super_battery", value); break;
            case Item.SuperBlueEgg:   superBlueEgg = value;  RaiseItem("super_blue_egg", value); break;
            case Item.SuperRedEgg:    superRedEgg = value;   RaiseItem("super_red_egg", value); break;
            default: return;
        }
        OnProfileChanged?.Invoke(); // inventory also counts as profile change
    }

    void RaiseItem(string id, int value) => OnItemChanged?.Invoke(id, value);

#if UNITY_EDITOR
    void OnValidate()
    {
        level   = Mathf.Max(1, level);
        ranking = Mathf.Max(1, ranking);
        eggs    = Mathf.Max(0, eggs);
        farms   = Mathf.Max(0, farms);
        friends = Mathf.Max(0, friends);
        fp      = Mathf.Max(0, fp);

        nest = Mathf.Max(0, nest);
        silverEgg = Mathf.Max(0, silverEgg);
        food = Mathf.Max(0, food);
        goldEgg = Mathf.Max(0, goldEgg);
        booster = Mathf.Max(0, booster);
        battery = Mathf.Max(0, battery);
        keyFarm = Mathf.Max(0, keyFarm);
        robot = Mathf.Max(0, robot);
        superFood = Mathf.Max(0, superFood);
        superBooster = Mathf.Max(0, superBooster);
        superBattery = Mathf.Max(0, superBattery);
        superBlueEgg = Mathf.Max(0, superBlueEgg);
        superRedEgg = Mathf.Max(0, superRedEgg);
    }
#endif
}
