using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class DataDownloader : MonoBehaviour {
    public string linkReplace = "gviz/tq?tqx=out:csv&sheet=";

    public string[] sheetNames;

    public string sheetName;
    public int row;
    public int col;
    public string SheetToLoad = "";

    public static Dictionary<string, string> datas = new Dictionary<string, string>();

    public string url;
    public string path = @"F:\Unity Projects\Rogue Text\Assets\Resources\Items\";

    public int lineAmount = 0;


    public string sheetToLoad;
    #region parse
    public virtual void Load() {

        for (int i = 0; i < sheetNames.Length; ++i) {
            var sheet = sheetNames[i];

            if ( !string.IsNullOrEmpty( sheetToLoad) && sheet != sheetToLoad ) { continue; }

            var text = "";
            if ( datas.ContainsKey( sheet )) {
                text = datas[ sheet ];
            } else {
                var s = $"{path}/{sheet}";
                var textAsset = Resources.Load(s) as TextAsset;
                text = textAsset.text;
            }

            sheetName = sheetNames[i];
            lineAmount = fgCSVReader.GetLineAmount(text);
            fgCSVReader.LoadFromString(text, new fgCSVReader.ReadLineDelegate(GetCell));
        }

        FinishLoading();
    }

    public virtual void FinishLoading() {

    }


    public virtual void GetCell(int rowIndex, List<string> cells) {
        row = rowIndex;
    }
    #endregion



    public void DownloadCSVs() {
        _ = StartCoroutine(DownloadsCSVs());
    }

    public IEnumerator DownloadsCSVs() {
        var textAssets = Resources.LoadAll<TextAsset>(path);

        yield return null;
        for (var sheetIndex = 0; sheetIndex < sheetNames.Length; sheetIndex++) {
            var editIndex = url.IndexOf("edit");
            if (editIndex != -1) {
                var tmpUrl = url.Remove(editIndex) + linkReplace + sheetNames[sheetIndex];

                Debug.Log("(" + sheetIndex + "/" + sheetNames.Length + ")" + " Fetching " + sheetNames[sheetIndex] + "...");
                yield return DownloadCSV(tmpUrl, sheetNames[sheetIndex]);
            } else {
                Debug.LogError("no index for edit in link " + url);
            }
        }

        Debug.Log("Done !");

    }

    public IEnumerator DownloadsCSV(string sheetName) {
        int index = System.Array.FindIndex(sheetNames, x=> x == sheetName);
        yield return DownloadsCSV(index);
    }

    public IEnumerator DownloadsCSV(int sheetIndex) {
        yield return null;

        var editIndex = url.IndexOf("edit");
        if (editIndex != -1) {
            var tmpUrl = url.Remove(editIndex) + linkReplace + sheetNames[sheetIndex];
            Debug.Log("Fetching " + sheetNames[sheetIndex] + "...");
            yield return DownloadCSV(tmpUrl, sheetNames[sheetIndex]);
        } else {
            Debug.LogError("no index for edit in link " + url);
        }

        Debug.Log("Done !");

    }

    IEnumerator DownloadCSV(string tmpUrl, string sheetName) {
        _ = Time.realtimeSinceStartup + 10f;

        var www = UnityWebRequest.Get(tmpUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError("Error when requesting CSV file (responseCode:" + www.responseCode + ")");
            Debug.LogError(www.error);
        } else {
            if (Application.isPlaying) {
                if (datas.ContainsKey(sheetName)) {
                    datas[sheetName] = www.downloadHandler.text;
                } else {
                    datas.Add(sheetName, www.downloadHandler.text);
                }
                // load from local data
            } else {
                var filepath = $"{Application.dataPath}/Resources/{path}/{sheetName}.csv";
                System.IO.File.WriteAllText(filepath, www.downloadHandler.text);
            }
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

}
