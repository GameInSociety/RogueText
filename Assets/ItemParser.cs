using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.UIElements;
using UnityEngine;

[System.Serializable]
public static class ItemParser
{

    public static string text;
    public static Item pendingItem;
    public static Item sustainItem;
    static List<Item> items = new List<Item>();
    public static bool waitingForItem = false;
    public static bool replacingItem = false;

    public static List<Item> debug_inputItems = new List<Item>();
    public static List<Item> debug_outputItems = new List<Item>();

    public static List<string> debug_searches = new List<string>();

    public enum Type
    {
        First,
        Highest,
        Random,
    }

    public class ItemKey
    {
        public ItemKey(string c, Item it)
        {
            item = it;
            key = c;
        }

        public string key;
        public Item item;
    }

    // for now the only use for the history is in the text manager
    public static List<ItemKey> history = new List<ItemKey>();
    public static void ParseItems(string _text)
    {
        text = _text;

        if (Regex.IsMatch(text, @$"\bit\b") || Regex.IsMatch(text, @$"\bthem\b"))
        {
            if (pendingItem != null)
            {
                Spec(pendingItem);
            }
        }
        else
        {
            ParseItems();
        }

        // success

        debug_inputItems = new List<Item>(items);
    }

    public static void ParseItems ()
    {
        if (!waitingForItem)
        {
            items.Clear();
        }

        List<Item> availableItems = AvailableItems.GetItems;

        int smallestIndex = 100;
        foreach (var item in availableItems)
        {
            if (items.Contains(item))
            {
                continue;
            }

            int index = 0;
            if (item.ContainedInText(text, out index))
            {
                if ( index < smallestIndex)
                {
                    smallestIndex = index;
                    items.Insert(0, item);
                }
                else
                {
                    items.Add(item);
                }
            }
        }

        debug_outputItems = new List<Item>(items);
        
        // try verb alone
        if (IsEmpty)
        {
            if (Verb.HasCurrent)
            {
                Item verbItem = Item.GetDataItem("no item");
                if (Verb.GetCurrent.HasCell(verbItem))
                {
                    Add(verbItem);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
            
        }

        if (items.Count == 1 /*&& */)
        {
            return;
        }

        if (AllIdentical(items))
        {
            // Don't differenciate item, throwing any
            // ex there are some plates
            if (items[0].word.number == Word.Number.Plural)
            {
                // keep list as is, because function on all 
            }
            else
            {
                // keep only one, because it's not dif and singluar
                Spec(items.First());
            }
        }

        // check for number items
        Item numberItem = GetNumberItem();

        if (numberItem != null)
        {
            Spec(numberItem);
        }

        // look for item in containers
        foreach (var cItem in items)
        {
            foreach (var it in items)
            {
                if (cItem.HasItem(it))
                {
                    Spec(it);
                }
            }
        }

        if (waitingForItem)
        {
            waitingForItem = false;
            items.Clear();
            InputInfo.Instance.Reset();
            return;
        }

        if (pendingItem != null)
        {
            if (items.Contains(pendingItem) && !text.Contains("other"))
            {
                Add(pendingItem);
                return;
            }
        }

        if (GetDuplicates() == null)
        {
            return;
        }

        string message = "input_itemConfusion";
        if (Verb.GetCurrent == null)
        {
            message = "Which &dog&";
        }

        if (items.Count == 0)
        {
            TextManager.Write(message);
        }
        else
        {
            TextManager.Write(message, items[0]);
        }


        waitingForItem = true;
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

            if ( text.Contains(num) )
            {
                if ( GetDuplicates() != null)
                {
                    return GetDuplicates()[i];
                }
            }
            ++i;
        }

        return null;
    }

    public static bool HasItem(string itemName)
    {
        return items.Find(x => x.debug_name == itemName) != null;
    }

    static void Spec(Item item)
    {
        items.RemoveAll(x => x != item && x.dataIndex == item.dataIndex);
        waitingForItem = false;
    }

    static void Add(Item it)
    {
        items.Add(it);
        waitingForItem = false;
    }

    static List<Item> GetDuplicates()
    {
        foreach (var item in items)
        {
            List<Item> its = items.FindAll(x=> x.dataIndex == item.dataIndex && x.HasInfo("dif"));

            if (its.Count > 1)
            {
                return its;
            }
        }

        return null;
    }

    public static bool AllIdentical(List<Item> items)
    {
        bool allIdentical = items.TrueForAll(x =>
        x.debug_name == items.First().debug_name
        &&
        !x.HasInfo("dif"));

        /*Debug.Log("all identical : " + allIdentical);
        Debug.Log("container present : " + containerPresent);*/

        return allIdentical;
    }

    public static bool AllSameType(List<Item> items)
    {
        bool allSameType = items.TrueForAll(x =>
        x.debug_name == items.First().debug_name);

        /*Debug.Log("all identical : " + allIdentical);
        Debug.Log("container present : " + containerPresent);*/

        return allSameType;
    }

    static Item parentItem;

    public static Property SearchProperty (string content)
    {
        string propertyName = "";
        Item item = SearchItem(content, out propertyName);

        Debug.Log("property search : property name : " + propertyName);

        if ( item == null)
        {
            Debug.LogError("no property for : " + content);
            return null;
        }

        return item.GetProperty(propertyName);
    }
    public static Item SearchItem(string key)
    {
        string modKey = "";
        return SearchItem(key, out modKey);
    }
    public static Item SearchItem(string key, out string modifiedKey)
    {
        Debug.Log("[SEARCH] : " + key);

        // check if parent item
        if (key.Contains('/'))
        {
            string[] parts = key.Split(new char[] { '/' });
            parentItem = GetPotentialItems(parts[0]).FirstOrDefault();
            Debug.Log("searching inside : " + parentItem.debug_name);
            key = parts[1];
        }

        Type type = Type.First;

        // get type if not default
        if (key.Contains('.'))
        {
            string[] parts= key.Split(new char[] { '.' });
            type = GetType(parts[0]);
            key = parts[1];
        }

        List<Item> items = GetPotentialItems(key);

        if (items.Count == 0)
        {
            modifiedKey = "error";
            return null;
        }

        Item item = null;

        switch (type)
        {
            case Type.First:
                item = items.First();
                break;
            case Type.Highest:

                Property highestProp = items.First().GetProperty(key);
                int i = 0;
                for (int itIndex = 1; itIndex < items.Count; itIndex++)
                {
                    Item it = items[itIndex];
                    Property prop = it.GetProperty(key);

                    if (prop == null)
                    {
                        Debug.LogError("no property " + key + " in " + it.debug_name);
                        break;
                    }

                    if ( prop.GetInt() > highestProp.GetInt())
                    {
                        i = itIndex;
                        highestProp = prop;
                    }
                }

                item = items[i];
                Debug.Log("item with highest " + key + " : " + item.debug_name);
                break;
            case Type.Random:
                item = items[UnityEngine.Random.Range(0, items.Count)];
                Debug.Log("randomly selected " + key + " : " + item.debug_name);
                break;
            default:
                item = items[0];
                break;
        }

        history.Add(new ItemKey(key, item));

        Debug.Log("found : " + item.debug_name);
        modifiedKey = key;
        return item;
    }

    static Type GetType(string str)
    {
        switch (str)
        {
            case "h":
                return Type.Highest;
            case "r":
                return Type.Random;
            default:
                return Type.First;
        }
    }

    private static List<Item> GetPotentialItems (string content)
    {
        Predicate<Item> match = x => x.debug_name == content
            ||
            x.HasInfo(content)
            ||
            x.HasProperty(content);

        // first search in history
        /*ItemKey key = history.Find(x=> x.key == content);

        if ( key != null)
        {
            Debug.Log("found " + content + " in history");
            return key.item;
        }*/


        List<Item> tmp_items = new List<Item>();

        // search for item in pending items
        if ( parentItem != null)
        {
            Debug.Log("searching in " + parentItem.debug_name);
            tmp_items.AddRange(parentItem.GetAllItems().FindAll(match));
            parentItem = null;
            return tmp_items;
        }

        // add items from input
        tmp_items.AddRange(GetItems.FindAll(match));

        // if there is an matching item in the input, return for priority
        if ( tmp_items.Count > 0 )
        {
            Debug.Log("found " + content + " in input");
            return tmp_items;
        }

        Debug.Log("found " + content + " in available items");

        // add available items
        tmp_items.AddRange(AvailableItems.GetItems.FindAll(match));

        return tmp_items;
    }


    public static List<Item> GetItems
    {
        get
        {
            return items;
        }
    }

    public static Item FirstItem
    {
        get
        {
            return items[0];
        }
    }

    public static bool IsEmpty
    {
        get
        {
            return items.Count == 0;
        }
    }
}
