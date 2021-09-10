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
    private GameMode gameMode;

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

    private float playerSpawnRate;
    private float enemySpawnRate;


    [HideInInspector]
    public bool isSetup = false;

    [HideInInspector]
    public float playerEnergy;
    [HideInInspector]
    public float enemyEnergy;

    private Vector3 minEnemyField;
    private Vector3 maxEnemyField;

    private float lastPlayerFillEnergy;
    private float lastEnemyFillEnergy;

    private float fieldLength;

    private float timeRemaining;
    [HideInInspector]
    public bool timerIsRunning = false;
    private float matchStartDelay = 2f;
    private float matchEndDelay = 4f;

    private bool isRushTime;
    private int playerScore;
    private int enemyScore;
    
    private int gameMatch;

    public GameObject ball;
    [HideInInspector]
    public BallScript ballScript;


    public List<GameObject> playerTeam;
    public List<GameObject> enemyTeam;

    public GameObject ui_LoadMatch;
    public GameObject ui_EndMatch;
    public GameObject ui_GameModeInfo;
    public TextMeshProUGUI ui_PlayName;
    public TextMeshProUGUI ui_EnemyName;
    public TextMeshProUGUI ui_PlayScoreText;
    public TextMeshProUGUI ui_EnemyScoreText;
    private TextMeshProUGUI ui_MatchText;
    


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
    }

    void Start()
    {
        FootballField();
        EnemyField();

        ball = GameObject.FindGameObjectWithTag("Ball");  
        ballScript = ball.GetComponent<BallScript>();
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

    public void PlayWithAIMode()
    {
        gameMode = GameMode.Player_AI;
        InitGame();
    }

    public void PlayWithFiendsMode()
    {
        gameMode = GameMode.Player_Player;
        InitGame();
    }

    void InitGame()
    {
        Debug.Log("Init Game");

        gameMatch = 1;
        playerScore = 0;
        enemyScore = 0;

        PlayerPool.SharedInstance.InitPlayer(playerMode == PlayerMode.ATTACKER);
        PlayerPool.SharedInstance.InitEnemy(enemyMode == PlayerMode.ATTACKER);

        InitMatch();
    }

    void InitMatch()
    {
        DisplayLoadMatch();
        isRushTime = false;   

        Rigidbody rgBall = ball.gameObject.GetComponent<Rigidbody>();
        rgBall.isKinematic = false;
        //ball.transform.SetParent(null);
        ballScript.SetBallOwner(null);
        ball.transform.position = new Vector3(0f, 0.2f, 0f); 


        // match level 1 -> PlayerTeam2 will be defender
        // match level 2 -> PlayerTeam2 will be attacker
        if (gameMatch < 6)  //normal match (5 match)
        {
            if ((gameMatch % 2) != 0)
            {
                playerMode = PlayerMode.ATTACKER;
                playerSpawnCost = att_EnergyCost;
                playerEnergyRegeneration = att_EnergyRegeneration;
                playerSpawnRate = att_SpawnRate;

                enemyMode = PlayerMode.DEFENDER;
                enemySpawnCost = def_EnergyCost;
                enemyEnergyRegeneration = def_EnergyRegeneration;
                enemySpawnRate = def_SpawnRate;
            }
            else
            {
                playerMode = PlayerMode.DEFENDER;
                playerSpawnCost = def_EnergyCost;
                playerEnergyRegeneration = def_EnergyRegeneration;
                playerSpawnRate = def_SpawnRate;

                enemyMode = PlayerMode.ATTACKER;
                enemySpawnCost = att_EnergyCost;
                enemyEnergyRegeneration = att_EnergyRegeneration;
                enemySpawnRate = att_SpawnRate;
            }

            //PlayerPool.SharedInstance.InitPlayer(playerMode == PlayerMode.ATTACKER);
            //PlayerPool.SharedInstance.InitEnemy(enemyMode == PlayerMode.ATTACKER);

        }
        else if (gameMatch == 6)
        {
            playerMode = PlayerMode.PENALTY;
            enemyMode = PlayerMode.NONE;
        }

        MatchSetup();
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

    void MatchSetup()
    {
        timerIsRunning = false;
        isSetup = true; // start setup matching

        timeRemaining = timeLimit;
        playerEnergy = maxEnergy;
        enemyEnergy = maxEnergy;
        lastPlayerFillEnergy = timeRemaining;
        lastEnemyFillEnergy = timeRemaining;

        matchTimer.text = (int)timeRemaining + "s";
        playerEnergySlider.maxValue = maxEnergy;
        enemyEnergySlider.maxValue = maxEnergy;

        UpdateEnergySlider();
        UpdatePlayerName();

        ResetPlayer();

        Time.timeScale = 1f;
        isSetup = false;
        //timerIsRunning = true;

    }

    public void ResetPlayer()
    {
        foreach (GameObject go in playerTeam)
        {
            go.SetActive(false);            
        }
        foreach (GameObject go in enemyTeam)
        {
            go.SetActive(false);            
        }
        enemyTeam = new List<GameObject>();
        playerTeam = new List<GameObject>();

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
        DisplayEndMatch();
        //check endgame
        if (CheckEndGame() == false)
        {
            StartNewMatch();
        }
        else
        {
            Debug.Log("End Match true");
            Debug.Log("playerScore = "+ playerScore);
            Debug.Log("enemyScore = "+ enemyScore);
            EndGame();
        }
    }

    public void EndGame()
    {
        Debug.Log("End game");
        DisplayEndMatch();

    }

    public bool CheckEndGame()
    {
        if (gameMatch == 5)
        {
            if (playerScore == enemyScore)
                return false; //Should launch Penalty match
            else
                return true; //Player/Enemy win the game!
        }
        else if (gameMatch == 6)
        {
            return true;
        }
        else
        {
            return false;
        }   
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

    public void StartNewMatch()
    {
        gameMatch++;
        InitMatch();

    }

    void UpdatePlayerName()
    {
        if(gameMatch < 6)
        {
            //Update name
            if (playerMode == PlayerMode.ATTACKER)
            {
                ui_PlayName.text = "Player (Attacker)";
                if (gameMode == GameMode.Player_AI)
                {
                    ui_EnemyName.text = "Enemy AI (Defender)";
                }
                else
                {
                    ui_EnemyName.text = "Enemy (Defender)";
                }                
            }
            else
            {
                ui_PlayName.text = "Player (Defender)";
                if (gameMode == GameMode.Player_AI)
                {
                    ui_EnemyName.text = "Enemy AI (Attacker)";
                }
                else
                {
                    ui_EnemyName.text = "Enemy (Attacker)";
                }  
            }
            ui_GameModeInfo.SetActive(true);
        }
        else
        {
            // Penalty Maze game -> Hide this Info
            ui_GameModeInfo.SetActive(false);
        }
    }


    void DisplayEndMatch()
    {
        ui_PlayScoreText.text = playerScore + "";
        ui_EnemyScoreText.text = enemyScore + "";

        ui_LoadMatch.SetActive(true);
        Invoke("HideEndMatch", matchEndDelay);
    }

    void HideEndMatch()
    {
        ui_LoadMatch.SetActive(false);
    }

    void DisplayLoadMatch()
    {
        if (ui_LoadMatch)
        {
            TextMeshProUGUI matchText = GetChildWithName(ui_LoadMatch, "LoadMatchText").GetComponent<TextMeshProUGUI>(); 
            matchText.text = "Match : " + gameMatch;

            ui_LoadMatch.SetActive(true);
            Invoke("HideLoadMatch", matchStartDelay);
        }
    }
    void HideLoadMatch()
    {
        ui_LoadMatch.SetActive(false);
        timerIsRunning = true;
    }

    private void FootballField()
    {
        GameObject field = GameObject.FindGameObjectWithTag("FootballField");
        if (field)
        {
            Mesh planeMesh = field.GetComponent<MeshFilter>().mesh;
            Bounds bounds = planeMesh.bounds;

            float boundsX = field.transform.localScale.x * bounds.size.x;
            float boundsY = field.transform.localScale.y * bounds.size.y;
            float boundsZ = field.transform.localScale.z * bounds.size.z;

            fieldLength = Mathf.Max(Mathf.Max(boundsX, boundsY), boundsZ);
        }
    }

    private void EnemyField()
    {
        GameObject field = GameObject.FindGameObjectWithTag("EnemyField");
        if (field)
        {
            Vector3 posLB = GetChildWithName(field, "LB").transform.position;
            Vector3 posRT = GetChildWithName(field, "RT").transform.position;

            minEnemyField = Vector3.Min(posLB ,posRT);
            maxEnemyField = Vector3.Max(posLB ,posRT);

            // Debug.Log("minField " + minField);
            // Debug.Log("maxField " + maxField);
        }
    }

    public Vector3 GetMinEnemyField()
    {
        return minEnemyField;
    }

    public Vector3 GetMaxEnemyField()
    {
        return maxEnemyField;
    }

    public float GetFieldLength()
    {
        return fieldLength;
    }

    public int GetCurrentMatch()
    {
        return gameMatch;
    }

    public PlayerMode GetPlayerMode()
    {
        return playerMode;
    }

    public GameMode GetGameMode()
    {
        return gameMode;
    }

    public float GetPlayerSpawnRate()
    {
        return playerSpawnRate;
    }

    public float GetEnemySpawnRate()
    {
        return enemySpawnRate;
    }

}
