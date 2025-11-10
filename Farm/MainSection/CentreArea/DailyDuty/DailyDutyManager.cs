using UnityEngine;
using UnityEngine.UI;

public class DailyDutyManager : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject mainPanel; // The main menu panel

    [Header("Game Panel")]
    public GameObject gamePanel; // Container for all game sub-panels

    [Header("Specific Game Panels (children of Game Panel)")]
    public GameObject cleaningPanel;
    public GameObject feedingPanel;
    public GameObject collectingPanel;

    [Header("Buttons (on Main Panel)")]
    public Button cleaningButton;
    public Button feedingButton;
    public Button collectingButton;

    private void Start()
    {
        // Assign main panel button listeners
        cleaningButton.onClick.AddListener(() => OpenSpecificGame(cleaningPanel));
        feedingButton.onClick.AddListener(() => OpenSpecificGame(feedingPanel));
        collectingButton.onClick.AddListener(() => OpenSpecificGame(collectingPanel));

        // Start with only the main panel visible
        ShowMainPanel();
    }

    private void OpenSpecificGame(GameObject specificGamePanel)
    {
        // Hide main panel
        mainPanel.SetActive(false);

        // Show game panel container
        gamePanel.SetActive(true);

        // Hide all specific game panels first
        cleaningPanel.SetActive(false);
        feedingPanel.SetActive(false);
        collectingPanel.SetActive(false);

        // Show only the selected specific game panel
        specificGamePanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        // Hide game panel container and all specific games
        gamePanel.SetActive(false);
        cleaningPanel.SetActive(false);
        feedingPanel.SetActive(false);
        collectingPanel.SetActive(false);

        // Show main panel
        mainPanel.SetActive(true);
    }
}