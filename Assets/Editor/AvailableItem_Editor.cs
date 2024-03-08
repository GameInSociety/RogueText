using UnityEditor;
using UnityEngine;

public class AvailableItem_Editor : EditorWindow {

    GUIStyle style;

    // data
    Vector2 scrollPos = Vector2.zero;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Available Items")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (AvailableItem_Editor)GetWindow(typeof(AvailableItem_Editor));
        window.Show();
    }
    private void OnGUI() {

        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.richText = true;
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label(AvailableItems.log, style);
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
}
