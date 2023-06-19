using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class AvailableItems
{
    public static List<Item> list = new List<Item>();
    public static List<Item> recentItems = new List<Item>();

    public static Item FindInText(string text)
    {
        return Get.Find(x => x.ContainedInText(text));
    }
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
        get
        {


            List<Item> tmpList = new List<Item>();

            tmpList.Add(Tile.GetCurrent);
            tmpList.Add(Player.Inventory);
            tmpList.AddRange(Player.Instance.body.GetAllItems());

            if (FunctionSequence.current != null && FunctionSequence.current.tile != Tile.GetCurrent)
            {
                tmpList.AddRange(FunctionSequence.current.tile.GetAllItems());
            }
            else
            {
                tmpList.AddRange(Tile.GetCurrent.GetAllItems().FindAll(x=> x.visible));
            }

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
