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

    [Header("References to clear highlights")]
    [SerializeField] private FooterPanelSwitcher footerPanelSwitcher;
    [SerializeField] private FarmHeaderManager farmHeaderManager;

    [Header("Auto-find references")]
    [SerializeField] private bool autoFind = true;

    private void Start()
    {
        if (autoFind)
        {
            if (footerPanelSwitcher == null)
            {
                footerPanelSwitcher = FindAnyObjectByType<FooterPanelSwitcher>();
                if (footerPanelSwitcher != null)
                {
                    Debug.Log("✅ Auto-found FooterPanelSwitcher");
                }
            }

            if (farmHeaderManager == null)
            {
                farmHeaderManager = FindAnyObjectByType<FarmHeaderManager>();
                if (farmHeaderManager != null)
                {
                    Debug.Log("✅ Auto-found FarmHeaderManager");
                }
            }
        }

        foreach (Button btn in openMenuButtons)
        {
            if (btn != null)
                btn.onClick.AddListener(OpenMenu);
        }
    }

    public void OpenMenu()
    {
        if (centerArea != null)
        {
            foreach (Transform child in centerArea)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (menuMainPanel != null)
        {
            menuMainPanel.SetActive(true);
        }

        if (footerPanelSwitcher != null)
        {
            footerPanelSwitcher.ClearFooterSelection(closePanels: false);
        }
        else
        {
            Debug.LogWarning("⚠️ FooterPanelSwitcher not found - cannot clear footer highlights");
        }

        if (farmHeaderManager != null)
        {
            farmHeaderManager.ClearFarmSelection();
        }
        else
        {
            Debug.LogWarning("⚠️ FarmHeaderManager not found - cannot clear farm selection");
        }
    }
}