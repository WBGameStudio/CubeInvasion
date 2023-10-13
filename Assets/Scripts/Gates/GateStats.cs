using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GateStats : MonoBehaviour
{
    private TextMeshPro text;
    private PlayerStats _statsManager;
    public enum StatSelector
    {
        Health,
        Damage,
        FireRate,
        Speed,
    }

    public StatSelector stat;
    [SerializeField] private float statAmount;

    private void Start()
    {
        _statsManager = FindObjectOfType<PlayerStats>();
        text = GetComponentInChildren<TextMeshPro>();
        SetText();
    }


    public void SetStats()
    {
        switch (stat)
        {
            case StatSelector.Health:
                _statsManager.UpgradeHealth(int.Parse(statAmount.ToString()));
                break;
            case StatSelector.Damage:
                _statsManager.UpgradeDamage(int.Parse(statAmount.ToString()));
                break;
            case StatSelector.FireRate:
                _statsManager.UpgradeFireTime(statAmount);
                break;
            case StatSelector.Speed:
                _statsManager.UpgradeSpeed(statAmount);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetText()
    {
        text.text = "+" + statAmount.ToString() + "\n" + stat.ToString();
    }
}
