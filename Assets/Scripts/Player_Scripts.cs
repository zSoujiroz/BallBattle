using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;

public class Player_Scripts : MonoBehaviour
{
    public enum TypePlayer 
	{
		DEFENDER,
		ATTACKER
	};

	public enum Player_State 
	{ 
		STAND_BY,
		HOLDING_BALL,
		PASSING,
		MOVE_AUTOMATIC,
		CHASING_BALL,
		OPPONENT_CAUGHT,
		PICK_BALL,
		GO_ORIGIN,
		INACTIVATED
	};

    private float att_EnergyRegeneration = 0.5f;
	private float def_EnergyRegeneration = 0.5f;
	private float att_SpawnRate = 0.4f;
	private float def_SpawnRate = 0.6f;
	private float att_EnergyCost = 2f;
	private float def_EnergyCost = 3f;
	private float att_SpawnTime = 0.5f;
	private float def_SpawnTime = 0.5f;
	private float att_ReactiveTime = 2.5f;
	private float def_ReactiveTime = 4f;
	private float att_NormalSpeed = 1.5f;
	private float def_NormalSpeed = 1f;
	private float att_CarryingSpeed = 0.75f;
	private float ballSpeed = 1.5f;
	private float passForce = 100f;
	private float def_ReturnSpeed = 2f;
	private float def_DetectionRange = 0.35f;

	[SerializeField]
	private GameObject detectionCircle;

	private GameObject[] allTeam1Players;
	private GameObject[] allTeam2Players;
    private GameObject ball;

	public Player_State playerState;
	public TypePlayer typePlayer;

	private Vector3 originPos;
	private Vector3 goalTarget;

	private bool isActivating = false;

	void Awake()
	{
		

		//playerState = Player_State.STAND_BY;
		updateTeamMembers();

		
	}

	void Start()
	{
		originPos = gameObject.transform.position;
		ball = GameObject.FindGameObjectWithTag("Ball");
		
		if (gameObject.tag == "PlayerTeam1")
		{
			goalTarget = GameObject.FindGameObjectWithTag("GoalTeam2").transform.position;
		}
		else
		{
			goalTarget = GameObject.FindGameObjectWithTag("GoalTeam1").transform.position;
		}

		if (ball.transform.parent == null)
		{
			Debug.Log("Ball should be taken");
		}

		if (IHaveBall())
			playerState = Player_State.HOLDING_BALL;
		else
			playerState = Player_State.MOVE_AUTOMATIC;
	}

	void Update()
	{
		switch ( playerState ) 
		{
			case Player_State.CHASING_BALL:
				if (typePlayer == TypePlayer.DEFENDER)
				{
					// Do chasing ball

					// Go back to origin position
					playerState = Player_State.GO_ORIGIN;
				}
				// Defender side
				// Chasing ball within Detection circle
				// Change to Inactive state
			break;

			case Player_State.HOLDING_BALL:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					//if (Input.GetMouseButtonDown(0))
					if (Input.GetKeyDown("space"))
					{
						Debug.Log ("This guy is holding bal");
						playerState = Player_State.PASSING;
					}					
				}
				// Attacker side
				// Move with slow speed -> goal
			break;

			case Player_State.MOVE_AUTOMATIC:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					//Debug.Log ("This guy is MOVE_AUTOMATIC");
					//playerState = Player_State.STAND_BY;
					//objectMotor.MoveObjectTo(goalTarget);
					//float step =  att_NormalSpeed * Time.deltaTime * 0.001f;
					//transform.position = Vector3.MoveTowards(transform.position, goalTarget, step);

				}
				// Attacker side
				// move with normal speed -> goal

			break;

			case Player_State.OPPONENT_CAUGHT:
				// Defender side
				// Caught the ball from attacker side within Detection circle & make them passing ball
				// -> change to Player_State.INACTIVATED
			break;

			case Player_State.PASSING:
				Debug.Log("Passing ball");
				if (typePlayer == TypePlayer.ATTACKER)
				{
					GameObject closetPlayer = null;
					if (gameObject.tag == "PlayerTeam1")
					{
						closetPlayer = GetClosetPlayer(gameObject, allTeam1Players);
					}
					else
					{
						closetPlayer = GetClosetPlayer(gameObject, allTeam2Players);
					}

					if(closetPlayer)
					{
						Debug.Log("Do pass a ball to this guy");
						Debug.Log("x = " + closetPlayer.transform.position.x + " y = " + closetPlayer.transform.position.y + "z = " + closetPlayer.transform.position.z);
						PassBallToPlayer(closetPlayer);
						
					}
					else
					{
						Debug.Log("There's no active team-mate to pass a ball");
						Debug.Log("This guy still holding ball");
						playerState = Player_State.HOLDING_BALL;
					}	
				}
				// Attacker side
				// Do passing to closest team player
			break;

