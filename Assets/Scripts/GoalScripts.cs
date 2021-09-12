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
            if (ballOwner != null)
            {
                Player_Scripts ballOwnerScripts = ballOwner.GetComponent<Player_Scripts>();

                ballOwnerScripts.SetPlayerState(Player_Scripts.Player_State.END_MATCH);
                GameManager.instance.EndMatch(true);
            }
            else
            {
                other.gameObject.transform.position = Vector3.zero;
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
                    StartCoroutine(PlayDeathAnimation(otherScripts));
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

                if (otherScripts.GetPlayerState() == Player_Scripts.Player_State.HOLDING_BALL)
                {
                    otherScripts.StopMoving();
                    otherScripts.SetPlayerState(Player_Scripts.Player_State.CATCH_GOAL);
                }
                else if (otherScripts.GetPlayerState() != Player_Scripts.Player_State.PENALTY)
                {
                    StartCoroutine(PlayDeathAnimation(otherScripts));
                    GameManager.instance.playerTeam.Remove(other.gameObject);
                    other.gameObject.SetActive(false);
                }
            }

        }
    }

    private IEnumerator PlayDeathAnimation(Player_Scripts scripts)
	{
        SoundManager.PlaySound(SoundManager.Sound.PlayerDie);
        scripts.PlayAnimation(Player_Scripts.Player_State.DEATH);
        yield return new WaitForSeconds(0.5f);
	}


}
