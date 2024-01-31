using Newtonsoft.Json;
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

    int typeIndex = 0;
    float typeTimer = 0f;
    public float rate = 0.2f;
    bool typing = false;
    Color initColor;
    public List<Chunk> chunks = new List<Chunk>();

    float timer = 0f;

    bool newText = false;
    public string text;
    public string wholeText;

    [System.Serializable]
    public class Chunk {
        public Chunk(string targetText, Color color) {
            currText = "";
            this.targetText = targetText;
            this.color = color;
            htlm = ColorUtility.ToHtmlStringRGBA(color);
        }
        public Color color;
        public string targetText;
        public string currText;
        string htlm = "";
        public string GetText() {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{currText}</color>";
        }
    }

    void Awake() {
        Instance = this;
    }

    void Start() {
        initColor = uiText.color;
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
        chunkIndex = 0;
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

    public int chunkIndex;
    void Type() {

        var currChunk = chunks[chunkIndex];

        if (typeIndex >= currChunk.targetText.Length){
            currChunk.currText = currChunk.targetText;
            text += currChunk.GetText();
            typeIndex = 0;
            ++chunkIndex;
        }
        if ( chunkIndex == chunks.Count) {
            Typing_Exit();
            return;
        }

        currChunk.currText = currChunk.targetText.Remove(typeIndex);
        uiText.text = text+currChunk.GetText()+"■";

        ++typeIndex;
    }

    void Typing_Exit(){
        text = "";
        foreach (var chunk in chunks) {
            chunk.currText = chunk.targetText;
            text += chunk.GetText();
        }
        wholeText += text;
        uiText.text = $"{wholeText}";
        typing = false;
        DisplayInput.Instance.Show();
        newText = false;
        chunks.Clear();
    }

    public void ClearDescription() {
        newText = false;
        typing = false;
        uiText_Old.text += wholeText;
        wholeText = "";
        text = "";
        typeIndex = 0;
        uiText.text = "";
        chunks.Clear();
    }


    public void AddToDescription(string str, Color c) {
        AddToDescription(new Chunk(str, c));
    }
    public void AddToDescription(string str) {
        AddToDescription(new Chunk(str, initColor));
    }
    private void AddToDescription(Chunk chunk) {
        chunk.targetText = TextUtils.FirstLetterCap(chunk.targetText);
        chunks.Add(chunk);
        newText = true;
    }



}
