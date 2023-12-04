using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class ItemData {

    public string debugName = "";

    public static List<ItemData> itemDatas = new List<ItemData>();

    [System.Serializable]
    public class Sequence {

        public Sequence(string _verbs, string _seq) {
            verbs = _verbs.Split(" / ");
            text = _seq;
        }

        public string text;
        public string[] verbs;
    }

    public string name {
        get {  return words[0].text; }
    }
    public int index;
    public List<Word> words = new List<Word>();
    public string className;
    public List<Property> properties = new List<Property>();
    public List<Sequence> sequences = new List<Sequence>();
    #region static
    public static ItemData GetItemData(string key) {
        var id = GetItemDataIndex(key);
        if ( id == -1) {
            Debug.LogError($"found no item data with key {key}");
            return null;
        }
        return itemDatas[id];
    }
    public static int GetItemDataIndex(string key) {
        int index = -1;
        if (key.StartsWith("type")) {
            key = TextUtils.Extract('(', key);
            index = GetDataOfType(key);
        } else
            index = itemDatas.FindIndex(x => x.words.Find(x => x.text == key) != null);

        if (index == -1) {
            Debug.LogError("no " + key + " in item datas");
            return -1;
        }
        return index;
    }

    public static int GetDataOfType(string type) {
        int index = itemDatas.FindIndex(x => 
        x.properties.Find(x=> x.name == "types") != null
        &&
        x.properties.Find(x=> x.name == "types").HasPart(type) );
        if (index == -1) {
            Debug.LogError("no type " + type + " in item datas");
            return -1;
        }
        return index;
    }
    public static List<int> GetDatasOfType(string type) {

        var ints = Enumerable.Range(0, itemDatas.Count).Where(i => 
        itemDatas[i].properties.Find(x=> x.name == "types") != null &&
        itemDatas[i].properties.Find(x => x.name == "types").HasPart(type)
        ).ToList();
        return ints;
    }
    public static Item Generate_Simple(string name) {
        var dataIndex = GetItemDataIndex(name);
        var item = new Item();
        item.dataIndex = dataIndex;
        item.debug_randomID = UnityEngine.Random.Range(0, 1000);
        item.Init();
        return item;
    }
    public static object Generate_Special(string name, bool debug = false) {

        var index = GetItemDataIndex(name);
        var ItemType = Type.GetType(itemDatas[index].className);
        if (ItemType == null) {
            Debug.LogError("pas d'item type pour " + itemDatas[index].name);
            return null;
        }

        var item = new Item();
        item.dataIndex = index;
        item.debug_randomID = UnityEngine.Random.Range(0, 1000);
        var serializedParent = JsonConvert.SerializeObject(item);
        var obj = JsonConvert.DeserializeObject(serializedParent, ItemType);
        ((Item)obj).Init();
        return obj;
    }
    #endregion
}
