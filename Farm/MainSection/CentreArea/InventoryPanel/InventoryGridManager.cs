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
        yield return null;
        Canvas.ForceUpdateCanvases();
        
        if (!ValidateReferences())
        {
            Debug.LogError("❌ Reference validation failed!");
            yield break;
        }

        SetupGrid();
        
        RefreshInventoryDisplay();
    }

    void OnEnable()
    {
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
        if (playerWallet != null)
        {
            playerWallet.OnItemChanged -= OnWalletItemChanged;
            playerWallet.OnProfileChanged -= OnWalletProfileChanged;
        }
    }

    void OnWalletItemChanged(string itemId, int newQuantity)
    {
        UpdateSingleItem(itemId, newQuantity);
    }

    void OnWalletProfileChanged()
    {
        RefreshInventoryDisplay();
    }

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
                    break;
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
        Canvas.ForceUpdateCanvases();
        
        float viewportWidth = viewport.rect.width;
        
        if (viewportWidth <= 0)
        {
            Debug.LogWarning("⚠️ Viewport width invalid, using Screen.width as fallback");
            viewportWidth = Screen.width;
        }
        
        float spacing = grid.spacing.x;

        bool isTablet = (Screen.dpi < 260 && Mathf.Min(Screen.width, Screen.height) >= 900);

        int columns = isTablet ? 4 : 3;

        float totalSpacing = spacing * (columns - 1);
        float cellSize = (viewportWidth - totalSpacing) / columns;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = new Vector2(cellSize, cellSize);

        float totalUsed = columns * cellSize + spacing * (columns - 1);
        float sidePadding = Mathf.Max(0, (viewportWidth - totalUsed) / 2f);
        grid.padding.left = Mathf.RoundToInt(sidePadding);
        grid.padding.right = Mathf.RoundToInt(sidePadding);

        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportWidth);
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    }

    public void RefreshInventoryDisplay()
    {
        if (playerWallet == null || grid == null)
        {
            Debug.LogWarning("⚠️ Cannot refresh display: missing references");
            return;
        }

        int cellCount = 0;
        foreach (Transform child in grid.transform)
        {
            InventoryCellId cellId = child.GetComponent<InventoryCellId>();
            if (cellId != null && !string.IsNullOrEmpty(cellId.productId))
            {
                int quantity = playerWallet.GetItemCount(cellId.productId);
                
                TMP_Text quantityText = child.GetComponentInChildren<TMP_Text>();
                if (quantityText != null)
                {
                    quantityText.text = quantity > 0 ? quantity.ToString() : "0";
                    cellCount++;
                }
            }
        }
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    }

    public void OnScreenSizeChanged()
    {
        SetupGrid();
    }

    public void RefreshLayout()
    {
        SetupGrid();
    }

    public void RefreshFromWallet()
    {
        RefreshInventoryDisplay();
    }
}