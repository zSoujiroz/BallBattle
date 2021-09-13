using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class SpawnPlayer : MonoBehaviour
{
    Ray myRay;
    RaycastHit hit;

    public LayerMask playerLayer;
    public LayerMask enemyLayer;

    private Vector3 minEnemyField;
    private Vector3 maxEnemyField;

    private float spawnPlayerTimer = 0f;
    private float spawnEnemyTimer = 0f;

    void Start()
    {
        minEnemyField = GameManager.instance.GetMinEnemyField();
        maxEnemyField = GameManager.instance.GetMaxEnemyField();
    }

    void Update()
    {
        spawnPlayerTimer += Time.deltaTime;
        spawnEnemyTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (GameManager.instance.timerIsRunning)
        {
            if (GameManager.instance.GetCurrentMatch() < 6) // normal match
            {
                if(Input.GetMouseButtonDown(0) && CanSpawnPlayer())
                {
                    myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if(Physics.Raycast(myRay, out hit, Mathf.Infinity, playerLayer))
                    {
                        SpawnPlayerObj(hit.point);
                    }
                }

                if (GameManager.instance.GetGameMode() == GameManager.GameMode.Player_Player)
                {
                #if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
                    if(Input.GetMouseButtonDown(0) && CanSpawnEnemy())
                #else
                    if(Input.GetMouseButtonDown(1) && CanSpawnEnemy())
                #endif
                    {
                        myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if(Physics.Raycast(myRay, out hit, Mathf.Infinity, enemyLayer))
                        {
                            SpawnEnemyObj(hit.point);
                        }
                    }
                }
                else
                {
                    if (GameManager.instance.playerEnergy >= GameManager.instance.playerSpawnCost && CanSpawnEnemy())
                    {
                        SpawnEnemyObj(GenerateEnemyPosition());
                    }
                }

            }
        }
    }

    Vector3 GenerateEnemyPosition()
    {
        float xPos = Random.Range(minEnemyField.x, maxEnemyField.x);
        float zPos = Random.Range(minEnemyField.z, maxEnemyField.z);
        //Debug.Log("xPos = " +  xPos + " zPos = " + zPos);
        return new Vector3(xPos, 0f, zPos);
    }

    private bool CanSpawnPlayer()
    {
        if (spawnPlayerTimer > GameManager.instance.timeSpawnPlayerDelay)
        {
            spawnPlayerTimer = 0f;
            return true;
        }
        else
            return false;
    }

    private bool CanSpawnEnemy()
    {
        if (spawnEnemyTimer > GameManager.instance.timeSpawnEnemyDelay)
        {
            spawnEnemyTimer = 0f;
            return true;
        }
        else
            return false;
    }

    private bool SpawnPlayerSuccess()
    {
        if (Random.Range(0f, 1f) < GameManager.instance.GetPlayerSpawnRate())
            return true;
        return false;
    }

    private bool SpawnEnemySuccess()
    {
        if (Random.Range(0f, 1f) < GameManager.instance.GetEnemySpawnRate())
            return true;
        return false;
    }

    public void SpawnPlayerObj(Vector3 pos)
    {
        GameObject ob = PlayerPool.SharedInstance.GetPooledPlayer();
        if (ob != null)
        {
            if (GameManager.instance.playerEnergy >= GameManager.instance.playerSpawnCost)
            {
                GameManager.instance.playerEnergy -= GameManager.instance.playerSpawnCost;
                GameManager.instance.UpdateEnergySlider();

                if (SpawnPlayerSuccess())
                {
                    ob.transform.position = new Vector3(pos.x, ob.transform.position.y, pos.z);
                    Player_Scripts obScripts = ob.GetComponent<Player_Scripts>();
                    if (GameManager.instance.playerMode == GameManager.PlayerMode.ATTACKER)
                    {
                        obScripts.SetPlayerType(true);
                    }
                    else
                    {
                        obScripts.SetPlayerType(false);
                    }
                    obScripts.SetPlayerState(Player_Scripts.Player_State.INIT);
                    ob.SetActive(true);
                    GameManager.instance.playerTeam.Add(ob);
                    //Debug.Log("playerTeam count = " + GameManager.instance.playerTeam.Count);
                }
                // else
                // {
                //     Debug.Log("spawn player fails");
                // }
                
            }
            // else
            // {
            //     Debug.Log("Cant' spawn");
            // }
        }
    }

    public void SpawnEnemyObj(Vector3 pos)
    {
        GameObject ob = PlayerPool.SharedInstance.GetPooledEnemy();
        if (ob != null)
        {
            if (GameManager.instance.enemyEnergy >= GameManager.instance.enemySpawnCost)
            {
                GameManager.instance.enemyEnergy -= GameManager.instance.enemySpawnCost;
                GameManager.instance.UpdateEnergySlider();

                if (SpawnEnemySuccess())
                {
                    ob.transform.position = new Vector3(pos.x, ob.transform.position.y, pos.z);
                    Player_Scripts obScripts = ob.GetComponent<Player_Scripts>();
                    if (GameManager.instance.playerMode == GameManager.PlayerMode.ATTACKER)
                    {
                        obScripts.SetPlayerType(false);
                    }
                    else
                    {
                        obScripts.SetPlayerType(true);
                    }
                    obScripts.SetPlayerState(Player_Scripts.Player_State.INIT);
                    ob.SetActive(true);

                    GameManager.instance.enemyTeam.Add(ob);
                    //Debug.Log("enemyTeam count = " + GameManager.instance.enemyTeam.Count);
                }
                // else
                // {
                //     Debug.Log("spawn enemy fails");
                // }
            }
            // else
            // {
            //     Debug.Log("Cant' spawn");
            // }
        }
    }
}
