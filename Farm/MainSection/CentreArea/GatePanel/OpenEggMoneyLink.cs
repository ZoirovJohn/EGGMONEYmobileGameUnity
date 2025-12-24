using UnityEngine;

public class OpenEggMoneyLink : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWallet playerWallet;

    [Header("URLs")]
    [SerializeField] private string globalUrl = "https://eggmoney.io/";
    [SerializeField] private string koreaUrl  = "https://kr.eggmoney.io/";

    public void Open()
    {
        if (playerWallet == null)
        {
            Debug.LogError("❌ PlayerWallet not assigned!");
            return;
        }

        string location = playerWallet.Location;

        if (location == "KR")
        {
            Application.OpenURL(koreaUrl);
        }
        else
        {
            Application.OpenURL(globalUrl);
        }
    }
}
