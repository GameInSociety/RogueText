using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDays : MonoBehaviour{

    // Use this for initialization
    public void Start()
    {
        TimeManager.Instance.onNextDay += HandleOnNextDay;

        UpdateUI();
    }

    private void HandleOnNextDay()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        //Display("j:" + TimeManager.Instance.daysPasted);
    }
}
