using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private int damage = 20;
    private void OnTriggerEnter(Collider other)
    {
        //If it is not touching the player it will destroy self when it touches any other object.
        //It will prevent to have too many bullets on the scene
        if (other.transform.CompareTag("Enemy")) return;
        //When the bullet touch a collider with a tag named "Enemy" it will give a damage to the enemy.
        if (other.transform.CompareTag("Player"))
        {
            other.transform.GetComponent<Player>().GetDamage(damage);
        }
        Destroy(gameObject);
    }

    private void Start()
    {
        //It will destroy itself in 20 seconds no matter what to prevent any bugs etc.
        Destroy(gameObject, 20f);
    }
}
