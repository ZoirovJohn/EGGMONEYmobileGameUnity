using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] int fp = 10000;   // starting money
    public int FP => fp;
    public event Action<int> OnChanged;

    public bool Has(int amount) => fp >= amount;

    public bool TrySpend(int amount)
    {
        if (!Has(amount)) return false;
        fp -= amount;
        OnChanged?.Invoke(fp);
        return true;
    }

    public void Add(int amount)
    {
        fp += amount;
        OnChanged?.Invoke(fp);
    }
}
