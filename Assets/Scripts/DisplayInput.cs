using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInput : MonoBehaviour {

    public static DisplayInput Instance;

    public InputField inputField;

    void Awake() {
        Instance = this;
    }

    void Start() {
        Focus();
    }

    public void Show() {
        gameObject.SetActive(true);

    }
    public void Hide() {
        gameObject.SetActive(false);
    }

    public void OnEndEdit() {
        string text = inputField.text;
        if (string.IsNullOrEmpty(text))
            return;
        ItemParser.GetCurrent.Parse(text);
        Clear();
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
        inputField.text = "";
        Focus();
    }
    void Focus() {
        inputField.Select();
        inputField.ActivateInputField();
    }
}
