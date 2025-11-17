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
        Debug.Log("🚀 InventoryGridManager Start() - Beginning initialization");
        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        Debug.Log("⏳ Waiting one frame for canvas initialization...");
        
        // Wait for canvas to fully initialize
        yield return null;
        Canvas.ForceUpdateCanvases();
        
        Debug.Log("✅ Canvas updated, starting validation...");
        
        // Validate references
        if (!ValidateReferences())
        {
            Debug.LogError("❌ Reference validation failed!");
            yield break;
        }

        Debug.Log($"✅ Grid has {grid.transform.childCount} item cells");
        
        // Setup grid layout
        SetupGrid();
        
        // ✅ Initial display from wallet
        RefreshInventoryDisplay();
        
        Debug.Log("🎉 Inventory grid initialization complete!");
    }

    void OnEnable()
    {
        Debug.Log("🔌 InventoryGridManager OnEnable() called");
        
        // ✅ Subscribe to wallet events for real-time updates
        if (playerWallet != null)
        {
            Debug.Log($"✅ Subscribing to PlayerWallet events (Wallet instance: {playerWallet.GetInstanceID()})");
            playerWallet.OnItemChanged += OnWalletItemChanged;
            playerWallet.OnProfileChanged += OnWalletProfileChanged;
            Debug.Log("✅ Successfully subscribed to PlayerWallet events");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerWallet is NULL in OnEnable - cannot subscribe to events!");
        }
    }

    void OnDisable()
    {
        Debug.Log("🔌 InventoryGridManager OnDisable() called");
        
        // ✅ Unsubscribe from wallet events
        if (playerWallet != null)
        {
            playerWallet.OnItemChanged -= OnWalletItemChanged;
            playerWallet.OnProfileChanged -= OnWalletProfileChanged;
            Debug.Log("🔌 Unsubscribed from PlayerWallet events");
        }
    }

    // ✅ Called automatically when any item changes in wallet
    void OnWalletItemChanged(string itemId, int newQuantity)
    {
        Debug.Log($"🔔🔔🔔 [InventoryGridManager] Wallet item changed EVENT RECEIVED: {itemId} = {newQuantity}");
        UpdateSingleItem(itemId, newQuantity);
    }

    // ✅ Called when profile changes (fallback - refresh all)
    void OnWalletProfileChanged()
    {
        Debug.Log("🔔🔔🔔 [InventoryGridManager] Wallet profile changed EVENT RECEIVED - refreshing all items");
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

        Debug.Log($"🔍 Searching for cell with productId: '{itemId}'");
        
        bool found = false;
        foreach (Transform child in grid.transform)
        {
            InventoryCellId cellId = child.GetComponent<InventoryCellId>();
            if (cellId != null)
            {
                Debug.Log($"   📦 Checking cell: '{cellId.productId}' vs '{itemId}'");
                
                if (cellId.productId == itemId)
                {
                    TMP_Text quantityText = child.GetComponentInChildren<TMP_Text>();
                    if (quantityText != null)
                    {
                        quantityText.text = quantity > 0 ? quantity.ToString() : "0";
                        Debug.Log($"✅✅✅ Updated UI: {itemId} = {quantity} (TEXT CHANGED)");
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
            Debug.Log("🔍 PlayerWallet not assigned, searching...");
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

        Debug.Log($"✅ All references validated");
        return true;
    }

    void SetupGrid()
    {
        // Force canvas update to ensure viewport has correct size
        Canvas.ForceUpdateCanvases();
        
        float viewportWidth = viewport.rect.width;
        
        Debug.Log($"🔍 Viewport width: {viewportWidth}, Screen.width: {Screen.width}");
        
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

        Debug.Log(isTablet ? "📱 Detected Tablet → 4 columns" : "📱 Detected Phone → 3 columns");

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
        
        Debug.Log($"📐 Grid setup: {columns} columns, cell size: {cellSize}x{cellSize}, spacing: {spacing}");
        
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

        Debug.Log("🔄 Refreshing all inventory items from PlayerWallet...");

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
                    Debug.Log($"   📦 Cell {cellCount}: {cellId.productId} = {quantity}");
                    cellCount++;
                }
            }
        }
        
        Debug.Log($"✅ Refreshed {cellCount} inventory cells from wallet");
        
        // Force layout update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    }

    // PUBLIC: Handle screen rotation or resize
    public void OnScreenSizeChanged()
    {
        SetupGrid();
        Debug.Log("🔄 Inventory grid layout refreshed");
    }

    // PUBLIC: Manually refresh grid layout if needed
    public void RefreshLayout()
    {
        SetupGrid();
    }

    // PUBLIC: Manually refresh from wallet (for external calls)
    public void RefreshFromWallet()
    {
        Debug.Log("🔄 RefreshFromWallet() called externally");
        RefreshInventoryDisplay();
    }
}