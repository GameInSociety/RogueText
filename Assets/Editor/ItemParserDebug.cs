using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemParserDebug : EditorWindow
{
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Item Parser")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ItemParserDebug window = (ItemParserDebug)GetWindow(typeof(ItemParserDebug));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Button("Input");
        foreach (var item in ItemParser.debug_inputItems)
        {
            GUILayout.Label(item.debug_name + "/" + item.GetHashCode(), EditorStyles.boldLabel);
        }
        GUILayout.Button("Output");
        foreach (var item in ItemParser.debug_outputItems)
        {
            GUILayout.Label(item.debug_name + "/" + item.GetHashCode(), EditorStyles.boldLabel);
        }
        GUILayout.Button("History");
        foreach (var history in ItemParser.history)
        {
            GUILayout.Label(history.item.debug_name , EditorStyles.boldLabel);
        }
        GUILayout.Button("Last Search");
        foreach (var history in ItemParser.history)
        {
            GUILayout.Label(history.item.debug_name , EditorStyles.boldLabel);
        }
    }
}
