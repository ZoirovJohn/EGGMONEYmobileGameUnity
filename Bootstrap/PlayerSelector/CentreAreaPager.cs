using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CentreAreaPager : MonoBehaviour
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
    public int pageCount = 4;

    private int currentPage = 0;
    private float[] pagePositions;
    private Image[] dots;

    // ✅ NEW INPUT SYSTEM SAFE
    private bool isDragging = false;

    void Start()
    {
        if (!scrollRect) return;

        // ScrollRect setup
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;

        // Auto page count from content
        if (content)
            pageCount = Mathf.Max(1, content.childCount);

        // Create dots
        EnsureDots();

        // Precompute positions
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
        if (!scrollRect || pagePositions == null) return;

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

    // ---------------- DRAG EVENTS (NEW INPUT SAFE) ----------------

    public void OnBeginDrag(BaseEventData data)
    {
        isDragging = true;
    }

    public void OnEndDrag(BaseEventData data)
    {
        isDragging = false;
    }

    // ---------------- DOTS ----------------

    void EnsureDots()
    {
        if (!dotContainer) return;

        // Remove old dots
        for (int i = dotContainer.childCount - 1; i >= 0; i--)
            Destroy(dotContainer.GetChild(i).gameObject);

        dots = new Image[pageCount];

        for (int i = 0; i < pageCount; i++)
        {
            GameObject go;

            if (dotPrefab)
            {
                go = Instantiate(dotPrefab, dotContainer);
            }
            else
            {
                go = new GameObject($"Dot{i + 1}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(dotContainer, false);

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = dotSize;

                var img = go.GetComponent<Image>();
                img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                img.preserveAspect = true;
            }

            dots[i] = go.GetComponent<Image>();

            // Optional click support
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                int idx = i;
                btn.onClick.AddListener(() => GoToPage(idx));
            }
        }
    }

    void UpdateDots(int index)
    {
        if (dots == null) return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (!dots[i]) continue;
            dots[i].color = (i == index) ? activeColor : inactiveColor;
        }
    }

    public void GoToPage(int target)
    {
        if (pagePositions == null) return;

        target = Mathf.Clamp(target, 0, pageCount - 1);
        currentPage = target;
        scrollRect.horizontalNormalizedPosition = pagePositions[currentPage];
        UpdateDots(currentPage);
    }
}
