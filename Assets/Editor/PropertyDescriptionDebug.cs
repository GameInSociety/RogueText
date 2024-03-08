using UnityEditor;
using UnityEngine;

public class PropertyDescriptionDebug : EditorWindow {
    GUIStyle style;

    // data
    Verb verb;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Property Description")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (PropertyDescriptionDebug)GetWindow(typeof(PropertyDescriptionDebug));
        window.Show();
    }
    private void OnGUI() {
        style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.richText = true;
        GUILayout.Label(PropertyDescription.log, style);
    }

}