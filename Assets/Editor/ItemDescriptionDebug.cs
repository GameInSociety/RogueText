using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class ItemDescriptionDebug : EditorWindow {
    GUIStyle style;

    // data
    Vector2 scrollPos = Vector2.zero;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Item Descriptions")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (ItemDescriptionDebug)GetWindow(typeof(ItemDescriptionDebug));
        window.Show();
    }
    private void OnGUI() {

    }


    public static void LOG(string message, Color c) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(c)}>";
        string str = $"\n{txt_color}{message}</color>";
        DescriptionGroup.log += str;
    }

}
