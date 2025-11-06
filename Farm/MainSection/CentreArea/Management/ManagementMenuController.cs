using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class ManagementMenuController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] GameObject farmTabsRoot;      // strip to HIDE when opening management
    [SerializeField] GameObject panelButtonsRoot;  // container with 6 management buttons (to SHOW)
    [SerializeField] Transform centreAreaRoot;     // parent of ALL center panels (mgmt + footer)

    [Header("Optional extras")]
    [SerializeField] List<GameObject> centrePanels = new List<GameObject>(); // any specific panels to also hide
    [SerializeField] GameObject defaultManagementPanel;  // fallback (GeneralPanel) if no TabSwitcher
    [SerializeField] MonoBehaviour managementTabSwitcher; // assign the object that has Select(int)
    [SerializeField] int defaultTabIndex = 0;
    [SerializeField] bool autoOpenDefaultTabOnShow = true;

    [Header("Management Buttons")]
    [SerializeField] GameObject managementBtnOffClick; // shows when management closed
    [SerializeField] GameObject managementBtnOnClick;  // shows when management open

    [Header("Farm Panel")]
    [SerializeField] GameObject farmPanel; // assign in Inspector

    public void ToggleManagement()  => SetManagementVisible(!(panelButtonsRoot && panelButtonsRoot.activeSelf));
    public void OpenManagement()    => SetManagementVisible(true);
    public void CloseManagement()   => SetManagementVisible(false);

    void SetManagementVisible(bool visible)
    {
        // Toggle main menu roots
        if (panelButtonsRoot) panelButtonsRoot.SetActive(visible);
        if (farmTabsRoot)     farmTabsRoot.SetActive(!visible);

        // Toggle management ON/OFF buttons
        if (managementBtnOffClick) managementBtnOffClick.SetActive(!visible);
        if (managementBtnOnClick)  managementBtnOnClick.SetActive(visible);

        // Hide all center panels except the menu itself
        HideAllCentrePanels(visible ? panelButtonsRoot : null);

        if (visible)
        {
            // Auto open default tab
            if (autoOpenDefaultTabOnShow)
            {
                if (managementTabSwitcher && TrySelectOn(managementTabSwitcher, Mathf.Max(0, defaultTabIndex)))
                    return;

                if (defaultManagementPanel) defaultManagementPanel.SetActive(true);
            }
        }
        else
        {
            // ✅ When closing Management → ENABLE Farm Panel again
            if (farmPanel) farmPanel.SetActive(true);
        }
    }



    void HideAllCentrePanels(GameObject except = null)
    {
        foreach (var p in centrePanels) if (p && p != except) p.SetActive(false);

        if (centreAreaRoot)
        {
            for (int i = 0; i < centreAreaRoot.childCount; i++)
            {
                var child = centreAreaRoot.GetChild(i).gameObject;
                if (child && child != except) child.SetActive(false);
            }
        }
    }

    bool TrySelectOn(MonoBehaviour comp, int index)
    {
        var t = comp.GetType();
        var mi = t.GetMethod("Select", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null);
        if (mi == null) return false;
        mi.Invoke(comp, new object[] { index });
        return true;
        }
}
