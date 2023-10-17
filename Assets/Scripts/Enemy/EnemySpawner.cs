using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoints;
    [SerializeField] private Transform enemies;

    public void SpawnEnemies()
    {
        foreach (Transform child in spawnPoints)
        {
            UIManager UIManager = FindAnyObjectByType<UIManager>();
           
                //Playing spawning effect.
                ParticleSystem spawnParticle = child.GetComponentInChildren<ParticleSystem>();
                spawnParticle.Play();
                //Getting the type of the enemy from the spawnPoint object.
                var enemyObject = child.GetComponent<EnemySpawnerType>().enemyType.enemyObject;
                //Getting all the spawnPoints and spawn the enemies according to them.
                var enemy = Instantiate(enemyObject, child.position, Quaternion.identity);
                enemy.transform.parent = enemies;
                //It sends the stats to the spawned enemy
                enemy.GetComponent<Enemy>().SetStats(child.GetComponent<EnemySpawnerType>().enemyType);
                enemy.GetComponent<Enemy>().gate =
                    enemies.parent.parent.GetComponentInChildren<GateController>().gameObject;
        }
    }

   

}
