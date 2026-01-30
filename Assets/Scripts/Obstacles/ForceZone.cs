using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ForceZone : MonoBehaviour
{
    [Tooltip("The magnitude of the force applied to objects.")]
    [SerializeField] private float forceMagnitude = 10f;

    // Apply force while the object is inside the trigger
    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.AddForce(transform.right * forceMagnitude);
        }
    }

    private void OnValidate()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("ForceZone collider must be a Trigger. Setting isTrigger to true.", this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 direction = transform.right * 1.5f;
        Gizmos.DrawRay(transform.position, direction);
        
        // Draw arrow head
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(transform.position + direction, right * 0.25f);
        Gizmos.DrawRay(transform.position + direction, left * 0.25f);
    }
}
