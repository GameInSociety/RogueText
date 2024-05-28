using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using System;
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

    public WorldAction _worldAction;
    public Line _line;

    public bool clear = false;

    public enum Type {
        WA,
        Line,
        Part
    }
    public Type type;

    public void SetWA(WorldAction wa) {
        _worldAction = wa;
        type = Type.WA;
    }
    public void SetLine(Line line) {
        _line = line;
        type = Type.Line;
    }

    CanvasGroup CanvasGroup {
        get {
            if (_cg == null)
            {
                _cg = GetComponent<CanvasGroup>();
            }
            return _cg;
        }
    }


    public void Display(string text, Color color) {
        uiText.text = seconds > 0 ? seconds.ToString() : text;
        image.color = color;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (clear) {
            WorldAction.debug_list.Clear();
        } else {
            if ( type == Type.WA) {
                _worldAction.debug_selected = !_worldAction.debug_selected;
                UpdateUI(_worldAction.debug_selected);
            } else if (type == Type.Line) {
                Debug.Log($"pressin line");
                _line.debug_selected = !_line.debug_selected;
                UpdateUI(_line.debug_selected);
            }
        }
    }

    public void UpdateUI(bool selected) {
        CanvasGroup.alpha = selected ? 0.4f : 1f;
    }
}
