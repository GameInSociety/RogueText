#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextParser), true)]
public class TextParserEditor : Editor {
    public override void OnInspectorGUI() {
        var myScript = (TextParser)target;

        if (GUILayout.Button("Download Data")) {
            myScript.DownloadCSVs();
        }

        if (GUILayout.Button("Open Link")) {
            Application.OpenURL(myScript.url);
        }

        _ = DrawDefaultInspector();
    }
}
#endif