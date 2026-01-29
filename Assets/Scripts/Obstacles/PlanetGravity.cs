using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    [SerializeField] float gravityStrength = 20f;
    [SerializeField] float maxDistance = 10f;
    [SerializeField] float minDistance = 1.5f; 

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (!rb) return;

        Vector2 dir = (Vector2)transform.position - rb.position;
        float dist = dir.magnitude;

        if (dist > maxDistance) return;

        dist = Mathf.Clamp(dist, minDistance, maxDistance);
        dir.Normalize();

        // Smooth game-style gravity
        float force = gravityStrength * (1f - dist / maxDistance);

        rb.AddForce(dir * force);
    }
}