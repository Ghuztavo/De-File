using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public Transform censorsRoot;

    [Header("Overflow Behavior")]
    public bool truncateToFitBounds = true;

    [Header("Ink")]
    [SerializeField] private GameManager gameManager;
    public float ink
    {
        get => gameManager != null ? gameManager.Ink : 10f;
        set { if (gameManager != null) gameManager.Ink = value; }
    }
    public float inkCostPerCell = 1f;

    [Header("Drag Paint")]
    public Camera paintCamera;              // if null -> Camera.main
    public LayerMask cellLayerMask = ~0;    // should include ONLY cell colliders

    [Header("Redo Button (UI)")]
    // Assign the UI Button from your Canvas here (preferred)
    [Tooltip("Assign the Canvas UI Button here (preferred).")]
    public Button redoUIButton;

    // Backwards-compat: optional world object (sprite with collider) - not required if using UI Button
    [Header("Redo Button (world object, optional)")]
    public GameObject redoButton;
    private Collider2D redoButtonCollider;

    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Censor Settings")]
    public int censorLayer = 2; // 2 = Ignore Raycast (recommended)

    private readonly List<GridCell> cells = new();
    private readonly List<GridCell> lastCensoredCells = new();
    private bool canRedo = false;
    private BoxCollider2D bounds;
    private bool wasMousePressed = false;

    private readonly Dictionary<int, GameObject> censorByCellIndex = new();
    private CompositeCollider2D censorComposite;
    private Rigidbody2D censorRb;

    private bool compositeRefreshQueued = false;

    private void Awake()
    {
        bounds = GetComponent<BoxCollider2D>();
        bounds.isTrigger = true;

        if (censorsRoot == null)
        {
            var t = transform.Find("CensorsRoot");
            if (t != null) censorsRoot = t;
        }

        EnsureCensorComposite();

        // Make sure your paint raycasts never consider the censor layer
        cellLayerMask &= ~(1 << censorLayer);

        // Wire up UI Button listener (preferred)
        if (redoUIButton != null)
        {
            redoUIButton.onClick.AddListener(PerformRedo);
            // ensure initial interactable state matches canRedo
            redoUIButton.interactable = canRedo;
        }

        // Backwards-compatible: grab collider if a world game object is used instead
        if (redoButton != null)
        {
            redoButtonCollider = redoButton.GetComponent<Collider2D>();
            if (redoButtonCollider == null)
                Debug.LogWarning("Redo button world object needs a Collider2D component!");
        }
    }

    private void Start()
    {
        Generate();
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        bool isPressed = Mouse.current.leftButton.isPressed;

        // Read screen position once (used by world-space button check and paint)
        Vector2 screenPos = Mouse.current.position.ReadValue();

        // If pointer is over any UI element, don't paint. UI will handle the click (e.g. button OnClick).
        bool pointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (isPressed)
        {
            // If we're clicking a world-space redo button (legacy), we want to handle it.
            if (!pointerOverUI && CheckRedoButtonClick(screenPos))
            {
                // world-space redo button pressed
                PerformRedo();
            }
            else
            {
                // If the pointer is over UI, don't start painting or clear the lastCensoredCells.
                if (pointerOverUI)
                {
                    // do nothing (let UI handle the click)
                }
                else
                {
                    // Normal painting flow
                    if (!wasMousePressed)
                    {
                        lastCensoredCells.Clear();
                        canRedo = false;
                        UpdateRedoButtonState();
                    }

                    PaintAtScreenPosition(screenPos);
                }
            }
        }
        else if (wasMousePressed && !isPressed)
        {
            // Mouse released: if we painted cells this press, allow redo.
            if (lastCensoredCells.Count > 0)
            {
                canRedo = true;
                UpdateRedoButtonState();
                if (showDebugLogs)
                    Debug.Log($"Paint action completed. {lastCensoredCells.Count} cells censored. Redo available.");
            }
        }

        wasMousePressed = isPressed;
    }

    private void EnsureCensorComposite()
    {
        if (censorsRoot == null)
        {
            Debug.LogError("censorsRoot not assigned. Create a child 'CensorsRoot' and assign it.");
            return;
        }

        censorRb = censorsRoot.GetComponent<Rigidbody2D>();
        if (censorRb == null) censorRb = censorsRoot.gameObject.AddComponent<Rigidbody2D>();
        censorRb.bodyType = RigidbodyType2D.Static;
        censorRb.simulated = true;

        censorComposite = censorsRoot.GetComponent<CompositeCollider2D>();
        if (censorComposite == null) censorComposite = censorsRoot.gameObject.AddComponent<CompositeCollider2D>();

        censorComposite.geometryType = CompositeCollider2D.GeometryType.Outlines;
        censorComposite.generationType = CompositeCollider2D.GenerationType.Synchronous;
        censorComposite.isTrigger = false;
    }

    private void QueueRefreshCensorComposite()
    {
        if (censorComposite == null || compositeRefreshQueued) return;
        compositeRefreshQueued = true;
        StartCoroutine(RefreshCompositeNextFrame());
    }

    private IEnumerator RefreshCompositeNextFrame()
    {
        yield return null; // wait end-of-frame so Destroy() & collider updates apply
        Physics2D.SyncTransforms();

        if (censorComposite != null)
        {
            censorComposite.enabled = false;
            censorComposite.enabled = true;
        }

        compositeRefreshQueued = false;
    }

    private bool CheckRedoButtonClick(Vector2 screenPos)
    {
        // Legacy world-space collider check (kept for compatibility).
        if (redoButton == null || redoButtonCollider == null || !canRedo)
            return false;

        Camera cam = paintCamera != null ? paintCamera : Camera.main;
        if (cam == null) return false;

        Vector3 worldPoint;
        if (cam.orthographic)
            worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        else
        {
            float distance = Mathf.Abs(cam.transform.position.z - redoButton.transform.position.z);
            worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
        }

        return redoButtonCollider.OverlapPoint(new Vector2(worldPoint.x, worldPoint.y));
    }

    public void PerformRedo()
    {
        if (!canRedo || lastCensoredCells.Count == 0)
        {
            if (showDebugLogs) Debug.Log("No redo available");
            return;
        }

        if (showDebugLogs) Debug.Log($"Performing redo on {lastCensoredCells.Count} cells");

        foreach (GridCell cell in lastCensoredCells)
        {
            if (cell == null || !cell.censored) continue;

            if (censorByCellIndex.TryGetValue(cell.index, out var censorGo) && censorGo != null)
                Destroy(censorGo);

            censorByCellIndex.Remove(cell.index);

            cell.censored = false;
            ink += inkCostPerCell;
        }

        if (showDebugLogs)
            Debug.Log($"Redo complete. Restored {lastCensoredCells.Count * inkCostPerCell} ink. Total ink: {ink}");

        lastCensoredCells.Clear();
        canRedo = false;
        UpdateRedoButtonState();

        QueueRefreshCensorComposite();
    }

    private void UpdateRedoButtonState()
    {
        if (redoUIButton != null)
        {
            redoUIButton.interactable = canRedo;
        }

        // optional: if using world object button, toggle its visual state here too
        if (redoButton != null)
        {
            // Example: enable/disable sprite renderer so it's visibly disabled
            var sr = redoButton.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = canRedo ? Color.white : new Color(1f, 1f, 1f, 0.5f);
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

        Vector3 worldPoint;
        if (cam.orthographic)
            worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        else
        {
            float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
            worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
        }

        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);

        if (!bounds.OverlapPoint(worldPoint2D)) return;

        RaycastHit2D hit = Physics2D.Raycast(worldPoint2D, Vector2.zero, 0f, cellLayerMask);
        if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
        {
            GridCell cell = hit.collider.GetComponent<GridCell>();
            if (cell != null)
            {
                TryCensorCell(cell);
                return;
            }
        }

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint2D, cellLayerMask);
        foreach (var col in hits)
        {
            if (col == null || col == bounds) continue;
            if (!col.transform.IsChildOf(transform)) continue;

            GridCell cell = col.GetComponent<GridCell>();
            if (cell != null)
            {
                TryCensorCell(cell);
                return;
            }
        }
    }

    public void Generate()
    {
        Clear();

        if (cellPrefab == null || string.IsNullOrEmpty(stringContent))
            return;

        Vector2 cellSize = new Vector2(cellWidth, cellHeight);

        float stepX = Mathf.Max(0.01f, cellWidth + cellGapX);
        float stepY = Mathf.Max(0.01f, cellHeight + cellGapY);

        Vector2 b = bounds.size;
        float usableW = Mathf.Max(0f, b.x);
        float usableH = Mathf.Max(0f, b.y);

        int cols = Mathf.Max(1, Mathf.FloorToInt((usableW + Mathf.Abs(cellGapX)) / stepX));
        int rows = Mathf.Max(1, Mathf.FloorToInt((usableH + Mathf.Abs(cellGapY)) / stepY));

        int capacity = cols * rows;

        string toPlace = stringContent;
        if (toPlace.Length > capacity)
        {
            if (!truncateToFitBounds) return;
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
    }

    public void TryCensorCell(GridCell cell)
    {
        if (cell == null || cell.censored) return;
        if (censorPrefab == null) return;
        if (ink < inkCostPerCell) return;
        if (censorsRoot == null) return;

        GameObject censor = Instantiate(censorPrefab, censorsRoot);
        censorByCellIndex[cell.index] = censor;

        // Apply layer to whole censor hierarchy (important if collider is on a child)
        foreach (Transform t in censor.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = censorLayer;

        // Position/rotation
        censor.transform.position = cell.transform.position;
        censor.transform.rotation = Quaternion.identity;
        censor.name = $"Censor_{cell.index}";

        // Ensure composite + mark ALL colliders usedByComposite (important if collider is on a child)
        EnsureCensorComposite();

        var colliders = censor.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            col.usedByComposite = true;
            col.isTrigger = false;
        }

        // Scale sprite (visual)
        SpriteRenderer censorSprite = censor.GetComponentInChildren<SpriteRenderer>();
        if (censorSprite != null && censorSprite.sprite != null)
        {
            Sprite sprite = censorSprite.sprite;
            float spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
            float spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;

            float scaleX = cellWidth / spriteWidth;
            float scaleY = cellHeight / spriteHeight;
            censor.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
        else
        {
            censor.transform.localScale = new Vector3(cellWidth, cellHeight, 1f);
        }

        ink -= inkCostPerCell;
        cell.censored = true;
        lastCensoredCells.Add(cell);

        QueueRefreshCensorComposite();
    }

    public void Clear()
    {
        // Destroy generated CELLS (children except CensorsRoot)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (censorsRoot != null && child == censorsRoot) continue;
            Destroy(child.gameObject);
        }

        // Destroy all censors under the composite root (keep the root)
        if (censorsRoot != null)
        {
            for (int i = censorsRoot.childCount - 1; i >= 0; i--)
                Destroy(censorsRoot.GetChild(i).gameObject);
        }

        cells.Clear();
        lastCensoredCells.Clear();
        censorByCellIndex.Clear();
        canRedo = false;

        UpdateRedoButtonState();
        QueueRefreshCensorComposite();
    }
}