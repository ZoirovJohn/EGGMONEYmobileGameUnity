using UnityEngine;
using UnityEngine.UI;

public class OpenMenuFromButtons : MonoBehaviour
{
    [Header("Center Area (parent of panels to close)")]
    public Transform centerArea;

    [Header("Menu panel to open")]
    public GameObject menuMainPanel;

    [Header("All buttons that trigger this action")]
    public Button[] openMenuButtons;

    private void Start()
    {
        foreach (Button btn in openMenuButtons)
        {
            if (btn != null)
                btn.onClick.AddListener(OpenMenu);
        }
    }

    public void OpenMenu()
    {
        // 1️⃣ Close ALL panels under centerArea
        if (centerArea != null)
        {
            foreach (Transform child in centerArea)
            {
                child.gameObject.SetActive(false);
            }
        }

        // 2️⃣ Open menuMainPanel
        if (menuMainPanel != null)
        {
            menuMainPanel.SetActive(true);
        }
    }
}
