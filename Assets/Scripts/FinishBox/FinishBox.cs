using UnityEditor.Build.Content;
using UnityEngine;

public class FinishBox : MonoBehaviour
{
    // There is no game manager yet
    [SerializeField] private GameManager gameManager;
    [SerializeField] private bool substractMode;
    [SerializeField] private int scoreAmount = 1;

    // Detect when particles enter the finish the box, destroy them and increase score
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Destroy(collision.gameObject);
            
            if (gameManager != null)
            {
                int finalAmount = substractMode ? -scoreAmount : scoreAmount;
                gameManager.UpdateScore(finalAmount);
            }
        }
    }
}
