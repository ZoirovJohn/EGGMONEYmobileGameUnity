using UnityEngine;
using UnityEngine.UI;

public class DailyDutyManager : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject mainPanel;
    public GameObject gamesPanel;

    [Header("Buttons (on Main Panel)")]
    public Button simonButton;
    public Button collectButton;
    public Button matchingButton;

    [Header("Mini Game Manager Reference")]
    public MiniGamePanelManager miniGameManager;

    private void Start()
    {
        // Assign main panel button listeners
        simonButton.onClick.AddListener(OpenSimon);
        collectButton.onClick.AddListener(OpenCollect);
        matchingButton.onClick.AddListener(OpenMatching);

        // Start with only the main panel visible
        OpenMainPanel();
    }

    // =========================
    // Open Methods
    // =========================
    public void OpenSimon()
    {
        mainPanel.SetActive(false);
        gamesPanel.SetActive(true);
        miniGameManager.OpenSimon();
    }

    public void OpenCollect()
    {
        mainPanel.SetActive(false);
        gamesPanel.SetActive(true);
        miniGameManager.OpenCollect();
    }

    public void OpenMatching()
    {
        mainPanel.SetActive(false);
        gamesPanel.SetActive(true);
        miniGameManager.OpenMatching();
    }

    // =========================
    // Open Main Panel
    // =========================
    public void OpenMainPanel()
    {
        gamesPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
}