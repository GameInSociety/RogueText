using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DescriptionText : EditorWindow {

    [MenuItem("Window/Description Test")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (DescriptionText)GetWindow(typeof(DescriptionText));
        window.Show();
    }

    // Start is called before the first frame update
    void OnGUI() {

        if(GUILayout.Button("Refresh")) {
        }

    }
}
    
