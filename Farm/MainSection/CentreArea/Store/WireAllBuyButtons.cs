using UnityEngine;
using UnityEngine.UI;

public class WirePerCellPopups : MonoBehaviour
{
    Transform currentOpenPopup; // track which popup is open

    void Start()
    {
        var allButtons = GetComponentsInChildren<Button>(true);

        foreach (var btn in allButtons)
        {
            if (btn.name != "BtnBuy") continue;

            // capture a local copy to avoid closure issues
            Button localBtn = btn;
            localBtn.onClick.AddListener(() => OnBuyClicked(localBtn));
        }
    }

    void OnBuyClicked(Button btn)
    {
        // 1) Go up to the ItemCell (BtnBuy is a direct child per your structure)
        Transform cell = btn.transform.parent;

        // 2) Find this cell's popup
        Transform popupTr = cell.Find("PurchasePopup");
        if (popupTr == null)
        {
            Debug.LogWarning("[Store] No PurchasePopup found under " + cell.name);
            return;
        }

        // If another popup is open, close it first
        if (currentOpenPopup != null && currentOpenPopup != popupTr)
            currentOpenPopup.gameObject.SetActive(false);

        // Toggle this popup
        bool willOpen = !popupTr.gameObject.activeSelf;
        popupTr.gameObject.SetActive(willOpen);

        if (willOpen)
        {
            // make sure it draws above siblings inside the same cell
            popupTr.SetAsLastSibling();
            currentOpenPopup = popupTr;

            // OPTIONAL: wire a close button inside the popup if it exists
            Transform closeTr = popupTr.Find("BtnClose");
            if (closeTr != null)
            {
                var closeBtn = closeTr.GetComponent<Button>();
                if (closeBtn != null)
                {
                    // ensure only this close behavior is attached
                    closeBtn.onClick.RemoveAllListeners();
                    closeBtn.onClick.AddListener(() =>
                    {
                        popupTr.gameObject.SetActive(false);
                        if (currentOpenPopup == popupTr) currentOpenPopup = null;
                    });
                }
            }
        }
        else
        {
            if (currentOpenPopup == popupTr) currentOpenPopup = null;
        }
    }
}
