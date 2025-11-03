using UnityEngine;

public class MatchPanelWidthToScreen : MonoBehaviour
{
    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();

        if (rt != null && rt.parent != null)
        {
            // get the parent of the parent
            RectTransform grandParent = rt.parent.parent as RectTransform;

            if (grandParent != null)
            {
                // match width and height to grandparent
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, grandParent.rect.width);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, grandParent.rect.height);
            }
            else
            {
                Debug.LogWarning("Grandparent RectTransform not found for " + gameObject.name);
            }
        }
    }
}
