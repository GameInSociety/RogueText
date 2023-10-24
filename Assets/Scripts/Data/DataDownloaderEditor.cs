#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataDownloader), true)]
public class DataDownloaderEditor : Editor {
    public override void OnInspectorGUI() {
        var myScript = (DataDownloader)target;

        if (GUILayout.Button("Download Data")) {
            myScript.DownloadCSVs();
        }

        for (var i = 0; i < myScript.sheetNames.Length; i++) {
            if (GUILayout.Button(myScript.sheetNames[i])) {
                _ = myScript.StartCoroutine(myScript.DownloadsCSV(i));
            }
        }

        if (GUILayout.Button("Open Link")) {
            Application.OpenURL(myScript.url);
        }

        _ = DrawDefaultInspector();
    }
}
#endif