using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using static UnityEditor.Progress;

public class ItemManager : MonoBehaviour {

    public List<Item> dataItems = new List<Item>();

    public Item GetDataItem(string _name)
    {
        Item item;
        if (_name.StartsWith("type:"))
        {
            _name = _name.Remove(0, 5);
            List<Item> items = dataItems.FindAll(x => x.HasInfo(_name));

            item = items[Random.Range(0, items.Count)];
            Debug.Log("random item of type : " + _name + " : " + item.debug_name);

            Debug.Log(item.debug_name);
        }
        else
        {
            item = dataItems.Find(x => x.debug_name == _name);
        }

        if (item == null)
        {
            Debug.LogError("no " + _name + " in item datas");

            // find plural
            item = dataItems.Find(x => x.word.GetPlural() == _name);

            if (item != null)
            {
                return item;
            }
        }

        return item;
    }

    public Tile CreateTile(Coords _coords, string tileName)
    {

        Item item = CreateFromData(tileName);

        var serializedParent = JsonConvert.SerializeObject(item);
        Tile newTile = JsonConvert.DeserializeObject<Tile>(serializedParent);
        newTile.coords = _coords;

        return newTile;
    }

    public Item CreateInTile(Tile tile, string itemName)
    {
        Item newItem = CreateFromData(itemName);
        tile.AddItem(newItem);
        return newItem;
    }
    public Item CreateInItem(Item item, string itemName)
    {
        Item newItem = CreateFromData(itemName);
        item.AddItem(newItem);
        return newItem;
    }

    public List<AppearInfo> appearInfos = new List<AppearInfo>();

    public Item CreateFromData(string name)
    {
        Item copy = GetDataItem(name);

        return CreateFromData(copy);
    }

    /// create a new item by name
    public Item CreateFromData(Item copy)
    {
        // common to all
        Item newItem = new Item();

        newItem.debug_name = copy.debug_name;
        newItem.debug_randomID = Random.Range(0, 10000);
        newItem.dataIndex = copy.dataIndex;

        newItem.infos = copy.infos;

        // the word never changes, non ? pourquoi en copy
        newItem.words = copy.words;

        // unique
        foreach (var prop_copy in copy.properties)
        {
            newItem.CreateProperty(prop_copy);
        }

        return newItem;
    }

    public List<Item> FindItemsWithProperty(string propName)
    {
        return dataItems.FindAll(x => x.HasProperty(propName));
    }

    private static ItemManager _instance;
    public static ItemManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ItemManager>().GetComponent<ItemManager>();
            }

            return _instance;
        }
    }
}
