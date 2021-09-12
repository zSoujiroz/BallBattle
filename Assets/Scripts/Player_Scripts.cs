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
		// CATCH_GOAL,
		DEATH,
		PENALTY,
		END_MATCH
	};

	public enum AnimationType
	{
		Idle = 0,
		SlowRun,
		FastRun,
		Dance,
		Die,
		Pass,
		PickBall
	}


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

	//[SerializeField]
	//public GameObject detectionCircle;

    //private GameObject ball;
	//private BallScript ballScript;

	private float rotationSpeed = 1f;

	private Animator animator;

	public LayerMask ballLayerMask;
	public Player_State playerState;
	public TypePlayer typePlayer;

	private Vector3 originPos;
	private Vector3 goalTarget;

	private float detectionLength;

	private bool isActivating = false;

	private float movementX;
    private float movementY;

	public Texture barRedTexture;
	public Texture barBlueTexture;
	private Texture barTexture;
	private float barHpTimer;
	private float barHpTimerCD;
	private float barHp;

	private  CharacterController characterController;

	[SerializeField] ParticleSystem successParticles;
	[SerializeField] ParticleSystem moveParticles;


	void Start()
	{
		animator = GetComponent<Animator>();
		originPos = gameObject.transform.position;
		characterController = GetComponent<CharacterController>();
		detectionLength = GameManager.instance.GetFieldLength() * def_DetectionRange;	
		UpdateBarHp();

		
	}

	void Update()
	{
		//if (Input.GetKeyDown("space"))
			//SoundManager.PlaySound();
			//SoundManager.PlaySound(SoundManager.Sound.PlayerMove);


		switch ( playerState ) 
		{
			case Player_State.INIT:
				successParticles.Play();
				SoundManager.PlaySound(SoundManager.Sound.PlayerInit , transform.position);
				animator.SetInteger("PlayerAnimationState", 5); // Dancing
				RotationToTarget(GameManager.instance.ball.transform.position);
				if (GameManager.instance.playerMode == GameManager.PlayerMode.ATTACKER)
				{
					goalTarget = GameObject.FindGameObjectWithTag("GoalTeam2").transform.position;
				}
				else
				{
					goalTarget = GameObject.FindGameObjectWithTag("GoalTeam1").transform.position;
				}
				barHpTimerCD = barHpTimer;	// reset countdown
				playerState = Player_State.INACTIVATED;
			break;

			case Player_State.CHASING_BALL:
				if (typePlayer == TypePlayer.DEFENDER)
				{
					GameObject ballOwner = GameManager.instance.ballScript.GetBallOwner();
					if (ballOwner != null)
					{
						animator.SetInteger("PlayerAnimationState", 2); // FastRun = 2
						moveParticles.Play();
						MoveToTarget(ballOwner.transform.position, def_NormalSpeed * Time.deltaTime);	
					}
				}
			break;

			case Player_State.HOLDING_BALL:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					if (GameManager.instance.IsRushTime())
					{
						animator.SetInteger("PlayerAnimationState", 2); // FastRun = 2
						MoveToTarget(goalTarget, att_NormalSpeed * Time.deltaTime);
					}
					else
					{
						animator.SetInteger("PlayerAnimationState", 1); // SlowRun = 2
						MoveToTarget(goalTarget, att_CarryingSpeed * Time.deltaTime);
					}
				}
			break;

			case Player_State.MOVE_AUTOMATIC:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					moveParticles.Play();
					animator.SetInteger("PlayerAnimationState", 2); // FastRun = 2
					float step =  att_NormalSpeed * Time.deltaTime;
					MoveToTarget(goalTarget, step);
				}
			break;

			case Player_State.PASSING:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					//animator.SetInteger("PlayerAnimationState", 3); // pass ball = 2
					GameObject closetPlayer = null;
					if (GameManager.instance.playerMode == GameManager.PlayerMode.ATTACKER)
					{
						closetPlayer = GetClosetPlayer(gameObject, GameManager.instance.playerTeam);
					}
					else
					{
						closetPlayer = GetClosetPlayer(gameObject, GameManager.instance.enemyTeam);
					}

					PassBallToPlayer(closetPlayer);	
				}
			break;

			case Player_State.GO_ORIGIN:
				moveParticles.Play();
				animator.SetInteger("PlayerAnimationState", 2); // FastRun = 2
				MoveToTarget(originPos, def_ReturnSpeed * Time.deltaTime);

				if (gameObject.transform.position == originPos)
				{	
					barHpTimerCD = barHpTimer;	// reset countdown
					playerState = Player_State.INACTIVATED;
				}
			break;

			case Player_State.PICK_BALL:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					moveParticles.Play();
					animator.SetInteger("PlayerAnimationState", 2); // FastRun = 2
					if (GameManager.instance.ballScript.GetBallOwner() != null)
					{
						playerState = Player_State.MOVE_AUTOMATIC;
					}
					else
					{
						Vector3 ballPosition = GameManager.instance.ball.transform.position;
						MoveToTarget(ballPosition, att_NormalSpeed * Time.deltaTime);
					}					
				}
			break;

			case Player_State.STAND_BY:
				animator.SetInteger("PlayerAnimationState", 0); // Idle
				if (typePlayer == TypePlayer.DEFENDER)
				{
					if (CheckIfAttackerComeInside())
					{
						playerState = Player_State.CHASING_BALL;
					}
				}
			break;

			case Player_State.INACTIVATED:
				animator.SetInteger("PlayerAnimationState", 0); // Idle
				if (typePlayer == TypePlayer.DEFENDER)
				{
					if (GameManager.instance.IsRushTime())
					{
						playerState = Player_State.ACTIVATED;
					}
					else
					{
						if (!isActivating)
						{
							isActivating = true;
							StartCoroutine(ReActivePlayer());
						}
					}
				}	
				else
				{
					if (!isActivating)
					{
						isActivating = true;
						StartCoroutine(ReActivePlayer());
					}
				}	

			break;

			case Player_State.ACTIVATED:
				if (typePlayer == TypePlayer.ATTACKER)
				{
					GameObject ballOwner = GameManager.instance.ballScript.GetBallOwner();
					//if (ball.transform.parent != null)
					if (ballOwner != null)
					{
						//if (gameObject == ball.transform.parent)
						if (gameObject == ballOwner)
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

			// case Player_State.CATCH_GOAL:
			// 	animator.SetInteger("PlayerAnimationState", 5); // Dancing
			// 	//Reset Ball
			// 	Rigidbody rgBall = GameManager.instance.ball.gameObject.GetComponent<Rigidbody>();
			// 	rgBall.isKinematic = false;
			// 	GameManager.instance.ball.transform.SetParent(null);
			// 	GameManager.instance.ball.transform.position = Vector3.zero;

			// 	playerState	= Player_State.END_MATCH;
			// 	Debug.Log("Catgoal");
			// 	//GameManager.instance.EndMatch(true);
			// 	GameManager.instance.EndMatch(true);
			// 	Debug.Log("Catgoal2");

			// 	StartCoroutine(EndMatch());
			// 	// End match -> You win Match -> start new match.
			// break;

			case Player_State.PENALTY:
				HandleMovement();
			break;

			case Player_State.END_MATCH:
				animator.SetInteger("PlayerAnimationState", 5); // Dancing
				//Don't do anything
			break;
		}
	}

	private void UpdateBarHp()
	{
		if (typePlayer == TypePlayer.ATTACKER)
		{
			barHpTimer = att_ReactiveTime;
		}
		else
		{
			barHpTimer = def_ReactiveTime;
		}

		if (gameObject.tag == "PlayerTeam1")
		{
			barTexture = barBlueTexture;
		}
		else
		{
			barTexture = barRedTexture;
		}

		barHpTimerCD = barHpTimer;
		barHp = 100f;

	}
	private void HandleMovement()
	{
		float moveSpeed = 1f;

		Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		characterController.Move(move * Time.deltaTime * moveSpeed);
		transform.Rotate (new Vector3(0,Input.GetAxis("Horizontal") * 360f * Time.deltaTime,0));

		if (move != Vector3.zero)
		{
			moveParticles.Play();
			SoundManager.PlaySound(SoundManager.Sound.PlayerMove , transform.position);
			animator.SetInteger("PlayerAnimationState", 1);
			gameObject.transform.forward = move;
		}   
		else
		{
			animator.SetInteger("PlayerAnimationState", 0);
		}

	}

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
		SoundManager.PlaySound(SoundManager.Sound.PlayerPass);
		StopMoving();
		this.playerState = Player_State.INACTIVATED;
		barHpTimerCD = barHpTimer;	// reset countdown

		Rigidbody rgBall = GameManager.instance.ball.gameObject.GetComponent<Rigidbody>();
		rgBall.isKinematic = false;
		GameManager.instance.ballScript.SetBallOwner(null);

		if (targetPlayer)
		{
			Vector3 ballPos = GameManager.instance.ball.transform.position;
			Vector3 directionBall = (targetPlayer.transform.position - ballPos).normalized;
			float distanceBall = (targetPlayer.transform.position - ballPos).magnitude;
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
		Rigidbody rbBall = GameManager.instance.ball.GetComponent<Rigidbody>();

		rbBall.velocity = transform.forward * ballSpeed;
		yield return new WaitForSeconds(2f);
		rbBall.velocity = Vector3.zero;
	}

	private IEnumerator EndMatch()
	{
		yield return new WaitForSeconds(2f);
		GameManager.instance.EndMatch(true);
		Debug.Log("Endmaamtasdf");
	}

	private void MoveToTarget(Vector3 target, float speed)
	{
		RotationToTarget(target);
		transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, transform.position.y, target.z), speed);	
	}

	private void RotationToTarget(Vector3 target)
	{
		Vector3 direction = Vector3.Normalize(target - transform.position);
		if (direction != Vector3.zero)
		{
			Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 360f * Time.deltaTime);
		}
	}

	public void StopMoving()
	{
		transform.position += Vector3.zero;
	}

	void OnTriggerEnter(Collider other)
	{
		if (typePlayer == TypePlayer.ATTACKER)
		{
			if (playerState == Player_State.PENALTY)
			{
				var otherTag = other.gameObject.tag;
				if (otherTag == "Ball")
				{
					Rigidbody rbBall = GameManager.instance.ball.GetComponent<Rigidbody>();
					rbBall.velocity = Vector3.zero;
					rbBall.isKinematic = true;
					GameManager.instance.ballScript.SetBallOwner(gameObject);
				}
			}
			else if (playerState != Player_State.INACTIVATED)
			{
				var otherTag = other.gameObject.tag;
				if (otherTag == "Ball")
				{
					//if (other.gameObject.transform.parent == null)
					if (GameManager.instance.ballScript.GetBallOwner() == null)
					{
						//Ball ball = other.GetComponent<Ball>();
						Rigidbody rbBall = GameManager.instance.ball.GetComponent<Rigidbody>();
						rbBall.velocity = Vector3.zero;
						rbBall.isKinematic = true;
						//ball.transform.SetParent(transform);
						GameManager.instance.ballScript.SetBallOwner(gameObject);
						animator.SetInteger("PlayerAnimationState", 4); // Kick/take ball = 4
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
							animator.SetInteger("PlayerAnimationState", 4); // Kick ball = 4
							otherScripts.animator.SetInteger("PlayerAnimationState", 3); // Pass the ball = 3
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
		Collider[] rangeChecks = Physics.OverlapSphere(transform.position, detectionLength, ballLayerMask);
		if (rangeChecks.Length != 0)
		{
			if (rangeChecks[0].transform.gameObject.tag == "Ball")
				return rangeChecks[0].transform;
		} 
		//Debug.Log("check length = " + detectionLength);
		return null;
	}	

	public void PlayAnimation(Player_State state)
	{
		switch (state)
		{
			case Player_State.DEATH: 
				animator.SetInteger("PlayerAnimationState", 6);
			break;
		}
	}

	void OnGUI() 
	{
		if (playerState == Player_State.INACTIVATED)
		{
			Vector3 posBar = Camera.main.WorldToScreenPoint( transform.position + new Vector3(0,1.0f,0) );
			GUI.DrawTexture( new Rect(posBar.x-30, (Screen.height-posBar.y) , (int)barHp , 10 ), barTexture );
			
			barHpTimerCD -= Time.deltaTime;
			barHp = barHpTimerCD/barHpTimer * 100;
			if (barHp < 0)
				barHp = 0;
		}
	}
}
