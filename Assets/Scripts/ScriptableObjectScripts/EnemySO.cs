
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Enemy", fileName = "New Enemy")]
public class EnemySO : ScriptableObject
{
    public enum EnemyTpye
    {
        Pusher,
        Shooter,
        Bomber
    }
    [Header("*** OBJECT ***")] 
    public GameObject enemyObject;
    [Space][Space] 
    [Header("*** TYPE ***")]
    public EnemyTpye enemyTpye;
    [Space][Space] 
    [Header("*** COMBAT ***")] 
    public int health;
    public int damagePower;
    public float pushPower;
    [Space][Space]
    [Header("*** MOVEMENT ***")]
    public float speed;
    public float fov;
    [Space][Space] 
    [Header("*** ECONOMY ***")]
    public int moneyWorth;
}