			case Player_State.GO_ORIGIN:
				Debug.Log("Origin position: x = " + originPos.x + " y = " + originPos.y + " z = " + originPos.z);
			break;

			case Player_State.PICK_BALL:
				// Attacker side
				// Init game -> pick ball in attacker land
			break;

			case Player_State.STAND_BY:
				// set origin pos / Detection circle
				// chasing ball if Attacker with Ball reach the Detection circle
				// -> Player_State.OPPONENT_CAUGHT
				detectionCircle.SetActive(true);
			break;

			case Player_State.INACTIVATED:
				if (!isActivating)
				{
					isActivating = true;
					StartCoroutine(ReActivePlayer());
				}
				
				// change to greyscale / another animation

				// Waiting for Activated again

				// Attacker -> Stop moving

				// Defender -> Move back to origin pos

				// allow another soldier go through

			break;
		}
	}

	private void updateTeamMembers()
	{
		var temp = gameObject.tag;
		Debug.Log("Player Tag = " + temp);

		allTeam1Players = GameObject.FindGameObjectsWithTag("PlayerTeam1");
		allTeam2Players = GameObject.FindGameObjectsWithTag("PlayerTeam2");
	}

	private IEnumerator ReActivePlayer()
	{
		if (typePlayer == TypePlayer.ATTACKER){
			yield return new WaitForSeconds(att_ReactiveTime);
			playerState = Player_State.MOVE_AUTOMATIC;
		}
		else{
			detectionCircle.SetActive(false);
			yield return new WaitForSeconds(def_ReactiveTime);
			playerState = Player_State.STAND_BY;
		}		

		isActivating = false;
	}

	public Player_State GetPlayerState()
	{
		return this.playerState;
	}

	public void SetPlayerState(Player_State state)
	{
		this.playerState = state;
	}

	private GameObject GetClosetPlayer(GameObject fromThis, GameObject[] allOtherTeamPlayer)
	{
		GameObject targetPlayer = null;
		float closetDistanceSqr = Mathf.Infinity;
		Vector3 currentPos = fromThis.transform.position;

		foreach (GameObject go in allOtherTeamPlayer)
		{
			Player_Scripts goScripts = go.GetComponent<Player_Scripts>();
			if (go != fromThis && goScripts.GetPlayerState() != Player_State.INACTIVATED)
			{
				Vector3 directionToTarget = go.transform.position - currentPos;
				float dSqrToTarget = directionToTarget.sqrMagnitude;
				if (dSqrToTarget < closetDistanceSqr)
				{
					closetDistanceSqr = dSqrToTarget;
					targetPlayer = go;
				}
			}
		}
		return targetPlayer;
	}

	private void PassBallToPlayer(GameObject targetPlayer)
    {
		Rigidbody rgBall = ball.gameObject.GetComponent<Rigidbody>();
		if (targetPlayer)
		{
			Debug.Log("Passed ball to target player");
			Vector3 directionBall = (targetPlayer.transform.position - ball.transform.position).normalized;
			float distanceBall = (targetPlayer.transform.position - ball.transform.position).magnitude;
			rgBall.velocity = directionBall * (distanceBall * passForce) * (ballSpeed * Time.deltaTime);

			rgBall.isKinematic = false;
			ball.transform.SetParent(null);
			this.playerState = Player_State.INACTIVATED;
		}
		else
		{
			// if not found a candidate just throw the ball forward....
			Debug.Log("throw the ball forward....");
			ball.GetComponent<Rigidbody>().velocity = transform.forward * passForce;
		}
        
    }

	private void MoveToTarget(GameObject target, float speed)
	{
		
	}

	private bool IHaveBall()
	{
		return transform.childCount > 0;
	}

	void OnTriggerEnter(Collider other)
	{
		Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
			Rigidbody rbBall = ball.GetComponent<Rigidbody>();
            rbBall.velocity = Vector3.zero;
            rbBall.isKinematic = true;
            ball.transform.SetParent(transform);

			SetPlayerState(Player_State.HOLDING_BALL);
        }
	}
	
}
