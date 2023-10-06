using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int health;
    public int damage;
    public float fireTime;

    public void SetUpgrades()
    {
        Debug.Log("UpgradeChange");
        health = PlayerPrefs.GetInt("PlayerHealth");
        damage = PlayerPrefs.GetInt("PlayerDamage");
        fireTime = PlayerPrefs.GetFloat("PlayerFireRate");
    }
}
