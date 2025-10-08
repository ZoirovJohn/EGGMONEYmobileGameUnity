using UnityEngine;
using System;

public class FarmStats : MonoBehaviour
{
    [Header("Farm — resources currently on farms (not inventory)")]
    [SerializeField, Min(0)] int farmSilverEgg = 0;
    [SerializeField, Min(0)] int farmGoldEgg = 0;
    [SerializeField, Min(0)] int farmRedEventEgg = 0;
    [SerializeField, Min(0)] int farmBlueEventEgg = 0;
    [SerializeField, Min(0)] int nestsOnFarms = 0;

    [Header("Referral / Coco tiers (fill if you track them)")]
    [SerializeField, Min(0)] int chickensCoco = 0;
    [SerializeField, Min(0)] int champsCoco = 0;
    [SerializeField, Min(0)] int legendCoco = 0;
    [SerializeField, Min(0)] int superLegendCoco = 0;

    // Getters
    public int FarmSilverEgg      => farmSilverEgg;
    public int FarmGoldEgg        => farmGoldEgg;
    public int FarmRedEventEgg    => farmRedEventEgg;
    public int FarmBlueEventEgg   => farmBlueEventEgg;
    public int NestsOnFarms       => nestsOnFarms;

    public int ChickensCoco       => chickensCoco;
    public int ChampsCoco         => champsCoco;
    public int LegendCoco         => legendCoco;
    public int SuperLegendCoco    => superLegendCoco;

    // Events (id, newValue)
    public event Action<string,int> OnFarmStatChanged;

    // --- Mutators (call these when farm state changes) ---
    public void SetFarmSilverEgg     (int v) => Set(ref farmSilverEgg, v, "farm_silver_egg");
    public void SetFarmGoldEgg       (int v) => Set(ref farmGoldEgg, v, "farm_gold_egg");
    public void SetFarmRedEventEgg   (int v) => Set(ref farmRedEventEgg, v, "farm_red_event_egg");
    public void SetFarmBlueEventEgg  (int v) => Set(ref farmBlueEventEgg, v, "farm_blue_event_egg");
    public void SetNestsOnFarms      (int v) => Set(ref nestsOnFarms, v, "nests_on_farms");

    public void SetChickensCoco      (int v) => Set(ref chickensCoco, v, "chickens_coco");
    public void SetChampsCoco        (int v) => Set(ref champsCoco, v, "champs_coco");
    public void SetLegendCoco        (int v) => Set(ref legendCoco, v, "legend_coco");
    public void SetSuperLegendCoco   (int v) => Set(ref superLegendCoco, v, "super_legend_coco");

    void Set(ref int field, int value, string id)
    {
        int nv = Mathf.Max(0, value);
        if (nv == field) return;
        field = nv;
        OnFarmStatChanged?.Invoke(id, nv);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        farmSilverEgg    = Mathf.Max(0, farmSilverEgg);
        farmGoldEgg      = Mathf.Max(0, farmGoldEgg);
        farmRedEventEgg  = Mathf.Max(0, farmRedEventEgg);
        farmBlueEventEgg = Mathf.Max(0, farmBlueEventEgg);
        nestsOnFarms     = Mathf.Max(0, nestsOnFarms);

        chickensCoco     = Mathf.Max(0, chickensCoco);
        champsCoco       = Mathf.Max(0, champsCoco);
        legendCoco       = Mathf.Max(0, legendCoco);
        superLegendCoco  = Mathf.Max(0, superLegendCoco);
    }
#endif
}
