using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScripts : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        var otherTag = other.gameObject.tag;
        if (otherTag == "Ball")
        {
            GameObject ballOwner = GameManager.instance.ballScript.GetBallOwner();
            //GameObject ballOwner = ballScript.GetBallOwner();
            //if(other.gameObject.transform.parent != null)
            if (ballOwner != null)
            {
                //GameObject ballOwner = other.gameObject.transform.parent.gameObject;
                Player_Scripts ballOwnerScripts = ballOwner.GetComponent<Player_Scripts>();
                ballOwnerScripts.StopMoving();
                ballOwnerScripts.SetPlayerState(Player_Scripts.Player_State.CATCH_GOAL);
            }
            else
            {
                other.gameObject.transform.position += Vector3.zero;
            }
        }

        if (gameObject.tag == "GoalTeam1")
        {
            if (otherTag == "PlayerTeam2")
            {
                Player_Scripts otherScripts = other.GetComponent<Player_Scripts>();
                otherScripts.StopMoving();

                if (otherScripts.GetPlayerState() == Player_Scripts.Player_State.HOLDING_BALL)
                    otherScripts.SetPlayerState(Player_Scripts.Player_State.CATCH_GOAL);
                else
                {
                    GameManager.instance.enemyTeam.Remove(other.gameObject);
                    other.gameObject.SetActive(false);
                }
            }
        }

        if (gameObject.tag == "GoalTeam2")
        {
            if (otherTag == "PlayerTeam1")
            {
                Player_Scripts otherScripts = other.GetComponent<Player_Scripts>();
                otherScripts.StopMoving();

                if (otherScripts.GetPlayerState() == Player_Scripts.Player_State.HOLDING_BALL)
                    otherScripts.SetPlayerState(Player_Scripts.Player_State.CATCH_GOAL);
                else
                {
                    GameManager.instance.playerTeam.Remove(other.gameObject);
                    other.gameObject.SetActive(false);
                }
            }

        }
    }
}
