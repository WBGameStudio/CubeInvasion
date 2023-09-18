using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    
    private Transform playerTransform;

    private FOVManager fovManager;
    

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        playerTransform = FindObjectOfType<Controller>().transform.GetChild(0).transform;
        fovManager = GetComponent<FOVManager>();
        _navMeshAgent.speed = GetComponent<Enemy>().speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
             Push(collision);
        }
    }

    private void Update()
    {
        Movement();
    }

     private void Push(Collision col)
     {
        Debug.Log("ADD FORCE");
        float knockbackForce = GetComponent<Enemy>().pushPower;
        
        Vector3 pushDirection = transform.forward;
        pushDirection.Normalize();
        Vector3 desiredPosition = col.transform.position + pushDirection;
        desiredPosition.y = knockbackForce;
        Debug.Log("Force: " + desiredPosition);
        //col.transform.position = Vector3.Lerp(col.transform.position, desiredPosition, 0.2f);
        col.rigidbody.velocity = desiredPosition * knockbackForce;
        
        

    }

    private void Movement() 
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, fovManager.fov);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                _navMeshAgent.destination = playerTransform.position;
            }

        }
    }
    
}
