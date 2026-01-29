using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ParticleAttractionScript : MonoBehaviour
{
    public Rigidbody2D rb { get; private set; }

    [SerializeField] public float attractionRadius = 0.4f; // How far particles can detect each other
    [SerializeField] public float attractionStrength = 0.4f; // how strongly particles attract each other

    [SerializeField] public float repulsionStrength = 0.5f; // How strongly particles repel each other at close range
    [SerializeField] public float restDistance = 0.25f; // Preferred minimum distance between particles

    [SerializeField] private float restDensity = 2f; // target density for the fluid
    [SerializeField] private float pressureStrength = 1.2f; // How strongly density turns into force

    private float density = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Register in spatial grid
        ParticleGrid.Instance.AddParticle(this);

        var neighbors = ParticleGrid.Instance.GetNeighbors(rb.position);

        // --- Density pass ---
        density = 0f;

        foreach (var other in neighbors)
        {
            if (other == this){ continue; }

            float d = Vector2.Distance(rb.position, other.rb.position);
            if (d > attractionRadius) { continue; }

            float influence = 1f - (d / attractionRadius);
            density += influence;
        }

        // --- Force pass ---
        float pressure = density - restDensity;

        foreach (var other in neighbors)
        {
            if (other == this){ continue; }

            Vector2 dir = other.rb.position - rb.position;
            float dist = dir.magnitude;
            if (dist <= 0f || dist > attractionRadius) { continue; }

            Vector2 normal = dir / dist;

            // Attraction
            float safeDist = Mathf.Max(dist, 0.05f);
            rb.AddForce(normal * (attractionStrength / safeDist));

            // Repulsion
            if (dist < restDistance)
            {
                rb.AddForce(-normal * repulsionStrength * (restDistance - dist));
            }

            // Pressure
            if (pressure > 0f)
            {
                rb.AddForce(-normal * pressure * pressureStrength);
            }
        }
    }

    public float GetParticleAttractionRadius()
    {
        return attractionRadius;
    }
}
