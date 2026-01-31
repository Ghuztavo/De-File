using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlanetGravity : MonoBehaviour
{
    public enum GravityCenterMode
    {
        Center,
        EdgeTop,
        EdgeBottom,
        EdgeLeft,
        EdgeRight
    }

    [Header("Gravity")]
    [SerializeField] private float gravityStrength = 20f;
    [SerializeField] private float minDistance = 1.5f;

    [Header("Gravity Center")]
    [SerializeField] private GravityCenterMode centerMode = GravityCenterMode.Center;

    [Header("Pulse")]
    [SerializeField] private bool pulsing = true;
    [SerializeField] private float timeWindow = 0.35f; // seconds ON, then seconds OFF

    private BoxCollider2D box;
    private float maxDistance;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        UpdateMaxDistanceFromBox();
    }

    private void OnValidate()
    {
        if (!box) box = GetComponent<BoxCollider2D>();
        timeWindow = Mathf.Max(0.01f, timeWindow);
        UpdateMaxDistanceFromBox();
    }

    private void UpdateMaxDistanceFromBox()
    {
        if (!box) return;

        // World-space size (accounts for transform scale).
        Vector2 worldSize = Vector2.Scale(box.size, transform.lossyScale);
        Vector2 extents = worldSize * 0.5f;

        // Center->corner distance
        maxDistance = extents.magnitude;

        minDistance = Mathf.Min(minDistance, Mathf.Max(0.01f, maxDistance));
    }

    private bool PulseIsOn()
    {
        if (!pulsing) return true;

        float period = timeWindow * 2f;
        return (Time.time % period) < timeWindow;
    }

    private Vector2 GetGravityPointWorld()
    {
        // BoxCollider2D offset is in LOCAL space
        Vector2 localCenter = box.offset;

        Vector2 localPoint = localCenter;

        // Move from center to an edge in LOCAL space (rotation handled by TransformPoint)
        Vector2 half = box.size * 0.5f;

        switch (centerMode)
        {
            case GravityCenterMode.EdgeTop:
                localPoint += Vector2.up * half.y;
                break;
            case GravityCenterMode.EdgeBottom:
                localPoint += Vector2.down * half.y;
                break;
            case GravityCenterMode.EdgeLeft:
                localPoint += Vector2.left * half.x;
                break;
            case GravityCenterMode.EdgeRight:
                localPoint += Vector2.right * half.x;
                break;
            case GravityCenterMode.Center:
            default:
                break;
        }

        return transform.TransformPoint(localPoint);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!PulseIsOn()) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (!rb) return;

        if (maxDistance <= 0.001f) UpdateMaxDistanceFromBox();

        Vector2 gravityPoint = GetGravityPointWorld();
        Vector2 dir = gravityPoint - rb.position;
        float dist = dir.magnitude;

        if (dist > maxDistance) return;

        dist = Mathf.Clamp(dist, minDistance, maxDistance);
        dir.Normalize();

        float force = gravityStrength * (1f - dist / maxDistance);
        rb.AddForce(dir * force, ForceMode2D.Force);
    }
}
