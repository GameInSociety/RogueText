
using UnityEngine;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour, IDragHandler {
    public void OnDrag(PointerEventData eventData) {
        Debug.Log($"bite");
    }
}
