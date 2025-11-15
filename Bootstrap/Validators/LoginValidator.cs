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

                // ⚡ For now: ignore pending device verification
                // FIXME: handle this properly later
                string dummyToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyZThmNzEyZi01ZDUxLTRmYzQtYjYwNi03OGU1YjExZjNhNDgiLCJzaWQiOiJhYWY5MmIzYy1mN2I2LTQzNzktOGI4Ny04MDc4YTdhYjVhYWYiLCJpYXQiOjE3NjMwMzkwMTIsImV4cCI6MTc2MzkwMzAxMiwiYXVkIjoiZWdnbW9uZXkuY2xpZW50IiwiaXNzIjoiZWdnbW9uZXkuYXBpIn0.9-H_jSWKFm7pA0nNhMpg8Aomq3ICR9btkOBFCOMl608";
                AuthStorage.SaveAccessToken(dummyToken);

                // ✅ Double-check token
                string savedToken = AuthStorage.GetAccessToken();
                if (!string.IsNullOrEmpty(savedToken))
                {
                    SceneManager.LoadScene("Farm");
                }
                else
                {
                    Debug.LogError("Access token not saved properly, cannot proceed to Farm.");
                    if (generalError) generalError.text = "Login failed: unable to save token.";
                }

            },
            onError: (err) =>
            {
                Debug.LogError("Login failed: " + err);

                // Show in username error
                if (errorUsername != null)
                    errorUsername.text = "Username or password is not correct.";

                // Optional: tint background
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
}
