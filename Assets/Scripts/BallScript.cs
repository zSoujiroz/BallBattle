using UnityEngine;

public class BallScript : MonoBehaviour
{
    GameObject ballOwner;
    private float[] fooballField;

    void Start()
    {
        fooballField = GameManager.instance.GetFootBallField();
    }
    void Update()
    {
        if (ballOwner)
        {
            transform.position = ballOwner.transform.position + (ballOwner.transform.forward / 1.5f) + (ballOwner.transform.up / 5.0f);
        }
        else
        {
            ResetBallPosition();
        }
    }

    public void ResetBallPosition()
    {
        if (transform.position.x < -fooballField[0]/2f || transform.position.x > fooballField[0]/2f || transform.position.z < -fooballField[1]/2f || transform.position.z > fooballField[1]/2f)
            transform.position = Vector3.zero;
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
