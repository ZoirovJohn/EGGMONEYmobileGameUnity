using UnityEngine;
using UnityEngine.UI;

public class CenterAreaFlow : MonoBehaviour
{
    [Header("Refs")]
    public GameObject loadingAnimHolder;   // LoadingAnimHolder
    public GameObject instructionsPanel;   // Instructions overlay
    public GameObject inventoryPanel;      // Inventory panel (start inactive)
    public Toggle dontShowAgainToggle;     // "Don't show again" (default OFF)

    private const string PREFS_KEY = "HideInstructions";

    void Start()
    {
        // Start state
        if (loadingAnimHolder) loadingAnimHolder.SetActive(true);
        if (instructionsPanel) instructionsPanel.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);

        // After 5s, show instructions (unless previously skipped)
        Invoke(nameof(ShowInstructionsIfNeeded), 5f);

        // When user turns the toggle ON, persist + close + open inventory
        if (dontShowAgainToggle)
            dontShowAgainToggle.onValueChanged.AddListener(OnSkipToggleChanged);
    }

    private void ShowInstructionsIfNeeded()
    {
        if (loadingAnimHolder) loadingAnimHolder.SetActive(false);

        if (PlayerPrefs.GetInt(PREFS_KEY, 0) == 1)
        {
            OpenInventory(); // already opted out → skip overlay
        }
        else
        {
            if (instructionsPanel) instructionsPanel.SetActive(true);
        }
    }

    private void OnSkipToggleChanged(bool isOn)
    {
        // Only act when the user turns it ON while the panel is visible
        if (!isOn || !instructionsPanel || !instructionsPanel.activeSelf) return;

        PlayerPrefs.SetInt(PREFS_KEY, 1);
        PlayerPrefs.Save();

        instructionsPanel.SetActive(false);
        OpenInventory();
    }

    private void OpenInventory()
    {
        if (inventoryPanel) inventoryPanel.SetActive(true);
        // (Optional) else Debug.LogWarning("InventoryPanel not assigned.");
    }
}
