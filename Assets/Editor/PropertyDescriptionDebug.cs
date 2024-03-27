using UnityEditor;
using UnityEngine;

public class PropertyDescriptionDebug : EditorWindow {
    GUIStyle style;

    // data
    Verb verb;
    Vector2 scrollPos = Vector2.zero;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Property Description")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (PropertyDescriptionDebug)GetWindow(typeof(PropertyDescriptionDebug));
        window.Show();
    }
    private void OnGUI() {
        style = new GUIStyle();
        style.alignment = TextAnchor.MiddleLeft;
        style.richText = true;
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label(PropertyDescription.log, style);
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

}