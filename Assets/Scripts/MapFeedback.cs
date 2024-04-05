using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapFeedback : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI uiText;
    public RectTransform rectTransform;
    public Coords coords;

    public void Hide() {
        gameObject.SetActive(false);
    }
    public void Show() {
        gameObject.SetActive(true);
    }

    public void Display(Coords coords, string title, Color c) {
        this.coords = coords;
        Show();
        uiText.text = title;
        uiText.color = c;

        image.color = c;
    }
}
