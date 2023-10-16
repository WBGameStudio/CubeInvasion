using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("*** STATS ***")]
    [Space]
    public int health;
    public int damage;
    public float fireTime;
    public float speed;

    public void SetUpgrades()
    {
        Debug.Log("UpgradeChange");
        health = PlayerPrefs.GetInt("PlayerHealth");
        damage = PlayerPrefs.GetInt("PlayerDamage");
        fireTime = PlayerPrefs.GetFloat("PlayerFireRate");
        speed = PlayerPrefs.GetFloat("PlayerSpeed");
    }

    public void UpgradeHealth(int upgradeAmount)
    {
        health += upgradeAmount;
    }
    public void UpgradeDamage(int upgradeAmount)
    {
        damage += upgradeAmount;
    }
    public void UpgradeFireTime(float upgradeAmount)
    {
        fireTime -= upgradeAmount;
    }
    public void UpgradeSpeed(float upgradeAmount)
    {
        speed += upgradeAmount;
    }
    
}
