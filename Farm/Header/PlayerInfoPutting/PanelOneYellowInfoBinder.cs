using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System;

public class PanelOneYellowInfoBinder : MonoBehaviour
{
    [Header("Source (assign your PlayerWallet)")]
    public PlayerWallet wallet;

    [Header("Optional item IDs (for chickens via wallet.GetItemCount)")]
    [Tooltip("Inventory id name for white chick (uses PlayerWallet.GetItemCount).")]
    public string whiteChickId = "white_chick";
    [Tooltip("Inventory id name for champion chick (uses PlayerWallet.GetItemCount).")]
    public string champChickId = "champ_chick";

    [Header("Outputs (assign one per field: either TMP_Text or legacy Text)")]
    public TMP_Text friendsNumTMP;   public Text friendsNumUI;
    public TMP_Text farmTMP;         public Text farmUI;
    public TMP_Text whiteChickTMP;   public Text whiteChickUI;
    public TMP_Text champChickTMP;   public Text champChickUI;
    public TMP_Text siilverEggTMP;   public Text siilverEggUI;  // spelling kept as requested
    public TMP_Text goldEggTMP;      public Text goldEggUI;

    const int MaxDigits = 8; // clamp to 8 digits, then add ".." if longer

    void OnEnable()
    {
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (wallet != null)
        {
            wallet.OnProfileChanged += Refresh;
        }
        Refresh();
    }

    void OnDisable()
    {
        if (wallet != null)
        {
            wallet.OnProfileChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (!wallet) return;

        // Friends / Farms directly from wallet
        SetDigits(friendsNumTMP, friendsNumUI, wallet.Friends);
        SetDigits(farmTMP,       farmUI,       wallet.Farms);

        // Chick counts via inventory IDs (configure ids if needed)
        int white = wallet.GetItemCount(whiteChickId);
        int champ = wallet.GetItemCount(champChickId);
        SetDigits(whiteChickTMP, whiteChickUI, white);
        SetDigits(champChickTMP, champChickUI, champ);

        // Eggs from wallet
        SetDigits(siilverEggTMP, siilverEggUI, wallet.SilverEgg);
        SetDigits(goldEggTMP,    goldEggUI,    wallet.GoldEgg);
    }

    // ---------- helpers ----------
    static void SetDigits(TMP_Text tmp, Text ui, int value)
        => SetTextClamped(tmp, ui, ClampDigits(value.ToString()));

    static string ClampDigits(string s)
    {
        if (string.IsNullOrEmpty(s)) return "0";
        // keep only digits
        var sb = new StringBuilder(s.Length);
        foreach (char c in s) if (char.IsDigit(c)) sb.Append(c);
        var digits = sb.Length == 0 ? "0" : sb.ToString();

        if (digits.Length <= MaxDigits) return digits;
        if (MaxDigits <= 2) return "..";
        return digits.Substring(0, MaxDigits - 2) + "..";
    }

    static void SetTextClamped(TMP_Text tmp, Text ui, string value)
    {
        if (tmp) { tmp.text = value; return; }
        if (ui)  { ui.supportRichText = true; ui.text = value; return; }
        // nothing assigned -> do nothing
    }
}
