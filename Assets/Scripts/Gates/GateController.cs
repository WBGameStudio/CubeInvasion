using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("*** GATE ***")]
    Rigidbody rb;
    [Space][Space]
    [Header("*** ENEMİES ***")]
    public int EnemyCountOnCube = 0;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GateOpening();
    }

    private void GateOpening()
    {
        //if there is no enemies on our cube, gate is falling down and we can move on to the next cube.
        if (EnemyCountOnCube != 0) { return; }
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        rb.useGravity = true;
        rb.isKinematic = false;
    }

   
}
