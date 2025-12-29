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
            if (gridLayout == null)
            {
                gridLayout = GetComponent<GridLayoutGroup>();
                if (gridLayout != null)
                {
                    Debug.Log("✅ Found GridLayoutGroup on this GameObject");
                }
            }
            
            if (viewport == null)
            {
                Transform parent = transform.parent;
                while (parent != null)
                {
                    if (parent.name.ToLower().Contains("viewport"))
                    {
                        viewport = parent.GetComponent<RectTransform>();
                        if (viewport != null)
                        {
                            break;
                        }
                    }
                    parent = parent.parent;
                }
            }
        }
        
        if (gridLayout == null)
        {
            return;
        }
        
        lastScreenWidth = Screen.width;
    }

    void Start()
    {
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
            return;
        }
        
        if (gridLayout == null)
        {
            return;
        }
        
        Canvas.ForceUpdateCanvases();
        float viewportWidth = viewport.rect.width;
        
        if (viewportWidth <= 0)
        {
            StartCoroutine(RetryUpdateCellSizes());
            return;
        }
        
        float cellWidth = viewportWidth;
        float cellHeight = cellWidth / heightDivider;
        
        Vector2 newCellSize = new Vector2(cellWidth, cellHeight);
        if (Vector2.Distance(gridLayout.cellSize, newCellSize) > 0.1f)
        {
            gridLayout.cellSize = newCellSize;
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridLayout.GetComponent<RectTransform>());
        }
    }

    IEnumerator RetryUpdateCellSizes()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateCellSizes();
    }

    [ContextMenu("Update Cell Sizes")]
    public void ManualUpdate()
    {
        UpdateCellSizes();
    }

    public void RefreshSizes()
    {
        UpdateCellSizes();
    }
    
    void OnValidate()
    {
        if (Application.isPlaying && gridLayout != null && viewport != null)
        {
            UpdateCellSizes();
        }
    }
}