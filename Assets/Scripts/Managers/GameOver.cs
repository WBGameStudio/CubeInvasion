using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    
    [SerializeField] public int LevelNum;


    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            Died();
        }
    }

    public void Died() 
    {
        UIManager uiManager = FindObjectOfType<UIManager>();    
        uiManager.canvas.transform.Find("PlayMenu").gameObject.SetActive(false);
        uiManager.canvas.transform.Find("GameOverMenu").gameObject.SetActive(true);
        uiManager.isTimeSet = false;
    }
}
