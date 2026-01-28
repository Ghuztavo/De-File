using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GridCell : MonoBehaviour
{
    [HideInInspector] public int index;
    [HideInInspector] public bool censored;
    private GridCensorGenerator generator;
    private BoxCollider2D box;
    private TextMeshPro tmp;

    public void Init(GridCensorGenerator gen, int idx, char letter, Vector2 cellSize, float lockedFontSize)
    {
        generator = gen;
        index = idx;
        censored = false;

        box = GetComponent<BoxCollider2D>();
        box.size = cellSize;
        box.offset = Vector2.zero;

        tmp = GetComponentInChildren<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = letter.ToString();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.enableAutoSizing = false;
            tmp.fontSize = lockedFontSize;
            tmp.overflowMode = TextOverflowModes.Masking;
        }
    }
}