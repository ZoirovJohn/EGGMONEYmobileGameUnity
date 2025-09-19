using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Mail;
using System.Text.RegularExpressions;

public class SignUpValidator : MonoBehaviour
{
  [Header("Inputs")]
  public TMP_InputField inputName;
  public TMP_InputField inputEmail;
  public TMP_Dropdown ddCountry;      // country selector (0 = "Select country")
  public TMP_InputField inputPhone;
  public TMP_Dropdown ddLocation;     // location selector (0 = "Select location")
  public TMP_InputField inputPassword;
  public TMP_InputField inputConfirm;
  public Toggle toggleTerms;

  [Header("Error labels (Text TMP)")]
  public TMP_Text errorName;
  public TMP_Text errorEmail;
  public TMP_Text errorPhone;       // covers both country & phone
  public TMP_Text errorLocation;    // NEW
  public TMP_Text errorPassword;
  public TMP_Text errorConfirm;
  public TMP_Text errorTerms;

  [Header("Optional: backgrounds to tint on error")]
  public Image bgName;
  public Image bgEmail;
  public Image bgPhone;
  public Image bgLocation;          // NEW
  public Image bgPassword;
  public Image bgConfirm;
  public Color errorTint = new Color(0.92f, 0.23f, 0.27f);
  public Color normalTint = Color.white;

  [Header("Next step")]
  public PanelSwitcher switcher;    // to go to Character after success

  void Start()
  {
    // Clear errors at start
    SetErr(errorName, bgName, null);
    SetErr(errorEmail, bgEmail, null);
    SetErr(errorPhone, bgPhone, null);
    SetErr(errorLocation, bgLocation, null);  // NEW
    SetErr(errorPassword, bgPassword, null);
    SetErr(errorConfirm, bgConfirm, null);
    Show(errorTerms, null);

    // Clear as user types/changes
    if (inputName) inputName.onValueChanged.AddListener(_ => SetErr(errorName, bgName, null));
    if (inputEmail) inputEmail.onValueChanged.AddListener(_ => SetErr(errorEmail, bgEmail, null));
    if (ddCountry) ddCountry.onValueChanged.AddListener(_ => SetErr(errorPhone, bgPhone, null));
    if (inputPhone) inputPhone.onValueChanged.AddListener(_ => SetErr(errorPhone, bgPhone, null));
    if (ddLocation) ddLocation.onValueChanged.AddListener(_ => SetErr(errorLocation, bgLocation, null)); // NEW
    if (inputPassword) inputPassword.onValueChanged.AddListener(_ => SetErr(errorPassword, bgPassword, null));
    if (inputConfirm) inputConfirm.onValueChanged.AddListener(_ => SetErr(errorConfirm, bgConfirm, null));
    if (toggleTerms) toggleTerms.onValueChanged.AddListener(_ => Show(errorTerms, null));
  }

  // Hook this to Btn_SignUp OnClick
  public void OnSignUpPressed()
  {
    bool ok = true;

    // Name
    var name = (inputName ? inputName.text : "").Trim();
    if (string.IsNullOrEmpty(name))
    {
      SetErr(errorName, bgName, "Name is required.");
      ok = false;
    }

    // Email
    var email = (inputEmail ? inputEmail.text : "").Trim();
    if (string.IsNullOrEmpty(email))
    {
      SetErr(errorEmail, bgEmail, "E-mail is required.");
      ok = false;
    }
    else if (!IsValidEmail(email))
    {
      SetErr(errorEmail, bgEmail, "Please enter a valid e-mail address.");
      ok = false;
    }

    // Country + Phone
    int countryIndex = ddCountry ? ddCountry.value : 0; // 0 = "Select country"
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

    // Location (between phone and password)
    int locIndex = ddLocation ? ddLocation.value : 0;   // 0 = "Select location"
    if (locIndex == 0)
    {
      SetErr(errorLocation, bgLocation, "Location is required.");
      ok = false;
    }

    // Password
    var pw = inputPassword ? inputPassword.text : "";
    var pw2 = inputConfirm ? inputConfirm.text : "";
    if (string.IsNullOrEmpty(pw))
    {
      SetErr(errorPassword, bgPassword, "Password is required.");
      ok = false;
    }
    else if (pw.Length < 8)
    {
      SetErr(errorPassword, bgPassword, "Password must be at least 8 characters.");
      ok = false;
    }

    // Confirm password
    if (string.IsNullOrEmpty(pw2))
    {
      SetErr(errorConfirm, bgConfirm, "Please re-enter your password.");
      ok = false;
    }
    else if (pw != pw2)
    {
      SetErr(errorConfirm, bgConfirm, "Passwords do not match.");
      ok = false;
    }

    // Terms
    if (toggleTerms && !toggleTerms.isOn)
    {
      Show(errorTerms, "Please accept the terms to continue.");
      ok = false;
    }

    if (!ok) return;

    // ✅ Success → go next (Character Select)
    if (switcher != null) switcher.ShowCharacter();
    // Or: SceneManager.LoadScene("CharacterSelect");
  }

  // --- helpers ---
  void SetErr(TMP_Text label, Image bg, string msg)
  {
    Show(label, msg);
    if (bg) bg.color = string.IsNullOrEmpty(msg) ? normalTint : errorTint;
  }

  void Show(TMP_Text label, string msg)
  {
    if (!label) return;
    label.text = msg ?? "";
    label.gameObject.SetActive(!string.IsNullOrEmpty(msg));
  }

  bool IsValidEmail(string email)
  {
    try { var addr = new MailAddress(email); return addr.Address == email; }
    catch { return false; }
  }

  bool IsDigits(string s) => Regex.IsMatch(s, @"^\d+$");
}
