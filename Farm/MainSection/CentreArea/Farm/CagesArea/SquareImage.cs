using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SquareImage : MonoBehaviour
{
    void Start()
    {
        MakeSquare();
    }

    void MakeSquare()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;

        // Get current width and height
        float width = rt.rect.width;
        float height = rt.rect.height;

        // Pick the smaller one
        float size = Mathf.Min(width, height);

        // Apply it to both width and height
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

    // Optional: keep it square even after resize
    void Update()
    {
        MakeSquare();
    }
}
