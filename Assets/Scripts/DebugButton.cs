using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugButton : MonoBehaviour, IPointerClickHandler {
    public RectTransform rectTransform;
    public Image image;
    public Image outline;
    public Text uiText;
    public Text uiText_secundary;
    CanvasGroup _cg;
    public int seconds = 0;
    public bool selected = false;
    public bool clear = false;

    CanvasGroup CanvasGroup {
        get {
            if (_cg == null)
                _cg = GetComponent<CanvasGroup>();
            return _cg;
        }
    }


    public void Display(string text, Color color, string sec, int fontSize, Color outlineColor) {
        uiText.text = seconds > 0 ? seconds.ToString() : text;
        uiText_secundary.text = sec;

        uiText.fontSize = fontSize;
        uiText_secundary.fontSize = fontSize;
        image.color = color;
        outline.color = outlineColor;
    }

    public void OnPointerClick(PointerEventData eventData) {
        selected = !selected;
        UpdateUI(selected);
    }

    public void UpdateUI(bool selected) {
        CanvasGroup.alpha = selected ? 0.4f : 1f;
    }
}
