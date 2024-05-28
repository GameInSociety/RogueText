using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDebug : MonoBehaviour
{
    public static TimeDebug Instance;

    public Image fillImage;
    public Text uiText;

    public int currentMax = 0;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Reset();
        Hide();
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
    public void Show() {
        gameObject.SetActive(true);
    }

    public void Push(int i) {

        Show();

        if ( i > currentMax ) {
            currentMax = i;
        }

        fillImage.fillAmount = (float)(currentMax - i) / currentMax;
    }

    public void DisplayText(string text) {
        uiText.text = text;
    }

    public void Reset() {   
        currentMax = 0;
        Push(0);
    }
}
