using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryGridManager : MonoBehaviour
{
    [Header("Grid References")]
    public GridLayoutGroup grid;
    public RectTransform viewport;

    [Header("Wallet Reference (Data Source)")]
    public PlayerWallet playerWallet;

    void Start()
    {
        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        // Wait for canvas to fully initialize
        yield return null;
        Canvas.ForceUpdateCanvases();
        
        // Validate references
        if (!ValidateReferences())
        {
            Debug.LogError("❌ Reference validation failed!");
            yield break;
        }

        // Setup grid layout
        SetupGrid();
        
        // ✅ Initial display from wallet
        RefreshInventoryDisplay();
    }

    void OnEnable()
    {
        // ✅ Subscribe to wallet events for real-time updates
        if (playerWallet != null)
        {
            playerWallet.OnItemChanged += OnWalletItemChanged;
            playerWallet.OnProfileChanged += OnWalletProfileChanged;
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerWallet is NULL in OnEnable - cannot subscribe to events!");
        }
    }

    void OnDisable()
    {
        // ✅ Unsubscribe from wallet events
        if (playerWallet != null)
        {
            playerWallet.OnItemChanged -= OnWalletItemChanged;
            playerWallet.OnProfileChanged -= OnWalletProfileChanged;
        }
    }

    // ✅ Called automatically when any item changes in wallet
    void OnWalletItemChanged(string itemId, int newQuantity)
    {
        UpdateSingleItem(itemId, newQuantity);
    }

    // ✅ Called when profile changes (fallback - refresh all)
    void OnWalletProfileChanged()
    {
        RefreshInventoryDisplay();
    }

    // ✅ Update a single item in the grid (efficient)
    void UpdateSingleItem(string itemId, int quantity)
    {
        if (grid == null)
        {
            Debug.LogWarning("⚠️ Grid is null, cannot update item");
            return;
        }

        bool found = false;
        foreach (Transform child in grid.transform)
        {
            InventoryCellId cellId = child.GetComponent<InventoryCellId>();
            if (cellId != null)
            {
                if (cellId.productId == itemId)
                {
                    TMP_Text quantityText = child.GetComponentInChildren<TMP_Text>();
                    if (quantityText != null)
                    {
                        quantityText.text = quantity > 0 ? quantity.ToString() : "0";
                        found = true;
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Cell for {itemId} found but has no TMP_Text child!");
                    }
                    break; // Found it, no need to continue
                }
            }
        }
        
        if (!found)
        {
            Debug.LogWarning($"⚠️ No cell found for productId: '{itemId}'");
        }
    }

    bool ValidateReferences()
    {
        if (grid == null)
        {
            Debug.LogError("❌ GridLayoutGroup is NULL! Assign it in Inspector!");
            return false;
        }
        
        if (viewport == null)
        {
            Debug.LogError("❌ Viewport is NULL! Assign it in Inspector!");
            return false;
        }

        // Auto-find wallet if not assigned
        if (playerWallet == null)
        {
            playerWallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
            
            if (playerWallet == null)
            {
                Debug.LogError("❌ PlayerWallet not found in scene!");
                return false;
            }
            else
            {
                Debug.Log($"✅ Found PlayerWallet: {playerWallet.gameObject.name} (Instance: {playerWallet.GetInstanceID()})");
            }
        }
        else
        {
            Debug.Log($"✅ PlayerWallet already assigned: {playerWallet.gameObject.name} (Instance: {playerWallet.GetInstanceID()})");
        }
        return true;
    }

    void SetupGrid()
    {
        // Force canvas update to ensure viewport has correct size
        Canvas.ForceUpdateCanvases();
        
        float viewportWidth = viewport.rect.width;
        
        // Fallback to screen width if viewport width is invalid
        if (viewportWidth <= 0)
        {
            Debug.LogWarning("⚠️ Viewport width invalid, using Screen.width as fallback");
            viewportWidth = Screen.width;
        }
        
        float spacing = grid.spacing.x;

        // Responsive column count (3 for narrow, 4 for wide)
        bool isTablet = (Screen.dpi < 260 && Mathf.Min(Screen.width, Screen.height) >= 900);

        int columns = isTablet ? 4 : 3;

        // Calculate cell size
        float totalSpacing = spacing * (columns - 1);
        float cellSize = (viewportWidth - totalSpacing) / columns;

        // Apply to grid
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = new Vector2(cellSize, cellSize);

        // Center alignment with padding
        float totalUsed = columns * cellSize + spacing * (columns - 1);
        float sidePadding = Mathf.Max(0, (viewportWidth - totalUsed) / 2f);
        grid.padding.left = Mathf.RoundToInt(sidePadding);
        grid.padding.right = Mathf.RoundToInt(sidePadding);

        // Set grid width
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportWidth);
        
        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    }

    // ✅ Refresh all items from PlayerWallet
    public void RefreshInventoryDisplay()
    {
        if (playerWallet == null || grid == null)
        {
            Debug.LogWarning("⚠️ Cannot refresh display: missing references");
            return;
        }

        int cellCount = 0;
        // Update all inventory cells with current quantities from wallet
        foreach (Transform child in grid.transform)
        {
            InventoryCellId cellId = child.GetComponent<InventoryCellId>();
            if (cellId != null && !string.IsNullOrEmpty(cellId.productId))
            {
                int quantity = playerWallet.GetItemCount(cellId.productId);
                
                // Find quantity text in the cell
                TMP_Text quantityText = child.GetComponentInChildren<TMP_Text>();
                if (quantityText != null)
                {
                    quantityText.text = quantity > 0 ? quantity.ToString() : "0";
                    cellCount++;
                }
            }
        }
        
        // Force layout update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    }

    // PUBLIC: Handle screen rotation or resize
    public void OnScreenSizeChanged()
    {
        SetupGrid();
    }

    // PUBLIC: Manually refresh grid layout if needed
    public void RefreshLayout()
    {
        SetupGrid();
    }

    // PUBLIC: Manually refresh from wallet (for external calls)
    public void RefreshFromWallet()
    {
        RefreshInventoryDisplay();
    }
}