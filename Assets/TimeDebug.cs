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
    }

    public void Push(int i) {
        if ( i > currentMax ) {
            currentMax = i;
        }

        fillImage.DOFillAmount((float)(currentMax-i) / currentMax, 0.3f);
        uiText.text = $"{currentMax-i} / {currentMax}";
    }

    public void Reset() {   
        currentMax = 0;
        Push(0);
    }
}
