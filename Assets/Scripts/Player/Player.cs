using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int health;

    private void Update()
    {
        health = GetComponent<PlayerStats>().health;
    }

    public void GetDamage(int damage)
    {
        FindObjectOfType<AudioManager>().Play("Hit");
        Debug.Log("Damage Taken");
        health -= damage;

        if (health > 0) return;
        Destroy(gameObject);
        FindObjectOfType<GameOver>().Died();
    }
}
