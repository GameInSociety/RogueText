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
}
