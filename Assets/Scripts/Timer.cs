using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    public float time = 20;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeUpdater();
    }

    void timeUpdater()
    {
        if(time <= 0) { return; }   
        time -= Time.deltaTime;
        int timeInt = (int)time;
        timerText.text = timeInt.ToString();
    }

    public void ResetTimer()
    {
        time = 20;
    }
}
