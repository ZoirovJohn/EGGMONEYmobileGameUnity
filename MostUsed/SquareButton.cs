using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SquareButton : MonoBehaviour
{
    [Header("Square Settings")]
    [Tooltip("Use smaller dimension (width or height)")]
    public bool useSmaller = true;
    
    [Tooltip("Update every frame (for responsive layouts)")]
    public bool updateContinuously = true;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Get parent RectTransform
        if (transform.parent != null)
        {
            parentRect = transform.parent.GetComponent<RectTransform>();
        }
        
        MakeSquare();
    }

    void Update()
    {
        if (updateContinuously)
        {
            MakeSquare();
        }
    }

    void MakeSquare()
    {
        if (rectTransform == null) return;

        // Get current width and height
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        // Pick the size based on setting
        float size = useSmaller ? Mathf.Min(width, height) : Mathf.Max(width, height);

        // Apply it to both width and height
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

    // Call this manually if you want to refresh
    public void RefreshSquare()
    {
        MakeSquare();
    }
}