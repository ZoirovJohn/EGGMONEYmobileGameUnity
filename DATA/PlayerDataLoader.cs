using UnityEngine;
using System;

public class PlayerDataLoader : MonoBehaviour
{
    [Header("References")]
    public AuthManager authManager;
    public PlayerWallet playerWallet;

    private void Start()
    {
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        // Check if we have an access token
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogWarning("No access token found. User not authenticated.");
            return;
        }

        // Fetch user data from backend
        authManager.GetMe(
            onSuccess: (response) =>
            {
                Debug.Log("Player data loaded: " + response);

                MeResponse userData = JsonUtility.FromJson<MeResponse>(response);

                // Populate PlayerWallet
                if (playerWallet != null)
                {
                    playerWallet.SetName(userData.nickName);
                    playerWallet.SetLocation(userData.nation);
                    
                    if (int.TryParse(userData.userFP, out int fp))
                        playerWallet.SetFP(fp);
                    else
                        playerWallet.SetFP(0);

                    Debug.Log($"PlayerWallet updated: {userData.nickName}, {userData.nation}, {userData.userFP} FP");
                }
                else
                {
                    Debug.LogError("PlayerWallet reference is missing!");
                }

                // Update PlayerPrefs for offline access
                PlayerPrefs.SetString("userId", userData.id);
                PlayerPrefs.SetString("email", userData.email);
                PlayerPrefs.SetString("nickname", userData.nickName);
                PlayerPrefs.Save();
            },
            onError: (err) =>
            {
                Debug.LogError("Failed to load player data: " + err);
                
                // Optional: If token is invalid, logout
                // UnityEngine.SceneManagement.SceneManager.LoadScene("Bootstrap");
            }
        );
    }

    [Serializable]
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
}