using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmHeaderManager : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject farmSlotPrefab;
    public GameObject lockSlotPrefab;
    public GameObject dividerPrefab;
    public Button leftButton;
    public Button rightButton;

    [Header("Counts")]
    public int farmCount = 3;
    public int lockCount = 5;

    [Header("Database")]
    public FarmDatabase farmDatabase;
    public FarmGridManager farmGridManager;

    [Header("Responsive Viewport Settings")]
    public int visibleItemsPhone = 4;      // iPhone - show 4 items
    public int visibleItemsTablet = 5;     // iPad - show 5 items
    public int visibleItemsNarrow = 3;     // Z Flip - show 3 items

    [Header("Screen Width Breakpoints (pixels)")]
    public float tabletMinWidth = 768f;    // iPad and larger
    public float narrowMaxWidth = 400f;    // Z Flip and similar narrow phones

    [Header("Scroll Settings")]
    public float scrollSpeed = 8f;

    private float targetNormalizedPos = 0f;
    private float singleStep = 0f;
    private bool isScrolling = false;
    private int currentVisibleItems = 4;
    private List<GameObject> spawned = new List<GameObject>();
    private int selectedFarmIndex = 0;
    private float slotPlusDividerWidth = 0f;

    void Start()
    {
        leftButton.onClick.AddListener(OnLeftClick);
        rightButton.onClick.AddListener(OnRightClick);

        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;

        Refresh();
    }

    void Update()
    {
        if (!isScrolling) return;

        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition,
            targetNormalizedPos,
            Time.deltaTime * scrollSpeed
        );

        if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetNormalizedPos) < 0.002f)
        {
            scrollRect.horizontalNormalizedPosition = targetNormalizedPos;
            isScrolling = false;
            UpdateButtonState();
        }
    }

    public void Refresh()
    {
        DetermineVisibleItems();
        BuildItems();
        ComputeStep();
        UpdateButtonState();
    }

    void DetermineVisibleItems()
    {
        float screenWidth = Screen.width;

        // Tablet (iPad, etc.) - 768px or wider
        if (screenWidth >= tabletMinWidth)
        {
            currentVisibleItems = visibleItemsTablet;  // 5 items
            Debug.Log($"Tablet mode: {screenWidth}px - showing {currentVisibleItems} items");
        }
        // Narrow phones (Z Flip, etc.) - 400px or narrower
        else if (screenWidth <= narrowMaxWidth)
        {
            currentVisibleItems = visibleItemsNarrow;  // 3 items
            Debug.Log($"Narrow phone mode: {screenWidth}px - showing {currentVisibleItems} items");
        }
        // Regular phones (iPhone, etc.)
        else
        {
            currentVisibleItems = visibleItemsPhone;   // 4 items
            Debug.Log($"Phone mode: {screenWidth}px - showing {currentVisibleItems} items");
        }
    }

    void BuildItems()
    {
        foreach (Transform t in content)
            Destroy(t.gameObject);

        spawned.Clear();

        int totalSlots = farmCount + lockCount;
        float viewportWidth = scrollRect.viewport.rect.width;
        float itemWidth = viewportWidth / currentVisibleItems;
        float dividerWidth = itemWidth * 0.25f;
        float height = scrollRect.viewport.rect.height;

        slotPlusDividerWidth = itemWidth + dividerWidth;

        float totalWidth = ((totalSlots * itemWidth) + ((totalSlots - 1) * dividerWidth));
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject prefab = i < farmCount ? farmSlotPrefab : lockSlotPrefab;
            CreateSlot(prefab, itemWidth, height);

            if (i < totalSlots - 1)
                CreateDivider(dividerWidth, height);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    void CreateSlot(GameObject prefab, float width, float height)
    {
        GameObject obj = Instantiate(prefab, content);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 0.5f);
        
        float xPos = 0;
        if (spawned.Count > 0)
        {
            RectTransform lastRT = spawned[spawned.Count - 1].GetComponent<RectTransform>();
            xPos = lastRT.anchoredPosition.x + lastRT.sizeDelta.x;
        }
        
        rt.anchoredPosition = new Vector2(xPos, 0);
        rt.sizeDelta = new Vector2(width, 0);

        if (prefab == farmSlotPrefab)
        {
            int index = spawned.Count / 2;
            obj.GetComponent<Button>()?.onClick.AddListener(() => OnFarmClicked(index));
        }

        spawned.Add(obj);
    }

    void CreateDivider(float width, float height)
    {
        GameObject obj = Instantiate(dividerPrefab, content);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 0.5f);
        
        float xPos = 0;
        if (spawned.Count > 0)
        {
            RectTransform lastRT = spawned[spawned.Count - 1].GetComponent<RectTransform>();
            xPos = lastRT.anchoredPosition.x + lastRT.sizeDelta.x;
        }
        
        rt.anchoredPosition = new Vector2(xPos, 0);
        rt.sizeDelta = new Vector2(width, 0);

        spawned.Add(obj);
    }

    void ComputeStep()
    {
        float viewport = scrollRect.viewport.rect.width;
        float contentW = content.rect.width;

        if (contentW <= viewport)
        {
            singleStep = 0f;
            targetNormalizedPos = 0f;
            return;
        }

        float scrollableDistance = contentW - viewport;
        singleStep = slotPlusDividerWidth / scrollableDistance;
        singleStep = Mathf.Clamp(singleStep, 0f, 1f);
    }

    void OnLeftClick()
    {
        if (singleStep <= 0f) return;
        targetNormalizedPos = Mathf.Clamp01(targetNormalizedPos - singleStep);
        isScrolling = true;
    }

    void OnRightClick()
    {
        if (singleStep <= 0f) return;
        targetNormalizedPos = Mathf.Clamp01(targetNormalizedPos + singleStep);
        isScrolling = true;
    }

    void UpdateButtonState()
    {
        float contentW = content.rect.width;
        float viewportW = scrollRect.viewport.rect.width;

        bool canScroll = contentW > viewportW;

        leftButton.interactable = canScroll && targetNormalizedPos > 0.01f;
        rightButton.interactable = canScroll && targetNormalizedPos < 0.99f;
    }

    void OnFarmClicked(int farmIndex)
    {
        selectedFarmIndex = farmIndex;
        farmDatabase?.SwitchToFarm(farmIndex);
        farmGridManager?.SwitchFarm(farmIndex);
        Debug.Log($"Farm {farmIndex + 1} selected ✅");
    }
}