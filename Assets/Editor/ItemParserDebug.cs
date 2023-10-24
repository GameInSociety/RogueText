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

    void OnGUI() {
        _ = GUILayout.Button("History");
        foreach (var history in ItemParser.GetCurrent.itemHistory) {
            GUILayout.Label(history.item.debug_name, EditorStyles.boldLabel);
        }
        _ = GUILayout.Button("Last Search");
        foreach (var history in ItemParser.GetCurrent.itemHistory){
            GUILayout.Label(history.item.debug_name, EditorStyles.boldLabel);
        }
    }
}
