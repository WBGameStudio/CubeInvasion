using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeDowner : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DownCube() 
    {
        rb.useGravity = true;
        //this.gameObject.isStatic = false;
    }
}
