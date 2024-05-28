using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_AvailableItems : MonoBehaviour
{
    public static Debug_AvailableItems Instance;
    public float decal = 30f;
    public int displayIndex = 0;

    public Transform parent;

    private void Awake() {
        Instance = this;
    }

    public void Display() {

    }

    public void Show() {

    }
}
