using UnityEngine;
using TMPro;

public class OpenWalletLink : MonoBehaviour
{
    [Header("References")]
    public PlayerWallet playerWallet;
    public TMP_Text buttonLabel; // Assign the button's text component

    [Header("Config")]
    [SerializeField] string url = "https://fafai-wallet.com";

    private void Start()
    {
        UpdateButtonLabel();
    }

    private void Update()
    {
        // Continuously update the label based on current location
        UpdateButtonLabel();
    }

    public void Open()
    {
        // ✅ Check if user location is KR
        if (playerWallet != null)
        {
            string userLocation = playerWallet.Location;
            
            if (userLocation == "KR")
            {
                Debug.LogWarning("❌ Wallet link blocked: User location is KR");
                return;
            }
        }
        else
        {
            Debug.LogError("PlayerWallet is not assigned!");
        }

        // If location check passes, open the URL
        Application.OpenURL(url);
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel == null || playerWallet == null)
        {
            return;
        }

        string userLocation = playerWallet.Location;

        if (userLocation == "KR")
        {
            buttonLabel.text = "Access wallet is not allowed. Change your location";
            buttonLabel.color = Color.red;
        }
        else
        {
            buttonLabel.text = "Access Wallet";
            // Set color to #6689DA
            ColorUtility.TryParseHtmlString("#6689DA", out Color allowedColor);
            buttonLabel.color = allowedColor;
        }
    }
}