using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Globalization;
using System;

public class PanelOneBlueInfoBinder : MonoBehaviour
{
    [Header("Source (assign your PlayerWallet)")]
    public PlayerWallet wallet;

    [Header("Outputs (assign one per field: either TMP_Text or Text)")]
    public TMP_Text nameTMP;     public Text nameUI;
    public TMP_Text levelTMP;    public Text levelUI;
    public TMP_Text locationTMP; public Text locationUI;
    public TMP_Text rankTMP;     public Text rankUI;
    public TMP_Text eggTMP;      public Text eggUI;
    public TMP_Text fpTMP;       public Text fpUI;

    // Limits
    const int MaxNameLen     = 7;   // Name
    const int MaxLocationLen = 7;   // Location
    const int MaxLevelDigits = 8;   // Level
    const int MaxRankDigits  = 8;   // Rank
    const int MaxEggDigits   = 18;  // Egg
    const int MaxFpDigits    = 18;  // FP

    // cached handlers for clean unsubscribe
    Action<int> fpHandler;

    void OnEnable()
    {
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);

        if (wallet)
        {
            fpHandler = OnFPChanged;
            wallet.OnFPChanged += fpHandler;
            wallet.OnProfileChanged += Refresh;
        }

        Refresh();
    }

    void OnDisable()
    {
        if (wallet)
        {
            if (fpHandler != null) wallet.OnFPChanged -= fpHandler;
            wallet.OnProfileChanged -= Refresh;
        }
    }

    void OnFPChanged(int _) => Refresh();

    public void Refresh()
    {
        if (!wallet) return;

        SetName(wallet.Name);
        SetLevel(wallet.Level.ToString(CultureInfo.InvariantCulture));
        SetLocation(wallet.Location);
        SetRank(wallet.Ranking.ToString(CultureInfo.InvariantCulture));
        SetEgg(wallet.Eggs.ToString(CultureInfo.InvariantCulture));
        SetFp(wallet.FP.ToString(CultureInfo.InvariantCulture));
    }

    // setters -> clamp -> write
    public void SetName(string v)     => SetTextClamped(nameTMP, nameUI, ClampText(v, MaxNameLen));
    public void SetLevel(string d)    => SetTextClamped(levelTMP, levelUI, ClampDigits(d, MaxLevelDigits));
    public void SetLocation(string v) => SetTextClamped(locationTMP, locationUI, ClampText(v, MaxLocationLen));
    public void SetRank(string d)     => SetTextClamped(rankTMP, rankUI, ClampDigits(d, MaxRankDigits));
    public void SetEgg(string d)      => SetTextClamped(eggTMP, eggUI, ClampDigits(d, MaxEggDigits));
    public void SetFp(string d)       => SetTextClamped(fpTMP, fpUI, ClampDigits(d, MaxFpDigits));

    static string ClampText(string s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max) return s ?? "";
        if (max <= 2) return "..";
        return s.Substring(0, max - 2) + ".."; // e.g., "Asdasdasd" -> "Asdas.."
    }

    static string ClampDigits(string s, int maxDigits)
    {
        if (string.IsNullOrEmpty(s)) return "0";
        var sb = new StringBuilder(s.Length);
        foreach (char c in s) if (char.IsDigit(c)) sb.Append(c);
        var digits = sb.Length == 0 ? "0" : sb.ToString();
        if (digits.Length <= maxDigits) return digits;
        if (maxDigits <= 2) return "..";
        return digits.Substring(0, maxDigits - 2) + "..";
    }

    static void SetTextClamped(TMP_Text tmp, Text ui, string value)
    {
        if (tmp) { tmp.text = value; return; }
        if (ui)  { ui.supportRichText = true; ui.text = value; return; }
    }
}
