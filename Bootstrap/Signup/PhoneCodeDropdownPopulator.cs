using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhoneCodeDropdownPopulator : MonoBehaviour
{
    [Header("Hierarchy refs (drag from scene)")]
    public TMP_Dropdown dropdown;        // Row_Phone/Dropdown
    public TMP_Text labelText;           // Row_Phone/Dropdown/Label (TMP_Text)
    public GameObject labelIcon;         // Row_Phone/Dropdown/Label/Icon
    public TMP_InputField inputPhone;    // Row_Phone/Input_Phone/phone (optional)

    // Phone codes (add more if needed)
    private readonly List<string> codes = new List<string>
    {
        "+00", // placeholder
        "+82", "+86", "+81", "+1", "+44", "+49", "+33", "+91", "+844", "+66"
    };

    void Awake()
    {
        if (!dropdown) dropdown = GetComponentInChildren<TMP_Dropdown>(true);

        if (!labelText)
        {
            labelText = dropdown ? dropdown.captionText : null;
            if (!labelText)
            {
                var label = transform.Find("Label");
                if (label) labelText = label.GetComponentInChildren<TMP_Text>(true);
            }
        }

        if (!labelIcon)
        {
            var label = transform.Find("Label");
            if (label) labelIcon = label.Find("Icon")?.gameObject;
        }
    }

    void Start()
    {
        PopulateDropdown();

        // Start with "+00" selected (index 0)
        dropdown.SetValueWithoutNotify(0);
        dropdown.RefreshShownValue();

        // Keep visuals empty
        if (labelText) labelText.text = string.Empty;
        if (labelIcon) labelIcon.SetActive(true);

        dropdown.onValueChanged.AddListener(OnCodeSelected);
    }

    void PopulateDropdown()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(codes);
    }

    void OnCodeSelected(int index)
    {
        string selectedCode = codes[Mathf.Clamp(index, 0, codes.Count - 1)];

        // "+00" behaves like “no selection”
        if (selectedCode == "+00")
        {
            if (labelText) labelText.text = string.Empty;
            if (labelIcon) labelIcon.SetActive(true);
            return;
        }

        // Otherwise show chosen code & hide icon
        if (labelText) labelText.text = selectedCode;
        if (labelIcon) labelIcon.SetActive(false);

        // Optional: prefill phone when empty
        // if (inputPhone && string.IsNullOrEmpty(inputPhone.text))
        //     inputPhone.text = selectedCode + " ";
    }

    public string GetSelectedCode()
    {
        string code = codes[Mathf.Clamp(dropdown.value, 0, codes.Count - 1)];
        return code == "+00" ? null : code;
    }
}
