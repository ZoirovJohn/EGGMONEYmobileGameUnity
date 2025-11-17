using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationChangeHandler : MonoBehaviour
{
    [Header("References")]
    public TMP_Dropdown locationDropdown;
    public Button changeButton;
    public AuthManager authManager;

    [Header("Optional UI Feedback")]
    public TMP_Text feedbackText;

    private void Start()
    {
        // Assign button click listener
        if (changeButton != null)
        {
            changeButton.onClick.AddListener(OnChangeButtonClicked);
        }
    }

    private void OnChangeButtonClicked()
    {
        // Check if placeholder is selected (index 0)
        if (locationDropdown.value == 0)
        {
            ShowFeedback("Please select a valid location", false);
            return;
        }

        // Get the selected location name
        string selectedLocation = locationDropdown.options[locationDropdown.value].text;

        // Create JSON data
        string jsonData = $"{{\"nation\": \"{selectedLocation}\"}}";

        // Show loading feedback
        ShowFeedback("Updating location...", true);

        // Call UpdateUser from AuthManager
        authManager.UpdateUser(
            jsonData,
            onSuccess: (response) =>
            {
                Debug.Log("Location updated successfully: " + response);
                ShowFeedback($"Location changed to {selectedLocation}", true);
            },
            onError: (error) =>
            {
                Debug.LogError("Failed to update location: " + error);
                ShowFeedback("Failed to update location: " + error, false);
            }
        );
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
        }
        
        Debug.Log(message);
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (changeButton != null)
        {
            changeButton.onClick.RemoveListener(OnChangeButtonClicked);
        }
    }
}