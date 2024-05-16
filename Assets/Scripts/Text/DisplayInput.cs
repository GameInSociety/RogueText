using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInput : MonoBehaviour {

    public static DisplayInput Instance;

    public InputField inputField;

    public bool canInteract = false;

    void Awake() {
        Instance = this;
    }

    public void DisplayFeedback(string str) {
        var placeholderText = (Text)inputField.placeholder;
        placeholderText.text = str;
    }

    public void Enable() {
        inputField.enabled = true;
        Focus();
    }
    public void Disable() {
        Clear();
        inputField.enabled = false;
    }

    public void OnEndEdit() {
        string text = inputField.text;
        if (string.IsNullOrEmpty(text))
            return;
        ItemParser.Clear();
        ItemParser.Instance.Parse(text);
    }

    public void EndInput() {
        inputField.interactable = false;
        inputField.enabled = false;
        inputField.text = "";
    }

    public void OnValueChanged() {
        Sound.Instance.PlayRandomTypeSound();
    }

    void Clear() {
        var placeholderText = (Text)inputField.placeholder;
        placeholderText.text = "";
        inputField.text = ""; 
    }
    void Focus() {
        var placeholderText = (Text)inputField.placeholder;
        placeholderText.text = "What do you want to do ?";
        inputField.Select();
        inputField.ActivateInputField();
    }
}
