using System;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
   

    private const string HealthKey = "PlayerHealth";
    private const string DamageKey = "PlayerDamage";
    private const string FireRateKey = "PlayerFireRate";

    private int currentHealth;
    private int currentDamage;
    private float currentFireRate;

    private void Awake()
    {
        // Load saved stats or use base stats if not found.
        currentHealth = PlayerPrefs.GetInt(HealthKey, 100);
        currentDamage = PlayerPrefs.GetInt(DamageKey, 20);
        currentFireRate = PlayerPrefs.GetFloat(FireRateKey, 0.5f);
    }

    private void Start()
    {
        UpgradeHealth(0);
        UpgradeDamage(0);
        UpgradeFireRate(0);
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public float GetDamage()
    {
        return currentDamage;
    }

    public float GetFireRate()
    {
        return currentFireRate;
    }

    public void UpgradeHealth(int upgradeAmount)
    {
        currentHealth += upgradeAmount;
        PlayerPrefs.SetInt(HealthKey, currentHealth);
        PlayerPrefs.Save();
    }

    public void UpgradeDamage(int upgradeAmount)
    {
        currentDamage += upgradeAmount;
        PlayerPrefs.SetInt(DamageKey, currentDamage);
        PlayerPrefs.Save();
    }

    public void UpgradeFireRate(float upgradeAmount)
    {
        currentFireRate -= upgradeAmount;
        PlayerPrefs.SetFloat(FireRateKey, currentFireRate);
        PlayerPrefs.Save();
    }
}
