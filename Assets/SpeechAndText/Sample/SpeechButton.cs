using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpeechButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public SampleSpeechToText sample;
    public GameObject effect;
    public float speedEffect = 1;
    public float scaleEffect = 1.2f;

    float speed;
    float scale = 1;

    public Text uiText;

    void Start() {
        effect.SetActive(false);
        speed = speedEffect;
    }
    void Update() {
        if (effect.activeSelf) {
            scale += Time.deltaTime * speed;
            if (scale > scaleEffect) {
                speed = -speedEffect;
            }
            if (scale < scaleEffect - 0.1f) {
                speed = speedEffect;
            }
            effect.transform.localScale = new Vector3(scale, scale, 1);
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        uiText.text = "Pressed Merde";

        Debug.LogError("On Pointer Down !");

        effect.SetActive(true);
        scale = 1;
        sample.StartRecording();

        Debug.LogError("Starting to record!");

        uiText.text = "Pressed Merde 2";

    }

    public void OnPointerUp(PointerEventData eventData) {
        uiText.text = "Pressed UP Merde";

        effect.SetActive(false);
        sample.StopRecording();
    }
}
