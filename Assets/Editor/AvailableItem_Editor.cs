using UnityEditor;
using UnityEngine;

public class AvailableItem_Editor : EditorWindow {

    [MenuItem("Window/Availble Items")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (EditorTools)EditorWindow.GetWindow(typeof(AvailableItem_Editor));
        window.Show();
    }

    void OnGUI() {
        for (int i = 0; i < AvailableItems.currItems.Count; i++) {
            var item = AvailableItems.currItems[i];
            GUILayout.Label(item.debug_name, EditorStyles.boldLabel);
        }
    }
}
