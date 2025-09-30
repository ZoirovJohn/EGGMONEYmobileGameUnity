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

        dropdown.ClearOptions();

        // Add placeholder first
        List<string> options = new List<string>
        {
            "Select location",   // placeholder
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

        dropdown.AddOptions(options);

        // Force start on placeholder
        dropdown.value = 0;
    }
}
