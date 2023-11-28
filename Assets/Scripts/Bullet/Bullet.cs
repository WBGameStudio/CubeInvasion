using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
      //The damage amount can be taken from the weapon script but it will be in here for now.
      //Weapons also can have scriptable objects to get the weapon stats.
      private bool isFirstDamageInfoTaken;
      public int damage;

      private void Update()
      {
            damage = FindObjectOfType<PlayerStats>().damage;
      }

      private void OnTriggerEnter(Collider other)
      {

            //If it is not touching the player it will destroy self when it touches any other object.
            //It will prevent to have too many bullets on the scene
            if (other.transform.CompareTag("Player")) return;
            //When the bullet touch a collider with a tag named "Enemy" it will give a damage to the enemy.
            if (other.transform.CompareTag("Enemy"))
            {
                  other.transform.GetComponent<Enemy>().GetDamage(damage);
                  ParticleSystem hit = other.transform.GetComponentInChildren<ParticleSystem>();
                  hit.Play();
                  Destroy(gameObject);
            }
            if (other.transform.CompareTag("Bomb"))
            {
                  other.transform.GetComponent<Bomb>().knockBack();
                  Destroy(gameObject);
            }

      }

      private void Start()
      {
            //It will destroy itself in 20 seconds no matter what to prevent any bugs etc.
            Destroy(gameObject, 20f);
      }

}
