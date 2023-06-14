using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;

public static class CurrentItems
{
    public static List<Item> list = new List<Item>();
    public static Item pendingItem;

    public static bool waitForItem = false;

    static string text;

    public static List<Item> Get
    {
        get
        {
            return list;
        }
    }

    public static bool Empty
    {
        get
        {
            return list.Count == 0;
        }
    }

    public static void Clear()
    {
        list.Clear();
        waitForItem = false;
    }

    public static Verb.Sequence GetSequence()
    {
        Item cellItem = null;

        foreach (Item item in list)
        {
            if (Verb.GetCurrent.HasCell(item))
            {
                Debug.Log("found sequence with : " + item.debug_name);

                cellItem = item;
                break;
            }
        }

        if ( cellItem == null)
        {
            return null;
        }

        list.Remove(cellItem);
        list.Insert(0, cellItem);
        return Verb.GetCurrent.GetSequence(cellItem);
    }

    public static void FindAll(string _text)
    {
        text = _text;

        List<Item> availableItems = AvailableItems.Get;
        List<Item> tmpItems = new List<Item>(list);
        

        List<Item> range = availableItems.FindAll(x => x.ContainedInText(text));

        if (Regex.IsMatch(text, @$"\bit\b") || Regex.IsMatch(text, @$"\bthem\b"))
        {
            if (pendingItem != null)
            {
                range = new List<Item>() { pendingItem };
            }
        }

        list.AddRange(range);

        DebugManager.Instance.currentItems_in = new List<Item>(list);

        // try verb alone
        if (list.Count == 0 && Verb.HasCurrent)
        {
            Item verbItem = ItemManager.Instance.GetDataItem("verbe seul");
            if (Verb.GetCurrent.HasCell(verbItem))
            {
                list.Add(verbItem);
            }
        }

        if (list.Count > 1 /*&& */)
        {
            DiffenciateItems();
        }



        DebugManager.Instance.currentItems_out = new List<Item>(list);
    }

    static void DiffenciateItems()
    {
        if (GetSimilarItems().Count == 0)
        {
            Debug.Log("no similar items in list");
            // no similar items, keep all
            return;
        }

        if (AllItemsAreSimilar(list))
        {
            // ex there are some plates
            if (list[0].word.defaultNumber == Word.Number.Plural)
            {
                // leave because taking all plates
                //return;
            }
            else
            {
                // just the first plate if the word is singular
                //list.RemoveRange(1, list.Count - 1);
            }
        }

        // check for number items
        Item numberItem = GetNumberItem();

        if (numberItem != null)
        {
            SetSpecificItem(numberItem);
            return;
        }

        // look for item in containers
        Item containerItem = null;
        Item specItem = null;
        foreach (var cItem in list)
        {
            foreach (var item in list)
            {
                if (cItem.HasItem(item))
                {
                    containerItem = cItem;
                    specItem = item;
                    break;
                }
            }
        }

        if ( specItem != null)
        {
            if ( containerItem != null)
            {
                list.Remove(containerItem);
            }
            SetSpecificItem(specItem);
            return;
        }

        // check if an item has been specified lately
        if (pendingItem != null)
        {
            if (text.Contains("other"))
            {
                pendingItem = null;
                return;
            }

            if (list.Contains(pendingItem))
            {
                Debug.Log("getting pending item : " + pendingItem.debug_name);
                SetSpecificItem(pendingItem);
                return;
            }

            // no pending items found try find new
            pendingItem = null;
        }

        string message = "input_itemConfusion";
        if (Verb.GetCurrent == null)
        {
            message = "Which &dog&";
        }
        WaitForSpecificItem(message);

    }

    static Item GetNumberItem()
    {
        string[] nums = new string[5]
        {
            "first",
            "second",
            "third",
            "fourth",
            "fifth"
        };

        // get numbers
        int i = 0;
        foreach (var num in nums)
        {
            if (text.Contains(num))
            {
                return GetSimilarItems()[i];
            }
            ++i;
        }

        return null;
    }

    static void SetSpecificItem(Item item)
    {
        pendingItem = item;

        waitForItem = false;

        /*int specItemIndex = list.IndexOf(item);
        Debug.Log("index of : " + item.debug_name + " is " +  specItemIndex);*/

        list.RemoveAll(x => x.SameTypeAs(item));
        list.Insert(0, item);
    }


    public static void WaitForSpecificItem(string message)
    {
        if (waitForItem)
        {
            Debug.LogError("already waiting for an item");
            InputInfo.Instance.Reset();
            return;
        }

        if ( list.Count == 0 )
        {
            TextManager.Write(message);
        }
        else
        { 
            TextManager.Write(message, GetSimilarItems()[0]);
        }

        waitForItem = true;
    }

    public static List<Item> GetSimilarItems()
    {
        List<Item> items = new List<Item>();

        if ( list.Count == 1) {
            return items;
        }

        foreach (var item in list)
        {
            List<Item> ts = list.FindAll(x => x.SameTypeAs(item));
            if ( ts.Count > 1)
            {
                items = ts;
                break;
            }
        }

        return items;
    }

    public static bool HasItem(string itemName)
    {
        return FindOfType(itemName) != null;
    }

    public static Item FindOfType(string itemName)
    {
        return list.Find(x => x.debug_name == itemName);
    }

    public static bool AllItemsAreSimilar(List<Item> items)
    {
        return items.TrueForAll(x => x.debug_name == items.First().debug_name);
    }
}
