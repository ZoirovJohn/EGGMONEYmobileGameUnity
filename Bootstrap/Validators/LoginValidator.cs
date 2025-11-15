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
    public AuthManager authManager; // assign in Inspector
    public TMP_Text generalError;   // optional message label

    void Start()
    {
        // Clear errors at start
        SetUsernameError(null);
        SetPasswordError(null);
        if (generalError) generalError.text = "";

        // Clear on typing
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
                Debug.Log("Login response: " + response);

                LoginResponse loginData = JsonUtility.FromJson<LoginResponse>(response);
                
                // Save access token
                AuthStorage.SaveAccessToken(loginData.accessToken);
                
                // ✅ Now fetch user data with /auth/me
                authManager.GetMe(
                    onSuccess: (meResponse) =>
                    {
                        Debug.Log("Me response: " + meResponse);

                        MeResponse userData = JsonUtility.FromJson<MeResponse>(meResponse);
                        
                        // Populate PlayerWallet
                        PlayerWallet wallet = UnityEngine.Object.FindFirstObjectByType<PlayerWallet>();
                        if (wallet != null)
                        {
                            wallet.SetName(userData.nickName);
                            wallet.SetLocation(userData.nation);
                            if (int.TryParse(userData.userFP, out int fp))
                                wallet.SetFP(fp);
                            else
                                wallet.SetFP(0);
                        }
                        
                        // Save PlayerPrefs
                        PlayerPrefs.SetString("userId", userData.id);
                        PlayerPrefs.SetString("email", userData.email);
                        PlayerPrefs.SetString("nickname", userData.nickName);
                        PlayerPrefs.Save();
                        
                        // Load Farm
                        SceneManager.LoadScene("Farm");
                    },
                    onError: (err) =>
                    {
                        Debug.LogError("Failed to fetch user data: " + err);
                        if (generalError) generalError.text = "Failed to load user data.";
                    }
                );
            },
            onError: (err) =>
            {
                Debug.LogError("Login failed: " + err);
                if (errorUsername != null)
                    errorUsername.text = "Username or password is not correct.";
                if (usernameBackground != null)
                    usernameBackground.color = errorTint;
            }
        );
    }

    // --- helpers ---
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
        public string nation;
        public string userFP;
        public string referralCode;
    }
}