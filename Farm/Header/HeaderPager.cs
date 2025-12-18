using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HeaderPager : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Scroll / Content")]
    public ScrollRect scrollRect;
    public RectTransform content;

    [Header("Dots")]
    public Transform dotContainer;
    public GameObject dotPrefab;
    public Vector2 dotSize = new Vector2(16, 16);
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    [Header("Pages")]
    public int pageCount = 3;

    private int currentPage = 0;
    private float[] pagePositions;
    private Image[] dots;

    private bool isDragging = false; // ✅ NEW

    void Start()
    {
        if (scrollRect)
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = false;
        }

        if (content)
            pageCount = Mathf.Max(1, content.childCount);

        EnsureDots();

        pagePositions = new float[pageCount];
        if (pageCount == 1)
        {
            pagePositions[0] = 0f;
        }
        else
        {
            for (int i = 0; i < pageCount; i++)
                pagePositions[i] = (float)i / (pageCount - 1);
        }

        currentPage = Mathf.Clamp(currentPage, 0, pageCount - 1);
        scrollRect.horizontalNormalizedPosition = pagePositions[currentPage];
        UpdateDots(currentPage);
    }

    void Update()
    {
        if (scrollRect == null || pagePositions == null) return;

        float pos = scrollRect.horizontalNormalizedPosition;
        int nearestPage = 0;
        float nearestDistance = Mathf.Abs(pos - pagePositions[0]);

        for (int i = 1; i < pageCount; i++)
        {
            float dist = Mathf.Abs(pos - pagePositions[i]);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestPage = i;
            }
        }

        // ✅ SNAP ONLY WHEN NOT DRAGGING
        if (!isDragging)
        {
            float target = pagePositions[nearestPage];
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition,
                target,
                Time.deltaTime * 8f
            );

            if (nearestPage != currentPage)
            {
                currentPage = nearestPage;
                UpdateDots(currentPage);
            }
        }
    }

    // ✅ INPUT SYSTEM SAFE
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    // ---------------- Navigation API ----------------

    public void NextPage() => GoToPage(currentPage + 1);
    public void PrevPage() => GoToPage(currentPage - 1);

    public void GoToPage(int target)
    {
        if (pageCount <= 0) return;

        target = Mathf.Clamp(target, 0, pageCount - 1);
        currentPage = target;
        scrollRect.horizontalNormalizedPosition = pagePositions[currentPage];
        UpdateDots(currentPage);
    }

    // ---------------- Dots ----------------

    void EnsureDots()
    {
        if (!dotContainer) return;

        int have = 0;
        for (int i = 0; i < dotContainer.childCount; i++)
        {
            if (dotContainer.GetChild(i).GetComponent<Image>())
                have++;
        }

        for (int i = have; i < pageCount; i++)
        {
            GameObject go;
            if (dotPrefab)
            {
                go = Instantiate(dotPrefab, dotContainer);
            }
            else
            {
                go = new GameObject($"Dot{i + 1}", typeof(RectTransform), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(dotContainer, false);
                rt.sizeDelta = dotSize;

                var img = go.GetComponent<Image>();
                img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                img.preserveAspect = true;
            }

            var btn = go.GetComponent<Button>();
            if (btn)
            {
                int idx = i;
                btn.onClick.AddListener(() => GoToPage(idx));
            }
        }

        dots = dotContainer.GetComponentsInChildren<Image>();
    }

    void UpdateDots(int index)
    {
        if (dots == null) return;

        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].color = (i == index) ? activeColor : inactiveColor;
        }
    }
}
