using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    // =========================
    // Profile
    // =========================
    [Header("Profile (loaded from backend)")]
    [SerializeField] string playerName = "playerName";
    [SerializeField, Min(1)] int level = 1;
    [SerializeField] string location = "location";
    [SerializeField, Min(0)] int ranking = 0;
    [SerializeField, Min(0)] int eggs = 0;
    [SerializeField, Min(0)] int video = 0;
    [SerializeField, Min(0)] int userFarms = 0;

    // =========================
    // Wallet
    // =========================
    [Header("Wallet")]
    [SerializeField, Min(0)] int fp = 0;

    // =========================
    // Social / Farms / Chickens (Yellow Panel)
    // =========================
    [Header("Social / Farms / Chickens (Yellow Panel)")]
    [SerializeField, Min(0)] int friends = 0;
    [SerializeField, Min(0)] int farms = 0;
    [SerializeField, Min(0)] int chick = 0;
    [SerializeField, Min(0)] int whiteChick = 0;
    [SerializeField, Min(0)] int champChick = 0;
    [SerializeField, Min(0)] int silverEgg = 0;
    [SerializeField, Min(0)] int goldEgg = 0;

    // =========================
    // Inventory
    // =========================
    [Header("Inventory (counts)")]
    [SerializeField, Min(0)] int nest = 0;
    [SerializeField, Min(0)] int food = 0;
    [SerializeField, Min(0)] int vitamin = 0;
    [SerializeField, Min(0)] int battery = 0;
    [SerializeField, Min(0)] int keyFarm = 0;
    [SerializeField, Min(0)] int robot = 0;
    [SerializeField, Min(0)] int superFood = 0;
    [SerializeField, Min(0)] int superVitamin = 0;
    [SerializeField, Min(0)] int superBattery = 0;
    [SerializeField, Min(0)] int superBlueEgg = 0;
    [SerializeField, Min(0)] int superRedEgg = 0;
    [SerializeField, Min(0)] int superFarmKey = 0;
    [SerializeField, Min(0)] int superNest = 0;

    // =========================
    // Public getters
    // =========================
    public string Name => playerName;
    public int Level => level;
    public string Location => location;
    public int Ranking => ranking;
    public int Eggs => eggs;
    public int Video => video;
    public int UserFarms => userFarms;
    public int FP => fp;

    public int Friends => friends;
    public int Farms => farms;
    public int Chick => chick;
    public int WhiteChick => whiteChick;
    public int ChampChick => champChick;
    public int SilverEgg => silverEgg;
    public int GoldEgg => goldEgg;

    public int Nest => nest;
    public int Food => food;
    public int Vitamin => vitamin;
    public int Battery => battery;
    public int KeyFarm => keyFarm;
    public int Robot => robot;
    public int SuperFood => superFood;
    public int SuperVitamin => superVitamin;
    public int SuperBattery => superBattery;
    public int SuperBlueEgg => superBlueEgg;
    public int SuperRedEgg => superRedEgg;
    public int SuperFarmKey => superFarmKey;
    public int SuperNest => superNest;

    // =========================
    // Events
    // =========================
    public event Action<int> OnFPChanged;
    public event Action OnProfileChanged;
    public event Action<string,int> OnItemChanged;

    void Awake()
    {
        Debug.Log($"💰 PlayerWallet Awake() - Instance ID: {GetInstanceID()}");
    }

    // =========================
    // FP operations
    // =========================
    public bool Has(int amount) => amount <= fp;

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || amount > fp) return false;
        fp -= amount;
        Debug.Log($"💸 TrySpend: Spent {amount} FP, new balance: {fp}");
        OnFPChanged?.Invoke(fp);
        return true;
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        fp += amount;
        Debug.Log($"💰 Add: Added {amount} FP, new balance: {fp}");
        OnFPChanged?.Invoke(fp);
    }

    public void SetFP(int newFP)
    {
        newFP = Mathf.Max(0, newFP);
        if (fp == newFP) return;
        fp = newFP;
        Debug.Log($"💰 SetFP: Set to {fp} FP");
        OnFPChanged?.Invoke(fp);
    }

    // =========================
    // Eggs / Level
    // =========================
    public bool TrySpendEggs(int amount)
    {
        if (amount <= 0 || amount > eggs) return false;
        eggs -= amount;
        OnProfileChanged?.Invoke();
        return true;
    }

    public void AddEggs(int amount)
    {
        if (amount <= 0) return;
        eggs += amount;
        OnProfileChanged?.Invoke();
    }

    public void SetEggs(int newEggs)
    {
        newEggs = Mathf.Max(0, newEggs);
        if (eggs == newEggs) return;
        eggs = newEggs;
        OnProfileChanged?.Invoke();
    }

    public void LevelUp(int by = 1)
    {
        int nl = Mathf.Max(1, level + Mathf.Max(1, by));
        if (nl == level) return;
        level = nl;
        OnProfileChanged?.Invoke();
    }

    public void SetLevel(int newLevel)
    {
        newLevel = Mathf.Max(1, newLevel);
        if (level == newLevel) return;
        level = newLevel;
        OnProfileChanged?.Invoke();
    }

    // =========================
    // Profile setters
    // =========================
    public void SetName(string newName)
    {
        if (string.IsNullOrEmpty(newName) || playerName == newName) return;
        playerName = newName;
        OnProfileChanged?.Invoke();
    }

    public void SetLocation(string newLocation)
    {
        if (string.IsNullOrEmpty(newLocation) || location == newLocation) return;
        location = newLocation;
        OnProfileChanged?.Invoke();
    }

    public void SetRanking(int newRank)
    {
        newRank = Mathf.Max(0, newRank);
        if (ranking == newRank) return;
        ranking = newRank;
        OnProfileChanged?.Invoke();
    }

    public void SetVideo(int newVideo)
    {
        newVideo = Mathf.Max(0, newVideo);
        if (video == newVideo) return;
        video = newVideo;
        OnProfileChanged?.Invoke();
    }

    public void SetUserFarms(int newUserFarms)
    {
        newUserFarms = Mathf.Max(0, newUserFarms);
        if (userFarms == newUserFarms) return;
        userFarms = newUserFarms;
        Debug.Log($"🚜 SetUserFarms: Set to {userFarms} farms");
        OnProfileChanged?.Invoke();
    }

    // =========================
    // Yellow panel setters
    // =========================
    public void SetFriends(int v) 
    { 
        v = Mathf.Max(0, v); 
        if (friends == v) return; 
        friends = v; 
        OnProfileChanged?.Invoke(); 
    }

    public void SetFarms(int v) 
    { 
        v = Mathf.Max(0, v); 
        if (farms == v) return; 
        farms = v; 
        OnProfileChanged?.Invoke(); 
    }

    public void SetChick(int v) 
    { 
        v = Mathf.Max(0, v); 
        if (chick == v) return; 
        chick = v;
        Debug.Log($"🐣 SetChick: {v} (Firing OnItemChanged)");
        OnItemChanged?.Invoke("chick", v); 
        OnProfileChanged?.Invoke(); 
    }

    public void SetWhiteChick(int v) 
    { 
        v = Mathf.Max(0, v); 
        if (whiteChick == v) return; 
        whiteChick = v;
        Debug.Log($"🐔 SetWhiteChick: {v} (Firing OnItemChanged)");
        OnItemChanged?.Invoke("whiteChick", v); 
        OnProfileChanged?.Invoke(); 
    }

    public void SetChampChick(int v) 
    { 
        v = Mathf.Max(0, v); 
        if (champChick == v) return; 
        champChick = v;
        Debug.Log($"🏆 SetChampChick: {v} (Firing OnItemChanged)");
        OnItemChanged?.Invoke("champChick", v); 
        OnProfileChanged?.Invoke(); 
    }

    public void SetSilverEgg(int v) 
    { 
        v = Mathf.Max(0, v); 
        if (silverEgg == v) return; 
        silverEgg = v;
        Debug.Log($"🥚 SetSilverEgg: {v} (Firing OnItemChanged)");
        OnItemChanged?.Invoke("silver_egg", v); 
        OnProfileChanged?.Invoke(); 
    }

    public void SetGoldEgg(int v) 
    { 
        v = Mathf.Max(0, v); 
        if (goldEgg == v) return; 
        goldEgg = v;
        Debug.Log($"🌟 SetGoldEgg: {v} (Firing OnItemChanged)");
        OnItemChanged?.Invoke("gold_egg", v); 
        OnProfileChanged?.Invoke(); 
    }

    public void SetSuperFarmKey(int v) 
    { 
        v = Mathf.Max(0, v); 
        if (superFarmKey == v) return; 
        superFarmKey = v;
        Debug.Log($"🔑 SetSuperFarmKey: {v} (Firing OnItemChanged)");
        OnItemChanged?.Invoke("super_farm_key", v); 
        OnProfileChanged?.Invoke(); 
    }

    // =========================
    // Inventory
    // =========================
    public int GetItemCount(string id)
    {
        switch (MapId(id))
        {
            case Item.Nest: return nest;
            case Item.SilverEgg: return silverEgg;
            case Item.Food: return food;
            case Item.GoldEgg: return goldEgg;
            case Item.vitamin: return vitamin;
            case Item.Battery: return battery;
            case Item.KeyFarm: return keyFarm;
            case Item.Robot: return robot;
            case Item.SuperFood: return superFood;
            case Item.SuperVitamin: return superVitamin;
            case Item.SuperBattery: return superBattery;
            case Item.SuperBlueEgg: return superBlueEgg;
            case Item.SuperRedEgg: return superRedEgg;
            case Item.Chick: return chick;
            case Item.WhiteChick: return whiteChick;
            case Item.ChampChick: return champChick;
            case Item.SuperFarmKey: return superFarmKey;
            case Item.SuperNest: return superNest;
            default: return 0;
        }
    }

    public void AddItem(string id, int amount)
    {
        if (amount <= 0) return;
        var key = MapId(id);
        int v = Mathf.Max(0, GetItemCount(id) + amount);
        Debug.Log($"📦 AddItem: {id} +{amount} → {v} (Calling SetItemCount)");
        SetItemCount(key, v);
    }

    public bool TryConsumeItem(string id, int amount)
    {
        if (amount <= 0) return false;
        var key = MapId(id);
        int cur = GetItemCount(id);
        if (cur < amount) return false;
        Debug.Log($"📤 TryConsumeItem: {id} -{amount} → {cur - amount} (Calling SetItemCount)");
        SetItemCount(key, cur - amount);
        return true;
    }

    // =========================
    // Internal
    // =========================
    enum Item
    {
        Unknown,
        Nest, SilverEgg, Food, GoldEgg, vitamin, Battery, KeyFarm, Robot,
        SuperFood, SuperVitamin, SuperBattery, SuperBlueEgg, SuperRedEgg,
        Chick, WhiteChick, ChampChick,
        SuperFarmKey, SuperNest
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

        if (n == "nest") return Item.Nest;
        if (n == "silveregg" || n == "silvere") return Item.SilverEgg;
        if (n == "food" || n == "prey") return Item.Food;
        if (n == "goldegg" || n == "goldenegg" || n == "ggg") return Item.GoldEgg;
        if (n == "vitamin" || n == "vitaminvitamin" || n == "booster") return Item.vitamin;
        if (n == "battery") return Item.Battery;
        if (n == "keyfarm" || n == "farmopenkey" || n == "premiumfarmopenkey" || n == "farmkey") return Item.KeyFarm;
        if (n == "robot" || n == "farmmanagementrobot") return Item.Robot;
        if (n == "superfood") return Item.SuperFood;
        if (n == "supernest") return Item.SuperNest;
        if (n == "supervitamin") return Item.SuperVitamin;
        if (n == "superbattery") return Item.SuperBattery;
        if (n == "superblueegg" || n == "bgg" || n == "eventblueegg") return Item.SuperBlueEgg;
        if (n == "superredegg" || n == "rgg" || n == "eventredegg") return Item.SuperRedEgg;
        if (n == "chick") return Item.Chick;
        if (n == "whitechick") return Item.WhiteChick;
        if (n == "champchick" || n == "championchick") return Item.ChampChick;
        if (n == "superfarmkey" || n == "premiumfarmkey") return Item.SuperFarmKey;

        return Item.Unknown;
    }

    void SetItemCount(Item key, int value)
    {
        value = Mathf.Max(0, value);
        switch (key)
        {
            case Item.Nest: nest = value; RaiseItem("nest", value); break;
            case Item.SilverEgg: silverEgg = value; RaiseItem("silver_egg", value); break;
            case Item.Food: food = value; RaiseItem("food", value); break;
            case Item.GoldEgg: goldEgg = value; RaiseItem("gold_egg", value); break;
            case Item.vitamin: vitamin = value; RaiseItem("vitamin", value); break;
            case Item.Battery: battery = value; RaiseItem("battery", value); break;
            case Item.KeyFarm: keyFarm = value; RaiseItem("farmKey", value); break;
            case Item.Robot: robot = value; RaiseItem("robot", value); break;
            case Item.SuperFood: superFood = value; RaiseItem("super_food", value); break;
            case Item.SuperNest: superNest = value; RaiseItem("super_nest", value); break;
            case Item.SuperVitamin: superVitamin = value; RaiseItem("super_vitamin", value); break;
            case Item.SuperBattery: superBattery = value; RaiseItem("super_battery", value); break;
            case Item.SuperBlueEgg: superBlueEgg = value; RaiseItem("super_blue_egg", value); break;
            case Item.SuperRedEgg: superRedEgg = value; RaiseItem("super_red_egg", value); break;
            case Item.Chick: chick = value; RaiseItem("chick", value); break;
            case Item.WhiteChick: whiteChick = value; RaiseItem("white_chick", value); break;
            case Item.ChampChick: champChick = value; RaiseItem("champ_chick", value); break;
            case Item.SuperFarmKey: superFarmKey = value; RaiseItem("super_farm_key", value); break;
            default: return;
        }
        OnProfileChanged?.Invoke();
    }

    void RaiseItem(string id, int value)
    {
        Debug.Log($"🔔 RaiseItem: Firing OnItemChanged for '{id}' = {value} (Listeners: {(OnItemChanged?.GetInvocationList().Length ?? 0)})");
        OnItemChanged?.Invoke(id, value);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        level = Mathf.Max(1, level);
        ranking = Mathf.Max(0, ranking);
        eggs = Mathf.Max(0, eggs);
        video = Mathf.Max(0, video);
        userFarms = Mathf.Max(0, userFarms);
        fp = Mathf.Max(0, fp);

        friends = Mathf.Max(0, friends);
        farms = Mathf.Max(0, farms);
        chick = Mathf.Max(0, chick);
        whiteChick = Mathf.Max(0, whiteChick);
        champChick = Mathf.Max(0, champChick);
        silverEgg = Mathf.Max(0, silverEgg);
        goldEgg = Mathf.Max(0, goldEgg);

        nest = Mathf.Max(0, nest);
        food = Mathf.Max(0, food);
        vitamin = Mathf.Max(0, vitamin);
        battery = Mathf.Max(0, battery);
        keyFarm = Mathf.Max(0, keyFarm);
        robot = Mathf.Max(0, robot);
        superFood = Mathf.Max(0, superFood);
        superNest = Mathf.Max(0, superNest);
        superVitamin = Mathf.Max(0, superVitamin);
        superBattery = Mathf.Max(0, superBattery);
        superBlueEgg = Mathf.Max(0, superBlueEgg);
        superRedEgg = Mathf.Max(0, superRedEgg);
        superFarmKey = Mathf.Max(0, superFarmKey);
    }
#endif
}