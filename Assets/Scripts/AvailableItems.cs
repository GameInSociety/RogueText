using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

public static class AvailableItems
{
    public static List<Item> list = new List<Item>();
    public static List<Item> recentItems = new List<Item>();

    public static Item FindInText(string text)
    {
        return GetItems.Find(x => x.ContainedInText(text));
    }

    public static Item SearchByType(string type)
    {
        return GetItems.Find(x => x.HasInfo(type));
    }

    public static Item SearchByProperty(string property)
    {
        return GetItems.Find(x=> x.HasProperty(property));
    }

    public static Item SearchByName(string name)
    {
        return GetItems.Find(x=> x.HasWord(name));
    }
    public static List<Item> FindAll(string name)
    {
        List<Item> list = new List<Item>();
        foreach (var item in GetItems)
        {
            if (item.HasWord(name))
            {
                list.Add(item);
            }
        }

        return GetItems.FindAll(x=> x.HasWord(name));
    }

    public static List<Item> GetDuplicates(List<Item> items)
    {

        foreach (var item in items)
        {
            Debug.Log("secong batch : " + item.debug_name);
        }

        foreach (var item in items)
        {
            Debug.Log("is " + item.debug_name + " a duplicate");
            List<Item> its = items.FindAll(x => x.dataIndex == item.dataIndex && x.HasInfo("dif"));

            if (its.Count > 1)
            {
                return its;
            }
        }

        return null;
    }

    public static List<Item> GetItems
    {
        get
        {
            List<Item> tmpList = new List<Item>();
            tmpList.AddRange(Tile.GetCurrent.GetAllItems());

            // je commente au cas ou ?
            //tmpList.Add(Tile.GetCurrent);
            /*tmpList.AddRange(Player.Instance.GetAllItems());

            if (FunctionSequence.current != null && FunctionSequence.current.tile != Tile.GetCurrent)
            {
                Debug.Log("curren ttile : " + FunctionSequence.current.FirstItem.debug_name);
                Debug.Log("function tile : " + FunctionSequence.current.FirstItem.debug_name);
                tmpList.AddRange(FunctionSequence.current.tile.GetAllItems());
            }
            else
            {
                tmpList.AddRange(Tile.GetCurrent.GetAllItems().FindAll(x=> x.visible));
            }*/

            tmpList.AddRange(Item.dataItems.FindAll(x => x.HasInfo("general")));

            // add recent items
            recentItems = tmpList.FindAll(x => !list.Contains(x));
            list.Clear();
            list.AddRange(tmpList);

            return list;
        }
    }

    /*public static List<Item> Get
    {
        get {

            list.Clear();

            list.Add(Inventory.Instance);
            list.AddRange(Inventory.Instance.GetContainedItems);

            if (FunctionSequence.current != null && FunctionSequence.current.tile != Tile.GetCurrent)
            {
                list.AddRange(FunctionSequence.current.tile.GetAllItems());
            }
            else
            {
                list.AddRange(Tile.GetCurrent.GetAllItems());
            }

            list.AddRange(Item.dataItems.FindAll(x => x.GetAppearInfo().usableAnytime));

            //list.Remove(Tile.GetCurrent);

            DebugManager.Instance.availableItems = list;

            return list;
        }
    }*/

    public static List<Item> GetFunctionItems
    {
        get
        {
            if (FunctionSequence.current == null)
            {
                return null;
            }

            return FunctionSequence.current.tile.GetAllItems();
        }
    }
}
