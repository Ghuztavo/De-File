using UnityEditor.Build.Content;
using UnityEngine;

public class FinishBox : MonoBehaviour
{
    // There is no game manager yet
    [SerializeField] private GameManager gameManager;
    // triggerCount replaced by GameManager.Score
    private int triggerCount
    {
        get => gameManager != null ? gameManager.Score : 0;
        set { if (gameManager != null) gameManager.Score = value; }
    }

    // Detect when particles enter the finish the box, destroy them and increase score
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Destroy(collision.gameObject);
            triggerCount++;
            Debug.Log("Particles finished: " + triggerCount);
            //gameManager.IncreaseScore(1);
        }
    }
}
