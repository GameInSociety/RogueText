using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ItemData {

    // Je mets ou ça ? 
    public static List<ItemData> itemDatas = new List<ItemData>();

    public List<ItemData> roots = new List<ItemData>();

    // Simple sting to get item's name in console
    public string debugName = "";
    // Overall identifier, to differenciate between instances.
    static int debugAll;

    public string name {
        get {  return words[0].GetText; }
    }
    // The Item's identifier
    public int id;
    // List of words the item is refered with.
    // ( Consider putting as hidden function in Writer )
    public List<Word> words = new List<Word>();

    // Peut etre obsolete. A voir si on a toujours besoin de la classe Tile ( Suremenet pas, et si oui faut pas )
    public string className;

    // Properties of the item
    public List<Property> properties = new List<Property>(); // DOIT ETRE DATA (les props dynamiques vont être générés à la création, et leur property data leur seront assignées).
    // Sequences activated by a verb in player's input.
    public List<Sequence> verbSequences = new List<Sequence>(); // DOITE ETRE DATA
    // Other sequences the item holds.
    public List<Sequence> sequences = new List<Sequence>(); // DOIT ETRE DATA

    #region static
    public static ItemData GetItemData(string key) {
        int index = -1;
        index = itemDatas.FindIndex(x => x.words.Find(x => x.GetText == key) != null);
        if (index == -1) {
            Debug.LogError("no " + key + " in item datas");
            TextManager.Write($"no item or type of item with type '{key}' in the data", Color.red);
            return null;
        }
        return itemDatas[index];
    }
    // give item name, get item data index. 
    public static int GetItemDataIndex(string key) {
        int index = -1;
        index = itemDatas.FindIndex(x => x.words.Find(x => x.GetText == key) != null);
        if (index == -1) {
            Debug.LogError("no " + key + " in item datas");
            TextManager.Write($"no item or type of item with type '{key}' in the data", Color.red);
            return 55;
        }
        return index;
    }

    // Get random item of a certain type of item ( Fruits, Locations etc... )
    // Ici la propriétés "types est importante." mais ptt elle peut devenir une fonction dans le WriteR.
    public static int GetRandomDataOfType(string type) {
        var items = itemDatas.FindAll(x => 
        x.properties.Find(x=> x.name == "types") != null
        &&
        x.properties.Find(x=> x.name == "types").HasPart(type) );
        if (items.Count == 0) {
            Debug.LogError("no type " + type + " in item datas");
            return -1;
        }
        return items[UnityEngine.Random.Range(0, items.Count)].id;
    }
    public static List<int> GetDatasOfType(string type) {

        var ints = Enumerable.Range(0, itemDatas.Count).Where(i => 
        itemDatas[i].properties.Find(x=> x.name == "types") != null &&
        itemDatas[i].properties.Find(x => x.name == "types").HasPart(type)
        ).ToList();
        return ints;
    }

    // Method to generate an item without specific class / behaviors
    public static Item Generate_Simple(string name) {
        var dataIndex = GetItemDataIndex(name);
        var item = new Item();
        item.dataIndex = dataIndex;
        item.id = debugAll;
        debugAll++;
        item.Init();
        return item;
    }
    // Generates Children of item class of specific behaviors ( Tile etc.. )
    public static object Generate_Special(string name, bool debug = false) {
        var index = GetItemDataIndex(name);
        var ItemType = Type.GetType(itemDatas[index].className);
        if (ItemType == null) {
            Debug.LogError("pas d'item type pour " + itemDatas[index].name);
            return null;
        }
        var item = new Item();
        item.dataIndex = index;
        item.id = debugAll;
        debugAll++;
        var serializedParent = JsonConvert.SerializeObject(item);
        var obj = JsonConvert.DeserializeObject(serializedParent, ItemType);
        ((Item)obj).Init();
        return obj;
    }

    public static ItemData Copy(ItemData itemData) {
        var newItemData = new ItemData();
        newItemData.roots = itemData.roots;
        newItemData.debugName = itemData.debugName;
        newItemData.id = itemData.id;
        newItemData.words = itemData.words;
        newItemData.className = itemData.className;
        newItemData.properties = itemData.properties;
        newItemData.verbSequences = itemData.verbSequences;
        newItemData.sequences = itemData.sequences;
        return newItemData;
    }
    #endregion
}
