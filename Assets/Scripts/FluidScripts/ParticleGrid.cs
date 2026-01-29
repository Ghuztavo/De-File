using System.Collections.Generic;
using UnityEngine;

public class ParticleGrid : MonoBehaviour
{
    public static ParticleGrid Instance; 

    public float cellSize = 0.444f; // keep the same or a bit larger than the attraction radius of the particles

    private Dictionary<Vector2Int, List<ParticleAttractionScript>> grid
        = new Dictionary<Vector2Int, List<ParticleAttractionScript>>();

    void Awake()
    {
        Instance = this;
        Debug.Log("ParticleGrid initialized.");
    }

    private void FixedUpdate()
    {
        grid.Clear();
    }

    public void Clear()
    {
        grid.Clear();
    }

    public void AddParticle(ParticleAttractionScript p)
    {
        Vector2Int cell = WorldToCell(p.transform.position);

        if (!grid.TryGetValue(cell, out var list))
        {
            list = new List<ParticleAttractionScript>();
            grid[cell] = list;
        }

        list.Add(p);
    }

    public List<ParticleAttractionScript> GetNeighbors(Vector2 position)
    {
        List<ParticleAttractionScript> neighbors = new();

        Vector2Int center = WorldToCell(position);

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int cell = center + new Vector2Int(x, y);
                if (grid.TryGetValue(cell, out var list))
                    neighbors.AddRange(list);
            }

        return neighbors;
    }

    Vector2Int WorldToCell(Vector2 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / cellSize),
            Mathf.FloorToInt(pos.y / cellSize)
        );
    }

}
