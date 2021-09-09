using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    Ray myRay;
    RaycastHit hit;

    public LayerMask playerLayer;
    public LayerMask enemyLayer;

    void FixedUpdate()
    {
        if(Input.GetMouseButtonDown(0))
        {
            myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(myRay, out hit, Mathf.Infinity, playerLayer))
            {
                Debug.DrawLine (myRay.origin, hit.point);
                Debug.Log("Left hit " + hit.point);
                SpawnPlayerObj(hit.point);
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(myRay, out hit, Mathf.Infinity, enemyLayer))
            {
                Debug.DrawLine (myRay.origin, hit.point);
                Debug.Log("Right hit " + hit.point);
                SpawnEnemyObj(hit.point);
            }
        }
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

                ob.transform.position = new Vector3(pos.x, ob.transform.position.y, pos.z);
                ob.SetActive(true);
                GameManager.instance.playerTeam.Add(ob);
                Debug.Log("playerTeam count = " + GameManager.instance.playerTeam.Count);
            }
            else
            {
                Debug.Log("Cant' spawn");
            }
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

                ob.transform.position = new Vector3(pos.x, ob.transform.position.y, pos.z);
                ob.SetActive(true);

                GameManager.instance.enemyTeam.Add(ob);
                Debug.Log("enemyTeam count = " + GameManager.instance.enemyTeam.Count);
            }
            else
            {
                Debug.Log("Cant' spawn");
            }
        }
    }
}
