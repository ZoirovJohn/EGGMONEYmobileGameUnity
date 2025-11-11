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

    [Header("Button Background Images")]
    [SerializeField] GameObject bgInventory;
    [SerializeField] GameObject bgDuties;
    [SerializeField] GameObject bgStore;

    public enum Panel { None, Inventory, Duties, Store }
    
    [Header("Default on enable")]
    [SerializeField] Panel defaultPanel = Panel.None;

    void Awake()
    {
        if (btnInventory) btnInventory.onClick.AddListener(ShowInventory);
        if (btnDuties) btnDuties.onClick.AddListener(ShowDuties);
        if (btnStore) btnStore.onClick.AddListener(ShowStore);
    }

    void OnEnable()
    {
        SwitchTo(defaultPanel);
    }

    void SwitchTo(Panel p)
    {
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

        // Clear UI focus (removes highlighted state)
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    public void ClearFooterSelection(bool closePanels = false)
    {
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

        // Clear UI focus (removes highlighted state)
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    // Public methods for Button OnClick
    public void ShowInventory() => SwitchTo(Panel.Inventory);
    public void ShowDuties() => SwitchTo(Panel.Duties);
    public void ShowStore() => SwitchTo(Panel.Store);
    public void ShowNone() => SwitchTo(Panel.None);
}