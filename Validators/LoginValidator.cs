using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Net.Mail;
using UnityEngine.SceneManagement;

public class LoginValidator : MonoBehaviour
{
  [Header("Inputs")]
  public TMP_InputField inputEmail;
  public TMP_InputField inputPassword;

  [Header("Error labels (Text TMP)")]
  public TMP_Text errorEmail;
  public TMP_Text errorPassword;

  [Header("Optional: backgrounds to tint on error")]
  public Image emailBackground;    // the Image on Input_Email (optional)
  public Image passwordBackground; // the Image on Input_Password (optional)
  public Color errorTint = new Color(0.92f, 0.23f, 0.27f); // red-ish
  public Color normalTint = Color.white;

  void Start()
  {
    // Hide errors at start
    SetEmailError(null);
    SetPasswordError(null);

    // Clear errors as user types
    if (inputEmail) inputEmail.onValueChanged.AddListener(_ => SetEmailError(null));
    if (inputPassword) inputPassword.onValueChanged.AddListener(_ => SetPasswordError(null));
  }

  public void OnLoginPressed()
  {
    bool ok = true;

    // Email checks
    string email = (inputEmail ? inputEmail.text : "").Trim();
    if (string.IsNullOrEmpty(email))
    {
      SetEmailError("E-mail is required.");
      ok = false;
    }
    else if (!IsValidEmail(email))
    {
      SetEmailError("Please enter a valid e-mail address.");
      ok = false;
    }
    else
    {
      SetEmailError(null);
    }

    // Password checks
    string pw = inputPassword ? inputPassword.text : "";
    if (string.IsNullOrEmpty(pw))
    {
      SetPasswordError("Password is required.");
      ok = false;
    }
    else
    {
      SetPasswordError(null);
    }

    if (!ok) return; // stop if validation failed

    // ✅ Success → load Farm scene
    SceneManager.LoadScene("Farm");
  }


  // --- helpers ---
  void SetEmailError(string msg)
  {
    if (errorEmail)
    {
      errorEmail.text = msg ?? "";
      errorEmail.gameObject.SetActive(!string.IsNullOrEmpty(msg));
    }
    if (emailBackground)
      emailBackground.color = string.IsNullOrEmpty(msg) ? normalTint : errorTint;
  }

  void SetPasswordError(string msg)
  {
    if (errorPassword)
    {
      errorPassword.text = msg ?? "";
      errorPassword.gameObject.SetActive(!string.IsNullOrEmpty(msg));
    }
    if (passwordBackground)
      passwordBackground.color = string.IsNullOrEmpty(msg) ? normalTint : errorTint;
  }

  bool IsValidEmail(string email)
  {
    try { var addr = new MailAddress(email); return addr.Address == email; }
    catch { return false; }
  }
}
