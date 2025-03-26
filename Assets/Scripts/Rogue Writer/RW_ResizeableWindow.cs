using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class RW_ResizeableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private float m_colorSpeed = 1f;

    private UnityEngine.UI.Image m_image;
    private RW_Menu m_menu;
    private Canvas m_Canvas;
    public float dragSpeed = 10f;
    
    // scale
    private Vector2 m_StartPos;
    private Vector2 m_StartScale;

    private bool m_Dragging;
    private bool m_over = false;

    public Vector2 _pivot;

    private void Start() {
        m_menu = GetComponentInParent<RW_Menu>();
        m_Canvas = m_menu.Canvas;
        m_image = GetComponent<UnityEngine.UI.Image>();
        m_image.color = Color.clear;
    }

    private void Update() {
        m_image.color = Color.Lerp(m_image.color, m_over ? Color.white : Color.clear, m_colorSpeed * Time.deltaTime);

    }

    void Drag_Update() {
        
    }

    public void OnBeginDrag(PointerEventData data) {

        Vector2 inputPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)m_Canvas.transform, data.position, m_Canvas.worldCamera, out inputPos);

        m_StartPos = (Vector2)m_Canvas.transform.TransformPoint(inputPos);
        m_StartScale = m_menu.GetMovableGroup().sizeDelta;
        m_Dragging = true;
        m_menu.Select();
    } 

    public void OnDrag(PointerEventData data) {
        Vector2 inputPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)m_Canvas.transform, data.position, m_Canvas.worldCamera, out inputPos);
        var dir = (Vector2)m_Canvas.transform.TransformPoint(inputPos - m_StartPos);
        dir.y = -dir.y;
        m_menu.SetScale(m_StartScale + dir);
    }

    public void OnEndDrag(PointerEventData data) {
        m_Dragging = false;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        m_over = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        m_over = false;
    }
}