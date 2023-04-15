using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class EditorTools : EditorWindow
{
    public bool mapVisible = false;

    bool showStates = false;
    bool showTime = false;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Tools")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        EditorTools window = (EditorTools)EditorWindow.GetWindow(typeof(EditorTools));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        DrawMap();

        GUILayout.Label("Feedbacks", EditorStyles.boldLabel);

        DrawTime();

        DrawStates();
    }

    void DrawTime()
    {
        showTime = EditorGUILayout.BeginToggleGroup("Show Time", showTime);

        if (!showTime)
        {
            EditorGUILayout.EndToggleGroup();
            return;
        }

        GUILayout.Label("Moves to next hours : " + TimeManager.GetInstance().currentMove + "/" + TimeManager.GetInstance().movesToNextHour, EditorStyles.label);
        GUILayout.Label("Heure : " + TimeManager.GetInstance().timeOfDay, EditorStyles.label);
        GUILayout.Label("Partie de la journée : " + TimeManager.GetInstance().currentPartOfDay, EditorStyles.label);

        if (!TimeManager.GetInstance().raining)
        {
            GUILayout.Label("Il va pleuvoir dans " + TimeManager.GetInstance().hoursLeftToRain + " heures", EditorStyles.label);
        }
        else
        {
            GUILayout.Label("Il va s'arrêter de pleuvoir dans " + TimeManager.GetInstance().hoursLeftToRain + " heures", EditorStyles.label);
        }

        GUILayout.Label("Jours passés : " + TimeManager.GetInstance().daysPasted, EditorStyles.label    );

        EditorGUILayout.EndToggleGroup();
    }

    void DrawStates()
    {
        showStates = EditorGUILayout.BeginToggleGroup("Show States", showStates);

        if (!showStates)
        {
            EditorGUILayout.EndToggleGroup();
            return;
        }

        GUIStyle gUIStyle = new GUIStyle();
        gUIStyle.richText = true;

        foreach (var state in ConditionManager.GetInstance().conditions)
        {
            GUILayout.Label(state.GetDebugText(), gUIStyle);
        }

        EditorGUILayout.EndToggleGroup();
    }

    void DrawMap()
    {
        if (mapVisible)
        {
            if (GUILayout.Button("Hide Map"))
            {
                mapVisible = false;
                GameObject.Find("Map Texture").GetComponent<Image>().enabled = false;
                GameObject.Find("Text Background").GetComponent<Image>().enabled = true;
            }
        }
        else
        {
            if (GUILayout.Button("Show Map"))
            {
                mapVisible = true;
                GameObject.Find("Map Texture").GetComponent<Image>().enabled = true;
                GameObject.Find("Text Background").GetComponent<Image>().enabled = false;
            }
        }
    }
}
