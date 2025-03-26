using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class RW_SlidableMenu : MonoBehaviour/*, IBeginDragHandler, IDragHandler*/ {

    /*[SerializeField]
    private RectTransform m_RectTransform;
    private RW_Menu m_Menu;
    public float dragSpeed = 10f;
    private Vector2 m_dragDelta;
    private Vector2 m_TargetPos;

    private void Start() {
        m_Menu = GetComponentInParent<RW_Menu>();
        m_TargetPos = m_RectTransform.position;
    }

    private void Update() {
        m_RectTransform.position = Vector3.Lerp(m_RectTransform.position, m_TargetPos, dragSpeed * Time.deltaTime);
    }

    public void OnBeginDrag(PointerEventData data) {
        Vector2 inputPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)m_Canvas.transform, data.position, m_Canvas.worldCamera, out inputPos);
        m_dragDelta = (Vector2)m_RectTransform.position - (Vector2)m_Canvas.transform.TransformPoint(inputPos);
    }

    public void OnDrag(PointerEventData data) {
        Vector2 inputPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)m_Canvas.transform, data.position, m_Canvas.worldCamera, out inputPos);
        m_TargetPos = m_Canvas.transform.TransformPoint(inputPos + m_dragDelta);
    }*/
}
