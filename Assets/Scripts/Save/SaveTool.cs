using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;


//tu peux changer le chemin de sauvegarde il y a troi ligne a changer :
//string path = Application.dataPath + dataPathSave;
public class SaveTool : MonoBehaviour {
    public int chunkLimit = 10;

    public static SaveTool Instance;

    void Awake() {
        Instance = this;
    }

    #region directories
    public void CreateDirectories() {
        if (DirectoryExists(GetSaveFolderPath()) == false) {
            //			Debug.Log ("BYTES SaveData folder doesnt exist, creating it");
            _ = Directory.CreateDirectory(GetSaveFolderPath());
        }
    }
    #endregion


    #region save & load
    public void ResetIslandFolder() {
        if (DirectoryExists(GetSaveFolderPath() + "/Islands") == true) {
            Directory.Delete(GetSaveFolderPath() + "/Islands", true);
        }

        _ = Directory.CreateDirectory(GetSaveFolderPath() + "/Islands");

    }

    public void DeleteFolder(string mapName) {
        Directory.Delete(GetSaveFolderPath(mapName), true);
    }

    public void DeleteGameData() {
        var path = GetSaveFolderPath() + "/game data.xml";
        File.Delete(path);
    }

    public void SaveToSpecificFolder(string folder, string fileName, object o) {
        fileName = GetSaveFolderPath(folder) + "/" + fileName + ".xml";
        Save(fileName, o);
    }

    public void SaveToCurrentMap(string path, object o) {

        path = GetSaveFolderPath() + "/" + path + ".xml";
        Save(path, o);
    }

    public void Save(string path, object o) {
        var bytes = Encoding.Unicode.GetBytes(path);
        path = Encoding.Unicode.GetString(bytes);

        File.Delete(path);

        var file = File.Open(path, FileMode.CreateNew);
        var serializer = new XmlSerializer(o.GetType());

        //		file = file.
        serializer.Serialize(file, o);

        file.Close();
    }

    public object LoadFromSpecificPath(string mapName, string path, string className) {
        path = GetSaveFolderPath(mapName) + "/" + path;
        return LoadFromPath(path, className);
    }
    public object LoadFromCurrentMap(string path, string className) {
        path = GetSaveFolderPath() + "/" + path;
        return LoadFromPath(path, className);
    }
    public object LoadFromPath(string path, string className) {
        var bytes = Encoding.Unicode.GetBytes(path);
        path = Encoding.Unicode.GetString(bytes);

        //		FileStream file = File.Open(path, FileMode.OpenOrCreate);
        var file = File.Open(path, FileMode.Open);
        var serializer = new XmlSerializer(Type.GetType(className));

        var o = serializer.Deserialize(file);

        file.Close();

        return o;
    }
    #endregion

    public bool FileExists(string mapName, string path) {
        path = GetSaveFolderPath(mapName) + "/" + path + ".xml";

        var bytes = Encoding.Unicode.GetBytes(path);
        path = Encoding.Unicode.GetString(bytes);

        var exists = File.Exists(path);

        return exists;
    }
    public bool FileExists(string path) {
        path = GetSaveFolderPath() + "/" + path + ".xml";

        var bytes = Encoding.Unicode.GetBytes(path);
        path = Encoding.Unicode.GetString(bytes);

        var exists = File.Exists(path);

        return exists;
    }
    public bool DirectoryExists(string path) {
        var bytes = Encoding.Unicode.GetBytes(path);
        path = Encoding.Unicode.GetString(bytes);

        var exists = Directory.Exists(path);

        return exists;
    }

    #region paths
    public string GetSaveFolderPath(string targetFolder) {
        var path = Application.dataPath + "/SaveData/" + targetFolder;

        if (Application.isMobilePlatform)
            path = Application.persistentDataPath + "/SaveData/" + targetFolder;

        return path;
    }
    public string GetSaveFolderPath() {
        var s = "Default";
        var path = Application.dataPath + "/SaveData/" + s;

        if (Application.isMobilePlatform)
            path = Application.persistentDataPath + "/SaveData/" + s;

        return path;
    }
    #endregion

}
