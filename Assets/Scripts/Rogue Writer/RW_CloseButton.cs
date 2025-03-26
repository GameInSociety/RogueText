using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEditor;
using TMPro;

public class RW_CloseButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    private UnityEngine.UI.Image image;
    bool over = false;
    Color targetColor;
    public RectTransform _textRT;
    private float _initTextDecal;
    private float _targetTextDecal = 25f;

    private void Start() {
        image = GetComponent<UnityEngine.UI.Image>();
        targetColor = image.color;
        image.color = Color.clear;
        _initTextDecal = _textRT.offsetMax.x;
    }

    private void OnEnable() {
        if (image == null)
            return;
        image.color = Color.clear;
        // reseting
    }

    private void Update() {
        image.color = Color.Lerp(image.color, over ? targetColor : Color.clear, 10f * Time.deltaTime);
        var pos = _textRT.offsetMax;
        pos.x = over ? -_targetTextDecal : _initTextDecal;
        _textRT.offsetMax = Vector2.Lerp(_textRT.offsetMax, pos, Time.deltaTime);
    }

    public void OnPointerClick(PointerEventData eventData) {
        GetComponentInParent<RW_Menu>().Close();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        over = true;    
    }

    public void OnPointerExit(PointerEventData eventData) {
        over =false;
    }
}
