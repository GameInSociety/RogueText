using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AvailableItems
{

    public static Item Find(string name)
    {
        return Get.Find(x=> x.HasWord(name));
    }
    public static List<Item> FindAll(string name)
    {
        List<Item> list = new List<Item>();
        foreach (var item in Get)
        {
            if (item.HasWord(name))
            {
                list.Add(item);
            }
        }

        return Get.FindAll(x=> x.HasWord(name));
    }


    public static List<Item> Get
    {
        get { 
        
            // old way, before add
            // not used, but add isn't perfect so keep in around

            List<Item> list = new List<Item>();

            list.Add(Inventory.Instance);
            list.AddRange(Inventory.Instance.GetContainedItems);

            list.AddRange(Tile.Current.GetAllItems());
            List<Item> functionItems = GetFunctionItems;
            if (functionItems!= null)
            {
                list.AddRange(functionItems);
            }

            DebugManager.Instance.availableItems = list;

            return list;
        }
    }

    public static List<Item> GetFunctionItems
    {
        get
        {
            if (WorldEvent.current == null)
            {
                return null;
            }

            return WorldEvent.current.tile.GetAllItems();
        }
    }
}
