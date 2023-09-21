using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class Shooter : MonoBehaviour
{
    [Header("*** BULLET ***")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;
    [Space] [Space]
    [Header("*** SHOOTING ***")]
    [SerializeField] private float shootingTime;
    [Space] [Space]
    [Header("*** DETECTION ***")]
    FOVManager fovManager;

    private bool isFirstFireRateInfoTaken;
    private float timer = 0f;

    private void Start()
    {
        fovManager = GetComponent<FOVManager>();
    }
    void Update()
    {
        
        shootingTime = GetComponent<PlayerStats>().fireTime;
            
        
        EnemyDetector();
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

    void EnemyDetector()
    {
        // Detect enemies within the FOV and detection range
        var colliders = Physics.OverlapSphere(transform.position, fovManager.fov);
        
        // I change that if because the older version was spawning bullets for every enemy in the sphere and that was causing a much faster shooting rate.
        // Example: If there is 4 enemies in the player's fov SpawnBullet() will be executed 4 times in a row and that would cause to player shoot faster.
        // Now the code works if there is "ANY" enemies in the fov the method will be executed only 1 time
        if (colliders.Any(collider => collider.CompareTag("Enemy")))
        {
            
            SpawnBullet();
        }
    }
}
