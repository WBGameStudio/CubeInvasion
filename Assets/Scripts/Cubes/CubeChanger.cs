using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeChanger : MonoBehaviour
{
    private GateStats _gateStats;

    private void Start()
    {
        _gateStats = transform.parent.GetComponent<GateStats>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //When the player gets in to the CubeChanger trigger it starts the coroutine for the cube change. And also destroys itself after that.
            StartCoroutine(ChangeCube());
            _gateStats.SetStats();
            Destroy(gameObject);
        }
    }

    private IEnumerator ChangeCube()
    {
        //Reason I am using the coroutine that when I make the timer time 0 all the cubes will instantly go down and I prevent that with the WaitForEndOfFrame method.
        
        FindObjectOfType<Timer>().time = 0f;
        yield return new WaitForEndOfFrame();
        
    }
}
