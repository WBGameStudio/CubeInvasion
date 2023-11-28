using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltiBall : MonoBehaviour
{
      private void OnTriggerEnter(Collider other)
      {
            if (other.gameObject.CompareTag("Player"))
            {
                  FindObjectOfType<UltiManager>().AddUltiPoint(30);
                  Destroy(gameObject);
            }
      }
}
