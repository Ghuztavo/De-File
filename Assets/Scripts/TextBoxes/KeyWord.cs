using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KeyWord : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text textTarget;
    [SerializeField] private GameManager gameManager;
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

    private void Awake()
    {
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
            RegisterCollision();
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

        // Progress from 0..1 based on collisions
        float t = Mathf.Clamp01(_collisionCount / (float)collisionsRequired);

        // Each collision gets closer to target color
        textTarget.color = Color.Lerp(_startColor, targetColor, t);

        // Award score only when requirement is met
        if (_collisionCount >= collisionsRequired)
        {
            _scoreAwarded = true;
            textTarget.color = targetColor; // snap to final color
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
        int finalAmount = amount;
        if (gameManager != null) gameManager.UpdateScore(finalAmount);
        Debug.Log($"Score +{amount} (hook this into your score system)");
    }
}
