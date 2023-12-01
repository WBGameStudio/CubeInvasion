using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered");
        if (other.gameObject.CompareTag("Player"))
        {
            FindObjectOfType<Player>().GetHealth(10);
            Destroy(gameObject);
        }
    }
     
}
