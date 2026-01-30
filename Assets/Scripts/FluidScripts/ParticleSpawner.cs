using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    //public bool isPouring { get; set; }

    [SerializeField] private GameObject particlePrefab;

    [SerializeField] private GameManager gameManager;
    private int totalCount => gameManager != null ? gameManager.TotalObjectSpawns : 500;
    [SerializeField] private float spawnRadius = 0.2f;

    [SerializeField] private float spawnRate = 100f;
    // particles per second

    private int spawned = 0;
    private float timer = 0f;

    void Update()
    {
        if (gameManager == null) { return; }

        //isPouring = true; // For testing purposes, always pour <----------------------------------
        if (gameManager.gameStarted == true)
        {
            StartPouring();
        }
    }

    public void StartPouring()
    {
        if (spawned >= totalCount) return;

        timer += Time.deltaTime;

        float interval = 1f / spawnRate;

        while (timer >= interval && spawned < totalCount)
        {
            SpawnOne();
            timer -= interval;
        }
    }

    void SpawnOne()
    {
        Vector2 pos = (Vector2)transform.position
                      + Random.insideUnitCircle * spawnRadius;

        Instantiate(particlePrefab, pos, Quaternion.identity);

        spawned++;
    }
}
