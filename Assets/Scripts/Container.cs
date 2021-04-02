using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
    public static bool opened = false;
    public static Item CurrentItem;

    bool emptied = false;

    public List<Item> items = new List<Item>();

    public int id = 0;

    public Item item;

    public bool hasBeenUsed = true;

    public void RemoveItem ( Item item)
    {
        items.Remove(item);

        if ( items.Count == 0)
        {
            emptied = true;
        }

        Debug.Log("removing container");
    }

    public static void Describe()
    {
        string str = CurrentItem.GetContainedItemsDescription();
        DisplayDescription.Instance.AddToDescription(str);
    }
}
         