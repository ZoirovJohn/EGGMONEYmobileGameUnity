using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ManagementMenuController))]
public class OpenManagementOnClickSelf : MonoBehaviour, IPointerClickHandler
{
    ManagementMenuController _ctrl;
    void Awake() => _ctrl = GetComponent<ManagementMenuController>();
    public void OnPointerClick(PointerEventData e) => _ctrl.OpenManagement();
}
