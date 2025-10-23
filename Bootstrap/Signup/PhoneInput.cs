using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class PhoneInput : MonoBehaviour
{
    [Header("Refs")]
    public TMP_InputField inputPhone;
    public PhoneCodeDropdownPopulator codeDropdown;  // link dropdown component
    public TMP_Text errorPhone;                      // "error_phone" TMP text

    private const int MaxPhoneDigits = 15; // E.164 limit
    private bool isUpdating = false;
    private bool inputLocked = false;

    void Start()
    {
        if (!inputPhone)
            inputPhone = GetComponent<TMP_InputField>();

        inputPhone.onValueChanged.AddListener(OnPhoneChanged);
        inputPhone.onSelect.AddListener(OnFocus);

        ApplyLockStateByCode();

        if (codeDropdown && codeDropdown.dropdown != null)
            codeDropdown.dropdown.onValueChanged.AddListener(_ => ApplyLockStateByCode());
    }

    private void ApplyLockStateByCode()
    {
        bool noCode = (codeDropdown == null) || string.IsNullOrEmpty(codeDropdown.GetSelectedCode());
        if (noCode) LockInput("Please choose country code first");
        else        UnlockInput();
    }

    private void LockInput(string msg)
    {
        inputLocked = true;
        inputPhone.readOnly = true;        // <-- keep visuals; just block typing
        inputPhone.text = string.Empty;
        inputPhone.DeactivateInputField();  // avoid caret
        ShowError(msg);
    }

    private void UnlockInput()
    {
        inputLocked = false;
        inputPhone.readOnly = false;       // re-enable typing without changing visuals
        ClearError();
    }

    private void OnFocus(string _)
    {
        if (inputLocked)
        {
            // Keep error visible and bounce focus out
            inputPhone.DeactivateInputField();
            return;
        }
    }

    private void OnPhoneChanged(string raw)
    {
        if (inputLocked || isUpdating) return;

        // digits only
        string digits = Regex.Replace(raw, @"\D", "");
        if (digits.Length > MaxPhoneDigits)
            digits = digits.Substring(0, MaxPhoneDigits);

        if (digits != inputPhone.text)
        {
            isUpdating = true;
            inputPhone.text = digits;
            inputPhone.caretPosition = digits.Length;
            isUpdating = false;
        }
    }

    private void ShowError(string msg)
    {
        if (errorPhone) errorPhone.text = msg;
    }

    private void ClearError()
    {
        if (errorPhone) errorPhone.text = string.Empty;
    }
}
