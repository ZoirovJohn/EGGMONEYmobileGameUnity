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

        // Username checks
        string username = (inputUsername ? inputUsername.text : "").Trim();
        if (string.IsNullOrEmpty(username))
        {
            SetUsernameError("Username is required.");
            ok = false;
        }

        // Password checks
        string pw = inputPassword ? inputPassword.text : "";
        if (string.IsNullOrEmpty(pw))
        {
            SetPasswordError("Password is required.");
            ok = false;
        }

        if (!ok) return;

        // ✅ Call backend login
        LoginData loginData = new LoginData(username, pw);

        authManager.Login(
            loginData,
            onSuccess: (response) =>
            {
                Debug.Log("Login success: " + response);
                if (generalError) generalError.text = "";
                SceneManager.LoadScene("Farm"); // or your next scene
            },
            onError: (err) =>
            {
                Debug.LogError("Login failed: " + err);
                if (generalError) generalError.text = "Login failed. Please check your credentials.";
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
}
