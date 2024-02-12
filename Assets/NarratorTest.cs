using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLMUnity;
using TMPro;

public class NarratorTest : MonoBehaviour
{
    public LLM llm;
    public TextMeshProUGUI uiText;
    public TMP_InputField inputField;
    string currentAnswer = "";

    public void onDescription(string description) {
        uiText.text = description;
    }
    public void onComplete() {
        Debug.Log($"reply completed");
    }

    public void OnType() {
        _ = llm.Chat(inputField.text, onDescription, onComplete);
    }
}
