using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class PhoneInput : MonoBehaviour
{
    public TMP_InputField inputPhone;
    private const int MaxPhoneDigits = 15; // E.164 limit

    void Start()
    {
        if (inputPhone == null)
            inputPhone = GetComponent<TMP_InputField>();

        // While typing → sanitize
        inputPhone.onValueChanged.AddListener(OnPhoneChanged);
    }

    private void OnPhoneChanged(string raw)
    {
        // Keep only digits
        string digits = Regex.Replace(raw, @"\D", "");

        // Limit length
        if (digits.Length > MaxPhoneDigits)
            digits = digits.Substring(0, MaxPhoneDigits);

        // Update silently to avoid infinite loop
        inputPhone.SetTextWithoutNotify(digits);

        // Keep caret at end
        inputPhone.caretPosition = inputPhone.text.Length;
    }
}
