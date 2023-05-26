using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class AvailableItems
{
    private static List<Item> _items = new List<Item>();

    public static void Clear()
    {
        _items.Clear();

        _items.Add(Inventory.Instance);

        _items.AddRange(Inventory.Instance.GetContainedItems);
    }

    public static void Add(Item item)
    {
        if (_items.Contains(item))
        {
            return;
        }

        _items.Add(item);
    }

    public static void Remove(Item item)
    {
        if (!_items.Contains(item))
        {
            UnityEngine.Debug.Log("no " + item.debug_name + " in available items");
            return;
        }

        _items.Remove(item);

    }

    public static List<Item> List
    {
        get { return _items; }
    }

    public static List<Item> Find (string name)
    {
        return _items.FindAll(x=> x.HasWord(name));
    }
}
