using UnityEngine;

public class MiniGamePanelManager : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject mainPanel;
    public GameObject gamePanel;

    [Header("Game Panels")]
    public GameObject gameSimon;
    public GameObject gameCollect;
    public GameObject gameMatching;

    // =========================
    // Open Methods
    // =========================
    public void OpenSimon()
    {
        CloseAll();
        gameSimon.SetActive(true);
    }

    public void OpenCollect()
    {
        CloseAll();
        gameCollect.SetActive(true);
    }

    public void OpenMatching()
    {
        CloseAll();
        gameMatching.SetActive(true);
    }

    // =========================
    // Open Main Panel
    // =========================
    public void OpenMainPanel()
    {
        CloseAll();
        gamePanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // =========================
    // Close All Game Panels
    // =========================
    public void CloseAll()
    {
        gameSimon.SetActive(false);
        gameCollect.SetActive(false);
        gameMatching.SetActive(false);
    }
}
