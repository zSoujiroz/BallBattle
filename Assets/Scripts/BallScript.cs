using UnityEngine;

public class BallScript : MonoBehaviour
{
    GameObject ballOwner;
    void Update()
    {
        if (ballOwner)
        {
            transform.position = ballOwner.transform.position + (ballOwner.transform.forward / 1.5f) + (ballOwner.transform.up / 5.0f);
        }
    }

    public void SetBallOwner(GameObject owner)
    {
        this.ballOwner = owner;
    }

    public GameObject GetBallOwner()
    {
        return this.ballOwner;
    }

    public Vector3 GetBallPosition()
    {
        return gameObject.transform.position;
    }
}
