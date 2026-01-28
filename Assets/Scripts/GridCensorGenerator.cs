using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(BoxCollider2D))]
public class GridCensorGenerator : MonoBehaviour
{
    [Header("Input")]
    [TextArea] public string stringContent = "HELLO WORLD";

    [Header("Cell Settings")]
    public float cellWidth = 1f;
    public float cellHeight = 1f;
    public float cellGapX = 0f;
    public float cellGapY = 0f;

    [Header("Text Settings (LOCKED)")]
    public float lockedTextSize = 6f;

    [Header("Prefabs")]
    public GridCell cellPrefab;
    public GameObject censorPrefab;

    [Header("Overflow Behavior")]
    public bool truncateToFitBounds = true;

    [Header("Ink")]
    public float ink = 10f;
    public float inkCostPerCell = 1f;

    [Header("Drag Paint")]
    public Camera paintCamera;              // if null -> Camera.main
    public LayerMask cellLayerMask = ~0;    // optionally put cells on a "Cell" layer and select it here

    [Header("Debug")]
    public bool showDebugLogs = true;

    private readonly List<GridCell> cells = new();
    private BoxCollider2D bounds;
    private bool wasMousePressed = false;

    private void Awake()
    {
        bounds = GetComponent<BoxCollider2D>();
        bounds.isTrigger = true;
    }

    private void Start()
    {
        Generate();
    }

    private void Update()
    {
        // Proper click detection with press/release tracking
        if (Mouse.current != null)
        {
            bool isPressed = Mouse.current.leftButton.isPressed;

            // Paint on both initial click and while dragging
            if (isPressed)
            {
                Vector2 screenPos = Mouse.current.position.ReadValue();
                PaintAtScreenPosition(screenPos);
            }

            wasMousePressed = isPressed;
        }
    }

    private void PaintAtScreenPosition(Vector2 screenPos)
    {
        Camera cam = paintCamera != null ? paintCamera : Camera.main;
        if (cam == null)
        {
            if (showDebugLogs) Debug.LogError("No camera found for painting!");
            return;
        }

        // For 2D orthographic camera, we need to properly convert screen to world
        Vector3 worldPoint;

        if (cam.orthographic)
        {
            // For orthographic camera, just use the camera's transform position z
            worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        }
        else
        {
            // For perspective camera
            float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
            worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
        }

        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);

        if (showDebugLogs)
        {
            Debug.Log($"Screen: {screenPos} -> World: {worldPoint2D}");
        }

        // Must be inside THIS generator's bounds
        if (!bounds.OverlapPoint(worldPoint2D))
        {
            if (showDebugLogs) Debug.Log($"Point {worldPoint2D} not in bounds");
            return;
        }

        if (showDebugLogs) Debug.Log($"Point IS in bounds! Raycasting...");

        // Try multiple approaches to find the cell

        // Approach 1: Raycast
        RaycastHit2D hit = Physics2D.Raycast(worldPoint2D, Vector2.zero, 0f, cellLayerMask);

        if (hit.collider != null)
        {
            if (showDebugLogs) Debug.Log($"Raycast hit: {hit.collider.name}");

            if (hit.collider.transform.IsChildOf(transform))
            {
                GridCell cell = hit.collider.GetComponent<GridCell>();
                if (cell != null)
                {
                    if (showDebugLogs) Debug.Log($"Found cell {cell.index}, attempting to censor...");
                    TryCensorCell(cell);
                    return;
                }
            }
        }

