using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject StartButton;
    [Header("Game State")]
    public int Score = 0;
    public float Ink = 10f;
    public int TotalObjectSpawns = 500;

    public float levelTime = 60f;
    public bool timeUp = false;

    [SerializeField] private LevelTimer timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //on click start game
        
        if (timer != null)
        {
            StartTimer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //score
    //total ink
    //total object spawns
    //total time is implemented later in another script

    public void StartTimer()
    {
        timer.StartCountdown(levelTime);
    }

}
