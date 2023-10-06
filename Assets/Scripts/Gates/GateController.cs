using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateController : MonoBehaviour
{
    [Header("*** GATE ***")]
    Rigidbody rb;

    private GameObject gate;
    [Header("*** ENEMİES ***")]
    public int EnemyCountOnCube = 0;

    //Created a timerInterval variable to call the GateOpening function in every (timerInterval) seconds.
    //The purpose of this to reduce the calling because when the active gate is changing the EnemyOnCube variable resets to 0 and that causes to gate to drop instantly.
    //It should wait the EnemyCounter to count the enemies but it wasn't so created a period to call the GateOpening function.
    private const float timerInterval = 0.4f;
    private float timer = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gate = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= timerInterval)
        {
            // Reset the timer
            timer = 0.0f;
            
            GateOpening();
        }
    }

    

    private void GateOpening()
    {
        

        if (GetComponentInParent<CubeSelector>().cubeCount >= GetComponentInParent<CubeSelector>()._cubeDowners.Length)
        {
            Debug.Log("Finish");
            //FindObjectOfType<GameOver>().LevelNum++;
            UIManager uiManager = FindObjectOfType<UIManager>();
            uiManager.canvas.transform.Find("PlayMenu").gameObject.SetActive(false);
            uiManager.canvas.transform.Find("FinishMenu").gameObject.SetActive(true);
            uiManager.isTimeSet = false;
        }

        //Checks if the gate is the active one.
        if (!GetComponentInParent<CubeDowner>().isActive) { return; }

        //If there is no enemies on our cube, gate is falling down and we can move on to the next cube.
        if (EnemyCountOnCube != 0) { return; }
        
        
        transform.GetChild(0).transform.GetComponent<GateOpener>().OpenGate();
        transform.GetChild(1).transform.GetComponent<GateOpener>().OpenGate();
        // BoxCollider boxCollider = GetComponent<BoxCollider>();
        // boxCollider.isTrigger = true;
        // rb.useGravity = true;
        // rb.isKinematic = false;
    }

   
}
