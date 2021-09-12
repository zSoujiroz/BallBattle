using UnityEngine;

public class BallScript : MonoBehaviour
{
    GameObject ballOwner;
    private float[] fooballField;
    GameObject highLight;

    void Start()
    {
        fooballField = GameManager.instance.GetFootBallField();
        highLight = GameObject.FindGameObjectWithTag("PlayerHighLight");

    }
    void Update()
    {
        if (ballOwner)
        {
            Vector3 newPos = ballOwner.transform.position + (ballOwner.transform.forward / 1.5f) + (ballOwner.transform.up / 5.0f);
            transform.position = new Vector3(newPos.x, 0.2f, newPos.z);
            highLight.transform.position = new Vector3(ballOwner.transform.position.x, 0.02f, ballOwner.transform.position.z);
            highLight.SetActive(true);
        }
        else
        {
            highLight.SetActive(false);
            ResetBallPosition();
        }
    }

    public void ResetBallPosition()
    {
        if (transform.position.x < -fooballField[0]/2f || transform.position.x > fooballField[0]/2f || transform.position.z < -fooballField[1]/2f || transform.position.z > fooballField[1]/2f)
            transform.position = Vector3.zero;
        transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
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
