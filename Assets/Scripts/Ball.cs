using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 GetBallPosition()
    {
        return gameObject.transform.position;
    }
}
