using UnityEngine;

public class Ball : MonoBehaviour
{
    public GameObject owner;
	public GameObject inputPlayer;
    private GameObject[] playerTeam1;
    private GameObject[] playerTeam2;

    void Start()
    {
        playerTeam1 = GameObject.FindGameObjectsWithTag("PlayerTeam1");
        playerTeam2 = GameObject.FindGameObjectsWithTag("PlayerTeam2");
    }

    public Vector3 GetBallPosition()
    {
        return gameObject.transform.position;
    }
}
