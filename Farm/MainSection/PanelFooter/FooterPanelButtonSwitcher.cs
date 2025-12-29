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
    [SerializeField] Button footerStart; 

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
        if (footerStart) footerStart.onClick.AddListener(ShowDuties); 
        
        if (!playerWallet) 
            playerWallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
    }

    void OnEnable()
    {
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged += UpdateDutiesButtonVisibility;
        }
        
        UpdateDutiesButtonVisibility();
        SwitchTo(defaultPanel);
    }

    void OnDisable()
    {
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged -= UpdateDutiesButtonVisibility;
        }
    }

    private void UpdateDutiesButtonVisibility()
    {
        if (playerWallet == null) return;

        bool hasWork = playerWallet.HensWithEggReady > 0 || 
                       playerWallet.HensNeedingFood > 0 || 
                       playerWallet.HensNeedingClean > 0;

        if (btnDuties != null)
        {
            btnDuties.gameObject.SetActive(true);
        }
        
        if (footerStart != null)
        {
            footerStart.gameObject.SetActive(hasWork);
        }
    }

    void SwitchTo(Panel p)
    {
        currentPanel = p;
        
        if (panelButtonsRoot) panelButtonsRoot.SetActive(false);
        if (farmTabsRoot) farmTabsRoot.SetActive(true);
        if (centreAreaRoot)
        {
            for (int i = 0; i < centreAreaRoot.childCount; i++)
            {
                centreAreaRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (inventoryPanel) inventoryPanel.SetActive(p == Panel.Inventory);
        if (dutiesPanel) dutiesPanel.SetActive(p == Panel.Duties);
        if (storePanel) storePanel.SetActive(p == Panel.Store);
        if (bgInventory) bgInventory.SetActive(p == Panel.Inventory);
        if (bgDuties) bgDuties.SetActive(p == Panel.Duties);
        if (bgStore) bgStore.SetActive(p == Panel.Store);
        if (p == Panel.Duties)
        {
            if (bgDuties != null) bgDuties.SetActive(true);
            if (footerStart != null) footerStart.gameObject.SetActive(false); 
        }
        else
        {
            UpdateDutiesButtonVisibility();
        }

        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Clear all footer button background highlights
    /// </summary>
    /// <param name="closePanels">If true, also closes the footer panels</param>
    public void ClearFooterSelection(bool closePanels = false)
    {
        currentPanel = Panel.None;
        
        if (closePanels)
        {
            if (inventoryPanel) inventoryPanel.SetActive(false);
            if (dutiesPanel) dutiesPanel.SetActive(false);
            if (storePanel) storePanel.SetActive(false);
        }

        if (bgInventory) bgInventory.SetActive(false);
        if (bgDuties) bgDuties.SetActive(false);
        if (bgStore) bgStore.SetActive(false);

        UpdateDutiesButtonVisibility();
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    public void ShowInventory() => SwitchTo(Panel.Inventory);
    public void ShowDuties() => SwitchTo(Panel.Duties);
    public void ShowStore() => SwitchTo(Panel.Store);
    public void ShowNone() => SwitchTo(Panel.None);
}