using UnityEngine;

public class MatchOnlyWidth : MonoBehaviour
{
    public RectTransform farmRoot;       // The reference object
    public RectTransform mainMenuPanel;  // The panel you want to resize

    void Update()
    {
        if (farmRoot == null || mainMenuPanel == null) return;

        // Get farmRoot width
        float targetWidth = farmRoot.rect.width;

        // Current height stays the same
        float currentHeight = mainMenuPanel.sizeDelta.y;

        // Apply width to mainMenuPanel
        mainMenuPanel.sizeDelta = new Vector2(targetWidth, currentHeight);
    }
}
