using UnityEngine;
using TMPro;

public class OpenWalletLink : MonoBehaviour
{
    [Header("References")]
    public PlayerWallet playerWallet;
    public TMP_Text buttonLabel;         
    private LocalizedText localizedText; 

    [Header("Config")]
    [SerializeField] string url = "https://fafai-wallet.com";

    private string lastLocation = null;

    private void Awake()
    {
        if (buttonLabel == null)
        {
            buttonLabel = GetComponentInChildren<TMP_Text>();
        }

        if (buttonLabel != null)
        {
            localizedText = buttonLabel.GetComponent<LocalizedText>();
        }
    }

    private void Start()
    {
        UpdateButtonLabel(force: true);
    }

    private void Update()
    {
        if (playerWallet == null) return;

        string currentLocation = playerWallet.Location;
        if (currentLocation != lastLocation)
        {
            UpdateButtonLabel();
            lastLocation = currentLocation;
        }
    }

    public void Open()
    {
        if (playerWallet != null)
        {
            string userLocation = playerWallet.Location;

            if (userLocation == "KR")
            {
                return;
            }
        }
        else
        {
            Debug.LogError("PlayerWallet is not assigned!");
        }

        Application.OpenURL(url);
    }

    private void UpdateButtonLabel(bool force = false)
    {
        if (buttonLabel == null || playerWallet == null)
        {
            return;
        }

        string userLocation = playerWallet.Location;

        if (userLocation == "KR")
        {
            if (localizedText != null)
            {
                localizedText.translationKey = "WalletAccessBlocked";
                localizedText.UpdateText();
            }
            else if (LanguageManager.Instance != null)
            {
                buttonLabel.text = LanguageManager.Instance.GetTranslation("WalletAccessBlocked");
            }

            buttonLabel.color = Color.red;
        }
        else
        {
            if (localizedText != null)
            {
                localizedText.translationKey = "AccessWallet";
                localizedText.UpdateText();
            }
            else if (LanguageManager.Instance != null)
            {
                buttonLabel.text = LanguageManager.Instance.GetTranslation("AccessWallet");
            }

            if (ColorUtility.TryParseHtmlString("#6689DA", out Color allowedColor))
            {
                buttonLabel.color = allowedColor;
            }
        }
    }
}
