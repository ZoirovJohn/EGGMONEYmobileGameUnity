using UnityEngine;
using UnityEngine.UI;

public class FooterPanelSwitcher : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject dutiesPanel;   // DailyDutyPanel
    [SerializeField] GameObject storePanel;

    [Header("Footer Buttons (optional auto-wire)")]
    [SerializeField] Button btnInventory;
    [SerializeField] Button btnDuties;
    [SerializeField] Button btnStore;

    public enum Panel { Inventory, Duties, Store }
    [Header("Default on enable")]
    [SerializeField] Panel defaultPanel = Panel.Store;

    void Awake()
    {
        if (btnInventory) btnInventory.onClick.AddListener(ShowInventory);
        if (btnDuties)    btnDuties   .onClick.AddListener(ShowDuties);
        if (btnStore)     btnStore    .onClick.AddListener(ShowStore);
    }

    void OnEnable() => SwitchTo(defaultPanel);

    void SwitchTo(Panel p)
    {
        if (inventoryPanel) inventoryPanel.SetActive(p == Panel.Inventory);
        if (dutiesPanel)    dutiesPanel   .SetActive(p == Panel.Duties);
        if (storePanel)     storePanel    .SetActive(p == Panel.Store);

        // (Optional) simple visual feedback: disable the active tab's button
        if (btnInventory) btnInventory.interactable = p != Panel.Inventory;
        if (btnDuties)    btnDuties   .interactable = p != Panel.Duties;
        if (btnStore)     btnStore    .interactable = p != Panel.Store;
    }

    // Public methods for hooking up via the Button OnClick list (if you prefer manual wiring)
    public void ShowInventory() => SwitchTo(Panel.Inventory);
    public void ShowDuties()    => SwitchTo(Panel.Duties);
    public void ShowStore()     => SwitchTo(Panel.Store);
}
