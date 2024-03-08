using System.Collections.Generic;
using TextSpeech;
using UnityEngine;

public class AudioInteraction : MonoBehaviour {
    public static AudioInteraction Instance;

    private bool speaking = false;

    public float pitch = 1f;
    public float rate = 1f;

    public List<string> phrasesToSpeak = new List<string>();

    private void Awake() {
        Instance = this;
    }

    void Start() {
        Setting("fr-FR");

        SpeechToText.instance.onResultCallback = OnSpeechResult;
        TextToSpeech.instance.onDoneCallback += HandleOnDoneSpeaking;

    }



    #region voice recognition
    public void StartRecording() {
        DisplayRecordFeedback.Instance.uiText.text = "Attente...";

        SpeechToText.instance.StartRecording("Speak any");
    }
    public void StopRecording() {
        SpeechToText.instance.StopRecording();
    }

    private void OnSpeechResult(string str) {
        DisplayRecordFeedback.Instance.uiText.text = str;
    }
    #endregion

    #region text speak
    public void StartSpeaking(string str) {

        if (speaking) {
            phrasesToSpeak.Add(str);
            return;
        }

        speaking = true;

        TextToSpeech.instance.StartSpeak(str);
    }

    void HandleOnDoneSpeaking() {
        speaking = false;

        if (phrasesToSpeak.Count > 0) {
            StartSpeaking(phrasesToSpeak[0]);
            phrasesToSpeak.RemoveAt(0);
        }
    }

    public void ClearSpeaking() {
        phrasesToSpeak.Clear();
        StopSpeaking();
    }

    public void StopSpeaking() {
        TextToSpeech.instance.StopSpeak();
    }
    #endregion

    void Setting(string str) {
        TextToSpeech.instance.Setting(str, pitch, rate);
        SpeechToText.instance.Setting(str);
    }
}
