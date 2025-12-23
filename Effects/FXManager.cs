using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance;

    [Header("FX Prefabs")]
    [SerializeField] GameObject purchaseFXPrefab;

    [Header("FX Parent (world-space empty object)")]
    [SerializeField] Transform fxParent; // your "Effects" object

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ⭐ Spawn FX at SCREEN CENTER (world space)
    public void PlayPurchaseFX_Center()
    {
        if (!purchaseFXPrefab || !fxParent)
        {
            Debug.LogWarning("FXManager: Missing prefab or parent");
            return;
        }

        Camera cam = Camera.main;
        if (!cam) return;

        // Center of screen → world position
        Vector3 worldCenter = cam.ViewportToWorldPoint(
            new Vector3(0.5f, 0.5f, cam.nearClipPlane + 5f)
        );

        Instantiate(purchaseFXPrefab, worldCenter, Quaternion.identity, fxParent);
    }
}
