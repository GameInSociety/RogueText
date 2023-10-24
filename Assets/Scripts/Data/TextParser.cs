using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class TextParser : MonoBehaviour {
    private string linkReplace = "export?format=csv";

    public string url;
    public Object targetAsset;

    #region parse
    public void Load() {
        var fileName = targetAsset.name + ".csv";

        //TextAsset textAsset = Resources.Load<TextAsset>("TextAssets/" + targetAsset.name);
        var textAsset = targetAsset as TextAsset;
        fgCSVReader.LoadFromString(textAsset.text, new fgCSVReader.ReadLineDelegate(GetCell));

        FinishLoading();
    }

    public virtual void FinishLoading() {

    }

    public virtual void GetCell(int rowIndex, List<string> cells) {

    }
    #endregion


#if UNITY_EDITOR
    public void DownloadCSVs() {
        _ = StartCoroutine(DownloadCSV(url, targetAsset));
    }

    IEnumerator DownloadCSV(string tmpUrl, UnityEngine.Object asset) {
        tmpUrl = tmpUrl.Replace("edit", linkReplace);

        Debug.Log("Sending request fir " + asset.name + "...");
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
            Debug.Log("Finished Downloading : " + asset.name);


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
