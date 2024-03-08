using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ItemData {

    public string debugName = "";

    public static List<ItemData> itemDatas = new List<ItemData>();

    public string name {
        get {  return words[0].GetText; }
    }
    public int index;
    public List<Word> words = new List<Word>();
    public string className;

    public List<Property> properties = new List<Property>();
    public List<Sequence> verbSequences = new List<Sequence>();
    public List<Sequence> events = new List<Sequence>();
    public List<Sequence> acts = new List<Sequence>();

    #region static
    public static ItemData GetItemData(int i) {
        return itemDatas[i];
    }
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
        index = itemDatas.FindIndex(x => x.words.Find(x => x.GetText == key) != null);
        if (index == -1 )
            index = GetRandomDataOfType(key);
        if (index == -1) {
            Debug.LogError("no " + key + " in item datas");
            TextManager.Write($"no item or type of item with type '{key}' in the data", Color.red);
            return -1;
        }
        return index;
    }

    public static int GetRandomDataOfType(string type) {
        var items = itemDatas.FindAll(x => 
        x.properties.Find(x=> x.name == "types") != null
        &&
        x.properties.Find(x=> x.name == "types").HasPart(type) );
        if (items.Count == 0) {
            Debug.LogError("no type " + type + " in item datas");
            return -1;
        }
        return items[UnityEngine.Random.Range(0, items.Count)].index;
    }
    public static List<int> GetDatasOfType(string type) {

        var ints = Enumerable.Range(0, itemDatas.Count).Where(i => 
        itemDatas[i].properties.Find(x=> x.name == "types") != null &&
        itemDatas[i].properties.Find(x => x.name == "types").HasPart(type)
        ).ToList();
        return ints;
    }
    static int debugAll;
    public static Item Generate_Simple(string name) {
        var dataIndex = GetItemDataIndex(name);
        var item = new Item();
        item.dataIndex = dataIndex;
        item.debug_Id = debugAll;
        debugAll++;
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
        item.debug_Id = debugAll;
        debugAll++;
        var serializedParent = JsonConvert.SerializeObject(item);
        var obj = JsonConvert.DeserializeObject(serializedParent, ItemType);
        ((Item)obj).Init();
        return obj;
    }
    #endregion
}
