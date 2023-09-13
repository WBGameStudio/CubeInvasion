using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [Space][Space]
    [SerializeField] private Transform spawnPoints;
    [SerializeField] private Transform enemies;

    public void SpawnEnemies()
    {
        foreach (Transform child in spawnPoints)
        {
            //Getting all the spawnPoints and spawn the enemies according to them.
            var enemy = Instantiate(this.enemy, child.position, Quaternion.identity);
            enemy.transform.parent = enemies;
        }
    }
}
