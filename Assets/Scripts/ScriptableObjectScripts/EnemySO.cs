
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Enemy", fileName = "New Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("*** COMBAT ***")] 
    public int health;
    public int damagePower;
    public float pushPower;
    [Space][Space]
    [Header("*** MOVEMENT ***")]
    public float speed;
}
