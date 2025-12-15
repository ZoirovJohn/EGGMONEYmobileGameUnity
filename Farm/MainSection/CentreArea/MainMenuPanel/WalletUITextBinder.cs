using UnityEngine;
using TMPro; // or UnityEngine.UI if you use Text

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
        sggText.text        = $"Sgg: {wallet.SilverEgg}";
        gggText.text        = $"Ggg: {wallet.GoldEgg}";
        champText.text      = $"Champ: {wallet.ChampChick}";
        soondongText.text   = $"Soondong: {wallet.WhiteChick}";
        birthEggText.text  = $"Birth Egg: {wallet.Eggs}";
        rggText.text        = $"Rgg: {wallet.SuperRedEgg}";
    }

    private void OnDestroy()
    {
        wallet.OnProfileChanged -= Refresh;
        wallet.OnItemChanged -= (_, __) => Refresh();
    }
}
