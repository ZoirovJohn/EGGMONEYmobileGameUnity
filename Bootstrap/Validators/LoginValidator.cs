using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class LoginValidator : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;

    [Header("Error labels (Text TMP)")]
    public TMP_Text errorUsername;
    public TMP_Text errorPassword;

    [Header("Optional: backgrounds to tint on error")]
    public Image usernameBackground;
    public Image passwordBackground;
    public Color errorTint = new Color(0.92f, 0.23f, 0.27f);
    public Color normalTint = Color.white;

    [Header("References")]
    public AuthManager authManager;
    public TMP_Text generalError;

    void Start()
    {
        SetUsernameError(null);
        SetPasswordError(null);
        if (generalError) generalError.text = "";

        if (inputUsername) inputUsername.onValueChanged.AddListener(_ => SetUsernameError(null));
        if (inputPassword) inputPassword.onValueChanged.AddListener(_ => SetPasswordError(null));
    }

    public void OnLoginPressed()
    {
        bool ok = true;

        string username = (inputUsername ? inputUsername.text : "").Trim();
        if (string.IsNullOrEmpty(username))
        {
            SetUsernameError("Username is required.");
            ok = false;
        }

        string pw = inputPassword ? inputPassword.text : "";
        if (string.IsNullOrEmpty(pw))
        {
            SetPasswordError("Password is required.");
            ok = false;
        }

        if (!ok) return;

        LoginData loginData = new LoginData(username, pw);

        authManager.Login(
            loginData,
            onSuccess: (response) =>
            {
                Debug.Log("✅ Login response: " + response);

                LoginResponse loginData = JsonUtility.FromJson<LoginResponse>(response);
                AuthStorage.SaveAccessToken(loginData.accessToken);
                
                authManager.GetMe(
                    onSuccess: (meResponse) =>
                    {
                        Debug.Log("✅ Me response: " + meResponse);

                        MeResponse userData = JsonUtility.FromJson<MeResponse>(meResponse);
                        
                        PlayerWallet wallet = UnityEngine.Object.FindFirstObjectByType<PlayerWallet>();
                        if (wallet != null)
                        {
                            wallet.SetName(userData.nickName);
                            wallet.SetLocation(userData.nation);
                            wallet.SetUserFarms(userData.userFarms);
                            wallet.SetReferralCode(userData.referralCode); // ✅ NEW: Set referral code
                            
                            if (int.TryParse(userData.userFP, out int fp))
                                wallet.SetFP(fp);
                            else
                                wallet.SetFP(0);

                            Debug.Log($"💰 Wallet updated: {userData.nickName}, {userData.userFarms} farms, {userData.userFP} FP, Referral: {userData.referralCode}");
                        }
                        else
                        {
                            Debug.LogWarning("⚠️ PlayerWallet not found in scene");
                        }
                        
                        PlayerPrefs.SetString("userId", userData.id);
                        PlayerPrefs.SetString("email", userData.email);
                        PlayerPrefs.SetString("nickname", userData.nickName);
                        PlayerPrefs.SetInt("userFarms", userData.userFarms);
                        PlayerPrefs.SetString("referralCode", userData.referralCode); // ✅ NEW: Save to PlayerPrefs
                        PlayerPrefs.Save();
                        
                        Debug.Log("🎮 Loading Farm scene...");
                        SceneManager.LoadScene("Farm");
                    },
                    onError: (err) =>
                    {
                        Debug.LogError("❌ Failed to fetch user data: " + err);
                        if (generalError) generalError.text = "Failed to load user data.";
                    }
                );
            },
            onError: (err) =>
            {
                Debug.LogError("❌ Login failed: " + err);
                if (errorUsername != null)
                    errorUsername.text = "Username or password is not correct.";
                if (usernameBackground != null)
                    usernameBackground.color = errorTint;
            }
        );
    }

    void SetUsernameError(string msg)
    {
        if (errorUsername) errorUsername.text = msg ?? "";
        if (usernameBackground)
            usernameBackground.color = string.IsNullOrEmpty(msg) ? normalTint : errorTint;
    }

    void SetPasswordError(string msg)
    {
        if (errorPassword) errorPassword.text = msg ?? "";
        if (passwordBackground)
            passwordBackground.color = string.IsNullOrEmpty(msg) ? normalTint : errorTint;
    }

    [Serializable]
    private class LoginResponse
    {
        public string accessToken;
        public string refreshToken;
        public string sessionId;
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
        public int userFarms;
    }
}