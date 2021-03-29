using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class EditorTools : EditorWindow
{
    public bool mapVisible = false;

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
        if (mapVisible)
        {
            if (GUILayout.Button("Hide Map"))
            {
                mapVisible = false;
                GameObject.Find("Map Texture").GetComponent<Image>().enabled = false;
                GameObject.Find("Text Background").GetComponent<Image>().enabled = true;
            }
        }
        else {
            if (GUILayout.Button("Show Map"))
            {
                mapVisible = true;
                GameObject.Find("Map Texture").GetComponent<Image>().enabled = true;
                GameObject.Find("Text Background").GetComponent<Image>().enabled = false;
            }
        }
    }
}
