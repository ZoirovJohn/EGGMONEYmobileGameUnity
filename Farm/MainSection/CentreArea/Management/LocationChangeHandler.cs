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

    private void Start()
    {
        if (changeButton != null)
        {
            changeButton.onClick.AddListener(OnChangeButtonClicked);
        }
    }

    private void OnChangeButtonClicked()
    {
        if (locationDropdown.value == 0)
        {
            ShowFeedback("Please select a valid location", false);
            return;
        }

        string selectedLocation = locationDropdown.options[locationDropdown.value].text;
        string jsonData = $"{{\"nation\": \"{selectedLocation}\"}}";

        ShowFeedback("Updating location...", true);

        authManager.UpdateUser(
            jsonData,
            onSuccess: (response) =>
            {
                Debug.Log("✅ Location updated successfully: " + response);
                ShowFeedback($"Location changed to {selectedLocation}", true);
                
                RefreshPlayerData();
            },
            onError: (error) =>
            {
                Debug.LogError("❌ Failed to update location: " + error);
                ShowFeedback("Failed to update location: " + error, false);
            }
        );
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
                    playerWallet.SetVideo(userData.video);
                    playerWallet.SetUserFarms(userData.userFarms);
                    playerWallet.SetReferralCode(userData.referralCode); // ✅ NEW: Set referral code
                    
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