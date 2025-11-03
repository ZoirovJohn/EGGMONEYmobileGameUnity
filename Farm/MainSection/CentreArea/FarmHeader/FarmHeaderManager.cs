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
    public int visibleItemsPhone = 4;
    public int visibleItemsTablet = 5;
    public int visibleItemsNarrow = 3;

    [Header("Screen Width Breakpoints (pixels)")]
    public float tabletMinWidth = 768f;
    public float narrowMaxWidth = 400f;

    [Header("Scroll Settings")]
    public float scrollSpeed = 8f;

    private float targetNormalizedPos = 0f;
    private float singleStep = 0f;
    private bool isScrolling = false;
    private int currentVisibleItems = 4;
    private List<GameObject> spawned = new List<GameObject>();
    private List<GameObject> farmSlots = new List<GameObject>(); // Track only farm slots
    private int selectedFarmIndex = 0;
    private float slotPlusDividerWidth = 0f;

    void Start()
    {
        Debug.Log("🚀 FarmHeaderManager Start()");
        
        leftButton.onClick.AddListener(OnLeftClick);
        rightButton.onClick.AddListener(OnRightClick);

        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;

        // Check references
        if (farmDatabase == null)
        {
            Debug.LogError("❌ FarmDatabase not assigned in FarmHeaderManager!");
        }
        if (farmGridManager == null)
        {
            Debug.LogError("❌ FarmGridManager not assigned in FarmHeaderManager!");
        }

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

        if (screenWidth >= tabletMinWidth)
        {
            currentVisibleItems = visibleItemsTablet;
            Debug.Log($"Tablet mode: {screenWidth}px - showing {currentVisibleItems} items");
        }
        else if (screenWidth <= narrowMaxWidth)
        {
            currentVisibleItems = visibleItemsNarrow;
            Debug.Log($"Narrow phone mode: {screenWidth}px - showing {currentVisibleItems} items");
        }
        else
        {
            currentVisibleItems = visibleItemsPhone;
            Debug.Log($"Phone mode: {screenWidth}px - showing {currentVisibleItems} items");
        }
    }

    void BuildItems()
    {
        // Clear existing
        foreach (Transform t in content)
            Destroy(t.gameObject);

        spawned.Clear();
        farmSlots.Clear(); // Clear farm slots list

        int totalSlots = farmCount + lockCount;
        float viewportWidth = scrollRect.viewport.rect.width;
        float itemWidth = viewportWidth / currentVisibleItems;
        float dividerWidth = itemWidth * 0.25f;
        float height = scrollRect.viewport.rect.height;

        slotPlusDividerWidth = itemWidth + dividerWidth;

        float totalWidth = ((totalSlots * itemWidth) + ((totalSlots - 1) * dividerWidth));
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        int farmSlotIndex = 0; // Track actual farm index

        for (int i = 0; i < totalSlots; i++)
        {
            bool isFarmSlot = i < farmCount;
            GameObject prefab = isFarmSlot ? farmSlotPrefab : lockSlotPrefab;
            
            // Create the slot and capture the farm index at creation time
            if (isFarmSlot)
            {
                CreateFarmSlot(prefab, itemWidth, height, farmSlotIndex);
                farmSlotIndex++;
            }
            else
            {
                CreateLockSlot(prefab, itemWidth, height);
            }

            if (i < totalSlots - 1)
                CreateDivider(dividerWidth, height);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        
        Debug.Log($"✅ Created {farmCount} farm slots and {lockCount} lock slots");
        
        // Set initial selection to Farm 1
        UpdateFarmSelection(0);
    }

    void CreateFarmSlot(GameObject prefab, float width, float height, int farmIndex)
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

        // Find button - check both on root and in children
        Button btn = obj.GetComponent<Button>();
        if (btn == null)
        {
            btn = obj.GetComponentInChildren<Button>();
        }
        
        if (btn != null)
        {
            int capturedIndex = farmIndex; // Capture the index!
            btn.onClick.RemoveAllListeners(); // Clear any existing listeners
            btn.onClick.AddListener(() => OnFarmClicked(capturedIndex));
            Debug.Log($"✅ Added onClick to Farm {capturedIndex + 1} button");
        }
        else
        {
            Debug.LogWarning($"⚠️ Farm slot {farmIndex + 1} has no Button component (checked root and children)!");
        }

        spawned.Add(obj);
        farmSlots.Add(obj); // Track this as a farm slot
    }

    void CreateLockSlot(GameObject prefab, float width, float height)
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
        Debug.Log($"🖱️ Farm {farmIndex + 1} button clicked!");
        
        selectedFarmIndex = farmIndex;
        
        // Update visual selection
        UpdateFarmSelection(farmIndex);
        
        if (farmDatabase != null)
        {
            farmDatabase.SwitchToFarm(farmIndex);
        }
        else
        {
            Debug.LogError("❌ FarmDatabase is null!");
        }
        
        if (farmGridManager != null)
        {
            farmGridManager.SwitchFarm(farmIndex);
        }
        else
        {
            Debug.LogError("❌ FarmGridManager is null!");
        }
        
        Debug.Log($"✅ Farm {farmIndex + 1} selected and loaded!");
    }

    void UpdateFarmSelection(int farmIndex)
    {
        if (farmIndex < 0 || farmIndex >= farmSlots.Count) return;

        // Reset all farm slots to normal state
        for (int i = 0; i < farmSlots.Count; i++)
        {
            GameObject slot = farmSlots[i];
            
            // Get the Image component on the farm prefab root (the yellow background)
            Image backgroundImg = slot.GetComponent<Image>();
            
            if (backgroundImg != null)
            {
                if (i == farmIndex)
                {
                    // Selected - show yellow background (alpha = 255)
                    Color c = backgroundImg.color;
                    backgroundImg.color = new Color(c.r, c.g, c.b, 1f); // Alpha = 1 (255)
                }
                else
                {
                    // Unselected - hide yellow background (alpha = 0)
                    Color c = backgroundImg.color;
                    backgroundImg.color = new Color(c.r, c.g, c.b, 0f); // Alpha = 0
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Farm slot {i + 1} has no Image component on root!");
            }
        }
        
        Debug.Log($"🎨 Updated visual selection to Farm {farmIndex + 1}");
    }
}