using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RW_Menu : Displayable, IBeginDragHandler, IDragHandler {
    public ScrollRect m_ScrollRect;
    private Canvas m_Canvas;
    public float dragSpeed = 10f;
    private Vector2 m_dragDelta;

    private Vector2 m_TargetPos;
    [SerializeField] private Vector2 m_TargetScale;
    private Vector2 m_MaxScale = new Vector2(200f, 400f);

    [SerializeField] private RectTransform m_ContentRT;
    [SerializeField] private RectTransform _targetRectTransform;

    public override void Start() {
        base.Start();
        m_ScrollRect = GetComponentInChildren<ScrollRect>();
        SetPos(_targetRectTransform.position);
    }

    private void Update() {
        _targetRectTransform.position = Vector3.Lerp(_targetRectTransform.position, m_TargetPos, dragSpeed * Time.deltaTime);
        _targetRectTransform.sizeDelta = Vector2.Lerp(_targetRectTransform.sizeDelta, m_TargetScale, dragSpeed * Time.deltaTime);
    }

    public void OnBeginDrag(PointerEventData data) {
        Select();
        Vector2 inputPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Canvas.transform, data.position, Canvas.worldCamera, out inputPos);
        m_dragDelta = (Vector2)_targetRectTransform.position - (Vector2)Canvas.transform.TransformPoint(inputPos);
    }

    public void OnDrag(PointerEventData data) {
        Vector2 inputPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Canvas.transform, data.position, Canvas.worldCamera, out inputPos);
        SetPos(Canvas.transform.TransformPoint(inputPos + m_dragDelta));
    }

    public Canvas Canvas {
        get {
            if ( m_Canvas == null)
               m_Canvas = GetComponent<Canvas>();
            return m_Canvas;
        }
    }

    public void Select() {
        BringForward();
    }

    private void ResetTransform() {
        // pos
        var pos = RW_Draggable.s_LastSelected.GetRectTransform.position;
        SetPosImmediate(pos);

        // scale
        _targetRectTransform.sizeDelta = new Vector2(_targetRectTransform.sizeDelta.x, 0f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_targetRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_ContentRT);
        Invoke("UpdateTransformDelay", 0f);
    }

    void UpdateTransformDelay() {
        var height = Mathf.Clamp(m_ContentRT.rect.height, 0f, m_MaxScale.y);
        var scale = new Vector2(m_MaxScale.x, height);
        SetScale(scale);
    }

    public void BringForward() {
        GetTransform.SetAsLastSibling();
        CanvasSorter.Instance.Sort();
    }

    public void UpdateDisplay() {
        FadeIn();
        m_ScrollRect.verticalNormalizedPosition = 1f;
        BringForward();
        ResetTransform();
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_ContentRT);
    }

    public void SetPos(Vector2 pos) {
        m_TargetPos = pos;
    }

    public void SetPosImmediate(Vector2 pos) {
        SetPos(pos);
        _targetRectTransform.position = m_TargetPos;
    }

    public void SetScale(Vector2 scale) {
        m_TargetScale = new Vector2(Mathf.Clamp(scale.x, 100f, m_MaxScale.y), Mathf.Clamp(scale.y, 0f, m_MaxScale.y));
    }
    public void SetScaleImmediate(Vector2 scale) {
        m_TargetScale = new Vector2(Mathf.Clamp(m_TargetScale.x, 100f, m_MaxScale.y), Mathf.Clamp(m_TargetScale.y, 0f, m_MaxScale.y));
        _targetRectTransform.sizeDelta = scale;
    }

    // MENU
    public virtual void Close() {
        FadeOut();
    }

    public RectTransform GetMovableGroup() {
        return _targetRectTransform;
    }
}
