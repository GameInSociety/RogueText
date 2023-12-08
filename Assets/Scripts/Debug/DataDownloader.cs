using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class DataDownloader : MonoBehaviour {
    public string linkReplace = "gviz/tq?tqx=out:csv&sheet=";

    public string[] sheetNames;
    public Object[] targetAssets;

    public string sheetName;
    public int row;
    public int col;

    public string url;
    public string path = @"F:\Unity Projects\Rogue Text\Assets\Resources\Items\";

    #region parse
    public void Load() {
        foreach (var asset in targetAssets) {
            var textAsset = asset as TextAsset;
            sheetName = textAsset.name;
            fgCSVReader.LoadFromString(textAsset.text, new fgCSVReader.ReadLineDelegate(GetCell));
        }

        FinishLoading();
    }

    public virtual void FinishLoading() {

    }

    public virtual void GetCell(int rowIndex, List<string> cells) {
        row = rowIndex;
    }
    #endregion


#if UNITY_EDITOR

    public void DownloadCSVs() {
        _ = StartCoroutine(DownloadsCSVs());
    }

    IEnumerator DownloadsCSVs() {
        var textAssets = Resources.LoadAll<TextAsset>(path);

        sheetNames = new string[textAssets.Length];
        targetAssets = new Object[textAssets.Length];

        for (var i = 0; i < textAssets.Length; i++) {
            sheetNames[i] = textAssets[i].name;
            targetAssets[i] = textAssets[i];
        }


        yield return null;
        for (var sheetIndex = 0; sheetIndex < sheetNames.Length; sheetIndex++) {
            var editIndex = url.IndexOf("edit");
            if (editIndex != -1) {
                var tmpUrl = url.Remove(editIndex) + linkReplace + sheetNames[sheetIndex];

                Debug.Log("(" + sheetIndex + "/" + sheetNames.Length + ")" + " Fetching " + sheetNames[sheetIndex] + "...");
                yield return DownloadCSV(tmpUrl, targetAssets[sheetIndex]);
            } else {
                Debug.LogError("no index for edit in link " + url);
            }
        }

        Debug.Log("Done !");

    }

    public IEnumerator DownloadsCSV(int sheetIndex) {
        var textAssets = Resources.LoadAll<TextAsset>(path);

        sheetNames = new string[textAssets.Length];
        targetAssets = new Object[textAssets.Length];
        for (var i = 0; i < textAssets.Length; i++) {
            sheetNames[i] = textAssets[i].name;
            targetAssets[i] = textAssets[i];
        }

        yield return null;

        var editIndex = url.IndexOf("edit");
        if (editIndex != -1) {
            var tmpUrl = url.Remove(editIndex) + linkReplace + sheetNames[sheetIndex];
            Debug.Log("Fetching " + sheetNames[sheetIndex] + "...");
            yield return DownloadCSV(tmpUrl, targetAssets[sheetIndex]);
        } else {
            Debug.LogError("no index for edit in link " + url);
        }

        Debug.Log("Done !");

    }

    IEnumerator DownloadCSV(string tmpUrl, UnityEngine.Object asset) {
        _ = Time.realtimeSinceStartup + 10f;

        var www = UnityWebRequest.Get(tmpUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError("Error when requesting CSV file (responseCode:" + www.responseCode + ")");
            Debug.LogError(www.error);
        } else {
            var filepath = AssetDatabase.GetAssetPath(asset);
            System.IO.File.WriteAllText(filepath, www.downloadHandler.text);
            Undo.RecordObject(asset, "Downloaded CSV from distant file");

            AssetDatabase.ImportAsset(filepath);

        }

    }

    private static string GetCollumnName(int columnNumber) {

        // To store result (Excel column name)
        var columnName = "";

        while (columnNumber > 0) {

            // Find remainder
            var rem = columnNumber % 26;

            // If remainder is 0, then a
            // 'Z' must be there in output
            if (rem == 0) {
                columnName += "Z";
                columnNumber = (columnNumber / 26) - 1;
            }

            // If remainder is non-zero
            else {
                columnName += (char)(rem - 1 + 'A');
                columnNumber = columnNumber / 26;
            }
        }

        // Reverse the string
        columnName = Reverse(columnName);

        // Print result
        return columnName;
    }

    public static string Reverse(string s) {
        var charArray = s.ToCharArray();
        System.Array.Reverse(charArray);
        return new string(charArray);
    }

    public string GetCellName(int row, int cell) {
        return GetCollumnName(cell) + (row + 1);
    }
#endif

}
