using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatusPanelUI : MonoBehaviour
{
    [Header("Data Sources")]
    [SerializeField] PlayerWallet wallet;    // FP, Farms, Collected Eggs
    [SerializeField] FarmStats farmStats;    // Farm-side counts

    readonly Dictionary<string, TMP_Text> num = new();

    void Awake()
    {
        if (!wallet) wallet = FindFirstObjectByType<PlayerWallet>();
        if (!farmStats) farmStats = FindFirstObjectByType<FarmStats>();

        // right-side Inventory diagram numbers
        CacheNum("FpNum");
        CacheNum("CollectedEggNum");
        CacheNum("FarmSilverEggNum");
        CacheNum("FarnGoldEggNum");
        CacheNum("FarmRedEventEggNum");
        CacheNum("FarmBlueEventEggNum");
        CacheNum("ReferralProfNum");

        // right-side Referral diagram numbers
        CacheNum("FarmNum");
        CacheNum("NestOnFarmsNum");
        CacheNum("ChickensCocoNum");
        CacheNum("ChampsCocoNum");
        CacheNum("LegendCocoNum");
        CacheNum("SuperLegendCocoNum");
    }

    void OnEnable()
    {
        if (wallet != null)
        {
            wallet.OnFPChanged += HandleFPChanged;
            wallet.OnProfileChanged += HandleProfileChanged;
        }
        if (farmStats != null)
            farmStats.OnFarmStatChanged += HandleFarmStatChanged;
    }

    void Start() => PaintAll();

    void OnDisable()
    {
        if (wallet != null)
        {
            wallet.OnFPChanged -= HandleFPChanged;
            wallet.OnProfileChanged -= HandleProfileChanged;
        }
        if (farmStats != null)
            farmStats.OnFarmStatChanged -= HandleFarmStatChanged;
    }

    // ---------- paint ----------

    void PaintAll()
    {
        // wallet-driven
        if (wallet != null)
        {
            SetNum("FpNum",           wallet.FP);
            SetNum("CollectedEggNum", wallet.Eggs);   // collected eggs
            SetNum("FarmNum",         wallet.Farms);
            // If you temporarily mirror referral profit to FP, keep this:
            SetNum("ReferralProfNum", wallet.FP);
        }

        // farm-driven
        if (farmStats != null)
        {
            SetNum("FarmSilverEggNum",    farmStats.FarmSilverEgg);
            SetNum("FarnGoldEggNum",      farmStats.FarmGoldEgg);
            SetNum("FarmRedEventEggNum",  farmStats.FarmRedEventEgg);
            SetNum("FarmBlueEventEggNum", farmStats.FarmBlueEventEgg);
            SetNum("NestOnFarmsNum",      farmStats.NestsOnFarms);

            SetNum("ChickensCocoNum",     farmStats.ChickensCoco);
            SetNum("ChampsCocoNum",       farmStats.ChampsCoco);
            SetNum("LegendCocoNum",       farmStats.LegendCoco);
            SetNum("SuperLegendCocoNum",  farmStats.SuperLegendCoco);
        }
    }

    void HandleFPChanged(int newFP)
    {
        SetNum("FpNum", newFP);
        // (optional) keep ReferralProfNum tied to FP for now
        SetNum("ReferralProfNum", newFP);
    }

    void HandleProfileChanged()
    {
        // wallet-side: farms and collected eggs might change
        if (wallet == null) return;
        SetNum("CollectedEggNum", wallet.Eggs);
        SetNum("FarmNum",         wallet.Farms);
    }

    void HandleFarmStatChanged(string id, int newValue)
    {
        id = (id ?? "").Trim().ToLowerInvariant();
        switch (id)
        {
            case "farm_silver_egg":   SetNum("FarmSilverEggNum",    newValue); break;
            case "farm_gold_egg":     SetNum("FarnGoldEggNum",      newValue); break;
            case "farm_red_event_egg":SetNum("FarmRedEventEggNum",  newValue); break;
            case "farm_blue_event_egg":SetNum("FarmBlueEventEggNum",newValue); break;
            case "nests_on_farms":    SetNum("NestOnFarmsNum",      newValue); break;

            case "chickens_coco":     SetNum("ChickensCocoNum",     newValue); break;
            case "champs_coco":       SetNum("ChampsCocoNum",       newValue); break;
            case "legend_coco":       SetNum("LegendCocoNum",       newValue); break;
            case "super_legend_coco": SetNum("SuperLegendCocoNum",  newValue); break;
        }
    }

    // ---------- helpers ----------

    void CacheNum(string childName)
    {
        var t = FindDeep(childName);
        var txt = t ? t.GetComponent<TMP_Text>() : null;
        if (txt) num[childName] = txt;
        else Debug.LogWarning($"[StatusPanelUI] Missing TMP_Text '{childName}'");
    }

    Transform FindDeep(string childName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == childName) return t;
        return null;
    }

    void SetNum(string key, int value)
    {
        if (!num.TryGetValue(key, out var txt) || !txt) return;
        txt.text = value.ToString("N0"); // 1,234 formatting
    }
}
