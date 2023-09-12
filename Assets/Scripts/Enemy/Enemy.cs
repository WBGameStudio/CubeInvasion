using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //We can create scriptable objects for the enemy types and we can get the enemy stats from them.
    //It will be same stats for all the enemies for now
    [SerializeField] private int health = 100;

    public void GetDamage(int _damage)
    {
        Debug.Log("Damage Taken");
        health -= _damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
