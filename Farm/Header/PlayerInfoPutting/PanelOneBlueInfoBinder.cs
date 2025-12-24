using UnityEngine;
using TMPro;
using System.Text;
using System.Globalization;
using System;

public class PanelOneBlueInfoBinder : MonoBehaviour
{
    [Header("Source (assign your PlayerWallet)")]
    public PlayerWallet wallet;

    [Header("Outputs (assign TMP_Text fields)")]
    public TMP_Text nameTMP;
    public TMP_Text levelTMP;
    public TMP_Text locationTMP;
    public TMP_Text eggTMP;
    public TMP_Text fpTMP;

    // Limits
    const int MaxNameLen     = 7;   // Name
    const int MaxLocationLen = 7;   // Location
    const int MaxLevelDigits = 8;   // Level
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

    static string FormatWithComma(long value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    public void Refresh()
    {
        if (!wallet) return;

        SetName(wallet.Name);
        SetLevel(wallet.Level.ToString(CultureInfo.InvariantCulture));
        SetLocation(wallet.Location);
        SetEgg(FormatWithComma(wallet.Eggs));
        SetFp(FormatWithComma(wallet.FP));
    }

    // setters -> clamp -> write
    public void SetName(string v)     => SetText(nameTMP, ClampText(v, MaxNameLen));
    public void SetLevel(string d)    => SetText(levelTMP, ClampDigits(d, MaxLevelDigits));
    public void SetLocation(string v) => SetText(locationTMP, ClampText(v, MaxLocationLen));
    public void SetEgg(string v) => SetText(eggTMP, v);
    public void SetFp(string v)  => SetText(fpTMP, v);


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

    static void SetText(TMP_Text tmp, string value)
    {
        if (tmp) tmp.text = value;
    }
}