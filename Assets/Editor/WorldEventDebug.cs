using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WorldEventDebug : EditorWindow {

    GUIStyle worldEvent_Style;
    GUIStyle item_Style;
    GUIStyle prop_Style;
    GUIStyle line_Style;

    [MenuItem("Window/World Events")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (WorldEventDebug)GetWindow(typeof(WorldEventDebug));
        window.Show();
    }

    // Start is called before the first frame update
    void OnGUI() {
        worldEvent_Style = new GUIStyle("button");
        worldEvent_Style.alignment = TextAnchor.UpperLeft;
        worldEvent_Style.fontSize = 12;
        worldEvent_Style.normal.textColor = Color.cyan;
        worldEvent_Style.richText = true;

        item_Style = new GUIStyle(worldEvent_Style);
        item_Style.normal.textColor = Color.yellow;

        prop_Style= new GUIStyle(worldEvent_Style);
        prop_Style.normal.textColor = Color.white;

        line_Style = new GUIStyle("label");
        line_Style.normal.textColor = Color.white;

        foreach (var worldEvent in WorldEvent.worldEvents) {
            if (GUILayout.Button(worldEvent.name, worldEvent_Style)) {
                worldEvent.debug_selected = !worldEvent.debug_selected;
            }

            if (worldEvent.debug_selected) {
                foreach (var itemGroup in worldEvent.itemGroups) {

                    if (GUILayout.Button(itemGroup.item.DebugName, item_Style))
                        itemGroup.debug_selected = !itemGroup.debug_selected;

                    if (itemGroup.debug_selected) {
                        foreach (var propGroup in itemGroup.propGroups) {
                            prop_Style.normal.textColor = propGroup.prop.enabled ? Color.green : Color.red;
                            if (GUILayout.Button(propGroup.prop.name, prop_Style)) {
                                propGroup.debug_selected = !propGroup.debug_selected;
                            }
                            if (propGroup.debug_selected) {
                                GUILayout.Label(propGroup.sequence, line_Style);
                            }
                        }
                    }
                }
            }


        }
    }
}
