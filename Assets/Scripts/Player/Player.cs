using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private int health;
    public int currentHealth;
   

   
    private void Update()
    {
        health = GetComponent<PlayerStats>().health;
        
    }

    public void GetDamage(int damage)
    {
        FindObjectOfType<AudioManager>().Play("Hit");
        Debug.Log("Damage Taken");
        health -= damage;
        currentHealth = health;
         
    
        if (currentHealth > 0) return;
        Destroy(gameObject);
        FindObjectOfType<GameOver>().Died();
    }

    public void GetHealth(int point) 
    {
        if(health > currentHealth) 
        { currentHealth += point; }
        

    
    }
}
