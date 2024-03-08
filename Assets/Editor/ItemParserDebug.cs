using UnityEditor;
using UnityEngine;

public class ItemParserDebug : EditorWindow {
    GUIStyle style;

    // data
    Verb verb;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Item Parser")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (ItemParserDebug)GetWindow(typeof(ItemParserDebug));
        window.Show();
    }
    private void OnGUI() {
        style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.richText = true;
        GUILayout.Label("previous");
        DisplayParser(ItemParser.GetPrevious);
        GUILayout.Label("current");
        DisplayParser(ItemParser.GetCurrent);
    }

    void DisplayParser(ItemParser parser) {

        if (parser == null ) {
            GUILayout.Label($"no parser", style);
            return;
        }
        GUILayout.Label($"log: {parser.log}", style);
    }

}
