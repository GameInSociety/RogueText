using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    public delegate void OnTouchUp();
    public static OnTouchUp onTouchUp;

    public delegate void OnTouchDown();
    public static OnTouchDown onTouchDown;

    public GameObject audioGroup;
    public GameObject writeGroup;

    void Start() {
        if (Application.isMobilePlatform) {
            audioGroup.SetActive(true);
            writeGroup.SetActive(false);
        } else {
            audioGroup.SetActive(false);
            writeGroup.SetActive(true);
        }

    }

    private void Update() {
        if (Application.isMobilePlatform) {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
                Debug.Log("invoking down");
                InvokeOnTouchDown();
            }
        } else {
            if (Input.GetMouseButtonDown(0)) {
                InvokeOnTouchDown();
            }
        }

        if (Application.isMobilePlatform) {
            if (Input.touchCount > 0
                && (Input.GetTouch(0).phase == TouchPhase.Canceled
                || Input.GetTouch(0).phase == TouchPhase.Ended)) {
                Debug.Log("invoking up");
                InvokeOnTouchUp();
            }
        } else {
            if (Input.GetMouseButtonUp(0)) {
                InvokeOnTouchUp();
            }
        }

    }

    public void InvokeOnTouchDown() {
        if (onTouchDown != null)
            onTouchDown();
    }

    public void InvokeOnTouchUp() {
        if (onTouchUp != null)
            onTouchUp();
    }
}
