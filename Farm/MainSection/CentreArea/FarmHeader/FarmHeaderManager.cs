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

    [Header("Counts - Will be loaded from PlayerWallet")]
    [Tooltip("These values will be overwritten by PlayerWallet on Start")]
    public int farmCount = 3;
    public int lockCount = 5;
    
    [Header("Max Farms Setting")]
    [Tooltip("Total number of farms available in the game (unlocked + locked)")]
    public int maxTotalFarms = 99999;
    
    [Header("Visible Locks Setting")]
    [Tooltip("How many lock slots to show to the user (doesn't affect max farms, just UI display)")]
    public int visibleLockSlots = 7;

    [Header("Backend Integration")]
    public PlayerWallet playerWallet;
    
    [Header("Database")]
    public FarmDatabase farmDatabase;
    public FarmGridManager farmGridManager;

    [Header("Inventory Bar")]
    public InventoryItemsBarChanger inventoryBarChanger;

    [Header("Centre Area")]
    public Transform centreAreaRoot;
    public GameObject farmPanel;

    [Header("Footer Panel")]
    [SerializeField] private FooterPanelSwitcher footerPanelSwitcher;

    [Header("Info Panels")]
    [SerializeField] private InfoErrorChanger infoErrorChanger;

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
    private List<GameObject> farmSlots = new List<GameObject>();
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

        // Check references
        if (farmDatabase == null)
        {
            Debug.LogError("❌ FarmDatabase not assigned in FarmHeaderManager!");
        }
        if (farmGridManager == null)
        {
            Debug.LogError("❌ FarmGridManager not assigned in FarmHeaderManager!");
        }
        if (playerWallet == null)
        {
            Debug.LogWarning("⚠️ PlayerWallet not assigned - will use default farm count");
        }

        // ✅ Auto-find FooterPanelSwitcher if not assigned
        if (footerPanelSwitcher == null)
        {
            footerPanelSwitcher = FindAnyObjectByType<FooterPanelSwitcher>();
            if (footerPanelSwitcher != null)
            {
                Debug.Log("✅ Auto-found FooterPanelSwitcher");
            }
        }

        // Load farm counts from PlayerWallet (or fallback to database)
        LoadFarmCountsFromBackend();
        
        Refresh();
    }

    /// <summary>
    /// Called when opening main menu or other panels where no farm should be selected
    /// </summary>
    public void ClearFarmSelection()
    {
        for (int i = 0; i < farmSlots.Count; i++)
        {
            GameObject slot = farmSlots[i];
            
            Image backgroundImg = slot.GetComponent<Image>();
            
            if (backgroundImg != null)
            {
                Color c = backgroundImg.color;
                backgroundImg.color = new Color(c.r, c.g, c.b, 0f); // Set alpha to 0 (transparent)
            }
        }
        
        Debug.Log("✅ Farm header selection cleared");
    }

    /// <summary>
    /// Load farm counts from PlayerWallet (loaded from backend via /auth/me)
    /// If PlayerWallet is not available, fallback to FarmDatabase
    /// </summary>
    void LoadFarmCountsFromBackend()
    {
        // ✅ PRIORITY 1: Try to load from PlayerWallet (backend data)
        if (playerWallet != null)
        {
            farmCount = playerWallet.UserFarms;
            
            // ✅ Show a fixed number of visible lock slots (e.g., 7)
            // This is just for UI display, doesn't limit actual max farms
            lockCount = visibleLockSlots;
            
            Debug.Log($"📊 Farm counts loaded: {farmCount} unlocked farms, showing {lockCount} lock slots");
            return;
        }
        
        // ✅ FALLBACK: Load from FarmDatabase if PlayerWallet not available
        if (farmDatabase == null || farmDatabase.farms == null)
        {
            Debug.LogWarning("⚠️ FarmDatabase not available, using default counts");
            return;
        }
        
        // Get actual unlocked farm count from database
        farmCount = farmDatabase.farms.Count;
        
        // Show visible lock slots
        lockCount = visibleLockSlots;
        
        Debug.Log($"📊 Farm counts from database: {farmCount} unlocked farms, showing {lockCount} lock slots");
    }

    string BuildFarmInfoText(FarmData farm)
    {
        if (farm == null) return "";

        int robotCount =
            (!string.IsNullOrEmpty(farm.robotType) && farm.robotType != "none") ? 1 : 0;

        return
            $"{T("Farm_Robot")}: {robotCount}\n" +
            $"{T("Farm_Hen")}: {farm.normalChicks}\n" +
            $"{T("Farm_Champ")}: {farm.champChicks}\n" +
            $"{T("Farm_Nest")}: {farm.nestsOccupied}";
    }

    string T(string key)
    {
        return LanguageManager.Instance != null
            ? LanguageManager.Instance.GetTranslation(key)
            : key;
    }

    public void RefreshFarmInfoLanguage()
    {
        if (infoErrorChanger == null || farmDatabase == null)
            return;

        FarmData farm = farmDatabase.GetFarmByIndex(selectedFarmIndex);
        if (farm == null) return;

        string infoText = BuildFarmInfoText(farm);
        infoErrorChanger.OpenInfoAboutFarmItself(infoText);
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
        // Reload counts from PlayerWallet/database before refreshing
        LoadFarmCountsFromBackend();
        
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
        }
        else if (screenWidth <= narrowMaxWidth)
        {
            currentVisibleItems = visibleItemsNarrow;
        }
        else
        {
            currentVisibleItems = visibleItemsPhone;
        }
    }

    void BuildItems()
    {
        // Clear existing
        foreach (Transform t in content)
            Destroy(t.gameObject);

        spawned.Clear();
        farmSlots.Clear();

        int totalSlots = farmCount + lockCount;
        float viewportWidth = scrollRect.viewport.rect.width;
        float itemWidth = viewportWidth / currentVisibleItems;
        float dividerWidth = itemWidth * 0.25f;
        float height = scrollRect.viewport.rect.height;

        slotPlusDividerWidth = itemWidth + dividerWidth;

        float totalWidth = ((totalSlots * itemWidth) + ((totalSlots - 1) * dividerWidth));
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        int farmSlotIndex = 0;

        for (int i = 0; i < totalSlots; i++)
        {
            bool isFarmSlot = i < farmCount;
            GameObject prefab = isFarmSlot ? farmSlotPrefab : lockSlotPrefab;
            
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
        
        // Set initial selection to Farm 1
        UpdateFarmSelection(0);
        
        if (farmGridManager != null)
        {
            farmGridManager.SwitchFarm(0);
        }
    }

    /// <summary>
    /// Update visual indicators for a specific farm slot
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
        
        FarmSlotVisual visual = farmSlot.GetComponent<FarmSlotVisual>();
        if (visual != null)
        {
            visual.UpdateVisual(farm);
        }
        else
        {
            Debug.Log($"ℹ️ No FarmSlotVisual component on farm slot {farmIndex + 1}");
        }
    }

    /// <summary>
    /// Update visuals for all farm slots
    /// </summary>
    public void UpdateAllFarmSlotVisuals()
    {
        for (int i = 0; i < farmSlots.Count; i++)
        {
            UpdateFarmSlotVisual(i);
        }
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

        Button btn = obj.GetComponentInChildren<Button>();
        
        if (btn != null)
        {
            int capturedIndex = farmIndex;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnFarmClicked(capturedIndex));
        }
        else
        {
            Debug.LogWarning($"⚠️ Farm slot {farmIndex + 1} has no Button component in children!");
        }

        spawned.Add(obj);
        farmSlots.Add(obj);
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

        LockSlotClick lockClick = obj.GetComponent<LockSlotClick>();
        if (lockClick == null)
        {
            lockClick = obj.AddComponent<LockSlotClick>();
        }
        
        // ✅ Assign FarmHeaderManager reference
        var farmHeaderField = typeof(LockSlotClick).GetField("farmHeaderManager", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (farmHeaderField != null)
        {
            farmHeaderField.SetValue(lockClick, this);
        }
        
        if (inventoryBarChanger != null)
        {
            var field = typeof(LockSlotClick).GetField("inventoryBarChanger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(lockClick, inventoryBarChanger);
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
    /// Also clears footer button background highlights
    /// </summary>
    public void CloseAllCentreAreaPanels()
    {
        if (centreAreaRoot == null)
        {
            Debug.LogWarning("⚠️ CentreAreaRoot not assigned!");
            return;
        }

        for (int i = 0; i < centreAreaRoot.childCount; i++)
        {
            GameObject child = centreAreaRoot.GetChild(i).gameObject;
            
            if (child == farmPanel)
                continue;
            
            child.SetActive(false);
        }
        
        if (farmPanel != null)
        {
            farmPanel.SetActive(true);
        }
        
        // ✅ Clear footer button highlights when switching to farm view
        if (footerPanelSwitcher != null)
        {
            footerPanelSwitcher.ClearFooterSelection(closePanels: false);
        }
    }

    void OnFarmClicked(int farmIndex)
    {
        CloseAllCentreAreaPanels();

        selectedFarmIndex = farmIndex;

        FarmData clickedFarm = farmDatabase.GetFarmByIndex(farmIndex);

        if (clickedFarm != null)
        {
            // 🔹 Inventory bar (already existing)
            if (inventoryBarChanger != null)
            {
                inventoryBarChanger.InventoryFarmBarMethod(clickedFarm.farmId);
            }

            if (infoErrorChanger != null)
            {
                string infoText = BuildFarmInfoText(clickedFarm);
                StartCoroutine(OpenFarmInfoNextFrame(infoText));
            }
        }

        UpdateFarmSelection(farmIndex);

        if (farmDatabase != null)
            farmDatabase.SwitchToFarm(farmIndex);

        if (farmGridManager != null)
            farmGridManager.SwitchFarm(farmIndex);
    }

    private System.Collections.IEnumerator OpenFarmInfoNextFrame(string infoText)
    {
        // Wait one frame so all CloseAllInfoErrorMethod() calls finish
        yield return null;

        if (infoErrorChanger != null)
        {
            infoErrorChanger.OpenInfoAboutFarmItself(infoText);
        }
    }


    /// <summary>
    /// ✅ PUBLIC method to update farm selection highlight (yellow background)
    /// Can be called from other scripts like InventoryLockItemApplier
    /// </summary>
    public void SelectFarm(int farmIndex)
    {
        if (farmIndex < 0 || farmIndex >= farmSlots.Count)
        {
            Debug.LogWarning($"⚠️ Cannot select farm index {farmIndex} - out of range (0-{farmSlots.Count - 1})");
            return;
        }
        
        selectedFarmIndex = farmIndex;
        UpdateFarmSelection(farmIndex);
        
        Debug.Log($"✅ Farm header selection updated to farm {farmIndex + 1}");
    }

    void UpdateFarmSelection(int farmIndex)
    {
        if (farmIndex < 0 || farmIndex >= farmSlots.Count) return;

        for (int i = 0; i < farmSlots.Count; i++)
        {
            GameObject slot = farmSlots[i];
            
            Image backgroundImg = slot.GetComponent<Image>();
            
            if (backgroundImg != null)
            {
                if (i == farmIndex)
                {
                    Color c = backgroundImg.color;
                    backgroundImg.color = new Color(c.r, c.g, c.b, 1f);
                }
                else
                {
                    Color c = backgroundImg.color;
                    backgroundImg.color = new Color(c.r, c.g, c.b, 0f);
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Farm slot {i + 1} has no Image component on root!");
            }
        }
    }
}