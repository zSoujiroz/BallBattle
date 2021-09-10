using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPool : MonoBehaviour
{
    public static PlayerPool SharedInstance;
    public List<GameObject> pooledPlayer;
    public List<GameObject> pooledEnemy;
    public GameObject enemyToPool;
    public GameObject playerToPool;
    //public GameObject detectionCircle;
    private int amountToPool = 11;
    private Transform Team1Holder;
    private Transform Team2Holder;

    void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
        }
        else if (SharedInstance != this)
        {
            Destroy(gameObject);
        }
    }

    public void InitPlayer(bool isAttacker)
    {
        Team1Holder = new GameObject ("PlayerTeam").transform;
        GameObject tmp;
        //GameObject tmp2;
        pooledPlayer = new List<GameObject>(); 
        for(int i = 0; i < amountToPool ;i++)
        {
            //tmp = Instantiate(objectedToPool, Team1Holder);
            tmp = Instantiate(playerToPool, Team1Holder);
            Player_Scripts objectScript = tmp.GetComponent<Player_Scripts>();
            //objectScript.detectionCircle.SetActive(false);
            //if (!isAttacker)
            {
                //tmp2 = Instantiate(detectionCircle, tmp.transform);
                //tmp2.name = "detectionCircle";
            }
            objectScript.tag = "PlayerTeam1";
            objectScript.SetPlayerType(isAttacker);
            tmp.name = "Player1";
            //tmp.GetComponent<MeshRenderer>().material.color = Color.blue;
            tmp.SetActive(false);
            pooledPlayer.Add(tmp);
        }
    }

    public void InitEnemy(bool isAttacker)
    {
        Team2Holder = new GameObject ("EnemyTeam").transform;
        GameObject tmp;
        //GameObject tmp2;
        pooledEnemy = new List<GameObject>(); 
        for(int i = 0; i < amountToPool ;i++)
        {
            tmp = Instantiate(enemyToPool, Team2Holder);
            Player_Scripts objectScript = tmp.GetComponent<Player_Scripts>();
            //if (!isAttacker)
            {
                //tmp2 = Instantiate(detectionCircle, tmp.transform);
                //tmp2.name = "detectionCircle";
            }
            objectScript.tag = "PlayerTeam2";
            objectScript.SetPlayerType(isAttacker);
            tmp.name = "Player2";
            //tmp.GetComponent<MeshRenderer>().material.color = Color.red;
            tmp.SetActive(false);
            pooledEnemy.Add(tmp);
        }
    }

    public GameObject GetPooledPlayer()
    {
        for(int i = 0; i < amountToPool; i++)
        {
            if(!pooledPlayer[i].activeInHierarchy)
            {
                return pooledPlayer[i];
            }
        }
        return null;
    }

    public GameObject GetPooledEnemy()
    {
        for(int i = 0; i < amountToPool; i++)
        {
            if(!pooledEnemy[i].activeInHierarchy)
            {
                return pooledEnemy[i];
            }
        }
        return null;
    }
}
