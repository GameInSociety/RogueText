using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestTile : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI uiText;
    public RectTransform rectTransform;
    public void Display (string str, Color c) {
        uiText.text = str;
        image.color = c;
    }
}
