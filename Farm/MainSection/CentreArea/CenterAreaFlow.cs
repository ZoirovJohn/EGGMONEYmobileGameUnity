using UnityEngine;
using UnityEngine.UI;

public class CenterAreaFlow : MonoBehaviour
{
    [Header("Refs")]
    public GameObject loadingAnimHolder;   // LoadingAnimHolder
    public GameObject instructionsPanel;   // Instructions overlay (start inactive)
    public GameObject inventoryPanel;      // Inventory panel (start inactive)
    public Toggle dontShowAgainToggle;     // "Don't show again" (default OFF)

    public GameObject panelFooter;         // NEW: Footer (default OFF in prefab)

    private const string PREFS_KEY = "HideInstructions";

    [Header("Dev (Editor Only)")]
    [SerializeField] private bool resetSkipOnPlay = false; // set true in Inspector to reset each Play (Editor only)

    private void Awake()
    {
    #if UNITY_EDITOR
        if (resetSkipOnPlay)
        {
            PlayerPrefs.DeleteKey(PREFS_KEY);
            PlayerPrefs.Save();
            Debug.Log("[CenterAreaFlow] Reset HideInstructions on Play (Editor)");
        }
     #endif
    }

    private void Start()
    {
        // Start state
        if (loadingAnimHolder) loadingAnimHolder.SetActive(true);
        if (instructionsPanel) instructionsPanel.SetActive(false);
        if (inventoryPanel)    inventoryPanel.SetActive(false);
        if (panelFooter)       panelFooter.SetActive(false); // NEW: footer off by default

        Debug.Log($"[CenterAreaFlow] Start; HideInstructions={PlayerPrefs.GetInt(PREFS_KEY, 0)}");

        // After 5s, decide what to show
        Invoke(nameof(ShowInstructionsIfNeeded), 5f);

        // When user turns the toggle ON, persist + close + open inventory
        if (dontShowAgainToggle)
        {
            dontShowAgainToggle.isOn = false; // default OFF
            dontShowAgainToggle.onValueChanged.AddListener(OnSkipToggleChanged);
        }
    }

    private void ShowInstructionsIfNeeded()
    {
        if (loadingAnimHolder) loadingAnimHolder.SetActive(false);

        bool skip = PlayerPrefs.GetInt(PREFS_KEY, 0) == 1;
        Debug.Log($"[CenterAreaFlow] ShowInstructionsIfNeeded skip={skip}");

        if (skip)
        {
            OpenInventory(); // already opted out → skip overlay
        }
        else
        {
            if (inventoryPanel) inventoryPanel.SetActive(false);
            if (instructionsPanel) instructionsPanel.SetActive(true);
            if (panelFooter) panelFooter.SetActive(false);   // NEW: keep footer hidden on instructions
            Debug.Log("[CenterAreaFlow] Instructions shown");
        }
    }

    private void OnSkipToggleChanged(bool isOn)
    {
        // Only act when the user turns it ON while the panel is visible
        if (!isOn || instructionsPanel == null || !instructionsPanel.activeSelf) return;

        PlayerPrefs.SetInt(PREFS_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("[CenterAreaFlow] Skip enabled → saving & opening Inventory");

        instructionsPanel.SetActive(false);
        OpenInventory();
    }

    private void OpenInventory()
    {
        if (inventoryPanel) inventoryPanel.SetActive(true);
        if (panelFooter)    panelFooter.SetActive(true);   // NEW: show footer with inventory
        Debug.Log("[CenterAreaFlow] Inventory shown");
    }
}
