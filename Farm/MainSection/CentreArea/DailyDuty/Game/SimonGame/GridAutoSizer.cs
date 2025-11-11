using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridAutoSizer : MonoBehaviour
{
    [Header("References")]
    public RectTransform mainPanel;    // Assign your mainPanel here
    public RectTransform centrePanel;  // Assign your centrePanel here (this object)
    
    [Header("Settings")]
    public float offset = 15f;         // The "-15" from your formula
    public int divideBy = 2;           // The "/2" from your formula

    private GridLayoutGroup grid;

    void Awake()
    {
        grid = centrePanel.GetComponent<GridLayoutGroup>();
        UpdateCellSize();
    }

    void Update()
    {
        // Optional: keep updating in case screen or panel size changes dynamically
        UpdateCellSize();
    }

    void UpdateCellSize()
    {
        if (mainPanel == null || grid == null) return;

        float mainHeight = mainPanel.rect.height;
        float cellSize = (mainHeight - offset) / divideBy;

        grid.cellSize = new Vector2(cellSize, cellSize);
    }
}
