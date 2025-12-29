using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationChangeHandler : MonoBehaviour
{
    [Header("References")]
    public TMP_Dropdown locationDropdown;
    public Button changeButton;
    public AuthManager authManager;
    public PlayerWallet playerWallet;

    [Header("Optional UI Feedback")]
    public TMP_Text feedbackText;

    private static readonly string[] CountryCodes = new string[]
    {
        "", // placeholder "Select location"
        "KR", // South Korea
        "JP", // Japan
        "CN", // China
        "HK", // Hong Kong
        "TW", // Taiwan
        "VN", // Vietnam
        "TH", // Thailand
        "ID", // Indonesia
        "PH", // Philippines
        "MY", // Malaysia
        "SG", // Singapore
        "IN", // India
        "US", // United States
        "CA", // Canada
        "MX", // Mexico
        "BR", // Brazil
        "AR", // Argentina
        "CL", // Chile
        "CO", // Colombia
        "GB", // United Kingdom
        "DE", // Germany
        "FR", // France
        "ES", // Spain
        "IT", // Italy
        "NL", // Netherlands
        "SE", // Sweden
        "PL", // Poland
        "PT", // Portugal
        "AE", // United Arab Emirates
        "ZA"  // South Africa
    };

    private void Start()
    {
        if (changeButton != null)
        {
            changeButton.onClick.AddListener(OnChangeButtonClicked);
        }
    }

    private void OnChangeButtonClicked()
    {
        if (locationDropdown == null)
        {
            ShowFeedback("Location dropdown is not assigned!", false);
            return;
        }

        if (locationDropdown.value == 0)
        {
            ShowFeedback("Please select a valid location", false);
            return;
        }

        string countryCode = GetCountryCode(locationDropdown.value);
        
        if (string.IsNullOrEmpty(countryCode))
        {
            ShowFeedback("Invalid location selection", false);
            return;
        }

        string jsonData = $"{{\"nation\": \"{countryCode}\"}}";

        ShowFeedback("Updating location...", true);
        authManager.UpdateUser(
            jsonData,
            onSuccess: (response) =>
            {
                ShowFeedback($"Location changed to {countryCode}", true);
                
                RefreshPlayerData();
            },
            onError: (error) =>
            {
                ShowFeedback("Failed to update location: " + error, false);
            }
        );
    }

    private string GetCountryCode(int index)
    {
        if (index >= 0 && index < CountryCodes.Length)
        {
            return CountryCodes[index];
        }
        return "KR";
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
        }
    }

    private void RefreshPlayerData()
    {
        authManager.GetMe(
            onSuccess: (response) =>
            {
                MeResponse userData = JsonUtility.FromJson<MeResponse>(response);

                if (playerWallet != null)
                {
                    playerWallet.SetName(userData.nickName);
                    playerWallet.SetLocation(userData.nation);
                    playerWallet.SetUserFarms(userData.userFarms);
                    playerWallet.SetReferralCode(userData.referralCode);
                    
                    if (int.TryParse(userData.userFP, out int fp))
                        playerWallet.SetFP(fp);
                    else
                        playerWallet.SetFP(0);
                }
                else
                {
                    Debug.LogError("❌ PlayerWallet reference is missing!");
                }
            },
            onError: (err) =>
            {
                ShowFeedback("Location updated but failed to refresh data", false);
            }
        );
    }

    [System.Serializable]
    private class MeResponse
    {
        public string id;
        public string nickName;
        public string email;
        public string phoneNumber;
        public string firebaseUid;
        public string nation;
        public int video;
        public bool emailVerified;
        public string totpSecret;
        public bool is2FAEnabled;
        public string createdAt;
        public string updatedAt;
        public string userFP;
        public string referralCode;
        public string referredByCode;
        public int userFarms;
    }

    private void OnDestroy()
    {
        if (changeButton != null)
        {
            changeButton.onClick.RemoveListener(OnChangeButtonClicked);
        }
    }
}