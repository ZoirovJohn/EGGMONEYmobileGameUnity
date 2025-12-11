using UnityEngine;
using UnityEngine.UI;

public class SwapDeliveryUIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button swapButton;
    public Button deliveryButton;

    [Header("Panels")]
    public GameObject swapPanel;
    public GameObject deliveryPanel;

    void Start()
    {
        // Add listeners
        if (swapButton != null)
            swapButton.onClick.AddListener(OpenSwapPanel);

        if (deliveryButton != null)
            deliveryButton.onClick.AddListener(OpenDeliveryPanel);

        // Optional: start with both disabled or one enabled
        OpenSwapPanel();   // Default start on Swap
        // OR:
        //CloseAllPanels();
    }

    void OpenSwapPanel()
    {
        if (swapPanel != null) swapPanel.SetActive(true);
        if (deliveryPanel != null) deliveryPanel.SetActive(false);
    }

    void OpenDeliveryPanel()
    {
        if (swapPanel != null) swapPanel.SetActive(false);
        if (deliveryPanel != null) deliveryPanel.SetActive(true);
    }

    void CloseAllPanels()
    {
        if (swapPanel != null) swapPanel.SetActive(false);
        if (deliveryPanel != null) deliveryPanel.SetActive(false);
    }
}
