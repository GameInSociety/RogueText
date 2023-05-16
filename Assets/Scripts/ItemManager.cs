using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using static UnityEditor.Progress;

public class ItemManager : MonoBehaviour {

    public List<Item> dataItems = new List<Item>();

    public Item TryGetItem(string _name)
    {
        _name = _name.ToLower();

        // to find the word on ly
        //Item item = dataItems.Find(x => x.word.text.ToLower() == _name);

        // to search also in syonyms
        Item item = dataItems.Find(x =>
        x.HasWord(_name)
            );

        if (item == null)
        {
            // find plural
            item = dataItems.Find(x => x.word.GetPlural() == _name);

            if (item != null)
            {
                // found the plural
                //Debug.Log("found plural");
                InputInfo.Instance.actionOnAll = true;
                return item;
            }
        }

        return item;
    }
    public List<Item> FindAllByName(string str)
    {
        str = str.ToLower();

        return dataItems.FindAll(x => x.word.text.StartsWith(str));
    }
    public Item FindByName(string str)
    {
        str = str.ToLower();
        return dataItems.Find(x => x.word.text.StartsWith(str));

    }
    public Item FindInWorld(string str)
    {
        List<Item> items = FindItemsInWorld(str);

        if ( items.Count == 0)
        {
            return null;
        }
        else
        {
            return items[0];
        }
    }


    public List<Item> GetAvailableItems()
    {
        List<Item> tmpItems = new List<Item>();

        tmpItems.Add(Inventory.Instance);

        tmpItems.Add(Tile.GetCurrent);

        tmpItems.AddRange(Tile.GetCurrent.GetItemsRecursive());

        tmpItems.AddRange(Inventory.Instance.GetItemsRecursive());

        return tmpItems;
    }
    public List<Item> FindItemsInWorld(string str)
    {
        return GetAvailableItems().FindAll(x => x.word.text == str);
    }

    public Tile CreateTile(Coords _coords, string tileName)
    {
        Item item = CreateFromData(tileName);

        var serializedParent = JsonConvert.SerializeObject(item);
        Tile newTile = JsonConvert.DeserializeObject<Tile>(serializedParent);
        newTile.coords = _coords;


        return newTile;
    }


    private static List<Item> FindInTile(string str)
    {
        List<Item> tmpItems = new List<Item>();

        // than return them direclty
        if( tmpItems.Count > 0)
        {
            return tmpItems;
        }

        // is the item the exact same tile as the one we're in ?
        if (Tile.GetCurrent.HasWord(str))
        {
            tmpItems.Add(Tile.GetCurrent);
            return tmpItems;
        }

        foreach (var item in Tile.GetCurrent.GetContainedItems.FindAll(x => x.word.Compare(str)))
        {
            tmpItems.Add(item);
        }

        /// ADJECTIVES ///
        return tmpItems;
    }


    public Item GetDataItem(string _name)
    {
        Item item = TryGetItem(_name);

        if (item == null)
        {
            Debug.LogError("couldn't find item : " + _name);
        }
        else
        {
        }

        return item;

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
        newItem.dataIndex = copy.dataIndex;
        newItem.usableAnytime = copy.usableAnytime;

        // the word never changes, non ? pourquoi en copy
        newItem.words = copy.words;

        newItem.stackable = copy.stackable;

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
