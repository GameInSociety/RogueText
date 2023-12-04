using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemParserDebug : EditorWindow {
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Item Parser")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (ItemParserDebug)GetWindow(typeof(ItemParserDebug));
        window.Show();
    }

    private void OnGUI() {
        GUILayout.Button("ononon");

        ItemParser previous = ItemParser.GetPrevious;
        string previous_text = previous == null ? "NO PREVIOUS PARSER" : $"PREVIOUS PARSER:";
        GUILayout.Button(previous_text);
        if (previous != null) {
            foreach (var item in previous.mainGroups)
                GUILayout.Label(item.debug_name);
        }

        ItemParser current = ItemParser.GetCurrent;
        string curr_text = current == null ? "NO CURRENT PARSER" : $"CURRENT PARSER:";
        GUILayout.Button(curr_text);
        if (current != null) {
            if ( current.mainGroups.Count == 0) {
                GUILayout.Label("no ITEMs");
            }
            foreach (var item in current.mainGroups) {
                GUILayout.Box(item.debug_name);
            }
        }
    }
}
