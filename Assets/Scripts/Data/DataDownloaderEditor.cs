#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataDownloader), true)]
public class DataDownloaderEditor: Editor
{
    public override void OnInspectorGUI()
    {
        DataDownloader myScript = (DataDownloader)target;

        if (GUILayout.Button("Download Data"))
        {
            myScript.DownloadCSVs();
        }

        if (GUILayout.Button("Open Link"))
        {
            Application.OpenURL(myScript.url);
        }

        DrawDefaultInspector();
    }
}
#endif