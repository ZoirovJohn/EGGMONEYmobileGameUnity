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

    [Header("Counts - Will be loaded from Database")]
    [Tooltip("These values will be overwritten by database on Start")]
    public int farmCount = 3;
    public int lockCount = 5;
    
    [Header("Max Farms Setting")]
    [Tooltip("Total number of farms available in the game (unlocked + locked)")]
    public int maxTotalFarms = 8;

    [Header("Database")]
    public FarmDatabase farmDatabase;
    public FarmGridManager farmGridManager;

    [Header("Inventory Bar")]
    public InventoryItemsBarChanger inventoryBarChanger;

    [Header("Centre Area")]
    public Transform centreAreaRoot; // Reference to close all panels
    public GameObject farmPanel; // The farm panel to keep open

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

        // Load farm counts from database
        LoadFarmCountsFromDatabase();
        
        Refresh();
    }

    /// <summary>
    /// Load farm counts from FarmDatabase instead of using hardcoded values
    /// </summary>
    void LoadFarmCountsFromDatabase()
    {
        if (farmDatabase == null || farmDatabase.farms == null)
        {
            Debug.LogWarning("⚠️ FarmDatabase not available, using default counts");
            return;
        }
        
        // Get actual unlocked farm count from database
        farmCount = farmDatabase.farms.Count;
        
        // Calculate locked farms
        lockCount = Mathf.Max(0, maxTotalFarms - farmCount);
        
        Debug.Log($"📊 Loaded from database: {farmCount} farms unlocked, {lockCount} locks remaining (Total: {maxTotalFarms})");
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
        // Reload counts from database before refreshing
        LoadFarmCountsFromDatabase();
        
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
        
        if (farmGridManager != null)
        {
            farmGridManager.SwitchFarm(0);
        }
    }

    /// <summary>
    /// Update visual indicators for a specific farm slot
    /// Call this after applying items to refresh the farm slot appearance
    /// </summary>
    public void UpdateFarmSlotVisual(int farmIndex)
    {
        if (farmIndex < 0 || farmIndex >= farmSlots.Count)
        {
            Debug.LogWarning($"⚠️ Invalid farm index: {farmIndex}");
            return;
        }
        
        GameObject farmSlot = farmSlots[farmIndex];
        FarmData farm = farmDatabase.GetFarmByIndex(farmIndex);
        
        if (farm == null)
        {
            Debug.LogError($"❌ Could not find farm data for index {farmIndex}");
            return;
        }
        
        // Update the visual component if it exists
        FarmSlotVisual visual = farmSlot.GetComponent<FarmSlotVisual>();
        if (visual != null)
        {
            visual.UpdateVisual(farm);
            Debug.Log($"🎨 Updated visual for {farm.farmName}");
        }
        else
        {
            Debug.Log($"ℹ️ No FarmSlotVisual component on farm slot {farmIndex + 1}");
        }
    }

    /// <summary>
    /// Update visuals for all farm slots
    /// Call this to refresh all farm appearances
    /// </summary>
    public void UpdateAllFarmSlotVisuals()
    {
        for (int i = 0; i < farmSlots.Count; i++)
        {
            UpdateFarmSlotVisual(i);
        }
        
        Debug.Log($"🎨 Updated visuals for all {farmSlots.Count} farm slots");
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

        // Find button in children (not on root)
        Button btn = obj.GetComponentInChildren<Button>();
        
        if (btn != null)
        {
            int capturedIndex = farmIndex; // Capture the index!
            btn.onClick.RemoveAllListeners(); // Clear any existing listeners
            btn.onClick.AddListener(() => OnFarmClicked(capturedIndex));
            Debug.Log($"✅ Added onClick to Farm {capturedIndex + 1} button (found in children)");
        }
        else
        {
            Debug.LogWarning($"⚠️ Farm slot {farmIndex + 1} has no Button component in children!");
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

        // ✅ Add LockSlotClick component if it doesn't exist
        LockSlotClick lockClick = obj.GetComponent<LockSlotClick>();
        if (lockClick == null)
        {
            lockClick = obj.AddComponent<LockSlotClick>();
            Debug.Log("✅ Added LockSlotClick component to lock slot");
        }
        
        // ✅ Assign InventoryItemsBarChanger reference
        if (inventoryBarChanger != null)
        {
            // Use reflection to set the private field, or make it public in LockSlotClick
            var field = typeof(LockSlotClick).GetField("inventoryBarChanger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(lockClick, inventoryBarChanger);
                Debug.Log("✅ Assigned InventoryItemsBarChanger to lock slot");
            }
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

    /// <summary>
    /// Close all panels inside CentreArea except farmPanel
    /// </summary>
    void CloseAllCentreAreaPanels()
    {
        if (centreAreaRoot == null)
        {
            Debug.LogWarning("⚠️ CentreAreaRoot not assigned!");
            return;
        }

        for (int i = 0; i < centreAreaRoot.childCount; i++)
        {
            GameObject child = centreAreaRoot.GetChild(i).gameObject;
            
            // Skip the farm panel - keep it open
            if (child == farmPanel)
                continue;
            
            child.SetActive(false);
        }
        
        // Ensure farm panel is active
        if (farmPanel != null)
        {
            farmPanel.SetActive(true);
        }
        
        Debug.Log("🚪 Closed all CentreArea panels except farm panel");
    }

    void OnFarmClicked(int farmIndex)
    {
        Debug.Log($"🖱️ Farm {farmIndex + 1} button clicked!");
        
        // ✅ FIRST: Close all panels in CentreArea
        CloseAllCentreAreaPanels();
        
        selectedFarmIndex = farmIndex;
        
        // Get the farm data to access farmId
        FarmData clickedFarm = farmDatabase.GetFarmByIndex(farmIndex);
        
        if (clickedFarm != null)
        {
            Debug.Log($"📍 Farm ID: {clickedFarm.farmId}");
            
            // Check if farmId is empty
            if (string.IsNullOrEmpty(clickedFarm.farmId))
            {
                Debug.LogError("❌ FARM ID IS EMPTY! Check your JSON file and reload FarmDatabase!");
            }
            
            // Open inventory farm bar with the farmId (ALWAYS)
            if (inventoryBarChanger != null)
            {
                inventoryBarChanger.InventoryFarmBarMethod(clickedFarm.farmId);
            }
            else
            {
                Debug.LogWarning("⚠️ InventoryBarChanger is not assigned in FarmHeaderManager!");
            }
        }
        else
        {
            Debug.LogError($"❌ Farm data is NULL for index {farmIndex}!");
        }
        
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