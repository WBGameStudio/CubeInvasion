using System;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
   

    private const string HealthKey = "PlayerHealth";
    private const string DamageKey = "PlayerDamage";
    private const string FireRateKey = "PlayerFireRate";
    private const string SpeedKey = "PlayerSpeed";

    private int currentHealth;
    private int currentDamage;
    private float currentFireRate;
    private float currentSpeed;

    private PlayerStats _playerStats;
    private void Awake()
    {
        // Load saved stats or use base stats if not found.
        currentHealth = PlayerPrefs.GetInt(HealthKey, 100);
        currentDamage = PlayerPrefs.GetInt(DamageKey, 20);
        currentFireRate = PlayerPrefs.GetFloat(FireRateKey, 0.5f);
        currentSpeed = PlayerPrefs.GetFloat(SpeedKey, 10f);
    }

    private void Start()
    {
        _playerStats = FindObjectOfType<PlayerStats>();
        UpgradeHealth(0);
        UpgradeDamage(0);
        UpgradeFireRate(0);
        UpgradeSpeed(0);
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
    public float GetSpeed()
    {
        return currentSpeed;
    }

    public void UpgradeHealth(int upgradeAmount)
    {
        currentHealth += upgradeAmount;
        PlayerPrefs.SetInt(HealthKey, currentHealth);
        PlayerPrefs.Save();
        _playerStats.SetUpgrades();
    }

    public void UpgradeDamage(int upgradeAmount)
    {
        currentDamage += upgradeAmount;
        PlayerPrefs.SetInt(DamageKey, currentDamage);
        PlayerPrefs.Save();
        _playerStats.SetUpgrades();
    }

    public void UpgradeFireRate(float upgradeAmount)
    {
        currentFireRate -= upgradeAmount;
        PlayerPrefs.SetFloat(FireRateKey, currentFireRate);
        PlayerPrefs.Save();
        _playerStats.SetUpgrades();
    }
    public void UpgradeSpeed(float upgradeAmount)
    {
        currentSpeed += upgradeAmount;
        PlayerPrefs.SetFloat(SpeedKey, currentSpeed);
        PlayerPrefs.Save();
        _playerStats.SetUpgrades();
    }
}
