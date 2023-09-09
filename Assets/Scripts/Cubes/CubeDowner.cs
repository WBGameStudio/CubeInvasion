using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CubeDowner : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    Timer timer;
    

   
    void Start()
    {
        timer = FindObjectOfType<Timer>();
    }

   
    void Update()
    {
        DownCube();
    }

    void DownCube() 
    {
        if(timer.time > 0) { return; }
        rb.useGravity = true;
        rb.isKinematic = false;
    }
}
