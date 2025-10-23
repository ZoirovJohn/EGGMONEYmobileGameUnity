using UnityEngine;
using UnityEngine.UI;

public class ScrollbarToFill : MonoBehaviour
{
    public Scrollbar scrollbar;
    public Image fillImage;

    void Update()
    {
        fillImage.fillAmount = scrollbar.value;
    }
}
