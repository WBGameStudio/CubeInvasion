using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{

    [SerializeField] CinemachineVirtualCamera _camVertical;
    [SerializeField] CinemachineVirtualCamera _camHorizontal;

    public void CameraChanger(int cubecount) 
    {
        
        CubeSelector cubeSelector = FindObjectOfType<CubeSelector>();
        Debug.Log("CUBE" + cubecount);
        Vector3 currentRotation = cubeSelector._cubeDowners[cubecount].transform.eulerAngles;
        Debug.Log("ROTATİONS" + currentRotation);
        //Changing the Camera according the cubes rotations.
        if (currentRotation.y == 90) 
        {
            _camHorizontal.Priority = 20;
            _camVertical.Priority = 10;
        }
        else
        {
            _camHorizontal.Priority = 10;
            _camVertical.Priority = 20;
        }
        
    } 


}
