using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

public class TextParser : MonoBehaviour
{
    private string linkReplace = "export?format=csv";

    public string url;
    public Object targetAsset;

    #region parse
    public void Load()
    {
        string fileName = targetAsset.name + ".csv";

        //TextAsset textAsset = Resources.Load<TextAsset>("TextAssets/" + targetAsset.name);
        TextAsset textAsset = targetAsset as TextAsset;
        fgCSVReader.LoadFromString(textAsset.text, new fgCSVReader.ReadLineDelegate(GetCell));

        FinishLoading();
    }

    public virtual void FinishLoading()
    {

    }

    public virtual void GetCell(int rowIndex, List<string> cells)
    {

    }
    #endregion


#if UNITY_EDITOR
    public void DownloadCSVs()
    {
        StartCoroutine(DownloadCSV(url, targetAsset));
    }

    IEnumerator DownloadCSV(string tmpUrl, UnityEngine.Object asset)
    {
        tmpUrl = tmpUrl.Replace("edit", linkReplace);

        Debug.Log("Sending request fir " +asset.name + "...");

        float timeOut = Time.realtimeSinceStartup + 10f;

        UnityWebRequest www = UnityWebRequest.Get(tmpUrl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError("Error when requesting CSV file (responseCode:" + www.responseCode + ")");
            Debug.LogError(www.error);
        }
        else
        {
            string filepath = AssetDatabase.GetAssetPath(asset);
            System.IO.File.WriteAllText(filepath, www.downloadHandler.text);
            Undo.RecordObject(asset, "Downloaded CSV from distant file");
            Debug.Log("Finished Downloading : " + asset.name);


            AssetDatabase.ImportAsset(filepath);

        }

    }

    private static string GetCollumnName(int columnNumber)
    {

        // To store result (Excel column name)
        string columnName = "";

        while (columnNumber > 0)
        {

            // Find remainder
            int rem = columnNumber % 26;

            // If remainder is 0, then a
            // 'Z' must be there in output
            if (rem == 0)
            {
                columnName += "Z";
                columnNumber = (columnNumber / 26) - 1;
            }

            // If remainder is non-zero
            else
            {
                columnName += (char)((rem - 1) + 'A');
                columnNumber = columnNumber / 26;
            }
        }

        // Reverse the string
        columnName = Reverse(columnName);

        // Print result
        return columnName;
    }

    public static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        System.Array.Reverse(charArray);
        return new string(charArray);
    }

    string alphabet = "abcdefghijklmnolqrsyuvwxyz";
    public string GetCellName (int row, int cell)
    {
        return GetCollumnName(cell) + (row+1);
    }
#endif

}
