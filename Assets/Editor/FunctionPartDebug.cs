using UnityEditor;
using UnityEngine;

public class FunctionPartDebug : EditorWindow {
    GUIStyle style;

    // data
    Vector2 scrollPos = Vector2.zero;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Function Part")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (FunctionPartDebug)GetWindow(typeof(FunctionPartDebug));
        window.Show();
    }
    private void OnGUI() {

        if (GUILayout.Button("CLEAR LOG"))
            Function.log = "";

        style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.richText = true;
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label(Function.log, style);
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

}
