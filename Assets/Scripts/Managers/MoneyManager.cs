using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    private void Start()
    {
        if (!PlayerPrefs.HasKey("Money"))
        {
            PlayerPrefs.SetInt("Money", 0);
        }
        SetMoneyText();
    }

    public void GetMoney(int money)
    {
        PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money") + money);
        SetMoneyText();
    }

    private void SetMoneyText()
    {
        moneyText.text = "$" + PlayerPrefs.GetInt("Money");
    }
}
