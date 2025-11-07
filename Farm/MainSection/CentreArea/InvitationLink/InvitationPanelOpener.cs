using UnityEngine;
using UnityEngine.EventSystems;

public class InvitationPanelOpener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ManagementMenuController managementController;
    [SerializeField] GameObject invitationPanel;  // QR Panel only

    [Header("Button visuals")]
    [SerializeField] GameObject invitationBtnOffClick; // visible when closed
    [SerializeField] GameObject invitationBtnOnClick;  // visible when opened

    public void OpenInvitation()
    {
        // ✅ Close management UI first (this hides all center panels too)
        if (managementController)
            managementController.CloseManagement();

        // ✅ Show Invitation panel only
        if (invitationPanel) invitationPanel.SetActive(true);

        // ✅ Button state
        if (invitationBtnOffClick) invitationBtnOffClick.SetActive(false);
        if (invitationBtnOnClick)  invitationBtnOnClick.SetActive(true);

        // ✅ Remove UI Focus highlight
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void CloseInvitation()
    {
        if (invitationPanel) invitationPanel.SetActive(false);

        if (invitationBtnOffClick) invitationBtnOffClick.SetActive(true);
        if (invitationBtnOnClick)  invitationBtnOnClick.SetActive(false);
    }

}
