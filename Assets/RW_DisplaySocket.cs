using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RW_DisplaySocket : Displayable
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _uiText;
    private int _selectedFontScale = 21;

    public string originalText;

    public void Display(Socket socket) {
        Debug.Log($"socket name : {socket.Content}");
        _uiText.text = socket.Content;
        originalText = socket.Content;
        _image.color = EngineManager.Instance.GetVisual(socket.Type).color;
    }

    void Update() {
        int charIndex = TMP_TextUtilities.FindIntersectingCharacter(_uiText, Input.mousePosition, null, true);

        if (charIndex != -1) {
            UpdateTextWithBold(charIndex);

            if (Input.GetMouseButtonDown(0)) {
                AddSocketPart(charIndex);
            }

        } else {
            ResetText();
        }
    }

    void UpdateTextWithBold(int index) {
        string newText = "";
        for (int i = 0; i < originalText.Length; i++) {
            if (i == index)
                newText += $"<size={_selectedFontScale}>" + originalText[i] + "</size>";
            else
                newText += originalText[i];
        }

        _uiText.text = newText;
    }

    void AddSocketPart(int charIndex) {
        
    }

    void ResetText() {
        _uiText.text = originalText;
    }
}
