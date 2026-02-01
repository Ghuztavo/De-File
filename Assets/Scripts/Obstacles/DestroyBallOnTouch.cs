using UnityEngine;

public class DestroyBallOnTouch : MonoBehaviour
{
    [SerializeField] private string targetTag = "Ball";
    [Header("Score Settings")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float amountToIncrease = 0.5f;

    [Header("Particle Prefabs (NOT scene objects)")]
    [SerializeField] private ParticleSystem posParticlePrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;

        Vector3 hitPos = collision.transform.position;
        Destroy(collision.gameObject);

        if (gameManager != null) gameManager.UpdateScore(amountToIncrease);

        SpawnHitParticles(hitPos);
    }

    private void SpawnHitParticles(Vector3 position)
    {
        ParticleSystem prefab = posParticlePrefab;
        if (!prefab) return;

        ParticleSystem ps = Instantiate(prefab, position, Quaternion.identity);

        ps.Play(true);

        // Auto-destroy after it finishes
        float life = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, life + 0.1f);
    }
}