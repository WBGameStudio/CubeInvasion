using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateOpener : MonoBehaviour
{
    private GameObject gate;

    private void Start()
    {
        gate = transform.GetChild(0).gameObject;
    }

    private IEnumerator GateOpeningAnimation()
    {
        float gateLength = gate.transform.localScale.y;
        while (gate.transform.localScale.y > 0)
        {
            gate.transform.localScale = new Vector3(gate.transform.localScale.x, gate.transform.localScale.y - gateLength / 100, gate.transform.localScale.z);
            yield return new WaitForEndOfFrame();
        }
    }

    public void OpenGate()
    {
        StartCoroutine(GateOpeningAnimation());
    }
}
