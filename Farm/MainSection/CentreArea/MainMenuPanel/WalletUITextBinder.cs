using System;
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
    [SerializeField] private TMP_Text bggText;

    private Action<string, int> onItemChangedHandler;

    private void Start()
    {
        Refresh();
        onItemChangedHandler = (_, __) => Refresh();
        wallet.OnProfileChanged += Refresh;
        wallet.OnItemChanged += onItemChangedHandler;
    }

    private void Refresh()
    {
        sggText.text        = $"Sgg: {FormatWalletValue(wallet.SilverEgg)}";
        gggText.text        = $"Ggg: {FormatWalletValue(wallet.GoldEgg)}";
        soondongText.text = $"Soondong: {FormatWalletValue(wallet.AllNormalHens)}";
        champText.text   = $"Champ: {FormatWalletValue(wallet.AllChampHens)}";
        birthEggText.text  = $"Birth Eggs:\n{FormatWalletValue(wallet.Eggs)}";
        rggText.text        = $"Rgg: {FormatWalletValue(wallet.SuperRedEgg)}";
        bggText.text        = $"Bgg: {FormatWalletValue(wallet.SuperBlueEgg)}";
    }

    private void OnDestroy()
    {
        wallet.OnProfileChanged -= Refresh;
        wallet.OnItemChanged -= onItemChangedHandler;
    }

    static string FormatWalletValue(long value)
    {
        if (value > 9999999999)
            return "9999999999+";

        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

}
