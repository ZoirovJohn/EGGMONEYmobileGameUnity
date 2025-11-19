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
    public AuthManager authManager;

    [Header("Panel Switcher")]
    public PanelSwitcher panelSwitcher;

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

    public void OnSignUpPressed()
    {
        bool ok = true;

        var name = (inputName ? inputName.text : "").Trim();
        if (string.IsNullOrEmpty(name)) { SetErr(errorName, bgName, "Name is required."); ok = false; }

        var email = (inputEmail ? inputEmail.text : "").Trim();
        if (string.IsNullOrEmpty(email)) { SetErr(errorEmail, bgEmail, "E-mail is required."); ok = false; }
        else if (!IsValidEmail(email)) { SetErr(errorEmail, bgEmail, "Please enter a valid e-mail address."); ok = false; }

        int countryIndex = ddCountry ? ddCountry.value : 0;
        var phone = (inputPhone ? inputPhone.text : "").Trim();
        if (countryIndex == 0 || string.IsNullOrEmpty(phone)) 
        { 
            SetErr(errorPhone, bgPhone, "Country and phone are required."); 
            ok = false; 
        }
        else if (!IsDigits(phone)) 
        { 
            SetErr(errorPhone, bgPhone, "Phone must be numbers only."); 
            ok = false; 
        }
        else if (phone.Length < 7 || phone.Length > 15) 
        { 
            SetErr(errorPhone, bgPhone, "Phone number must be between 7-15 digits."); 
            ok = false; 
        }

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

        // Get country code from dropdown index
        string countryCode = GetCountryCode(countryIndex);

        SignupData data = new SignupData(
            nickName: name,
            email: email,
            password: pw,
            phoneNumber: phone,
            nation: countryCode // ✅ Send short country code (e.g., "KR", "US", "JP")
        );

        authManager.Signup(
            data,
            onSuccess: (response) =>
            {
                Debug.Log("✅ Signup response: " + response);

                SignupResponse signupData = JsonUtility.FromJson<SignupResponse>(response);

                if (!string.IsNullOrEmpty(signupData.accessToken))
                {
                    AuthStorage.SaveAccessToken(signupData.accessToken);

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
                                wallet.SetReferralCode(userData.referralCode);
                                
                                if (int.TryParse(userData.userFP, out int fp))
                                    wallet.SetFP(fp);
                                else
                                    wallet.SetFP(0);

                                Debug.Log($"💰 Wallet updated: {userData.nickName}, {userData.userFarms} farms, Referral: {userData.referralCode}");
                            }

                            PlayerPrefs.SetString("userId", userData.id);
                            PlayerPrefs.SetString("email", userData.email);
                            PlayerPrefs.SetString("nickname", userData.nickName);
                            PlayerPrefs.SetInt("userFarms", userData.userFarms);
                            PlayerPrefs.SetString("referralCode", userData.referralCode);
                            PlayerPrefs.Save();

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
                            Debug.LogError("❌ Failed to fetch user data: " + err);
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
                Debug.LogError("❌ Signup failed: " + err);
                SetErr(errorEmail, bgEmail, "Signup failed. Please try again.");
            }
        );
    }

    // Helper method to get country code by dropdown index
    private string GetCountryCode(int index)
    {
        if (index >= 0 && index < CountryCodes.Length)
        {
            return CountryCodes[index];
        }
        return "KR"; // Default to South Korea
    }

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
        public int userFarms;
    }
}