using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Windows;

public class LogueWindow : EditorWindow {

    GUIStyle style;

    [MenuItem("Window/Logue")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (LogueWindow)GetWindow(typeof(LogueWindow));
        window.Show();
    }

    void OnGUI() {
        style = new GUIStyle(GUIStyle.none);
        style.richText = true;
        style.normal.textColor = Color.white;
        GUILayout.Label("TILE INFO");
        if (Tile.GetCurrent == null) {
            GUILayout.Button("no tile");
            return;
        }
        foreach (var item in Tile.GetCurrent.GetChildItems()) {
            GUILayout.Space(15);
            DisplayItem(item);
        }
    }

    void DisplayParser(ItemParser parser) {
        
        GUILayout.Space(20);
        if (parser == null) {
            GUILayout.Button("no parser");
        } else {
        }
    }

    void HandleInput(string text) {
        var expressions = new List<string>() {
            "in",
            "from"
        };

        var display = text;
        foreach (var ex in expressions) {
            var bound = @$"\b{ex}\b";
            if (Regex.IsMatch(text, bound)) {
                var index = text.IndexOf(ex);
                display = display.Insert(index+ex.Length, "\n");
                display = display.Insert(index, "\n");
                /*display = display.Insert(index + ex.Length + 1, "</color>");
                display = display.Insert(index, "<color=magenta>");*/
            }
        }
        GUIStyle style = GUIStyle.none;
        style.richText = true;
        GUILayout.Button(display, style);
    }

    void DisplayItem(Item item) {
        GUILayout.Space(7);
        var newStyle = new GUIStyle(style);
        newStyle.fontSize = 10;
        var itemStyle = new GUIStyle(style);
        itemStyle.fontSize = 20;
        GUILayout.Button($"<color=cyan>{item.debug_name}</color>", itemStyle);
        foreach (var prop in item.props) {
            string propName = $"<color=yellow>{prop.name}</color>";
            GUILayout.Label(propName, style);
            string str = "";
            foreach (var part in prop.parts)
                str += $"<color=cyan>{part.key}</color>:{part.content}\n";
            GUILayout.Label(str, newStyle);
        }
        if (!item.HasChildItems())
            return;
        foreach (var it in item.GetChildItems()) {
            DisplayItem(it);
        }
    }
}
