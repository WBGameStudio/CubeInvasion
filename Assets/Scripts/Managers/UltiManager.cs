using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UltiManager : MonoBehaviour
{
      [SerializeField] private float ultiPoint;
      [SerializeField] private float maxUltiPoint = 100;
      [SerializeField] private Image ultiBar;

      public void AddUltiPoint(int point)
      {
            ultiPoint += point;
            if (ultiPoint > maxUltiPoint)
            {
                  ultiPoint = maxUltiPoint;
            }
            UpdateUltiBar();
      }
      private void UpdateUltiBar()
      {
            ultiBar.fillAmount = ultiPoint / maxUltiPoint;
      }
      public float GetUltiPoint()
      {
            return ultiPoint;
      }
      public float GetMaxUltiPoint()
      {
            return maxUltiPoint;
      }

}
