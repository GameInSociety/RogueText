using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
    public static bool opened = false;
    public static Item CurrentItem;

    public List<Item> items = new List<Item>();

    public int id = 0;

    public Item item;

    public bool hasBeenUsed = true;

    public void RemoveItem ( Item item)
    {
        items.Remove(item);

        Debug.Log("removing container");
    }

    public static void Describe(Item item)
    {
        Phrase.Renew();
        CurrentItem = item;
        CurrentItem.WriteContainedDescription();
    }
}
         