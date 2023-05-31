using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AvailableItems
{

    public static Item Find(string name)
    {
        return GetItems.Find(x=> x.HasWord(name));
    }
    public static List<Item> FindAll(string name)
    {
        return GetItems.FindAll(x=> x.HasWord(name));
    }


    public static List<Item> GetItems
    {
        get { 
        
            // old way, before add
            // not used, but add isn't perfect so keep in around

            List<Item> list = new List<Item>();

            list.Add(Inventory.Instance);
            list.AddRange(Inventory.Instance.GetContainedItems);

            list.AddRange(Tile.GetCurrent.GetAllItems());

            return list;
        }
    }
}
