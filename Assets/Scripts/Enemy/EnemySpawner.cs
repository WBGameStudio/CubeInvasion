using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyObject;
    [SerializeField] private EnemySO enemySO;
    [Space][Space][Space]
    [SerializeField] private Transform spawnPoints;
    [SerializeField] private Transform enemies;

    public void SpawnEnemies()
    {
        foreach (Transform child in spawnPoints)
        {
            //Getting all the spawnPoints and spawn the enemies according to them.
            var enemy = Instantiate(this.enemyObject, child.position, Quaternion.identity);
            enemy.transform.parent = enemies;
            //It sends the stats to the spawned enemy
            enemy.GetComponent<Enemy>().SetStats(enemySO);
        }
    }
}
