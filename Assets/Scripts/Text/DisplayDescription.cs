using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDescription : MonoBehaviour {

    public static DisplayDescription Instance;
    public RectTransform verticalLayoutGroup;

    public ScrollRect scrollRect;

    public Text uiText;
    public Text uiText_Old;

    public int letterRate = 1;

    public string text_current;
    public string text_target;
    int typeIndex = 0;
    float typeTimer = 0f;
    public float rate = 0.2f;
    bool typing = false;

    float timer = 0f;

    bool newText = false;

    void Awake() {
        Instance = this;
    }

    void Start() {
        uiText.text = "";
        uiText_Old.text = "";
        ClearDescription();
    }

    private void Update() {
        if ( typing ) {
            Typing_Update();
        } else if (newText){
            Typing_Start();
        }
    }

    void Typing_Start(){
        timer = 0f;
        typing = true;
        DisplayInput.Instance.Hide();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void Typing_Update(){

        if ( Input.GetKeyDown(KeyCode.Return) && timer > 0F){
            Typing_Exit();
            return;
        }

        if (typeTimer >= rate) {
            typeTimer = 0f;
            Type();
        }
        typeTimer += Time.deltaTime;
        timer += Time.deltaTime;
    }

    void Type() {

        if (typeIndex >= text_target.Length ){
            Typing_Exit();
            return;
        }

        text_current = text_current.Insert(typeIndex, text_target[typeIndex].ToString() );
        uiText.text = text_current + "■";
        //

        ++typeIndex;
    }

    void Typing_Exit(){
        typing = false;
        DisplayInput.Instance.Show();
        text_current = text_target;
        typeIndex = text_target.Length;
        uiText.text = text_target;
        newText = false;
    }

    public void ClearDescription() {
        newText = false;
        typing = false;
        uiText_Old.text += uiText.text;
        typeIndex = 0;
        text_target = "";
        text_current = "";
        uiText.text = "";

    }


    public void AddToDescription(string str) {
        // majuscule
        str = TextUtils.FirstLetterCap(str);
        text_target += str;
        newText = true;
    }



}
