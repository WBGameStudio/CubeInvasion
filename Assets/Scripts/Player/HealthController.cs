using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    public float maxHealth;
    public float health;
    [Header("*** HealthBar ***")]
    [Space]
    public Image healthBar;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        maxHealth = FindObjectOfType<PlayerStats>().health;
        if(FindObjectOfType<Player>().currentHealth== 0) 
        {
            health = 110;
        }
        else 
        {
            health = FindObjectOfType<Player>().currentHealth;  
        }
        
        UpdateHealthBar();
    }
    void UpdateHealthBar()
    {
        healthBar.fillAmount = health / maxHealth;
    }
}
