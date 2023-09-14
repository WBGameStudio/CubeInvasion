using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("*** CINEMACHINE CAMERA ROTATION ***")]
    [SerializeField] CinemachineVirtualCamera _camVertical;
    [SerializeField] CinemachineVirtualCamera _camHorizontal;
    [Space]
    public Vector3 currentRotation;
    
    private bool isVertical;
    private CinemachineBasicMultiChannelPerlin noise;
    [Space] [Space]
    
    [Header("*** SHAKING ***")]
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeIntensity;
    [SerializeField] private float shakeFrequency;
    
    private void Awake()
    {
        GetNoise();
    }
    public void CameraChanger(int cubecount) 
    {
        
        CubeSelector cubeSelector = FindObjectOfType<CubeSelector>();
        Debug.Log("CUBE" + cubecount);
        currentRotation = cubeSelector._cubeDowners[cubecount].transform.eulerAngles;
        Debug.Log("ROTATIONS" + currentRotation);
        //Changing the Camera according the cubes rotations.
        if (currentRotation.y == 90) 
        {
            _camHorizontal.Priority = 20;
            _camVertical.Priority = 10;
            isVertical = false;
        }
        else
        {
            _camHorizontal.Priority = 10;
            _camVertical.Priority = 20;
            isVertical = true;
        }
        
    }

    private void GetNoise()
    {
        //Finding the active virtual camera to get its noise cinemachine component.
        noise = isVertical ? _camVertical.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>() : _camHorizontal.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }
    

    public void CameraShake()
    {
        //This method gets called in timer when the time hits 5
        
        GetNoise();
        //Start the shake camera coroutine and start shake the camera.
        StartCoroutine(ShakeCamera());
    }

    public void StopCameraShake()
    {
        //This method gets called when the player gets the other cube
        
        //Stopping the camera shake
        noise.m_AmplitudeGain = 0.0f;
    }
    private IEnumerator ShakeCamera()
    {
        // Enable the noise module to start the shake
        noise.m_AmplitudeGain = shakeIntensity;
        noise.m_FrequencyGain = shakeFrequency;
        
        // Wait for the specified duration
        yield return new WaitForSeconds(shakeDuration);

        // Disable the noise module to stop the shake
        noise.m_AmplitudeGain = 0.0f;
    }


}
