using UnityEngine;
using UnityEngine.UI;

public class FooterPanelSwitcher : MonoBehaviour
{
    [Header("Panels (centre area targets)")]
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject dutiesPanel;
    [SerializeField] GameObject storePanel;

    [Header("Layout roots (visibility control)")]
    [SerializeField] GameObject farmTabsRoot;
    [SerializeField] GameObject panelButtonsRoot;
    [SerializeField] Transform centreAreaRoot;

    [Header("Footer Buttons")]
    [SerializeField] Button btnInventory;
    [SerializeField] Button btnDuties;
    [SerializeField] Button btnStore;
    [SerializeField] Button footerStart; // ✅ Icon that appears when work is available (also opens Duties)

    [Header("Button Background Images")]
    [SerializeField] GameObject bgInventory;
    [SerializeField] GameObject bgDuties;
    [SerializeField] GameObject bgStore;

    [Header("Player Wallet Reference")]
    [SerializeField] PlayerWallet playerWallet;

    public enum Panel { None, Inventory, Duties, Store }
    
    [Header("Default on enable")]
    [SerializeField] Panel defaultPanel = Panel.None;

    private Panel currentPanel = Panel.None;

    void Awake()
    {
        if (btnInventory) btnInventory.onClick.AddListener(ShowInventory);
        if (btnDuties) btnDuties.onClick.AddListener(ShowDuties);
        if (btnStore) btnStore.onClick.AddListener(ShowStore);
        if (footerStart) footerStart.onClick.AddListener(ShowDuties); // Also opens Duties panel
        
        // Auto-find PlayerWallet if not assigned
        if (!playerWallet) 
            playerWallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
    }

    void OnEnable()
    {
        // Subscribe to wallet changes
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged += UpdateDutiesButtonVisibility;
        }
        
        // Initial check
        UpdateDutiesButtonVisibility();
        
        // Default panel on enable
        SwitchTo(defaultPanel);
    }

    void OnDisable()
    {
        // Unsubscribe when disabled
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged -= UpdateDutiesButtonVisibility;
        }
    }

    // ✅ Check if there's any work to do and show/hide the start icon
    private void UpdateDutiesButtonVisibility()
    {
        if (playerWallet == null) return;

        // Check if there's ANY work available (even one > 0)
        bool hasWork = playerWallet.HensWithEggReady > 0 || 
                       playerWallet.HensNeedingFood > 0 || 
                       playerWallet.HensNeedingClean > 0;

        // btnDuties is ALWAYS visible (like other footer buttons)
        if (btnDuties != null)
        {
            btnDuties.gameObject.SetActive(true);
        }
        
        // Only show footerStart icon when there's work
        if (footerStart != null)
        {
            footerStart.gameObject.SetActive(hasWork);
        }

        Debug.Log($"🔔 Footer Button Update - Work Available: {hasWork} → footerStart icon: {hasWork}");
    }

    void SwitchTo(Panel p)
    {
        currentPanel = p;
        
        // Footer mode: show FarmTabs, hide Management tabs
        if (panelButtonsRoot) panelButtonsRoot.SetActive(false);
        if (farmTabsRoot) farmTabsRoot.SetActive(true);

        // Close ALL panels under CentreArea
        if (centreAreaRoot)
        {
            for (int i = 0; i < centreAreaRoot.childCount; i++)
            {
                centreAreaRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        // Open requested footer panel (or none if Panel.None)
        if (inventoryPanel) inventoryPanel.SetActive(p == Panel.Inventory);
        if (dutiesPanel) dutiesPanel.SetActive(p == Panel.Duties);
        if (storePanel) storePanel.SetActive(p == Panel.Store);

        // Update background highlights
        if (bgInventory) bgInventory.SetActive(p == Panel.Inventory);
        if (bgDuties) bgDuties.SetActive(p == Panel.Duties);
        if (bgStore) bgStore.SetActive(p == Panel.Store);

        // ✅ When Duties panel is shown, show bgDuties background and HIDE the start icon
        if (p == Panel.Duties)
        {
            if (bgDuties != null) bgDuties.SetActive(true);
            if (footerStart != null) footerStart.gameObject.SetActive(false); // Hide icon when panel is open
        }
        else
        {
            // For other panels, update icon visibility based on work status
            UpdateDutiesButtonVisibility();
        }

        // Clear UI focus (removes highlighted state)
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Clear all footer button background highlights
    /// </summary>
    /// <param name="closePanels">If true, also closes the footer panels</param>
    public void ClearFooterSelection(bool closePanels = false)
    {
        currentPanel = Panel.None;
        
        // Optionally close footer panels too
        if (closePanels)
        {
            if (inventoryPanel) inventoryPanel.SetActive(false);
            if (dutiesPanel) dutiesPanel.SetActive(false);
            if (storePanel) storePanel.SetActive(false);
        }

        // Hide all background highlights
        if (bgInventory) bgInventory.SetActive(false);
        if (bgDuties) bgDuties.SetActive(false);
        if (bgStore) bgStore.SetActive(false);

        // Update button visibility
        UpdateDutiesButtonVisibility();

        // Clear UI focus (removes highlighted state)
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
        
        Debug.Log("✅ Footer selection cleared");
    }

    // Public methods for Button OnClick
    public void ShowInventory() => SwitchTo(Panel.Inventory);
    public void ShowDuties() => SwitchTo(Panel.Duties);
    public void ShowStore() => SwitchTo(Panel.Store);
    public void ShowNone() => SwitchTo(Panel.None);
}