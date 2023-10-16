using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    public float time = 20;

    private bool isShaked;

    private CameraManager cameraManager;

    // Start is called before the first frame update
    private void Start()
    {
        cameraManager = FindObjectOfType<CameraManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        timeUpdater();
    }

    private void timeUpdater()
    {
        if(time <= 0) { return; }   
        time -= Time.deltaTime;
        int timeInt = (int)time;
        
        //Shaking the camera when the time hits 5
        if (timeInt == 5 && !isShaked)
        {
            cameraManager.CameraShake();
            isShaked = true;
        }
        
        timerText.text = timeInt.ToString();
    }

    public void ResetTimer()
    {
        
        time = 20;
        isShaked = false;
        FindObjectOfType<CameraManager>().StopCameraShake();
    }
}
