using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraChanger : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<CinemachineVirtualCamera>().Follow = FindObjectOfType<Player>().gameObject.transform;
        GetComponent<CinemachineVirtualCamera>().LookAt = FindObjectOfType<Player>().gameObject.transform;
    }
}
