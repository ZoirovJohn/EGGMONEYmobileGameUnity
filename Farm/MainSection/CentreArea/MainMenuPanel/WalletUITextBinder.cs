using UnityEngine;
using TMPro;
using System.Globalization;

public class WalletUITextBinder : MonoBehaviour
{
    [SerializeField] private PlayerWallet wallet;

    [Header("Texts")]
    [SerializeField] private TMP_Text sggText;
    [SerializeField] private TMP_Text gggText;
    [SerializeField] private TMP_Text champText;
    [SerializeField] private TMP_Text soondongText;
    [SerializeField] private TMP_Text birthEggText;
    [SerializeField] private TMP_Text rggText;

    private void Start()
    {
        Refresh();
        wallet.OnProfileChanged += Refresh;
        wallet.OnItemChanged += (_, __) => Refresh();
    }

    private void Refresh()
    {
        sggText.text        = $"Sgg: {FormatWalletValue(wallet.SilverEgg)}";
        gggText.text        = $"Ggg: {FormatWalletValue(wallet.GoldEgg)}";
        champText.text      = $"Champ: {FormatWalletValue(wallet.ChampChick)}";
        soondongText.text   = $"Soondong: {FormatWalletValue(wallet.WhiteChick)}";
        birthEggText.text  = $"Birth Egg: {FormatWalletValue(wallet.Eggs)}";
        rggText.text        = $"Rgg: {FormatWalletValue(wallet.SuperRedEgg)}";
    }

    private void OnDestroy()
    {
        wallet.OnProfileChanged -= Refresh;
        wallet.OnItemChanged -= (_, __) => Refresh();
    }

    static string FormatWalletValue(long value)
    {
        if (value > 999)
            return "999+";

        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

}
