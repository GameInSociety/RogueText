using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {
    public Item CreateInTile(Tile tile, string itemName)
    {
        Item newItem = CreateFromData(itemName);
        tile.AddItem(newItem);
        return newItem;
    }
    public Item CreateInInventory(string itemName)
    {
        Item newItem = CreateFromData(itemName);
        Inventory.Instance.bag_Item.AddItem(newItem);
        return newItem;
    }
    public Item CreateInItem(Item item, string itemName)
    {
        Item newItem = CreateFromData(itemName);
        item.AddItem(newItem);
        return newItem;
    }

    /// create a new item by name
    public Item CreateFromData(string name)
    {
        Item copy = Item.GetDataItem(name);

        // common to all
        Item newItem = new Item();

        newItem.debug_name = copy.debug_name;
        newItem.index = copy.index;
        newItem.weight = copy.weight;
        newItem.usableAnytime = copy.usableAnytime;
        newItem.appearInfos = copy.appearInfos;
        newItem.sockets = copy.sockets;

        // the word never changes, non ? pourquoi en copy
        newItem.words = copy.words;

        newItem.stackable = copy.stackable;

        // unique
        foreach (var prop_copy in copy.properties)
        {
            newItem.CreateProperty(prop_copy);
        }

        /*foreach (var _property in copy.properties)
        {
            Property newPropety = newItem.CreateProperty(_property.parts);
        }*/

        //newItem.UpdateProperties();

        return newItem;
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