        // Approach 2: OverlapPoint as fallback
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint2D, cellLayerMask);
        if (showDebugLogs) Debug.Log($"OverlapPoint found {hits.Length} colliders");

        foreach (var col in hits)
        {
            if (col == null) continue;
            if (col == bounds) continue; // Skip the generator's own bounds

            if (showDebugLogs) Debug.Log($"Checking collider: {col.name}");

            if (col.transform.IsChildOf(transform))
            {
                GridCell cell = col.GetComponent<GridCell>();
                if (cell != null)
                {
                    if (showDebugLogs) Debug.Log($"Found cell {cell.index} via overlap, attempting to censor...");
                    TryCensorCell(cell);
                    return;
                }
            }
        }

        if (showDebugLogs) Debug.Log("No cell found at position");
    }


    public void Generate()
    {
        Clear();

        if (cellPrefab == null || string.IsNullOrEmpty(stringContent))
            return;

        Vector2 cellSize = new Vector2(cellWidth, cellHeight);

        float stepX = cellWidth + cellGapX;
        float stepY = cellHeight + cellGapY;

        float minStep = 0.01f;

        if (stepX < minStep)
        {
            Debug.LogWarning($"{name}: cellWidth + cellGapX is too small ({stepX}). Setting to minimum {minStep}.");
            stepX = minStep;
        }

        if (stepY < minStep)
        {
            Debug.LogWarning($"{name}: cellHeight + cellGapY is too small ({stepY}). Setting to minimum {minStep}.");
            stepY = minStep;
        }

        Vector2 b = bounds.size;
        float usableW = Mathf.Max(0f, b.x);
        float usableH = Mathf.Max(0f, b.y);

        int cols = Mathf.Max(1, Mathf.FloorToInt((usableW + Mathf.Abs(cellGapX)) / stepX));
        int rows = Mathf.Max(1, Mathf.FloorToInt((usableH + Mathf.Abs(cellGapY)) / stepY));

        if (cols == 0 || rows == 0)
        {
            Debug.LogWarning($"{name}: Bounds too small for even 1 cell.");
            return;
        }

        int capacity = cols * rows;

        string toPlace = stringContent;
        if (toPlace.Length > capacity)
        {
            if (!truncateToFitBounds)
            {
                Debug.LogWarning($"{name}: Text length {toPlace.Length} exceeds capacity {capacity}.");
                return;
            }
            toPlace = toPlace.Substring(0, capacity);
        }

        float gridW = (cols - 1) * stepX + cellWidth;
        float gridH = (rows - 1) * stepY + cellHeight;

        Vector2 center = bounds.offset;

        float left = center.x - gridW / 2f + cellWidth / 2f;
        float top = center.y + gridH / 2f - cellHeight / 2f;

        for (int i = 0; i < toPlace.Length; i++)
        {
            int r = i / cols;
            int c = i % cols;

            float x = left + c * stepX;
            float y = top - r * stepY;

            GridCell cell = Instantiate(cellPrefab, transform);
            cell.transform.localPosition = new Vector3(x, y, 0f);

            cell.Init(this, i, toPlace[i], cellSize, lockedTextSize);
            cells.Add(cell);
        }

        Debug.Log($"Generated {cells.Count} cells in {cols}x{rows} grid");
    }

    public void TryCensorCell(GridCell cell)
    {
        if (showDebugLogs)
            Debug.Log($"TryCensorCell called - Cell null? {cell == null}, Censored? {cell?.censored}, Prefab null? {censorPrefab == null}, Ink: {ink}/{inkCostPerCell}");

        if (cell == null || cell.censored)
        {
            if (showDebugLogs) Debug.Log($"Cell is null or already censored");
            return;
        }

        if (censorPrefab == null)
        {
            if (showDebugLogs) Debug.LogError("Censor prefab is null!");
            return;
        }

        if (ink < inkCostPerCell)
        {
            if (showDebugLogs) Debug.Log($"Not enough ink: {ink} < {inkCostPerCell}");
            return;
        }

        if (showDebugLogs) Debug.Log($"Creating censor for cell {cell.index}");

        GameObject censor = Instantiate(censorPrefab, cell.transform);
        censor.transform.localPosition = Vector3.zero;
        censor.transform.localRotation = Quaternion.identity;
        censor.name = $"Censor_{cell.index}";

        // Get the sprite renderer to determine the original sprite size
        SpriteRenderer censorSprite = censor.GetComponent<SpriteRenderer>();
        if (censorSprite != null && censorSprite.sprite != null)
        {
            // Calculate the scale needed to match cell size
            // Sprite size is in pixels, converted to units by PPU (pixels per unit)
            Sprite sprite = censorSprite.sprite;
            float spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
            float spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;

            // Calculate scale to fit cell exactly
            float scaleX = cellWidth / spriteWidth;
            float scaleY = cellHeight / spriteHeight;

            censor.transform.localScale = new Vector3(scaleX, scaleY, 1f);

            if (showDebugLogs)
                Debug.Log($"Censor sprite: {spriteWidth}x{spriteHeight} units, scaling to {scaleX}x{scaleY} for cell {cellWidth}x{cellHeight}");
        }
        else
        {
            // Fallback: assume prefab is 1x1 unit
            censor.transform.localScale = new Vector3(cellWidth, cellHeight, 1f);

            if (showDebugLogs)
                Debug.Log($"No sprite found, using direct scale: {cellWidth}x{cellHeight}");
        }

        // Update collider if present
        var censorCol = censor.GetComponent<BoxCollider2D>();
        if (censorCol != null)
        {
            censorCol.size = new Vector2(cellWidth, cellHeight);
            censorCol.offset = Vector2.zero;
        }

        ink -= inkCostPerCell;
        cell.censored = true;

        if (showDebugLogs) Debug.Log($"Successfully censored cell {cell.index}. Remaining ink: {ink}");
    }

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        cells.Clear();
    }
}