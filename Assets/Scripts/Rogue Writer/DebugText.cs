using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    public static DebugText Instance;

    public TextMeshProUGUI uiText;

    public string targetText = "";

    public RW_ItemDataButton[] rW_ItemDataButtons;

    public RectTransform parent;
    public int index;

    public Vector2 start;
    public Vector2 decal;

    private void Awake() {
        Instance = this;
    }

    private void LateUpdate() {
        return;
        rW_ItemDataButtons = parent.GetComponentsInChildren<RW_ItemDataButton>();
        float bW = parent.GetComponent<RectTransform>().rect.width;

        float x = start.x;
        float y = start.y;

        for (int i = 0; i < rW_ItemDataButtons.Length; i++) {
            float nX = x + rW_ItemDataButtons[i].GetRectTransform.rect.width + decal.x;
            if (nX >= bW - (start.x-decal.x)) {
                x = start.x;
                y += decal.y;
            }
            rW_ItemDataButtons[i].GetRectTransform.anchoredPosition = new Vector2(x, y);
            x = nX;
        }
    }

}
