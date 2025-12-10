using UnityEngine;
using TMPro;

public class OpenWalletLink : MonoBehaviour
{
    [Header("References")]
    public PlayerWallet playerWallet;
    public TMP_Text buttonLabel;          // Assign the button's text component
    private LocalizedText localizedText;  // Optional: if you also put LocalizedText on the same object

    [Header("Config")]
    [SerializeField] string url = "https://fafai-wallet.com";

    private string lastLocation = null;

    private void Awake()
    {
        if (buttonLabel == null)
        {
            buttonLabel = GetComponentInChildren<TMP_Text>();
        }

        // Try to grab LocalizedText on the same object as the label
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
        // Only update if location changed to avoid work every frame
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
                Debug.LogWarning("❌ Wallet link blocked: User location is KR");
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
            // Blocked state
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
            // Allowed state
            if (localizedText != null)
            {
                localizedText.translationKey = "AccessWallet";
                localizedText.UpdateText();
            }
            else if (LanguageManager.Instance != null)
            {
                buttonLabel.text = LanguageManager.Instance.GetTranslation("AccessWallet");
            }

            // Set color to #6689DA
            if (ColorUtility.TryParseHtmlString("#6689DA", out Color allowedColor))
            {
                buttonLabel.color = allowedColor;
            }
        }
    }
}
