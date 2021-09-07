using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    private int matchPerGame = 5;
    private float timeLimit = 140f;
    private int maxEnergy = 6;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        InitGame();
    }

    void InitGame()
    {
        Debug.Log("Init Game");
    }
}
