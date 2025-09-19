using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationDropdownPopulator : MonoBehaviour
{
    [Header("Assign your TMP_Dropdown here")]
    public TMP_Dropdown dropdown;

    void Start()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        // Clear any existing options
        dropdown.ClearOptions();

        // Create a list of options
        List<string> countries = new List<string>
        {
            "Korea",
            "China",
            "Japan",
            "USA",
            "UK",
            "Germany",
            "France",
            "India",
            "Vietnam",
            "Thailand"
        };

        // Add them to the dropdown
        dropdown.AddOptions(countries);

        // Optional: set placeholder (label) text
        if (dropdown.captionText != null)
        {
            dropdown.captionText.text = "location";
        }
    }
}
