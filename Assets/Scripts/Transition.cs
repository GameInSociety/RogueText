using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition : MonoBehaviour {
    public static Transition Instance;

    public GameObject group;

    public CanvasGroup canvasGroup;

    public float duration = 0.5f;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Hide();
    }

    void Show() {
        group.SetActive(true);
    }

    void Hide() {
        group.SetActive(false);
    }

    public void FadeIn() {
        Show();

        _ = canvasGroup.DOFade(1f, duration);
    }

    public void FadeOut() {
        Invoke("Hide", duration);

        _ = canvasGroup.DOFade(0f, duration);
    }
}
