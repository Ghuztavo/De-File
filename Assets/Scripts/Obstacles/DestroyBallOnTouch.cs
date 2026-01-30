using UnityEngine;

public class DestroyBallOnTouch : MonoBehaviour
{
    [SerializeField] private string targetTag = "Ball";
    [Header("Score Settings")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private bool subtractMode;
    [SerializeField] private int baseScore = 0;
    [SerializeField] [Range(0f, 1f)] private float scorePercentage = 1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            if (gameManager != null)
            {
                int amount = Mathf.RoundToInt(baseScore * scorePercentage);
                if (subtractMode) amount = -amount;
                gameManager.UpdateScore(amount);
            }
            Destroy(collision.gameObject);
        }
    }
}