using UnityEngine;
using System;

public class PlayerDataLoader : MonoBehaviour
{
    [Header("References")]
    public AuthManager authManager;
    public BasketManager basketManager;
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
                    playerWallet.SetVideo(userData.video);
                    
                    if (int.TryParse(userData.userFP, out int fp))
                        playerWallet.SetFP(fp);
                    else
                        playerWallet.SetFP(0);

                    Debug.Log($"PlayerWallet updated: {userData.nickName}, {userData.nation}, {userData.userFP} FP, {userData.video} videos");
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

                // ✅ Fetch eggs in basket
                LoadBasketData();
            },
            onError: (err) =>
            {
                Debug.LogError("Failed to load player data: " + err);
            }
        );
    }

    private void LoadBasketData()
    {
        // ✅ FIX: Call basketManager.GetBasket() instead of authManager.GetBasket()
        basketManager.GetBasket(
            onSuccess: (response) =>
            {
                Debug.Log("Basket data loaded: " + response);

                BasketResponse basketData = JsonUtility.FromJson<BasketResponse>(response);

                // Set eggs in PlayerWallet
                if (playerWallet != null)
                {
                    playerWallet.SetEggs(basketData.eggCount);
                    Debug.Log($"Eggs in basket: {basketData.eggCount}");
                }
            },
            onError: (err) =>
            {
                Debug.LogError("Failed to load basket data: " + err);
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

    [Serializable]
    private class BasketResponse
    {
        public int eggCount;
    }
}