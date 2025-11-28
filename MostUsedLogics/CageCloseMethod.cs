using UnityEngine;
using UnityEngine.UI;

public class CageCloseClass : MonoBehaviour
{
    public GameObject bigCage;          // root BigCage
    public GameObject bigCageInside1;   // BigCageInside1
    public GameObject bigCageInside2;   // BigCageInside2
    public Button closeBtn;             // CloseBtn inside BigCage

    void Start()
    {
        if (closeBtn != null)
            closeBtn.onClick.AddListener(CloseBigCage);
    }

    void CloseBigCage()
    {
        if (bigCageInside2 != null)
            bigCageInside2.SetActive(false); // close BigCageInside2
        
        if (bigCageInside1 != null)
            bigCageInside1.SetActive(true);  // open BigCageInside1
        
        if (bigCage != null)
            bigCage.SetActive(false);        // closes entire BigCage
    }
}