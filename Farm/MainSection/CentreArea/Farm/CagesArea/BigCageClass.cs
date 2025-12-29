using UnityEngine;
using UnityEngine.UI;

public class BigCageClass : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button closeCageButton;
    [SerializeField] private InventoryItemsBarChanger inventoryBarChanger;

    private void Start()
    {
        if (closeCageButton != null)
        {
            closeCageButton.onClick.AddListener(CloseCage);
        }
        else
        {
            Debug.LogWarning("⚠️ CloseCageButton is not assigned!");
        }
    }

    private void CloseCage()
    {
        if (inventoryBarChanger != null)
        {
            inventoryBarChanger.DefaultBannerMethod();
        }
        else
        {
            Debug.LogWarning("⚠️ InventoryBarChanger is not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (closeCageButton != null)
        {
            closeCageButton.onClick.RemoveListener(CloseCage);
        }
    }
}