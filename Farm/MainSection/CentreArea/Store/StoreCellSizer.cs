using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Automatically sizes store item cells to match viewport width
/// Height is calculated as width / 2.25
/// Works with GridLayoutGroup - updates the cell size property
/// Attach this to the Content GameObject that has GridLayoutGroup
/// </summary>
public class StoreCellSizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private GridLayoutGroup gridLayout;
    
    [Header("Settings")]
    [SerializeField] private float heightDivider = 2.25f;
    [SerializeField] private bool updateOnScreenResize = true;
    [SerializeField] private bool updateEveryFrame = false;
    
    [Header("Auto Find")]
    [SerializeField] private bool autoFind = true;
    
    private float lastScreenWidth;

    void Awake()
    {
        if (autoFind)
        {
            // If gridLayout not assigned, get from this GameObject
            if (gridLayout == null)
            {
                gridLayout = GetComponent<GridLayoutGroup>();
                if (gridLayout != null)
                {
                    Debug.Log("✅ Found GridLayoutGroup on this GameObject");
                }
            }
            
            // Try to find viewport
            if (viewport == null)
            {
                // Look for parent with "Viewport" in name
                Transform parent = transform.parent;
                while (parent != null)
                {
                    if (parent.name.ToLower().Contains("viewport"))
                    {
                        viewport = parent.GetComponent<RectTransform>();
                        if (viewport != null)
                        {
                            Debug.Log($"✅ Found viewport: {parent.name}");
                            break;
                        }
                    }
                    parent = parent.parent;
                }
            }
        }
        
        if (gridLayout == null)
        {
            Debug.LogError("❌ GridLayoutGroup not found! Please assign it or ensure it exists on this GameObject.");
            return;
        }
        
        lastScreenWidth = Screen.width;
    }

    void Start()
    {
        // Wait one frame for canvas to initialize
        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        
        UpdateCellSizes();
    }

    void Update()
    {
        if (updateEveryFrame)
        {
            UpdateCellSizes();
            return;
        }
        
        if (updateOnScreenResize && Mathf.Abs(Screen.width - lastScreenWidth) > 1f)
        {
            lastScreenWidth = Screen.width;
            UpdateCellSizes();
        }
    }

    void UpdateCellSizes()
    {
        if (viewport == null)
        {
            Debug.LogError("❌ Viewport not assigned!");
            return;
        }
        
        if (gridLayout == null)
        {
            Debug.LogError("❌ GridLayoutGroup not assigned!");
            return;
        }
        
        // Force canvas update to get correct viewport size
        Canvas.ForceUpdateCanvases();
        
        float viewportWidth = viewport.rect.width;
        
        if (viewportWidth <= 0)
        {
            Debug.LogWarning("⚠️ Viewport width is 0, waiting for next frame...");
            StartCoroutine(RetryUpdateCellSizes());
            return;
        }
        
        // Calculate cell dimensions
        float cellWidth = viewportWidth;
        float cellHeight = cellWidth / heightDivider;
        
        // Update GridLayoutGroup cell size
        Vector2 newCellSize = new Vector2(cellWidth, cellHeight);
        
        // Only update if there's a significant change (avoid constant updates)
        if (Vector2.Distance(gridLayout.cellSize, newCellSize) > 0.1f)
        {
            gridLayout.cellSize = newCellSize;
            
            Debug.Log($"📐 Updated GridLayoutGroup Cell Size: {cellWidth:F1} x {cellHeight:F1} (viewport width: {viewportWidth:F1})");
            
            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridLayout.GetComponent<RectTransform>());
        }
    }

    IEnumerator RetryUpdateCellSizes()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateCellSizes();
    }

    // Public method to manually trigger update
    [ContextMenu("Update Cell Sizes")]
    public void ManualUpdate()
    {
        UpdateCellSizes();
        Debug.Log("🔄 Manual update triggered");
    }

    // Call this when new items are added to the store
    public void RefreshSizes()
    {
        UpdateCellSizes();
    }
    
    void OnValidate()
    {
        // Update in editor when values change
        if (Application.isPlaying && gridLayout != null && viewport != null)
        {
            UpdateCellSizes();
        }
    }
}