using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class EditorTools : EditorWindow {
    public bool mapVisible = false;

    bool showStates = false;
    bool showTime = false;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Tools")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (EditorTools)EditorWindow.GetWindow(typeof(EditorTools));
        window.Show();
    }

    void OnGUI() {
        if (Player.Instance != null)
            GUILayout.Label("Player orientation : " + Player.Instance.currentCarnidal, EditorStyles.boldLabel);

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        DrawMap();

        GUILayout.Label("Feedbacks", EditorStyles.boldLabel);

        DrawTime();

        DrawStates();

        GUILayout.Label("Time", EditorStyles.boldLabel);

        if (GUILayout.Button("Wait 1 hour")) {
            TimeManager.Instance.Wait(1);
        }

        if (GUILayout.Button("Wait 10 hours")) {
            TimeManager.Instance.Wait(10);
        }
    }

    void DrawTime() {
        showTime = EditorGUILayout.BeginToggleGroup("Show Time", showTime);

        if (!showTime) {
            EditorGUILayout.EndToggleGroup();
            return;
        }

        GUILayout.Label("Moves to next hours : " + TimeManager.Instance.currentMove + "/" + TimeManager.Instance.movesToNextHour, EditorStyles.label);
        GUILayout.Label("Heure : " + TimeManager.Instance.timeOfDay, EditorStyles.label);
        GUILayout.Label("Partie de la journée : " + TimeManager.Instance.currentPartOfDay, EditorStyles.label);

        if (!TimeManager.Instance.raining) {
            GUILayout.Label("Il va pleuvoir dans " + TimeManager.Instance.hoursLeftToRain + " heures", EditorStyles.label);
        } else {
            GUILayout.Label("Il va s'arrêter de pleuvoir dans " + TimeManager.Instance.hoursLeftToRain + " heures", EditorStyles.label);
        }

        GUILayout.Label("Jours passés : " + TimeManager.Instance.daysPasted, EditorStyles.label);

        EditorGUILayout.EndToggleGroup();
    }

    void DrawStates() {
        showStates = EditorGUILayout.BeginToggleGroup("Show States", showStates);

        if (!showStates) {
            EditorGUILayout.EndToggleGroup();
            return;
        }

        var gUIStyle = new GUIStyle();
        gUIStyle.richText = true;

        foreach (var state in ConditionManager.GetInstance().conditions) {
            GUILayout.Label(state.GetDebugText(), gUIStyle);
        }

        EditorGUILayout.EndToggleGroup();
    }

    void DrawMap() {
        if (GameObject.Find("Map Texture").GetComponent<Image>().enabled) {
            mapVisible = true;
        } else {
            mapVisible = false;
        }

        if (mapVisible) {
            if (GUILayout.Button("Hide Map")) {
                mapVisible = false;
                GameObject.Find("Map Texture").GetComponent<Image>().enabled = false;
                GameObject.Find("Text Background").GetComponent<Image>().enabled = true;
            }
        } else {
            if (GUILayout.Button("Show Map")) {
                mapVisible = true;
                GameObject.Find("Map Texture").GetComponent<Image>().enabled = true;
                GameObject.Find("Text Background").GetComponent<Image>().enabled = false;
            }
        }
    }
}
