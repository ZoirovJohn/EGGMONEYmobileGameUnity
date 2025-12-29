using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReferralCodeManager : MonoBehaviour
{
    [Header("References")]
    public PlayerWallet playerWallet;
    public TMP_Text referralCodeText;
    public Button copyButton;

    private void Start()
    {
        if (playerWallet == null)
        {
            playerWallet = FindFirstObjectByType<PlayerWallet>();
        }

        if (copyButton != null)
        {
            copyButton.onClick.AddListener(OnCopyButtonClicked);
        }

        // Subscribe to profile changes to update the display
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged += UpdateDisplay;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (playerWallet == null || referralCodeText == null) return;

        string code = playerWallet.ReferralCode;
        
        if (string.IsNullOrEmpty(code))
        {
            referralCodeText.text = "Loading...";
        }
        else
        {
            referralCodeText.text = code;
        }

    }

    private void OnCopyButtonClicked()
    {
        if (playerWallet == null) return;

        string code = playerWallet.ReferralCode;

        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        // Copy to clipboard
        GUIUtility.systemCopyBuffer = code;
    }

    private void OnDestroy()
    {
        if (copyButton != null)
        {
            copyButton.onClick.RemoveListener(OnCopyButtonClicked);
        }

        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged -= UpdateDisplay;
        }
    }
}