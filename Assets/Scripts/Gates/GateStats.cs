using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GateStats : MonoBehaviour
{
    private TextMeshPro text;
    private StatsManager _statsManager;
    public enum StatSelector
    {
        Health,
        Damage,
        FireRate,
    }

    public StatSelector stat;
    [SerializeField] private float statAmount;

    private void Start()
    {
        _statsManager = FindObjectOfType<StatsManager>();
        text = GetComponentInChildren<TextMeshPro>();
        SetText();
    }


    public void SetStats()
    {
        if (stat == StatSelector.Health)
        {
            _statsManager.UpgradeHealth(int.Parse(statAmount.ToString()));
        }
        else if (stat == StatSelector.Damage)
        {
            Debug.Log("Zort");
            _statsManager.UpgradeDamage(int.Parse(statAmount.ToString()));
        }
        else if (stat == StatSelector.FireRate)
        {
            _statsManager.UpgradeFireRate(statAmount);
        }
    }

    private void SetText()
    {
        text.text = "+" + statAmount.ToString() + "\n" + stat.ToString();
    }
}
