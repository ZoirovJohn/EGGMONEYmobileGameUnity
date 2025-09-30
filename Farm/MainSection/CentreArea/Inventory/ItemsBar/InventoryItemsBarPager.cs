using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemsBarPager : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scroll;            // ItemsScroll (ScrollRect)
    public RectTransform viewport;       // ItemsScroll/Viewport
    public RectTransform content;        // ItemsScroll/Viewport/Content
    public Button btnPrev;               // ◄
    public Button btnNext;               // ►

    [Header("Paging")]
    public int visibleItems = 3;         // how many are visible per “page”
    public int stepItems = 3;            // how many items to move per click
    public float lerpSpeed = 8f;         // scroll smoothing

    float _cellW = 245f; // fixed width per item (px)
    float _viewW;        // viewport width
    float _contentW;     // total content width

    void OnEnable() { StartCoroutine(RefreshNextFrame()); }

    IEnumerator RefreshNextFrame()
    {
        yield return null; // wait 1 frame
        Canvas.ForceUpdateCanvases();
        CacheSizes();
        HookButtons();
        UpdateArrowStates();
    }

    void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled) return;
        CacheSizes();
        UpdateArrowStates();
    }

    void CacheSizes()
    {
        _viewW = viewport.rect.width;
        _contentW = content.rect.width;

        scroll.horizontal = true;
        scroll.vertical   = false;
        scroll.horizontalNormalizedPosition = Mathf.Clamp01(scroll.horizontalNormalizedPosition);
    }

    void HookButtons()
    {
        if (btnPrev != null) { btnPrev.onClick.RemoveAllListeners(); btnPrev.onClick.AddListener(() => Page(-1)); }
        if (btnNext != null) { btnNext.onClick.RemoveAllListeners(); btnNext.onClick.AddListener(() => Page(+1)); }
    }

    void Page(int dir)
    {
        // Each click = stepItems × 245px
        float deltaPx = Mathf.Max(1, stepItems) * _cellW;

        float scrollable = Mathf.Max(1f, _contentW - _viewW); // avoid div by zero
        float deltaNorm  = deltaPx / scrollable;

        float start = scroll.horizontalNormalizedPosition;
        float target = Mathf.Clamp01(start + dir * deltaNorm);

        StopAllCoroutines();
        StartCoroutine(LerpTo(target));

        Debug.Log($"Scroll step: {deltaPx}px (dir {dir})");
    }

    IEnumerator LerpTo(float target)
    {
        float t = 0f;
        float start = scroll.horizontalNormalizedPosition;
        while (t < 1f)
        {
            t += Time.deltaTime * lerpSpeed;
            scroll.horizontalNormalizedPosition = Mathf.Lerp(start, target, t);
            yield return null;
        }
        scroll.horizontalNormalizedPosition = target;
        UpdateArrowStates();
    }

    void UpdateArrowStates()
    {
        bool canScroll = _contentW > _viewW + 0.5f;
        if (btnPrev) btnPrev.interactable = canScroll && scroll.horizontalNormalizedPosition > 0.001f;
        if (btnNext) btnNext.interactable = canScroll && scroll.horizontalNormalizedPosition < 0.999f;
    }
}
