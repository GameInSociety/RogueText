using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DraggableManager : MonoBehaviour
{
    public static DraggableManager Instance;

    private RW_Draggable _draggedInfo;
    public RW_Draggable _dragged;
    public RW_Draggable _target;

    public RW_DragClone _clone;
    public RectTransform _dummyRT;
    public TextMeshProUGUI _dummyText;

    [SerializeField]
    private Transform _group;
    private Canvas _canvas;
    private Transform _previousParent;
    private int _prevSiblingIndex;
    private RectTransform _previousLayout;

    [SerializeField]
    private float _dragSpeed = 5f;

    private Vector2 _initPos;
    private Vector2 _targetPos;
    private Vector2 _delta;

    private bool _canDrag = true;

    private bool _dragging = false;
    private bool _activeClone = false;

    public delegate void OnMerge(RW_Draggable draggable);
    public OnMerge onMerge;
    public delegate void OnOver(RW_Draggable draggable);
    public OnOver onOver;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _canvas = GetComponent<Canvas>();
        _clone.Hide();
    }

    private void Update() {
        if (_dragging) {
            UpdateTargetPos();
            Drag_Update();
        }
    }

    void UpdateTargetPos() {
        Vector2 inputPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform, Input.mousePosition, _canvas.worldCamera, out inputPos);
        _targetPos = _canvas.transform.TransformPoint(inputPos + _delta);
    }

    public void Copy(RW_Draggable draggable, string text) {
        _clone.GetRectTransform.position = draggable.GetRectTransform.position;
        _clone.GetRectTransform.sizeDelta = draggable.GetRectTransform.sizeDelta;
        _dummyText.text = text;
        _clone.Display(text);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_dummyRT);

        _clone.GetRectTransform.DOSizeDelta(_dummyRT.sizeDelta, 0.2f).SetDelay(0.01f);
        _activeClone = true;
        Drag_Start(_clone);
        _draggedInfo = draggable;
    }

    public void Drag_Start(RW_Draggable draggable) {
        if (!_canDrag)
            return;
        if (_dragging)
            return;
        _dragged = draggable;        
        _draggedInfo = draggable; // when dragging a clone, need to keep info of the dragged thing.
        _canDrag = false;
        _dragging = true;
        _previousParent = _dragged.GetRectTransform.parent;
        _previousLayout =_dragged.GetRectTransform.parent as RectTransform;
        _prevSiblingIndex = _dragged.GetRectTransform.GetSiblingIndex();
        _initPos = _dragged.GetRectTransform.position;
        _dragged._canBeOverred = false;

        _dragged._image.raycastTarget = false;
        _dragged._image.color = Color.yellow;
        Tween.Bounce(_dragged.GetRectTransform);

        Vector2 inputPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform, Input.mousePosition, _canvas.worldCamera, out inputPos);
        
        _target = null;

        _dragged.GetRectTransform.SetParent(_group);

        // update draggable layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(_previousLayout);
    }

    public void Drag_Update() {
        _dragged.GetRectTransform.position = Vector3.Lerp(_dragged.GetRectTransform.position, _targetPos, _dragSpeed * Time.deltaTime);

        if (Input.GetMouseButtonUp(0))
            Drag_Exit();
    }

    public void Drag_Exit() {

        _dragging = false;
        _dragged._image.raycastTarget = true;

        if (_target != null ) {
            _target.HandleMerge(_draggedInfo);
            _dragged.FadeOut();
            //_dragged.Merge(_target);
        } else {
            Cancel();
        }
        Invoke("ExitDelay", _dragged.MoveDuration+0.01f);

    }
    void ExitDelay() {
        _canDrag = true;
        _activeClone = false;
        _dragged._canBeOverred = true;
        _dragged = null;
    }

    void Cancel() {
        if (!_activeClone) {
            _dragged.ReturnToPos(_initPos);
        } else {
            _dragged.FadeOut();
        }
        Invoke("CancelDelay", _dragged.MoveDuration+0.01f);
    }

    

    void CancelDelay() {
        _dragged.GetRectTransform.SetParent(_previousParent);
        _dragged.GetRectTransform.SetSiblingIndex(_prevSiblingIndex);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_previousLayout);
    }
}
