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

    public enum PlayerMode 
	{
        NONE,
		ATTACKER,
		DEFENDER,
        PENALTY
	};

    public TextMeshProUGUI matchTimer;
    public Slider playerEnergySlider;
    public Slider enemyEnergySlider;

    private int matchPerGame = 5;
    private float timeLimit = 140f;
    private float timeRush = 15f;
    private int maxEnergy = 6;

    private float att_EnergyRegeneration = 0.5f;
	private float def_EnergyRegeneration = 0.5f;
	private float att_SpawnRate = 0.4f;
	private float def_SpawnRate = 0.6f;
	private float att_EnergyCost = 2f;
	private float def_EnergyCost = 3f;

    private bool arMode = false;

    [HideInInspector]
    public PlayerMode playerMode;
    [HideInInspector]
    public PlayerMode enemyMode;

    [HideInInspector]
    public float playerSpawnCost;
    [HideInInspector]
    public float enemySpawnCost;

    [HideInInspector]
    public float playerEnergyRegeneration;
    [HideInInspector]
    public float enemyEnergyRegeneration;


    [HideInInspector]
    public bool isSetup = false;

    [HideInInspector]
    public float playerEnergy;
    [HideInInspector]
    public float enemyEnergy;

    private float lastPlayerFillEnergy;
    private float lastEnemyFillEnergy;

    private float fieldLength;

    private float timeRemaining;
    private bool timerIsRunning = false;

    private bool isRushTime;
    private int playerScore;
    private int enemyScore;
    
    private int gameMatch;

    public List<GameObject> playerTeam;
    public List<GameObject> enemyTeam;

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
        fieldLength = FootballField();
        MatchSetup(2);        
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
                GeneratePlayerEnergy();
                GenerateEnemyEnergy();
                UpdateEnergySlider();
            }
            else
            {
                timerIsRunning = false;
                EndMatch(false);
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

        playerScore = 0;
        enemyScore = 0;
        isRushTime = false;
    }

    void InitMatch(int level)
    {
        // match level 1 -> PlayerTeam2 will be defender
        // match level 2 -> PlayerTeam2 will be attacker
        if (level < 6)  //normal match (5 match)
        {
            if ((level % 2) != 0)
            {
                playerMode = PlayerMode.ATTACKER;
                playerSpawnCost = att_EnergyCost;
                playerEnergyRegeneration = att_EnergyRegeneration;

                enemyMode = PlayerMode.DEFENDER;
                enemySpawnCost = def_EnergyCost;
                enemyEnergyRegeneration = def_EnergyRegeneration;
            }
            else
            {
                playerMode = PlayerMode.DEFENDER;
                playerSpawnCost = def_EnergyCost;
                playerEnergyRegeneration = def_EnergyRegeneration;

                enemyMode = PlayerMode.ATTACKER;
                enemySpawnCost = att_EnergyCost;
                enemyEnergyRegeneration = att_EnergyRegeneration;
            }

            PlayerPool.SharedInstance.InitPlayer(playerMode == PlayerMode.ATTACKER);
            PlayerPool.SharedInstance.InitEnemy(enemyMode == PlayerMode.ATTACKER);
        }
        else if (level == 6)
        {
            playerMode = PlayerMode.PENALTY;
            enemyMode = PlayerMode.NONE;
        }        
    }

    void GameSetup()
    {
        // Player vs Player
        // Player vs AI
        //MatchSetup(gameMatch);
        //PlayerPool.SharedInstance.InitPlayer();
    }

    private void GeneratePlayerEnergy()
    {
        if (playerEnergy < maxEnergy)
        {
            if((timeRemaining + playerEnergyRegeneration) < lastPlayerFillEnergy)
            {
                RefillPlayerEnergy();
                lastPlayerFillEnergy = timeRemaining;
            }
        }
    }

    private void GenerateEnemyEnergy()
    {
        if (enemyEnergy < maxEnergy)
        {
            if((timeRemaining + enemyEnergyRegeneration) < lastEnemyFillEnergy)
            {
                RefillEnemyEnergy();
                lastEnemyFillEnergy = timeRemaining;
            }
        }
    }

    public void UpdateEnergySlider()
    {
        playerEnergySlider.value = playerEnergy;
        enemyEnergySlider.value = enemyEnergy;
    }

    void MatchSetup(int level)
    {
        timerIsRunning = false;
        isSetup = true; // start setup matching
        // level 1..5 -> normal game
        // level 6 -> Penalty - Maze game
        InitMatch(level);

        timeRemaining = timeLimit;
        playerEnergy = maxEnergy;
        enemyEnergy = maxEnergy;
        lastPlayerFillEnergy = timeRemaining;
        lastEnemyFillEnergy = timeRemaining;

        matchTimer.text = (int)timeRemaining + "s";
        playerEnergySlider.maxValue = maxEnergy;
        enemyEnergySlider.maxValue = maxEnergy;

        UpdateEnergySlider();

        isSetup = false;
        timerIsRunning = true;

    }

    public void EndMatch(bool isWin)
    {
        if (isWin)
        {
            if (playerMode == PlayerMode.ATTACKER)
                playerScore += 1;
            else
                enemyScore +=1;
        }
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

    private void RefillPlayerEnergy()
    {
        playerEnergy++;
        if (playerEnergy > maxEnergy)
            playerEnergy = maxEnergy;
    }

    private void RefillEnemyEnergy()
    {
        enemyEnergy++;
        if (enemyEnergy > maxEnergy)
            enemyEnergy = maxEnergy;
    }

    public GameObject GetChildWithName(GameObject obj, string name) 
	{
		Transform trans = obj.transform;
		Transform childTrans = trans. Find(name);
		if (childTrans != null) 
		{
			return childTrans.gameObject;
		} 
		else 
		{
			return null;
		}
	}

    private float FootballField()
    {
        GameObject field = GameObject.FindGameObjectWithTag("FootballField");

        Mesh planeMesh = field.GetComponent<MeshFilter>().mesh;
        Bounds bounds = planeMesh.bounds;

        float boundsX = field.transform.localScale.x * bounds.size.x;
        float boundsY = field.transform.localScale.y * bounds.size.y;
        float boundsZ = field.transform.localScale.z * bounds.size.z;

        float fLength = Mathf.Max(Mathf.Max(boundsX, boundsY), boundsZ);

        return fLength;
    }

    public float GetFieldLength()
    {
        return fieldLength;
    }

}
