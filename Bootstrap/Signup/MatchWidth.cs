using UnityEngine;

[ExecuteAlways] // Updates in Edit mode too
public class MatchWidth : MonoBehaviour
{
    [Header("The target to copy width from")]
    public RectTransform target;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (target == null || rect == null) return;

        // Keep current height, match target width
        Vector2 size = rect.sizeDelta;
        size.x = target.rect.width;
        rect.sizeDelta = size;
    }
}
