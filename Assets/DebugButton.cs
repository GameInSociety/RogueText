using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugButton : MonoBehaviour, IPointerClickHandler {
    public Image image;
    public Text uiText;
    CanvasGroup _cg;
    public int seconds = 0;

    public bool clear = false;
    CanvasGroup CanvasGroup {
        get {
            if (_cg == null)
            {
                _cg = GetComponent<CanvasGroup>();
            }
            return _cg;
        }
    }

    public bool isPressed = false;
    public bool selected = false;

    public void Display(string text, Color color) {
        uiText.text = seconds > 0 ? seconds.ToString() : text;
        image.color = color;
    }

    private void Update() {
        if (isPressed) {
            isPressed = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        selected = !selected;
        if (clear) {
            WorldAction.debug_list.Clear();
        }
    }

    public void UpdateUI(bool selected) {
        CanvasGroup.alpha = selected ? 0.4f : 1f;
    }
}
