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
		INIT,
		STAND_BY,
		HOLDING_BALL,
		PASSING,
		MOVE_AUTOMATIC,
		CHASING_BALL,
		OPPONENT_CAUGHT,
		PICK_BALL,
		GO_ORIGIN,
		INACTIVATED,
		ACTIVATED,
		CATCH_GOAL
	};


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
	public GameObject detectionCircle;

	//private GameObject[] allTeam1Players;
	//private GameObject[] allTeam2Players;
    private GameObject ball;
	//private Ball ball;

	public LayerMask ballLayerMask;
	public Player_State playerState;
	public TypePlayer typePlayer;

	private Vector3 originPos;
	private Vector3 goalTarget;

	private float detectionLength;

	private bool isActivating = false;

	void Start()
	{
		originPos = gameObject.transform.position;
		ball = GameObject.FindGameObjectWithTag("Ball");
		detectionCircle = GameManager.instance.GetChildWithName(gameObject, "detectionCircle");
		detectionLength = GameManager.instance.GetFieldLength() * def_DetectionRange;

		
		//if (gameObject.tag == "PlayerTeam1")
		if (GameManager.instance.playerMode == GameManager.PlayerMode.ATTACKER)
		{
			goalTarget = GameObject.FindGameObjectWithTag("GoalTeam2").transform.position;
		}
		else
		{
			goalTarget = GameObject.FindGameObjectWithTag("GoalTeam1").transform.position;
		}

		playerState = Player_State.INIT;		
	}

	void Update()
	{
		switch ( playerState ) 
		{
			case Player_State.INIT:
				playerState = Player_State.INACTIVATED;
			break;

			case Player_State.CHASING_BALL:
				if (typePlayer == TypePlayer.DEFENDER)
				{
					if (ball.transform.parent != null)
					{
						GameObject ballOwner = ball.transform.parent.gameObject;
						MoveToTarget(ballOwner.transform.position, def_NormalSpeed * Time.deltaTime);	
					}
				}
			break;

			case Player_State.HOLDING_BALL:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					//if (Input.GetKeyDown("space"))
					//	playerState = Player_State.PASSING;			
					MoveToTarget(goalTarget, att_CarryingSpeed * Time.deltaTime);
				}
			break;

			case Player_State.MOVE_AUTOMATIC:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					float step =  att_NormalSpeed * Time.deltaTime;
					MoveToTarget(goalTarget, step);
				}
			break;

			case Player_State.PASSING:
				//Debug.Log("Passing ball");
				if (typePlayer == TypePlayer.ATTACKER)
				{
					GameObject closetPlayer = null;
					if (GameManager.instance.playerMode == GameManager.PlayerMode.ATTACKER)
					{
						//Debug.Log("Player is attacker");
						closetPlayer = GetClosetPlayer(gameObject, GameManager.instance.playerTeam);
					}
					else
					{
						//Debug.Log("Player is defender");
						closetPlayer = GetClosetPlayer(gameObject, GameManager.instance.enemyTeam);
					}

					PassBallToPlayer(closetPlayer);	
				}
			break;

			case Player_State.GO_ORIGIN:

				Debug.Log("Origin position: x = " + originPos.x + " y = " + originPos.y + " z = " + originPos.z);
				MoveToTarget(originPos, def_NormalSpeed * Time.deltaTime);
				if (gameObject.transform.position == originPos)
					playerState = Player_State.INACTIVATED;
			break;

			case Player_State.PICK_BALL:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					if (ball.transform.parent != null)
					{
						playerState = Player_State.MOVE_AUTOMATIC;
					}
					else
					{
						Vector3 ballPosition = ball.transform.position;
						MoveToTarget(ballPosition, att_NormalSpeed * Time.deltaTime);
					}					
				}
			break;

			case Player_State.STAND_BY:
				if (typePlayer == TypePlayer.DEFENDER)
				{
					detectionCircle.SetActive(true);
					if (CheckIfAttackerComeInside())
					{
						playerState = Player_State.CHASING_BALL;
						detectionCircle.SetActive(false);
					}
				}
			break;

			case Player_State.INACTIVATED:
				if (typePlayer == TypePlayer.DEFENDER)
				{
					detectionCircle.SetActive(false);
				}	

				if (!isActivating)
				{
					isActivating = true;
					StartCoroutine(ReActivePlayer());
				}

			break;

			case Player_State.ACTIVATED:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					if (ball.transform.parent != null)
					{
						if (gameObject == ball.transform.parent)
						{
							playerState = Player_State.HOLDING_BALL;
						}
						else
						{
							playerState = Player_State.MOVE_AUTOMATIC;
						}
					}
					else
					{
						playerState = Player_State.PICK_BALL;
					}
				}
				else
				{
					playerState = Player_State.STAND_BY;
				}
			break;

			case Player_State.CATCH_GOAL:
				Debug.Log("Catch goal");
				bool isWin = true; // win match
				GameManager.instance.EndMatch(isWin);
				// End match -> You win Match -> start new match.
			break;
		}
	}

	// private void updateTeamMembers()
	// {
	// 	var temp = gameObject.tag;

	// 	allTeam1Players = GameObject.FindGameObjectsWithTag("PlayerTeam1");
	// 	allTeam2Players = GameObject.FindGameObjectsWithTag("PlayerTeam2");
	// }

	private IEnumerator ReActivePlayer()
	{
		if (typePlayer == TypePlayer.ATTACKER){
			yield return new WaitForSeconds(att_ReactiveTime);
		}
		else{
			yield return new WaitForSeconds(def_ReactiveTime);
		}		
		playerState = Player_State.ACTIVATED;
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

	public void SetPlayerType(bool isAttacker)
	{
		if (isAttacker)
			this.typePlayer = TypePlayer.ATTACKER;
		else
			this.typePlayer = TypePlayer.DEFENDER;
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

	private GameObject GetClosetPlayer(GameObject fromThis, List<GameObject> allOtherTeamPlayer)
	{
		GameObject targetPlayer = null;
		float closetDistanceSqr = Mathf.Infinity;
		Vector3 currentPos = fromThis.transform.position;

		Debug.Log("allOtherTeamPlayer " + allOtherTeamPlayer.Count);

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
		StopMoving();
		this.playerState = Player_State.INACTIVATED;

		Rigidbody rgBall = ball.gameObject.GetComponent<Rigidbody>();
		rgBall.isKinematic = false;
		ball.transform.SetParent(null);

		if (targetPlayer)
		{
			Vector3 directionBall = (targetPlayer.transform.position - ball.transform.position).normalized;
			float distanceBall = (targetPlayer.transform.position - ball.transform.position).magnitude;
			rgBall.velocity = directionBall * (distanceBall * passForce) * (ballSpeed * Time.deltaTime);

			Player_Scripts sc = targetPlayer.GetComponent<Player_Scripts>();
			sc.SetPlayerState(Player_State.PICK_BALL);
		}
		else
		{
			StartCoroutine(ThrowTheBalForward());
		}
    }

	private IEnumerator ThrowTheBalForward()
	{
		Rigidbody rbBall = ball.GetComponent<Rigidbody>();

		rbBall.velocity = transform.forward * ballSpeed;
		yield return new WaitForSeconds(2f);
		rbBall.velocity = Vector3.zero;
	}

	private void MoveToTarget(Vector3 target, float speed)
	{
		transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, transform.position.y, target.z), speed);
	}

	public void StopMoving()
	{
		transform.position += Vector3.zero;
	}

	void OnTriggerEnter(Collider other)
	{
		if (typePlayer == TypePlayer.ATTACKER)
		{
			if (playerState != Player_State.INACTIVATED)
			{
				var otherTag = other.gameObject.tag;
				if (otherTag == "Ball")
				{
					if (other.gameObject.transform.parent == null)
					{
						Ball ball = other.GetComponent<Ball>();
						Rigidbody rbBall = ball.GetComponent<Rigidbody>();
						rbBall.velocity = Vector3.zero;
						rbBall.isKinematic = true;
						ball.transform.SetParent(transform);
						SetPlayerState(Player_State.HOLDING_BALL);
					}
				}

				if ( otherTag != gameObject.tag && (otherTag == "PlayerTeam1" || otherTag == "PlayerTeam2"))
				{
					if (playerState == Player_State.HOLDING_BALL)
					{
						Player_Scripts otherScripts = other.GetComponent<Player_Scripts>();
						if (otherScripts.playerState == Player_State.CHASING_BALL)
						{
							StopMoving();
							this.playerState = Player_State.PASSING;
							otherScripts.SetPlayerState(Player_State.GO_ORIGIN);
						}
					}
				}
			}
		}	
	}

	private Transform CheckIfAttackerComeInside()
	{
		GameObject targetEnemy = null;

		Collider[] rangeChecks = Physics.OverlapSphere(transform.position, detectionLength, ballLayerMask);
		if (rangeChecks.Length != 0)
		{
			if (rangeChecks[0].transform.gameObject.tag == "Ball")
				return rangeChecks[0].transform;
		} 

		//Debug.Log("check length = " + detectionLength);

		return null;
	}
	
}
