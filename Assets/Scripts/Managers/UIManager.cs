using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    bool isTimeSet;
    GameObject canvas;
    
    // Start is called before the first frame update
    void Start()
    {
        isTimeSet = false;
        canvas = GameObject.FindGameObjectWithTag("Canvas");
    }

    // Update is called once per frame
    void Update()
    {
        SetTime();
    }

    public void TapToPlayBtn()
    {
        canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        canvas.transform.Find("PlayMenu").gameObject.SetActive(true);
        isTimeSet =true;
    }

    public void SettingsMenuBtn()
    {
        canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        canvas.transform.Find("SettingsMenu").gameObject.SetActive(true);
    }

    public void MainMenuBtn()
    {
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            Transform child = canvas.transform.GetChild(i);

            // If childs name is not MainMenu, then make it false
            if (child.name == "MainMenu")
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    void SetTime() 
    {
        if(isTimeSet) 
        {
            Time.timeScale = 1;
        }
        else 
        {
            Time.timeScale = 0;
        }
    }


}
