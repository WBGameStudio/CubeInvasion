using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyColorChanger : MonoBehaviour
{
    private EnemyColorManager _enemyColorManger;

    private void Awake()
    {
        _enemyColorManger = FindObjectOfType<EnemyColorManager>();
    }

    public void ChangeColor(int health)
    {
        //It changes the color of the enemy according to its health. Colors are changing depending on the multiples of 20.
        //Example:
        //100 >= health >= 80 Color is green
        //80 > health <= 60 Color is another color
        int colorIndex = Mathf.Clamp((100 - health) / 20, 0, 4);
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = _enemyColorManger.enemyColors[colorIndex];
        
    }
}
