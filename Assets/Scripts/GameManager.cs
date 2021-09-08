using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public enum GameMode 
	{
		Player_Player,
		Player_AI
	};

    private int matchPerGame = 5;
    private float timeLimit = 140f;
    private float timeRush = 15f;
    private int maxEnergy = 6;

    private bool arMode = false;

    public TextMeshProUGUI matchTimer;
    public Slider playerEnergySlider;
    public Slider enemyEnergySlider;



    private float timeRemaining;
    private bool timerIsRunning = false;
    private bool isRushTime;
    private int playerScore;
    private int enemyScore;
    private int playerEnergy;
    private int enemyEnergy;
    private int gameMatch;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        //InitGame();
    }

    void Start()
    {
        MatchSetup(1);
        
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if ( timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining < timeRush)
                    isRushTime = true;

                matchTimer.text = (int)timeRemaining + "s";

            }
            else
            {
                timerIsRunning = false;
                EndMatch();
            }
        }
    }

    void InitGame(GameMode mode)
    {
        Debug.Log("Init Game");
        switch (mode)
        {
            case GameMode.Player_Player:
                // player1 -> PlayerTeam1
                // player2 -> PLayerTeam2
            break;

            case GameMode.Player_AI:
                // player -> PlayerTeam1
                // AI -> PlayerTeam2
            break;
        }
        isRushTime = false;
    }

    void InitMatch(int level)
    {
        // match level 1 -> PlayerTeam2 will be defender
        // match level 2 -> PlayerTeam2 will be attacker
    }

    void GameSetup()
    {
        // Player vs Player
        // Player vs AI
    }

    void MatchSetup(int level)
    {
        // level 1..5 -> normal game
        // level 6 -> Penalty - Maze game

        timeRemaining = timeLimit;
        playerEnergy = maxEnergy;
        enemyEnergy = maxEnergy;
        timerIsRunning = true;

        matchTimer.text = (int)timeRemaining + "s";
        playerEnergySlider.maxValue = maxEnergy;
        playerEnergySlider.value = playerEnergy;
        enemyEnergySlider.maxValue = maxEnergy;
        enemyEnergySlider.value = enemyEnergy;
    }

    void EndMatch()
    {
        Debug.Log("End Match");
    }

    void EndGame()
    {
        Debug.Log("End Game");
    }

    public void SetArMode(bool mode)
    {
        this.arMode = mode;
    }

    public bool GetArMode()
    {
        return this.arMode;
    }
}
