using UnityEngine;
using UnityEngine.UI;

public class BigCageClass : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button closeCageButton;
    [SerializeField] private InventoryItemsBarChanger inventoryBarChanger;

    private void Start()
    {
        // Setup close button listener
        if (closeCageButton != null)
        {
            closeCageButton.onClick.AddListener(CloseCage);
            Debug.Log("✅ Close cage button listener added");
        }
        else
        {
            Debug.LogWarning("⚠️ CloseCageButton is not assigned!");
        }
    }

    private void CloseCage()
    {
        // Switch back to default banner
        if (inventoryBarChanger != null)
        {
            inventoryBarChanger.DefaultBannerMethod();
            Debug.Log("🎨 Switched to DefaultBanner via InventoryBarChanger");
        }
        else
        {
            Debug.LogWarning("⚠️ InventoryBarChanger is not assigned!");
        }
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (closeCageButton != null)
        {
            closeCageButton.onClick.RemoveListener(CloseCage);
        }
    }
}