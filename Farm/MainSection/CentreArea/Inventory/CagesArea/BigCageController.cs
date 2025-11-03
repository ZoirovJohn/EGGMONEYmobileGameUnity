using UnityEngine;
using UnityEngine.UI;

public class BigCageController : MonoBehaviour
{
    public GameObject bigCage;  // root BigCage
    public Button closeBtn;     // CloseBtn inside BigCage

    void Start()
    {
        if (closeBtn != null)
            closeBtn.onClick.AddListener(CloseBigCage);
    }

    void CloseBigCage()
    {
        if (bigCage != null)
            bigCage.SetActive(false); // closes entire BigCage
    }
}
