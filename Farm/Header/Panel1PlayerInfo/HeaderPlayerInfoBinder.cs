using UnityEngine;
using TMPro;                 // if you use legacy Text, see TrySetText()
using UnityEngine.UI;

public class HeaderPlayerInfoBinder : MonoBehaviour
{
    [Header("Where the two columns live")]
    [SerializeField] Transform playerInfoLeft;   // headerPanel/Panel_PlayerInfo/PlayerInfo_Left
    [SerializeField] Transform playerInfoRight;  // headerPanel/Panel_PlayerInfo/PlayerInfo_Right

    [Header("Source")]
    [SerializeField] PlayerWallet wallet;        // drag your PlayerWallet (or auto-find)

    [Header("Output style")]
    [SerializeField] bool includeLabelPrefixes = true; // "Name: Edwin" (true) vs "Edwin" (false)

    void OnEnable()
    {
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        Refresh();

        // live updates
        if (wallet != null)
        {
            wallet.OnFPChanged += _ => Refresh();
            wallet.OnProfileChanged += Refresh;
        }
    }

    void OnDisable()
    {
        if (wallet != null)
        {
            wallet.OnFPChanged -= _ => Refresh();     // unsubscribe safely if you used lambdas elsewhere
            wallet.OnProfileChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (!wallet) return;

        // LEFT column
        SetField(playerInfoLeft, "Name",    includeLabelPrefixes ? $"Name: {wallet.Name}"        : $"{wallet.Name}");
        SetField(playerInfoLeft, "LV",      includeLabelPrefixes ? $"LV: {wallet.Level:N0}"      : $"{wallet.Level:N0}");
        SetField(playerInfoLeft, "Farms",   includeLabelPrefixes ? $"Farms: {wallet.Farms:N0}"   : $"{wallet.Farms:N0}");
        SetField(playerInfoLeft, "Friends", includeLabelPrefixes ? $"Friends: {wallet.Friends:N0}" : $"{wallet.Friends:N0}");

        // RIGHT column
        SetField(playerInfoRight, "Location", includeLabelPrefixes ? $"Location: {wallet.Location}" : $"{wallet.Location}");
        SetField(playerInfoRight, "Ranking",  includeLabelPrefixes ? $"Ranking: {wallet.Ranking:N0}" : $"{wallet.Ranking:N0}");
        SetField(playerInfoRight, "FP",       includeLabelPrefixes ? $"FP: {wallet.FP:N0}" : $"{wallet.FP:N0}");
        SetField(playerInfoRight, "Egg",      includeLabelPrefixes ? $"Egg: {wallet.Eggs:N0}" : $"{wallet.Eggs:N0}");
    }

    // --- helpers ---

    void SetField(Transform root, string childName, string text)
    {
        if (!root) return;

        // exact child first
        var t = root.Find(childName);
        if (TrySetText(t, text)) return;

        // search deeper by name
        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            if (tr.name == childName && TrySetText(tr, text)) return;

        Debug.LogWarning($"[HeaderPlayerInfoBinder] '{childName}' not found under '{root.name}'.");
    }

    bool TrySetText(Transform t, string text)
    {
        if (!t) return false;

        var tmp = t.GetComponent<TMP_Text>();
        if (tmp) { tmp.text = text; return true; }

        var ui = t.GetComponent<Text>();
        if (ui)  { ui.supportRichText = true; ui.text = text; return true; }

        // sometimes the container holds the text on a child
        tmp = t.GetComponentInChildren<TMP_Text>(true);
        if (tmp) { tmp.text = text; return true; }
        ui = t.GetComponentInChildren<Text>(true);
        if (ui)  { ui.supportRichText = true; ui.text = text; return true; }

        return false;
    }
}
