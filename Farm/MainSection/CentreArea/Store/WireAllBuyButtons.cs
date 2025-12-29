using UnityEngine;
using UnityEngine.UI;

public class WirePerCellPopups : MonoBehaviour
{
    Transform currentOpenPopup; 

    void Start()
    {
        var allButtons = GetComponentsInChildren<Button>(true);

        foreach (var btn in allButtons)
        {
            if (btn.name != "BtnBuy") continue;

            Button localBtn = btn;
            localBtn.onClick.AddListener(() => OnBuyClicked(localBtn));
        }
    }

    void OnBuyClicked(Button btn)
    {
        Transform cell = btn.transform.parent;
        Transform popupTr = cell.Find("PurchasePopup");
        if (popupTr == null)
        {
            return;
        }

        if (currentOpenPopup != null && currentOpenPopup != popupTr)
            currentOpenPopup.gameObject.SetActive(false);
        bool willOpen = !popupTr.gameObject.activeSelf;
        popupTr.gameObject.SetActive(willOpen);

        if (willOpen)
        {
            popupTr.SetAsLastSibling();
            currentOpenPopup = popupTr;
            Transform closeTr = popupTr.Find("BtnClose");
            if (closeTr != null)
            {
                var closeBtn = closeTr.GetComponent<Button>();
                if (closeBtn != null)
                {
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
