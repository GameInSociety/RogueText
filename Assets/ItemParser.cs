using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        Input,
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
                Isolate(pendingItem);
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
        // clear the items if no spec item
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
                Debug.Log("parsed items already contain " + item.debug_name);
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
                Debug.Log("keep first one because not plural ?");
                // keep only one, because it's not dif and singluar
                Isolate(items.First());
            }
        }

        // check for number items
        Item numberItem = GetNumberItem();

        if (numberItem != null)
        {
            Debug.Log("catching number item");
            Isolate(numberItem);
        }

        Item itemFromContainer = null;
        // look for item in containers
        // je sais plus trop pourquoi mais ça pose des problemes.
        // c'est super important, donc se pencher dessus plus tard
        // et à terme remédier à ce truc que les lieux adjacents sont contenus dans la droite et la gauche
        // ça n'a aucun sens et ça fait régulièrement des loop infinis
        /*foreach (var cItem in items)
        {
            foreach (var it in items)
            {
                if (cItem.HasItem(it))
                {
                    itemFromContainer = it;
                    break;
                }
            }
        }*/

        if (itemFromContainer != null)
        {
            Debug.Log("item is from contained");
            Isolate(itemFromContainer);
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
            Debug.Log("there's a pending item");
            if (items.Contains(pendingItem) && !text.Contains("other"))
            {
                Add(pendingItem);
                return;
            }
        }

        if (AvailableItems.GetDuplicates(items)!= null)
        {
            WaitForSpecItem();
        }
        else
        {
            Debug.Log("no duplicates");
        }
    }

    static void WaitForSpecItem()
    {
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
                Debug.Log(num);
                if ( AvailableItems.GetDuplicates(items) != null)
                {
                    return AvailableItems.GetDuplicates(items)[i];
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

    static void Isolate(Item item)
    {
        items.RemoveAll(x => x != item && x.dataIndex == item.dataIndex);
        waitingForItem = false;
    }

    static void Add(Item it)
    {
        items.Add(it);
        waitingForItem = false;
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
        Logue.New("search", "[SEARCH] : " + key, Color.magenta);

        // check if parent item
        if (key.Contains('/'))
        {
            string[] parts = key.Split(new char[] { '/' });
            parentItem = SearchItem(parts[0]);
            key = parts[1];
        }

        // get type if not default
        Type type = Type.First;
        if (key.Contains('.'))
        {
            string[] parts= key.Split(new char[] { '.' });
            type = GetType(parts[0]);
            key = parts[1];
        }

        List<Item> items = GetPotentialItems(key);

        if (items.Count == 0)
        {
            Logue.Add("no results with key " + key + " in available items");
            modifiedKey = "error";
            return null;
        }


        // before sorting by type, check if the item's name appears in input
        string inputText = InputInfo.Instance.inputText;
        Item item = items.Find(x => x.ContainedInText(inputText));

        if ( item != null)
        {
            type = Type.Input;
            goto Result;
        }

        // if no results, sort by type
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
                break;
            case Type.Random:
                item = items[UnityEngine.Random.Range(0, items.Count)];
                break;
            default:
                item = items[0];
                break;
        }

        Result:


        history.Add(new ItemKey(key, item));

        Logue.Add($"[{type}]{key} : {item.debug_name}");
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

    public enum MatchType
    {
        Input,
        Info,
        Property,
    }
    private static List<Item> GetPotentialItems(string content)
    {
        // first search in history
        // IMPORTANT for target ... and target/body part
        // so that the target is the same and not searched twice
        ItemKey key = history.Find(x => x.key == content);

        if (key != null)
        {
            Logue.Add($"[HISTORY] : {key.item.debug_name}");
            return new List<Item> { key.item };
        }

        List<Predicate<Item>> matches = new List<Predicate<Item>>()
        {
            x => x.debug_name == content,
            x => x.HasInfo(content),
            x => x.HasProperty(content)
        };

        List<Item> list= new List<Item>();
        for (int matchIndex = 0; matchIndex < 3; matchIndex++)
        {
            MatchType matchType = (MatchType)matchIndex;
            Predicate<Item> match = matches[(int)matchIndex];
            list = GetPotentialItems(content, match);

            if (list != null && list.Count > 0)
            {
                Logue.Add($"[{matchType}] : " + TextManager.ListItems(list));
                parentItem = null;
                return list;
            }
        }

        parentItem = null;
        Logue.Add($"[NO MATCH]{key}");
        return list;
    }

    private static List<Item> GetPotentialItems (string content, Predicate<Item> match)
    {

        
        List<Item> tmp_items = new List<Item>();
        
        // search for item in pending items
        if ( parentItem != null)
        {
            Logue.Add("searching in " + parentItem.debug_name);
            tmp_items.AddRange(parentItem.GetAllItems().FindAll(match));
            return tmp_items;
        }
        

        // add available items
        tmp_items.AddRange(AvailableItems.GetItems.FindAll(match));
        if (tmp_items.Count > 0)
        {
            Logue.Add("results in available items");
            return tmp_items;
        }

        return null;
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
