using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject StartButton;
    [Header("Game State")]
    public int Score = 0;
    public float Ink = 10f;
    public int TotalObjectSpawns = 500;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //on click start game
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //score
    //total ink
    //total object spawns
    //total time is implemented later in another script

}
