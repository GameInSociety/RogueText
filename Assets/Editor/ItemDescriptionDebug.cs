using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class ItemDescriptionDebug : EditorWindow {
    GUIStyle style;

    // data
    Vector2 scrollPos = Vector2.zero;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Item Descriptions")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (ItemDescriptionDebug)GetWindow(typeof(ItemDescriptionDebug));
        window.Show();
    }
    private void OnGUI() {

    }

    void UpdateLog(List<ItemDescription> ids) {
        ItemDescription.log = "";
        foreach (var itemDescription in ids) {
            LOG($"\n______________________________________" +
                $"\n[{itemDescription.name}] : {itemDescription.groups.Count}", Color.cyan);
            foreach (var group in itemDescription.groups) {
                LOG($"[{group.key}:{group.itemSlots.Count}]", Color.yellow);

                foreach (var itemSlot in group.itemSlots) {
                    LOG($"[{itemSlot.key}] : {itemSlot.items.Count}", Color.white);
                    string nested_text = "";
                    foreach (var prop in itemSlot.nestedProps)
                        nested_text += $"({prop.GetCurrentDescription()}) ";
                    LOG($"Nested : {nested_text}", Color.white);
                    string describt_text = "";
                    foreach (var prop in itemSlot.describeProps)
                        describt_text += $"({prop.GetCurrentDescription()}) ";
                    LOG($"Describe : {describt_text}", Color.magenta);

                }
            }
        }
    }

    public static void LOG(string message, Color c) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(c)}>";
        string str = $"\n{txt_color}{message}</color>";
        ItemDescription.log += str;
    }

}
