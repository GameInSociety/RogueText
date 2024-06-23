using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FunctionPartDebug : EditorWindow {
    GUIStyle worldAction_Style;
    GUIStyle line_Style;
    GUIStyle part_Style;

    // data
    Vector2 scrollPos = Vector2.zero;
    Vector2 currOffset = Vector2.zero;

    // Add menu named "My Window" to the Window menu
    private void OnGUI() {

        if (WorldActionManager.Instance == null)
            return;


        foreach (var worldAction in WorldAction.debug_list) {
        }
    }

    void DisplayWorldAction(WorldAction worldAction) {
        string name = $" [{worldAction.TargetItem().DebugName}] ({worldAction.source})";
        if (worldAction.debug_selected) {
            foreach (var line in worldAction.lines) {
                if (GUILayout.Button(line.content, line_Style))
                    line.debug_selected = !line.debug_selected;

                if (line.debug_selected) {
                    foreach (var part in line.parts) {
                        GUILayout.Label($"<color=cyan>{part.output}</color>", part_Style);
                    }
                }
            }

           /* if (worldAction.nestedActions.Count > 0) {
                foreach (var nestedAction in worldAction.nestedActions) {
                    DisplayWorldAction(nestedAction);
                }
            }*/
        }

        
    }
    /*private void OnGUI() {

        if (GUILayout.Button("CLEAR LOG"))
            Function.log = "";

        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.richText = true;
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label(Function.log, style);
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }*/

}
