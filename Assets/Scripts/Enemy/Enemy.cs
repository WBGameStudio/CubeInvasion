using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //We can create scriptable objects for the enemy types and we can get the enemy stats from them.
    //It will be same stats for all the enemies for now
    [SerializeField] private int health = 100;
    GameObject gate;
    private void Start()
    {
        gate = FindNearestGate();
    }

    public void GetDamage(int _damage)
    {
        Debug.Log("Damage Taken");
        health -= _damage;
        if (health <= 0)
        {
            GateController gateController = gate.GetComponent<GateController>();
            gateController.EnemyCountOnCube--;
            Debug.Log(gateController.EnemyCountOnCube);
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
