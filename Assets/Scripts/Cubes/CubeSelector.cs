using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSelector : MonoBehaviour
{
    private CubeDowner[] _cubeDowners;
    private int cubeCount = 0;

    private void Start()
    {
        //Start for the first cube
        GetCubeDowners();
        StartCubeTimer();
    }

    private void GetCubeDowners()
    {
        //Getting all the CubeDowners.
        _cubeDowners = transform.GetComponentsInChildren<CubeDowner>();
    }

    private void StartCubeTimer()
    {
        //Resetting timer, activate the next cube, spawn the enemies for activated cube
        FindObjectOfType<Timer>().ResetTimer();
        _cubeDowners[cubeCount].isActive = true;
        _cubeDowners[cubeCount].transform.GetComponentInChildren<EnemySpawner>().SpawnEnemies();
    }

    public void ChangeCube()
    {
        //Increase to cubeCount to change the active cube.
        cubeCount++;
        _cubeDowners[cubeCount].transform.GetComponentInChildren<GateController>().EnemyCountOnCube = 0;
        StartCubeTimer();
    }
}
