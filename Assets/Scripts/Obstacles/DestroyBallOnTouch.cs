using UnityEngine;

public class DestroyBallOnTouch : MonoBehaviour
{
    [SerializeField] private string targetTag = "Ball";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            Destroy(collision.gameObject);
        }
    }
}