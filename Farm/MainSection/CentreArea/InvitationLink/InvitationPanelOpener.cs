using UnityEngine;
using UnityEngine.EventSystems;

public class InvitationPanelOpener : MonoBehaviour
{
    [Header("Layout roots")]
    [SerializeField] GameObject farmTabsRoot;       // show when opening invitation
    [SerializeField] GameObject managementTabsRoot; // hide when opening invitation (your ManagementTabs container)
    [SerializeField] Transform centreAreaRoot;      // parent of ALL center panels

    [Header("Target panel")]
    [SerializeField] GameObject invitationPanel;    // the QR panel to show

    [Header("Optional")]
    [SerializeField] FooterPanelSwitcher footerSwitcher; // to clear footer selected state

    public void OpenInvitation()
    {
        // header strips
        if (managementTabsRoot) managementTabsRoot.SetActive(false);
        if (farmTabsRoot)       farmTabsRoot.SetActive(true);

        // close all center panels
        if (centreAreaRoot)
            for (int i = 0; i < centreAreaRoot.childCount; i++)
                centreAreaRoot.GetChild(i).gameObject.SetActive(false);

        // open the invitation panel
        if (invitationPanel) invitationPanel.SetActive(true);

        // make footer look neutral (optional)
        footerSwitcher?.ClearFooterSelection(false);

        // clear UI focus highlight
        EventSystem.current?.SetSelectedGameObject(null);
    }
}
