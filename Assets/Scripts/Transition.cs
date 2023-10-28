using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Transition : MonoBehaviour {
    public static Transition Instance;

    public GameObject group;

    public CanvasGroup canvasGroup;

    public float duration = 0.5f;
    float timer = 0.0f;
    public bool visible = false;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Hide();
    }

    private void Update() {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / duration);

        if ( visible){
            timer += Time.deltaTime;
        } else {
            timer -= Time.deltaTime;
        }
    }

    void Show() {
        group.SetActive(true);
    }

    void Hide() {
        group.SetActive(false);
    }

    public void FadeIn() {
        visible = true;
    }

    public void FadeOut() {
        visible = false;
    }
}
