using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSelector : MonoBehaviour
{
    public CubeDowner[] _cubeDowners;
    public GameObject[] _cubes;
    public int cubeCount = 0;

    private void Start()
    {
        //Start for the first cube
        GetCubeDowners();
        StartCubeTimer();
        FindObjectOfType<CameraManager>().CameraChanger(cubeCount);
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
            FindObjectOfType<CameraManager>().CameraChanger(cubeCount);
            _cubeDowners[cubeCount].transform.GetComponentInChildren<GateController>().EnemyCountOnCube = 0;
            StartCubeTimer();
    }
}
