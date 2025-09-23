using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridAutoResizer : MonoBehaviour
{
    private int columns;
    private int rows;

    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;

    void Start()
    {
        columns = GameSettings.GridWidth;
        rows = GameSettings.GridHeight;
        //Debug.Log("i hate unity");
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        ResizeCells();
    }

    void ResizeCells()
    {
        float totalWidth = rectTransform.rect.width;
        float totalHeight = rectTransform.rect.height;

        float cellWidth = totalWidth / columns;
        float cellHeight = totalHeight / rows;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }

    void Update()
    {
        // Optional: Uncomment if you want dynamic resizing on window resize
         //ResizeCells();
    }
}

