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
    [SerializeField, Min(0)] int fp = 100_000_000; // 100,000,000

    // Public getters
    public string Name => playerName;
    public int Level => level;
    public int Farms => farms;
    public int Friends => friends;
    public string Location => location;
    public int Ranking => ranking;
    public int Eggs => eggs;
    public int FP => fp;

    // Events
    public event Action<int> OnFPChanged;
    public event Action OnProfileChanged;

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

    // --- Eggs / Level helpers (optional) ---
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

    public void LevelUp(int by = 1)
    {
        level = Mathf.Max(1, level + Mathf.Max(1, by));
        OnProfileChanged?.Invoke();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        level   = Mathf.Max(1, level);
        ranking = Mathf.Max(1, ranking);
        eggs    = Mathf.Max(0, eggs);
        farms   = Mathf.Max(0, farms);
        friends = Mathf.Max(0, friends);
        fp      = Mathf.Max(0, fp);
    }
#endif
}
