using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemsBarPager : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scroll;            // ItemsScroll (ScrollRect)
    public RectTransform viewport;       // ItemsScroll/Viewport
    public RectTransform content;        // ItemsScroll/Viewport/Content

    [Header("Paging")]
    public int visibleItems = 3;         // how many are visible per “page”
    public int stepItems = 3;            // how many items to move per scroll
    public float lerpSpeed = 8f;         // scroll smoothing

    float _cellH = 245f; // fixed height per item (px)
    float _viewH;        // viewport height
    float _contentH;     // total content height

    void OnEnable() { StartCoroutine(RefreshNextFrame()); }

    IEnumerator RefreshNextFrame()
    {
        yield return null; // wait 1 frame
        Canvas.ForceUpdateCanvases();
        CacheSizes();
    }

    void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled) return;
        CacheSizes();
    }

    void CacheSizes()
    {
        _viewH = viewport.rect.height;
        _contentH = content.rect.height;

        scroll.horizontal = false;
        scroll.vertical   = true;
        scroll.verticalNormalizedPosition = Mathf.Clamp01(scroll.verticalNormalizedPosition);
    }

    /// <summary>
    /// Call this to move up/down by N items.
    /// dir = +1 → move down, -1 → move up
    /// </summary>
    public void Page(int dir)
    {
        // Each page step = stepItems × 245px
        float deltaPx = Mathf.Max(1, stepItems) * _cellH;

        float scrollable = Mathf.Max(1f, _contentH - _viewH); // avoid div by zero
        float deltaNorm  = deltaPx / scrollable;

        float start = scroll.verticalNormalizedPosition;
        // verticalNormalizedPosition goes 1 (top) → 0 (bottom)
        float target = Mathf.Clamp01(start - dir * deltaNorm);

        StopAllCoroutines();
        StartCoroutine(LerpTo(target));
    }

    IEnumerator LerpTo(float target)
    {
        float t = 0f;
        float start = scroll.verticalNormalizedPosition;
        while (t < 1f)
        {
            t += Time.deltaTime * lerpSpeed;
            scroll.verticalNormalizedPosition = Mathf.Lerp(start, target, t);
            yield return null;
        }
        scroll.verticalNormalizedPosition = target;
    }
}
