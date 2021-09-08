using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPool : MonoBehaviour
{
    public static PlayerPool SharedInstance;
    public List<GameObject> pooledPlayerTeam1;
    public List<GameObject> pooledPlayerTeam2;
    public GameObject objectedToPool;
    private int amountToPool = 11;
    private Transform Team1Holder;
    private Transform Team2Holder;

    void Awake()
    {
        SharedInstance = this;
    }

    void Start()
    {
        Team1Holder = new GameObject ("PlayerTeam").transform;
        Team2Holder = new GameObject ("EnemyTeam").transform;
        GameObject tmp;

        // init attacker team
        pooledPlayerTeam1 = new List<GameObject>(); 
        for(int i =0;i<amountToPool;i++)
        {
            tmp = Instantiate(objectedToPool, Team1Holder);
            Player_Scripts objectScript = tmp.GetComponent<Player_Scripts>();
            objectScript.tag = "PlayerTeam1";
            tmp.name = "Player1";
            tmp.SetActive(false);
            pooledPlayerTeam1.Add(tmp);
        }

        // init defender team
        pooledPlayerTeam2 = new List<GameObject>(); 
        for(int i =0;i<amountToPool;i++)
        {
            tmp = Instantiate(objectedToPool, Team2Holder);
            tmp.name = "Player2";
            Player_Scripts objectScript = tmp.GetComponent<Player_Scripts>();
            objectScript.tag = "PlayerTeam2";
            tmp.SetActive(false);
            pooledPlayerTeam2.Add(tmp);
        }
    }

    public GameObject GetPooledPlayerTeam1()
    {
        for(int i = 0; i < amountToPool; i++)
        {
            if(!pooledPlayerTeam1[i].activeInHierarchy)
            {
                return pooledPlayerTeam1[i];
            }
        }
        return null;
    }

    public GameObject GetPooledPlayerTeam2()
    {
        for(int i = 0; i < amountToPool; i++)
        {
            if(!pooledPlayerTeam2[i].activeInHierarchy)
            {
                return pooledPlayerTeam2[i];
            }
        }
        return null;
    }
}
