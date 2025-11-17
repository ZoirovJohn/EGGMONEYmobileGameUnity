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
        // Assign button click listener
        if (changeButton != null)
        {
            changeButton.onClick.AddListener(OnChangeButtonClicked);
        }
    }

    private void OnChangeButtonClicked()
    {
        // Check if placeholder is selected (index 0)
        if (locationDropdown.value == 0)
        {
            ShowFeedback("Please select a valid location", false);
            return;
        }

        // Get the selected location name
        string selectedLocation = locationDropdown.options[locationDropdown.value].text;

        // Create JSON data
        string jsonData = $"{{\"nation\": \"{selectedLocation}\"}}";

        // Show loading feedback
        ShowFeedback("Updating location...", true);

        // Call UpdateUser from AuthManager
        authManager.UpdateUser(
            jsonData,
            onSuccess: (response) =>
            {
                Debug.Log("Location updated successfully: " + response);
                ShowFeedback($"Location changed to {selectedLocation}", true);
                
                // ✅ Refresh player data after successful update
                RefreshPlayerData();
            },
            onError: (error) =>
            {
                Debug.LogError("Failed to update location: " + error);
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
        // Call GetMe to fetch updated user data
        authManager.GetMe(
            onSuccess: (response) =>
            {
                Debug.Log("Player data refreshed: " + response);

                MeResponse userData = JsonUtility.FromJson<MeResponse>(response);

                // Update PlayerWallet with fresh data
                if (playerWallet != null)
                {
                    playerWallet.SetName(userData.nickName);
                    playerWallet.SetLocation(userData.nation);
                    playerWallet.SetVideo(userData.video);
                    
                    if (int.TryParse(userData.userFP, out int fp))
                        playerWallet.SetFP(fp);
                    else
                        playerWallet.SetFP(0);

                    Debug.Log($"PlayerWallet updated after location change: {userData.nation}");
                }
                else
                {
                    Debug.LogError("PlayerWallet reference is missing!");
                }
            },
            onError: (err) =>
            {
                Debug.LogError("Failed to refresh player data: " + err);
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
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (changeButton != null)
        {
            changeButton.onClick.RemoveListener(OnChangeButtonClicked);
        }
    }
}