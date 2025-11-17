using UnityEngine;
using UnityEngine.UI;

public class InstructionManager : MonoBehaviour
{
    public GameObject instructionPanel;
    public GameObject gamePanel;
    public Button okButton;

    void Start()
    {
        // Show instructions, hide game at start
        instructionPanel.SetActive(true);
        gamePanel.SetActive(false);

        // Add listener to OK button
        okButton.onClick.AddListener(OnOKButtonClicked);
    }

    public void OnOKButtonClicked()
    {
        // Hide instructions, show game
        instructionPanel.SetActive(false);
        gamePanel.SetActive(true);
    }
}