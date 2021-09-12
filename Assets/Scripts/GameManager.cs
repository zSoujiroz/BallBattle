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
    private float[] foobalField;

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

    [HideInInspector]
    public List<GameObject> playerTeam;
    [HideInInspector]
    public List<GameObject> enemyTeam;

    public GameObject ui_LoadMatch;
    public GameObject ui_EndMatch;
    public GameObject ui_GameModeInfo;
    public TextMeshProUGUI ui_PlayName;
    public TextMeshProUGUI ui_EnemyName;

    //public TextMeshProUGUI ui_PlayScoreText;
    //public TextMeshProUGUI ui_EnemyScoreText;
    public TextMeshProUGUI ui_GameOverText;

    public Button ui_HomeButton;
    public Button ui_StartMatchButton;

    public GameObject ui_EndGame;
    public TextMeshProUGUI ui_PlayScoreFinalText;
    public TextMeshProUGUI ui_EnemyScoreFinalText;


    
    private MazeController mazeController;
    


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

        InitBallBattleField();
        SoundManager.Initialize();
    }

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");  
        ballScript = ball.GetComponent<BallScript>();
        mazeController = GetComponent<MazeController>();

        PlayerPool.SharedInstance.InitPlayer(playerMode == PlayerMode.ATTACKER);
        PlayerPool.SharedInstance.InitEnemy(enemyMode == PlayerMode.ATTACKER);

        SoundManager.PlaySoundBG(SoundManager.Sound.Background);
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

    private void InitBallBattleField()
    {
        FootballField();
        EnemyField();
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

    public void GeneratePenaltyMap()
    {
        mazeController.CreateMazeMap();

        //Set bal as random position
        SetRandomBallPosition();

        //Set Player in front of the gate
        SpawnPenaltyPlayer();
    }

    private void SpawnPenaltyPlayer()
    {
        GameObject ob = PlayerPool.SharedInstance.GetPooledPlayer();
        if (ob != null)
        {
            Player_Scripts obScripts = ob.GetComponent<Player_Scripts>();
            obScripts.SetPlayerType(true);
            obScripts.SetPlayerState(Player_Scripts.Player_State.PENALTY);
            ob.SetActive(true);
            playerTeam.Add(ob);
            ob.transform.position = new Vector3(0.5f , ob.transform.position.y, -(fieldLength - 1f)/2f);
        }
    }

    private void SetRandomBallPosition()
    {
        ball.transform.position = new Vector3((int)(Random.Range(0, foobalField[0])/2f) + 0.5f, 0.2f, (int)(Random.Range(0f, foobalField[1])/2f) + 0.5f);
    }

    void InitGame()
    {
        gameMatch = 1;
        playerScore = 0;
        enemyScore = 0;

        InitMatch();
    }

    void InitMatch()
    {
        DisplayLoadMatch();
        isRushTime = false;   

        Rigidbody rgBall = ball.gameObject.GetComponent<Rigidbody>();
        rgBall.isKinematic = false;
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
        }
        else if (gameMatch == 6)
        {
            playerMode = PlayerMode.PENALTY;
            enemyMode = PlayerMode.NONE;
            GeneratePenaltyMap();
        }

        MatchSetup();
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

        Time.timeScale = 1f;
        isSetup = false;
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
            if (playerMode == PlayerMode.PENALTY)
            {
                mazeController.ClearMazeMap();
                playerScore += 1;
            }
            else if (playerMode == PlayerMode.ATTACKER)
                playerScore += 1;
            else
                enemyScore +=1;
        }

        bool isGameOver = CheckEndGame();
        DisplayEndGame(isGameOver);
        
        // if (CheckEndGame() == false)
        // {
        //     DisplayEndMatch();
        //     StartNewMatch();
        // }
        // else
        // {
        //     EndGame();
        // }
    }

    // public void EndGame()
    // {
    //     DisplayEndGame();
    // }

    public bool CheckEndGame()
    {
        ResetPlayer();
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

    void DisplayEndGame(bool isGameOver)
    {
        if (isGameOver)
        {
            Debug.Log("Endgame");
            ui_HomeButton.gameObject.SetActive(true);
            ui_StartMatchButton.gameObject.SetActive(false);
            ui_GameOverText.text = "Game Over!";
        }
        else
        {
            ui_HomeButton.gameObject.SetActive(false);
            ui_StartMatchButton.gameObject.SetActive(true);
            if (gameMatch == 5)
            {
                ui_GameOverText.text = "Finish Match : " + gameMatch + " Draw Result";
            }
            else
            {
                ui_GameOverText.text = "Finish Match : " + gameMatch;
            }
            Debug.Log("Start New match");
        }
        ui_PlayScoreFinalText.text = playerScore + "";
        ui_EnemyScoreFinalText.text = enemyScore + "";
        ui_EndGame.SetActive(true);
    }

    // void DisplayEndMatch()
    // {
    //     Debug.Log("endmat");
    //     ui_PlayScoreText.text = playerScore + "";
    //     ui_EnemyScoreText.text = enemyScore + "";

    //     ui_LoadMatch.SetActive(true);
    //     Invoke("HideEndMatch", matchEndDelay);
    // }

    // void HideEndMatch()
    // {
    //     ui_LoadMatch.SetActive(false);
    // }

    void DisplayLoadMatch()
    {
        if (ui_LoadMatch)
        {
            TextMeshProUGUI matchText = GetChildWithName(ui_LoadMatch, "LoadMatchText").GetComponent<TextMeshProUGUI>(); 
            if (gameMatch == 6)
            {
                matchText.text = "Penalty Match";
            }
            else
            {
                matchText.text = "Match : " + gameMatch;
            }

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

            foobalField = new float[]{boundsX, boundsZ};
        }
    }

    public float[] GetFootBallField()
    {
        Debug.Log("with = " + foobalField[0]);
        Debug.Log("length = " + foobalField[1]);
        return foobalField;
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

            Debug.Log("minField " + minEnemyField);
            Debug.Log("maxField " + maxEnemyField);
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
