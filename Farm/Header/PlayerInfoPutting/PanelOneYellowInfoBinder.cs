using UnityEngine;
using TMPro;
using System.Text;

public class PanelOneYellowInfoBinder : MonoBehaviour
{
    [Header("Source (assign your PlayerWallet)")]
    public PlayerWallet wallet;

    [Header("Optional item IDs")]
    public string whiteChickId = "whiteChick";
    public string champChickId = "champChick";

    [Header("Outputs (TMP only)")]
    public TMP_Text friendsNumTMP;
    public TMP_Text farmTMP;
    public TMP_Text userFarmsTMP;   // <--- NEW
    public TMP_Text whiteChickTMP;
    public TMP_Text champChickTMP;
    public TMP_Text siilverEggTMP; 
    public TMP_Text goldEggTMP;

    const int MaxDigits = 8;

    void OnEnable()
    {
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (wallet != null) wallet.OnProfileChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (wallet != null) wallet.OnProfileChanged -= Refresh;
    }

    public void Refresh()
    {
        if (!wallet) return;

        SetDigits(friendsNumTMP, wallet.Friends);
        SetDigits(farmTMP, wallet.Farms);

        // Get user farm count — depends on your PlayerWallet structure
        SetDigits(userFarmsTMP, wallet.UserFarms);   // <--- NEW LINE (replace with your real property)

        SetDigits(whiteChickTMP, wallet.GetItemCount(whiteChickId));
        SetDigits(champChickTMP, wallet.GetItemCount(champChickId));

        SetDigits(siilverEggTMP, wallet.SilverEgg);
        SetDigits(goldEggTMP, wallet.GoldEgg);
    }

    // --- Helpers ---
    static void SetDigits(TMP_Text tmp, int value)
    {
        if (!tmp) return;
        tmp.text = ClampDigits(value.ToString());
    }

    static string ClampDigits(string s)
    {
        if (string.IsNullOrEmpty(s)) return "0";

        var sb = new StringBuilder();
        foreach (char c in s)
            if (char.IsDigit(c)) sb.Append(c);

        string digits = sb.Length == 0 ? "0" : sb.ToString();

        if (digits.Length <= MaxDigits) return digits;

        return digits.Substring(0, MaxDigits - 2) + "..";
    }
}
