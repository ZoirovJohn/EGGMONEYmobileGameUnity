using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManageLocationDropdownPopulator : MonoBehaviour
{
    [Header("Assign your TMP_Dropdown here")]
    public TMP_Dropdown dropdown;

    void Start()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        dropdown.ClearOptions();

        // Add placeholder first
        List<string> options = new List<string>
        {
        "Select location",   // placeholder
        "South Korea",
        "Japan",
        "China",
        "Hong Kong",
        "Taiwan",
        "Vietnam",
        "Thailand",
        "Indonesia",
        "Philippines",
        "Malaysia",
        "Singapore",
        "India",
        "United States",
        "Canada",
        "Mexico",
        "Brazil",
        "Argentina",
        "Chile",
        "Colombia",
        "United Kingdom",
        "Germany",
        "France",
        "Spain",
        "Italy",
        "Netherlands",
        "Sweden",
        "Poland",
        "Portugal",
        "United Arab Emirates",
        "South Africa"
        };

        dropdown.AddOptions(options);

        // Force start on placeholder
        dropdown.value = 0;
    }
}
