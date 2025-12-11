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
        // Auto-find references if enabled
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

        // Setup button listeners
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

        // 3️⃣ ✅ NEW: Clear footer button highlights (yellow backgrounds)
        if (footerPanelSwitcher != null)
        {
            footerPanelSwitcher.ClearFooterSelection(closePanels: false);
            Debug.Log("✅ Cleared footer button highlights");
        }
        else
        {
            Debug.LogWarning("⚠️ FooterPanelSwitcher not found - cannot clear footer highlights");
        }

        // 4️⃣ ✅ NEW: Clear farm header selection (yellow background)
        if (farmHeaderManager != null)
        {
            farmHeaderManager.ClearFarmSelection();
            Debug.Log("✅ Cleared farm header selection");
        }
        else
        {
            Debug.LogWarning("⚠️ FarmHeaderManager not found - cannot clear farm selection");
        }
    }
}