using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SignUpValidator : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField inputName;
    public TMP_InputField inputEmail;
    public TMP_Dropdown ddCountry;     
    public TMP_InputField inputPhone;
    public TMP_Dropdown ddLocation;     
    public TMP_InputField inputPassword;
    public TMP_InputField inputConfirm;
    public Toggle toggleTerms;

    [Header("Error labels (Text TMP)")]
    public TMP_Text errorName;
    public TMP_Text errorEmail;
    public TMP_Text errorPhone;
    public TMP_Text errorLocation;
    public TMP_Text errorPassword;
    public TMP_Text errorConfirm;
    public TMP_Text errorTerms;

    [Header("Optional: backgrounds to tint on error")]
    public Image bgName;
    public Image bgEmail;
    public Image bgPhone;
    public Image bgLocation;          
    public Image bgPassword;
    public Image bgConfirm;
    public Color errorTint = new Color(0.92f, 0.23f, 0.27f);
    public Color normalTint = Color.white;

    [Header("Auth Manager")]
    public AuthManager authManager; // assign in Inspector

    [Header("Panel Switcher")]
    public PanelSwitcher panelSwitcher; // assign in Inspector

    public void OnSignUpPressed()
    {
        bool ok = true;

        // --- Validation ---
        var name = (inputName ? inputName.text : "").Trim();
        if (string.IsNullOrEmpty(name)) { SetErr(errorName, bgName, "Name is required."); ok = false; }

        var email = (inputEmail ? inputEmail.text : "").Trim();
        if (string.IsNullOrEmpty(email)) { SetErr(errorEmail, bgEmail, "E-mail is required."); ok = false; }
        else if (!IsValidEmail(email)) { SetErr(errorEmail, bgEmail, "Please enter a valid e-mail address."); ok = false; }

        int countryIndex = ddCountry ? ddCountry.value : 0;
        var phone = (inputPhone ? inputPhone.text : "").Trim();
        if (countryIndex == 0 || string.IsNullOrEmpty(phone)) { SetErr(errorPhone, bgPhone, "Country and phone are required."); ok = false; }
        else if (!IsDigits(phone)) { SetErr(errorPhone, bgPhone, "Phone must be numbers only."); ok = false; }

        int locIndex = ddLocation ? ddLocation.value : 0;
        if (locIndex == 0) { SetErr(errorLocation, bgLocation, "Location is required."); ok = false; }

        var pw = inputPassword ? inputPassword.text : "";
        var pw2 = inputConfirm ? inputConfirm.text : "";
        if (string.IsNullOrEmpty(pw)) { SetErr(errorPassword, bgPassword, "Password is required."); ok = false; }
        else if (pw.Length < 8) { SetErr(errorPassword, bgPassword, "Password must be at least 8 characters."); ok = false; }

        if (string.IsNullOrEmpty(pw2)) { SetErr(errorConfirm, bgConfirm, "Please re-enter your password."); ok = false; }
        else if (pw != pw2) { SetErr(errorConfirm, bgConfirm, "Passwords do not match."); ok = false; }

        if (toggleTerms && !toggleTerms.isOn) { Show(errorTerms, "Please accept the terms to continue."); ok = false; }

        if (!ok) return;

        // --- Validation passed: create SignupData and call backend ---
        SignupData data = new SignupData(
            nickName: name,
            email: email,
            password: pw,
            phoneNumber: phone,
            nation: ddCountry.options[countryIndex].text
        );

        authManager.Signup(
            data,
            onSuccess: (response) =>
            {
                Debug.Log("Signup response: " + response);

                // Parse the signup response
                SignupResponse signupData = JsonUtility.FromJson<SignupResponse>(response);

                // Save access token
                if (!string.IsNullOrEmpty(signupData.accessToken))
                {
                    AuthStorage.SaveAccessToken(signupData.accessToken);

                    // ✅ Now fetch user data with /auth/me
                    authManager.GetMe(
                        onSuccess: (meResponse) =>
                        {
                            Debug.Log("Me response: " + meResponse);

                            MeResponse userData = JsonUtility.FromJson<MeResponse>(meResponse);

                            // --- Populate PlayerWallet with user data ---
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

                            // ✅ Show Character Selection Panel instead of loading Farm scene
                            if (panelSwitcher != null)
                            {
                                panelSwitcher.ShowCharacter();
                            }
                            else
                            {
                                Debug.LogError("PanelSwitcher is not assigned!");
                            }
                        },
                        onError: (err) =>
                        {
                            Debug.LogError("Failed to fetch user data: " + err);
                        }
                    );
                }
                else
                {
                    Debug.LogError("Access token not received.");
                }
            },
            onError: (err) => 
            { 
                Debug.LogError("Signup failed: " + err);
                SetErr(errorEmail, bgEmail, "Signup failed. Please try again.");
            }
        );
    }

    // --- helpers ---
    void SetErr(TMP_Text label, Image bg, string msg) { Show(label, msg); if (bg) bg.color = string.IsNullOrEmpty(msg) ? normalTint : errorTint; }
    void Show(TMP_Text label, string msg) { if (!label) return; label.text = msg ?? ""; if (!label.gameObject.activeSelf) label.gameObject.SetActive(true); }
    bool IsValidEmail(string email) { try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; } catch { return false; } }
    bool IsDigits(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, @"^\d+$");

    [Serializable]
    private class SignupResponse
    {
        public string message;
        public SignupUserData user;
        public string accessToken;
        public string refreshToken;
        public string sessionId;
    }

    [Serializable]
    private class SignupUserData
    {
        public string id;
        public string nickName;
        public string email;
        public string referralCode;
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