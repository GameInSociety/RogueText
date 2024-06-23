using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDescription : MonoBehaviour {

    public static DisplayDescription Instance;
    public RectTransform verticalLayoutGroup;

    public ScrollRect scrollRect;

    public Text uiText;
    string text_archive;

    public int letterRate = 1;

    int typeIndex = 0;
    float typeTimer = 0f;
    public float rate = 0.2f;
    bool typing = false;
    Color initColor;
    public List<Chunk> oldChunks = new List<Chunk>();
    public List<Chunk> newChunks = new List<Chunk>();

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
        public string GetFullText() {
            return $"<color=#{htlm}>{targetText}</color>";
        }
        public string GetCurrentText() {
            return $"<color=#{htlm}>{currText}</color>";
        }
    }

    void Awake() {
        Instance = this;
    }

    public void Init() {
        initColor = uiText.color;
        uiText.text = "";
        text_archive = "";
        ClearDescription();
    }

    public delegate void OnPressReturn();
    public OnPressReturn onPressReturn;

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

    bool lineBreak = false;
    void Type() {

        var currChunk = newChunks[0];

        if (typeIndex >= currChunk.targetText.Length){
            // finish chunk
            text_archive += currChunk.GetFullText();
            newChunks.RemoveAt(0);
            typeIndex = 0;
        }
        if ( newChunks.Count == 0) {
            Typing_Exit();
            return;
        }

        currChunk.currText = currChunk.targetText.Remove(typeIndex)+"■";
        uiText.text = $"{text_archive}{currChunk.GetCurrentText()}";
        ++typeIndex;
    }

    public delegate void OnTypeExit();
    public OnTypeExit onTypeExit;

    void Typing_Exit(){

        foreach (var ch in newChunks) {
            text_archive += ch.GetFullText();
        }
        newChunks.Clear();

        uiText.text = text_archive;
        typing = false;
        newText = false;

        if (onTypeExit != null) {
            onTypeExit();
        }
    }

    public void ClearDescription() {
        newText = false;
        typing = false;
        text_archive = "";
        wholeText = "";
        text = "";
        typeIndex = 0;
        uiText.text = "";
        newChunks.Clear();
    }


    public void AddToDescription(string str, Color c) {
        AddToDescription(new Chunk(str, c));
    }
    public void AddToDescription(string str) {
        AddToDescription(new Chunk(str, initColor));
    }
    private void AddToDescription(Chunk chunk) {

        /*string[] lines = chunk.targetText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        chunk.targetText = "";

        // Loop through each line and capitalize the first character
        for (int i = 0; i < lines.Length; i++) {
            if (lines[i].Length > 0) {
                // Capitalize the first character and concatenate with the rest of the string
                chunk.targetText += char.ToUpper(lines[i][0]) + lines[i].Substring(1) + '\n';
            }
        }*/

        // Join the lines back into a single string with line breaks
        newChunks.Add(chunk);
        newText = true;
    }



}
