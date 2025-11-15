using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryGridManager : MonoBehaviour
{
    [Header("Grid References")]
    public GridLayoutGroup grid;
    public RectTransform viewport;

    [Header("Managers")]
    public InventoryManager inventoryManager;
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
        
        // ✅ Load inventory from backend
        LoadInventoryFromBackend();
        
        Debug.Log("🎉 Inventory grid initialization complete!");
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

        // Auto-find managers if not assigned
        if (inventoryManager == null)
        {
            inventoryManager = FindAnyObjectByType<InventoryManager>(FindObjectsInactive.Include);
            if (inventoryManager == null)
                Debug.LogWarning("⚠️ InventoryManager not found!");
        }

        if (playerWallet == null)
        {
            playerWallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
            if (playerWallet == null)
                Debug.LogWarning("⚠️ PlayerWallet not found!");
        }

        Debug.Log($"✅ All references assigned");
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

    // ✅ Load inventory from backend
    void LoadInventoryFromBackend()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("⚠️ Cannot load inventory: InventoryManager is null");
            return;
        }

        Debug.Log("📦 Loading inventory from backend...");

        inventoryManager.GetInventory(
            onSuccess: (response) =>
            {
                Debug.Log("✅ Inventory loaded successfully from backend!");
                
                // Refresh the grid display after inventory updates
                RefreshInventoryDisplay();
            },
            onError: (err) =>
            {
                Debug.LogError($"❌ Failed to load inventory: {err}");
            }
        );
    }

    // ✅ Refresh inventory display (call after inventory changes)
    void RefreshInventoryDisplay()
    {
        if (playerWallet == null || grid == null)
        {
            Debug.LogWarning("⚠️ Cannot refresh display: missing references");
            return;
        }

        // Update all inventory cells with current quantities
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
                    Debug.Log($"📦 Updated {cellId.productId}: {quantity}");
                }
            }
        }
        
        // Force layout update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
        
        Debug.Log("🔄 Inventory display refreshed");
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

    // PUBLIC: Reload inventory from backend (e.g., after purchase)
    public void ReloadInventory()
    {
        LoadInventoryFromBackend();
    }
}