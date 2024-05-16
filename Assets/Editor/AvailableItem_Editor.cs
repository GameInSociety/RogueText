using System.Collections.Generic;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;

public class AvailableItem_Editor : EditorWindow {

    GUIStyle infoStyle;
    GUIStyle itemStyle;
    GUIStyle catStyle;

    List<int> selectedItems = new List<int>();
    public List<string> selectedCats = new List<string>();

    // data
    Vector2 scrollPos = Vector2.zero;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Available Items")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (AvailableItem_Editor)GetWindow(typeof(AvailableItem_Editor));
        window.Show();
    }
    private void OnGUI() {

        infoStyle = new GUIStyle("label");
        infoStyle.richText = true;
        itemStyle = new GUIStyle("button");
        itemStyle.alignment = TextAnchor.UpperLeft;
        itemStyle.richText = true;
        catStyle = new GUIStyle(itemStyle);
        catStyle.normal.textColor = Color.cyan;
        catStyle.fontSize = 17;

        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var cat in AvailableItems.categories) {
            GUILayout.Space(30);

            if (GUILayout.Button(cat.name, catStyle)) {
                if (selectedCats.Contains(cat.name)) {
                    selectedCats.Remove(cat.name);
                } else {
                    selectedCats.Add(cat.name);

                }
            }

            GUILayout.Space(10);



            if (!selectedCats.Contains(cat.name))
                continue;

            foreach (var item in cat.items) {
                string str = "";
                var parent = item.HasParent() ? item.GetParent() : null;
                while (parent != null) {
                    str += "    ";
                    parent = parent.HasParent() ? parent.GetParent() : null;
                }
                str += $"{item.debug_name}";
                
                if (GUILayout.Button(str, itemStyle)) {
                    if (selectedItems.Contains(item.debug_Id)) {
                        selectedItems.Remove(item.debug_Id);
                    } else {
                        selectedItems.Add(item.debug_Id);
                    }
                }

                if (selectedItems.Contains(item.debug_Id)) {
                    string s = "";
                    foreach (var prop in item.props) {
                        string color = prop.enabled ? "cyan" : "blue";
                        if (GUILayout.Button($"<color={color}>{prop.name}</color> {(prop.HasPart("value") ? $"<color=magenta>{prop.GetPart("value").content}</color>" : "")}", infoStyle)) {
                            prop.debug_selected = !prop.debug_selected;
                        }
                        if (prop.debug_selected) {
                            foreach (var part in prop.parts) {
                                GUILayout.Label($"  <color=yellow>[{part.key}]</color> {part.content}", infoStyle);
                            }
                        } 
                    }
                }
            }
        }
        
        
        //GUILayout.Label(AvailableItems.log, style);
        
        
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
}
