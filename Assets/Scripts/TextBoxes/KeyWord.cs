using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KeyWord : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text textTarget;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ParticleSystem discover;
    [TextArea]
    [SerializeField] private string populateText = "Keyword";

    [Header("Progress Color")]
    [SerializeField] private Color targetColor = Color.yellow;
    [Min(1)]
    [SerializeField] private int collisionsRequired = 5;

    [Header("Score")]
    [SerializeField] private int scoreToAdd = 10;

    [Header("Collision Filter (optional)")]
    [Tooltip("If set, only objects with this tag will trigger.")]
    [SerializeField] private string requiredTag = "Player";

    [Header("Behavior")]
    [Tooltip("If true, progress resets when the last collider leaves the box.")]
    [SerializeField] private bool resetProgressWhenEmpty = false;

    private readonly HashSet<Collider2D> _inside = new HashSet<Collider2D>();

    private Color _startColor;
    private int _collisionCount;
    private bool _scoreAwarded;

    private Collider2D _myCollider;
    private Vector3 _hitPos; // <-- store last hit position here

    private void Awake()
    {
        _myCollider = GetComponent<Collider2D>();

        if (!textTarget) textTarget = GetComponentInChildren<TMP_Text>();
        if (!textTarget)
        {
            Debug.LogError($"{nameof(KeyWord)}: No TMP_Text found. Assign Text Target or put TMP under this object.");
            enabled = false;
            return;
        }

        textTarget.text = populateText;
        _startColor = textTarget.color;
    }

    private void OnValidate()
    {
        if (textTarget) textTarget.text = populateText;
        collisionsRequired = Mathf.Max(1, collisionsRequired);
    }

    private bool PassesFilter(Collider2D other)
    {
        if (string.IsNullOrWhiteSpace(requiredTag)) return true;
        return other.CompareTag(requiredTag);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!PassesFilter(other)) return;

        // Count only once per entry
        if (_inside.Add(other))
        {
            // Best guess for "hit position" in trigger world:
            // closest point on THIS collider to the OTHER collider's center
            Vector2 otherCenter = other.bounds.center;
            _hitPos = _myCollider.ClosestPoint(otherCenter);
            _hitPos.z = 0f; // keep particles in 2D plane

            RegisterCollision();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!PassesFilter(other)) return;

        _inside.Remove(other);

        if (resetProgressWhenEmpty && _inside.Count == 0)
            ResetProgress();
    }

    private void RegisterCollision()
    {
        if (_scoreAwarded) return;

        _collisionCount++;

        float t = Mathf.Clamp01(_collisionCount / (float)collisionsRequired);
        textTarget.color = Color.Lerp(_startColor, targetColor, t);

        if (_collisionCount >= collisionsRequired)
        {
            _scoreAwarded = true;
            textTarget.color = targetColor;
            ScoreAdd(scoreToAdd);
        }
    }

    private void ResetProgress()
    {
        _collisionCount = 0;
        _scoreAwarded = false;
        textTarget.color = _startColor;
    }

    private void ScoreAdd(int amount)
    {
        if (discover)
            Instantiate(discover, _hitPos, Quaternion.identity);

        if (gameManager != null)
            gameManager.UpdateScore(amount);
    }
}
