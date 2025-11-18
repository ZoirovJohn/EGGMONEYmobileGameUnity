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

    // Country code mapping (matches LocationDropdownPopulator order)
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

        // Get country code from dropdown index (same as SignUpValidator)
        string countryCode = GetCountryCode(locationDropdown.value);
        
        if (string.IsNullOrEmpty(countryCode))
        {
            ShowFeedback("Invalid location selection", false);
            return;
        }

        // Send short country code (e.g., "KR", "US", "JP") to the API
        string jsonData = $"{{\"nation\": \"{countryCode}\"}}";

        ShowFeedback("Updating location...", true);
        Debug.Log($"📍 Sending location update: {countryCode}");

        authManager.UpdateUser(
            jsonData,
            onSuccess: (response) =>
            {
                Debug.Log("✅ Location updated successfully: " + response);
                ShowFeedback($"Location changed to {countryCode}", true);
                
                RefreshPlayerData();
            },
            onError: (error) =>
            {
                Debug.LogError("❌ Failed to update location: " + error);
                ShowFeedback("Failed to update location: " + error, false);
            }
        );
    }

    // Helper method to get country code by dropdown index (same as SignUpValidator)
    private string GetCountryCode(int index)
    {
        if (index >= 0 && index < CountryCodes.Length)
        {
            return CountryCodes[index];
        }
        return "KR"; // Default to South Korea
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
        }
        
        Debug.Log(message);
    }

    private void RefreshPlayerData()
    {
        authManager.GetMe(
            onSuccess: (response) =>
            {
                Debug.Log("✅ Player data refreshed: " + response);

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

                    Debug.Log($"💰 PlayerWallet updated after location change: {userData.nation}, Referral: {userData.referralCode}");
                }
                else
                {
                    Debug.LogError("❌ PlayerWallet reference is missing!");
                }
            },
            onError: (err) =>
            {
                Debug.LogError("❌ Failed to refresh player data: " + err);
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