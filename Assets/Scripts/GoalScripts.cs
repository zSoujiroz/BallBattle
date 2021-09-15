using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScripts : MonoBehaviour
{
    [SerializeField] ParticleSystem successParticles;
    void OnTriggerEnter(Collider other)
    {
        var otherTag = other.gameObject.tag;
        if (otherTag == "Ball")
        {
            GameObject ballOwner = GameManager.instance.ballScript.GetBallOwner();
            if (ballOwner != null)
            {
                GameManager.instance.timerIsRunning = false; //Stop timer!
                successParticles.Play();
                Player_Scripts ballOwnerScripts = ballOwner.GetComponent<Player_Scripts>();

                ballOwnerScripts.SetPlayerState(Player_Scripts.Player_State.END_MATCH);
                StartCoroutine(PlayerCatchGoal());
            }
            else
            {
                other.gameObject.transform.position = Vector3.zero;
            }
        }

        if ((GameManager.instance.playerMode != GameManager.PlayerMode.PENALTY) && ((gameObject.tag == "GoalTeam1" && otherTag == "PlayerTeam2") || (gameObject.tag == "GoalTeam2" && otherTag == "PlayerTeam1")))
        {
            Player_Scripts otherScripts = other.GetComponent<Player_Scripts>();
            otherScripts.StopMoving();

            if (otherScripts.GetPlayerState() == Player_Scripts.Player_State.HOLDING_BALL)
            {
                GameManager.instance.timerIsRunning = false; //Stop timer!
                successParticles.Play();
                otherScripts.SetPlayerState(Player_Scripts.Player_State.END_MATCH);
                StartCoroutine(PlayerCatchGoal());
            }
            else
            {
                StartCoroutine(PlayDeathAnimation(otherScripts, other.gameObject.transform.position));
                GameManager.instance.enemyTeam.Remove(other.gameObject);
                other.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator PlayDeathAnimation(Player_Scripts scripts, Vector3 pos)
	{
        SoundManager.PlaySound(SoundManager.Sound.PlayerDie, pos);
        scripts.PlayAnimation(Player_Scripts.Player_State.DEATH);
        yield return new WaitForSeconds(0.5f);
	}

    private IEnumerator PlayerCatchGoal()
	{
        yield return new WaitForSeconds(3f);
        //GameManager.instance.EndMatch(true);
        GameManager.instance.EndMatch(GameManager.MatchResult.WIN);
	}
}
