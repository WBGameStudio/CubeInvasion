using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //We can create scriptable objects for the enemy types and we can get the enemy stats from them.
    //It will be same stats for all the enemies for now
    public int health;
    public int damagePower;
    public float speed;
    public float pushPower;
    public float fov;
    public string enemyType;
    public int moneyWorth;
    
    public GameObject gate;
    private void Start()
    {
        transform.GetComponent<EnemyColorChanger>().ChangeColor(health);
    }

    //It gets the stats from the Enemy Scriptable Object
    public void SetStats(EnemySO _enemySo)
    {
        health = _enemySo.health;
        damagePower = _enemySo.damagePower;
        speed = _enemySo.speed;
        pushPower = _enemySo.pushPower;
        fov = _enemySo.fov;
        enemyType = _enemySo.enemyTpye.ToString();
        moneyWorth = _enemySo.moneyWorth;
    }

    public void GetDamage(int _damage)
    {
        FindObjectOfType<AudioManager>().Play("Hit");
        Debug.Log("Damage Taken");
        health -= _damage;
        transform.GetComponent<EnemyColorChanger>().ChangeColor(health);

        if (health <= 0)
        {
            // gate = FindNearestGate();
            GateController gateController = gate.GetComponent<GateController>();
            gateController.EnemyCountOnCube--;
            Controller controller = FindObjectOfType<Controller>();
            controller.currentTarget = null;
            Debug.Log(gateController.EnemyCountOnCube);
            FindObjectOfType<MoneyManager>().GetMoney(moneyWorth);
            Destroy(gameObject);
        }
    }

    private GameObject FindNearestGate()
    {
        //For counting to died enemies on current cube, i used a system that finding the nearest gate on enemies.
        GameObject[] gates = GameObject.FindGameObjectsWithTag("Gate");
        GameObject nearestGate = null;
        
        float nearestDistance = float.MaxValue;

        foreach (GameObject gate in gates)
        {
            // Calculate the distance from the enemy to the gate
            float distanceToGate = Vector3.Distance(transform.position, gate.transform.position);

            // Check if this gate is closer than the nearest found so far
            if (distanceToGate < nearestDistance)
            {
                nearestGate = gate;
                nearestDistance = distanceToGate;
            }
        }

        return nearestGate;
    }
}
