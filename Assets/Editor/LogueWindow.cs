using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LogueWindow : EditorWindow
{
    [MenuItem("Window/Logue")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        LogueWindow window = (LogueWindow)GetWindow(typeof(LogueWindow));
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("CLEAR"))
        {
            Logue.Clear();
        }

        foreach (var entry in Logue.entries)
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.richText = true;
            titleStyle.normal.textColor = entry.color;
            titleStyle.fontSize = 15;

            GUILayout.Label(entry.title, titleStyle);

            GUIStyle contentStyle = new GUIStyle();
            contentStyle.richText = true;
            contentStyle.normal.textColor = Color.Lerp(entry.color, Color.black, 0.2f);

            GUILayout.Label(entry.content, contentStyle);


        }


    }
}
