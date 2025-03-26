using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEditorInternal;

public class RW_Draggable : Displayable, IBeginDragHandler, IDragHandler, IPointerClickHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {

    public static RW_Draggable s_LastSelected;

    public bool createCopy = false;
    public Image _image;
    public bool removeDelta = false;
    public Color _initColor;
    private float _moveDuration = .2f;
    private bool _canBeDragged = true;
    public bool _canBeOverred = true;

    public float MoveDuration {
        get => _moveDuration;
    }

    public override void Start() {
        base.Start();

        if (_image == null)
            _image = GetComponentInChildren<Image>();

        _initColor = _image.color;
    }

    #region drag
    public void OnBeginDrag(PointerEventData eventData) {
        if (!_canBeDragged)
            return;
        BeginDrag();
    }

    public virtual void BeginDrag() {
        if (createCopy) {
            Copy();
        } else {
            DraggableManager.Instance.Drag_Start(this);
        }
    }

    public virtual void Copy() {
        
    }
    #endregion

    public void OnPointerClick(PointerEventData eventData) {
        Select();
    }

    public virtual void Select() {
        s_LastSelected = this;
    }

    public virtual void HandleMerge(RW_Draggable otherDraggable) {
         
    }

    #region over
    public virtual void Over_Start() {
        if (!_canBeOverred)
            return;
        _image.color = Color.red;
        DraggableManager.Instance._target = this;
        if ( DraggableManager.Instance.onOver != null)
            DraggableManager.Instance.onOver(this);
    }
    public virtual void Over_Exit() {
        if (!_canBeOverred)
            return;
        _image.color = _initColor;
        DraggableManager.Instance._target = null;
    }
    public void OnPointerEnter(PointerEventData eventData) {
        Over_Start();
    }

    public void OnPointerExit(PointerEventData eventData) {
        Over_Exit();   
    }

    public void OnPointerUp(PointerEventData eventData) {
        
    }

    public void OnDrag(PointerEventData eventData) {
    }

    public virtual bool CanMerge(RW_Draggable target) {
        return false;
    }

    public virtual void Merge(RW_Draggable target) {
        _image.color = Color.green;
        GetRectTransform.DOMove(target.GetRectTransform.position, MoveDuration);
        FadeOut();
    }
    public virtual void ReturnToPos(Vector2 pos) {
        _image.color = _initColor;
        GetRectTransform.DOMove(pos, MoveDuration);
    }
    #endregion
}
