using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CentreAreaPager : MonoBehaviour
{
    [Header("Scroll / Content")]
    public ScrollRect scrollRect;
    public RectTransform content;

    [Header("Dots")]
    public Transform dotContainer;      // panel that will hold dots
    public GameObject dotPrefab;        // optional; if null we create a simple Image
    public Vector2 dotSize = new Vector2(16, 16);
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    [Header("Pages")]
    public int pageCount = 4;           // will still use this array for positions
    private int currentPage = 0;
    private float[] pagePositions;

    // internal
    private Image[] dots;

    void Start()
    {
        // Recommended for stability
        if (scrollRect)
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = false;
        }

        // If your content actually has N pages, you can sync like this:
        if (content) pageCount = Mathf.Max(1, content.childCount);

        // Ensure we have enough dots; if not, create them
        EnsureDots();

        // Precompute normalized positions
        pagePositions = new float[pageCount];
        if (pageCount == 1) pagePositions[0] = 0f;
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

        // Snap when dragging ends
        if (!Input.GetMouseButton(0))
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

    // ---------------- Dots ----------------

    void EnsureDots()
    {
        if (!dotContainer) return;

        // Count current child Images (direct children only)
        int have = 0;
        for (int i = 0; i < dotContainer.childCount; i++)
        {
            if (dotContainer.GetChild(i).GetComponent<Image>() != null) have++;
        }

        // Create missing dots
        for (int i = have; i < pageCount; i++)
        {
            GameObject go;
            if (dotPrefab)
            {
                go = Instantiate(dotPrefab, dotContainer);
            }
            else
            {
                go = new GameObject($"Dot{i+1}", typeof(RectTransform), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(dotContainer, false);
                rt.sizeDelta = dotSize;

                // Give it a default sprite if needed (built-in UI sprite)
                var img = go.GetComponent<Image>();
                if (img.sprite == null)
                {
                    img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                    img.type = Image.Type.Simple;
                    img.preserveAspect = true;
                }
            }

            // Optional: make dot clickable to jump
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                int idx = i;
                btn.onClick.AddListener(() => GoToPage(idx));
            }
        }

        // Cache all child Images as dots array
        dots = new Image[dotContainer.childCount];
        int k = 0;
        for (int i = 0; i < dotContainer.childCount; i++)
        {
            var img = dotContainer.GetChild(i).GetComponent<Image>();
            if (img != null) dots[k++] = img;
        }
        // Trim nulls if some children lacked Image
        System.Array.Resize(ref dots, k);
    }

    void UpdateDots(int index)
    {
        if (dots == null) return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (!dots[i]) continue;
            dots[i].color = (i == index ? activeColor : inactiveColor);
            // if your dot sprite supports fill, show filled vs empty
            dots[i].fillAmount = (i == index ? 1f : 0f);
        }
    }

    public void GoToPage(int target)
    {
        if (pageCount <= 0) return;
        target = Mathf.Clamp(target, 0, pageCount - 1);
        currentPage = target;
        scrollRect.horizontalNormalizedPosition = pagePositions[currentPage];
        UpdateDots(currentPage);
    }
}
