using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CubeSelector : MonoBehaviour
{
    public CubeDowner[] _cubeDowners;
    public GameObject[] _cubes;
    public int cubeCount = 0;
    public CinemachineVirtualCamera camera;

    private void Start()
    {
        GetCubeDowners();
        CameraChange();
        //Start for the first cube
        
        StartCubeTimer();
        FindObjectOfType<CameraManager>().CameraChanger(cubeCount);
        
    }

    private void CameraChange()
    {
        if (cubeCount > 0)
        {
            _cubeDowners[cubeCount - 1].gameObject.transform.Find("CinemachineVerty").gameObject.SetActive(false);
        }
        _cubeDowners[cubeCount].gameObject.transform.Find("CinemachineVerty").gameObject.SetActive(true);
        camera = _cubeDowners[cubeCount].gameObject.transform.Find("CinemachineVerty").gameObject.GetComponent<CinemachineVirtualCamera>();
    }
    
    private void GetCubeDowners()
    {
        //Getting all the CubeDowners.
        _cubeDowners = transform.GetComponentsInChildren<CubeDowner>();
        
        
    }

    private void StartCubeTimer()
    {
        //Resetting timer, activate the next cube, spawn the enemies for activated cube
        
        _cubeDowners[cubeCount].isActive = true;
        _cubeDowners[cubeCount].transform.GetComponentInChildren<EnemySpawner>().SpawnEnemies();
        FindObjectOfType<Timer>().ResetTimer();
    }

    public void ChangeCube()
    {
            
            //Increase to cubeCount to change the active cube.
            cubeCount++;
            CameraChange();
            FindObjectOfType<CameraManager>().CameraChanger(cubeCount);
            // _cubeDowners[cubeCount].transform.GetComponentInChildren<GateController>().EnemyCountOnCube = 0;
            StartCubeTimer();
           
    }
}
