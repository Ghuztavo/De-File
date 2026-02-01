using UnityEngine;

public class FinishBox : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private bool substractMode;
    [SerializeField] private float posAmount = 1;
    [SerializeField] private float negScore = 1;

    [Header("Particle Prefabs (NOT scene objects)")]
    [SerializeField] private ParticleSystem posParticlePrefab;
    [SerializeField] private ParticleSystem negParticlePrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ball")) return;

        Vector3 hitPos = collision.transform.position;
        Destroy(collision.gameObject);

        float finalAmount = substractMode ? -negScore : posAmount;
        if (gameManager != null) gameManager.UpdateScore(finalAmount);

        audioSource.PlayOneShot(hitSound);

        SpawnHitParticles(hitPos);
    }

    private void SpawnHitParticles(Vector3 position)
    {
        ParticleSystem prefab = substractMode ? negParticlePrefab : posParticlePrefab;
        if (!prefab) return;

        ParticleSystem ps = Instantiate(prefab, position, Quaternion.identity);

        ps.Play(true);

        // Auto-destroy after it finishes
        float life = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, life + 0.1f);
    }
}