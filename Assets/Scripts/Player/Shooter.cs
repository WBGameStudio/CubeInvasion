using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("*** BULLET ***")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;
    [Space] [Space]
    [Header("*** SHOOTING ***")]
    [SerializeField] private float shootingTime;
    
    private float timer = 0f;

    void Update()
    {
        SpawnBullet();
    }

    private void SpawnBullet()
    {
        //It spawns a bullet every (shootingTime) seconds.
        timer += Time.deltaTime;
        
        if (timer >= shootingTime)
        {
            //Reset the timer
            timer = 0.0f;

            //Spawning the bullet
            //Spawning position can be change later according to the weapon's firing position
            var _bullet = Instantiate(bullet, transform.position, Quaternion.identity);
            
            //Calling the move function for the created bullet
            MoveBullet(_bullet);
            
        }
    }

    private void MoveBullet(GameObject _bullet)
    {
        //Giving a velocity to move the bullet
        _bullet.GetComponent<Rigidbody>().velocity = (transform.forward * bulletSpeed);
    }
}
