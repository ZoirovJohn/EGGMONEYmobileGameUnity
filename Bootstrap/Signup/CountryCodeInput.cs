using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class CountryCodeInput : MonoBehaviour
{
    public TMP_InputField inputCode;
    private const int MaxDigits = 4; // country codes are 1–4 digits

    void Start()
    {
        if (inputCode == null)
            inputCode = GetComponent<TMP_InputField>();

        // When selected, ensure "+"
        inputCode.onSelect.AddListener(_ =>
        {
            if (string.IsNullOrEmpty(inputCode.text) || !inputCode.text.StartsWith("+"))
            {
                inputCode.text = "+";
                inputCode.caretPosition = inputCode.text.Length;
            }
        });

        // While typing, sanitize
        inputCode.onValueChanged.AddListener(OnCodeChanged);
    }

    private void OnCodeChanged(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            inputCode.text = "+";
            inputCode.caretPosition = inputCode.text.Length;
            return;
        }

        // Ensure only digits after "+"
        string digits = Regex.Replace(raw, @"\D", ""); // keep only numbers
        if (digits.Length > MaxDigits)
            digits = digits.Substring(0, MaxDigits);

        inputCode.SetTextWithoutNotify("+" + digits);
        inputCode.caretPosition = inputCode.text.Length;
    }
}
