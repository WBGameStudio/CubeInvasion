using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public bool isTimeSet;
    public GameObject canvas;
    AsyncOperation asyncLoad;
    // Start is called before the first frame update
    void Awake()
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
        asyncLoad = SceneManager.LoadSceneAsync(FindObjectOfType<GameOver>().LevelNum);

        canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        canvas.transform.Find("PlayMenu").gameObject.SetActive(true);
        isTimeSet =true;
        
        //Setting the made upgrades
        FindObjectOfType<PlayerStats>().SetUpgrades();
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
            child.gameObject.SetActive(child.name == "MainMenu");
        }
    }

    void SetTime()
    {
        Time.timeScale = isTimeSet ? 1 : 0;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(FindObjectOfType<GameOver>().LevelNum);
    }

    


}
