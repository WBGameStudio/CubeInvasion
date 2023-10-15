using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Bomb : MonoBehaviour
{
    [SerializeField] public float expForce;
    public Collider[] colliders;

    private void Update()
    {
        FindingNearObject();
    }

    private void FindingNearObject()
    {
        FOVManager fovManager = GetComponent<FOVManager>();
        // Detect enemies within the FOV and detection range
        colliders = Physics.OverlapSphere(transform.position, fovManager.fov);
    }
    public void knockBack() 
    {
        FOVManager fovManager = GetComponent<FOVManager>();
        ParticleSystem blast = GetComponentInChildren<ParticleSystem>();
        blast.Play();

        foreach (Collider collider in colliders) 
        {
            
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (collider.CompareTag("Enemy") || collider.CompareTag("Player")) 
            {
                
                Debug.Log("KABOOM");
                NavMeshAgent agent = rb.GetComponent<NavMeshAgent>();
                if (agent != null) { agent.enabled = false; }
                rb.AddExplosionForce(expForce,transform.position,fovManager.fov);
                Wait();
                if (agent != null) { agent.enabled = true; }

            }
        }

        Destroy(gameObject,1f);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2);
    }

   
}
