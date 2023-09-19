using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CubeDowner : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    Timer timer;
    public bool isActive;

   
    void Start()
    {
        timer = FindObjectOfType<Timer>();
    }

   
    void Update()
    {
        if (!isActive) return;
        DownCube();
    }

    void DownCube() 
    {
        if(timer.time > 0) { return; }
        rb.useGravity = true;
        rb.isKinematic = false;
        BoxCollider bc = rb.GetComponent<BoxCollider>();
        bc.isTrigger = true;
        FindObjectOfType<CubeSelector>().ChangeCube();
        Destroy(this.gameObject, 5);
    }
}
