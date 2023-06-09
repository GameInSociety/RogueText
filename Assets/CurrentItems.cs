using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Progress;

public static class CurrentItems
{
    public static List<Item> list = new List<Item>();

    public static Item pendingItem;

    public static bool foundItem = true;

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
        Debug.Log("clear current items");
        list.Clear();
        foundItem = true;
    }

    public static void FindAll(string _text)
    {
        text = _text;

        List<Item> availableItems = AvailableItems.Get;

        if (!foundItem)
        {
            Debug.Log("search for more item");
            List<Item> range = availableItems.FindAll(x => x.ContainedInText(text));

            list.AddRange(range);

            foreach (var item in range)
            {
                Debug.Log("adding : " + item.debug_name);
            }
            foundItem = true;


            if ( range.Count == 0)
            {
                Clear();
            }

        }
        else
        {
            list = availableItems.FindAll(x => x.ContainedInText(text));

                Debug.Log("item are :");
            foreach (var item in list)
            {
                Debug.Log(item.debug_name);
            }
        }

        if (list.Count > 1 /*&& */)
        {
            if (HasSimilarItems(list))
            {
                Debug.Log("has duplicates");
                if (AllItemsAreSimilar(list))
                {
                    // ex there are some plates
                    if (list[0].word.defaultNumber == Word.Number.Plural)
                    {
                        // leave because taking all plates
                    }
                    else
                    {
                        // just the first plate if the word is singular
                        list.RemoveRange(1, list.Count-1);
                    }


                }
                else
                {
                    DifferenciateItems();
                }
            }
        }

        // try verb alone
        if (list.Count == 0 && Verb.HasCurrent)
        {
            Item verbItem = ItemManager.Instance.GetDataItem("verbe seul");
            if (Verb.GetCurrent.HasCell(verbItem))
            {
                list.Add(verbItem);
            }
        }

        DebugManager.Instance.currentItems = list;
    }

    public static void DifferenciateItems()
    {
        Item containerItem = null;
        Item specItem = null;
        
        foreach (var cItem in list)
        {
            foreach (var sItem in list)
            {
                if (cItem.HasItem(sItem))
                {
                    Debug.Log("[SPEC ITEM] item : " + sItem.debug_name);
                    Debug.Log("[SPEC ITEM] container : " + cItem.debug_name);
                    containerItem = cItem;
                    specItem = sItem;
                    break;
                }
            }
        }

        string[] nums = new string[5]
        {
            "first",
            "second",
            "third",
            "fourth",
            "fifth"
        };

        if (specItem == null)
        {
            int i = 0;
            foreach (var num in nums)
            {
                if (text.Contains(num))
                {
                    Debug.Log("text contains " + num + " returning item " + list[i] + " id " + list[i].debug_randomID);
                    SetSpecificItem(list[i]);
                    return;
                }
                ++i;
            }
        }


        if (specItem != null)
        {
            if ( containerItem != null)
            {
                list.Remove(containerItem);
            }
            SetSpecificItem(specItem);
            return;
        }

        if (pendingItem != null)
        {
            if (list.Contains(pendingItem))
            {
                Debug.Log("getting pending item");
                SetSpecificItem(pendingItem);
                return;
            }

            pendingItem = null;
            AskForSpecificItem("input_itemConfusion");
            return;
        }
        else
        {
            AskForSpecificItem("input_itemConfusion");
            return;
        }
    }

    static void SetSpecificItem(Item item)
    {
        pendingItem = item;

        list.RemoveAll(x => x.SameTypeAs(item));
        list.Insert(0, item);
        Debug.Log("setting spec item : " + item.debug_name);
    }


    public static void AskForSpecificItem()
    {
        Debug.Log("ask for specific");
        foundItem = false;
    }
    public static void AskForSpecificItem(string message)
    {

        if( Verb.GetCurrent == null)
        {
            message = "Which &dog&";
        }
        TextManager.Write(message, list[0]);
        AskForSpecificItem();
    }

    public static bool HasSimilarItems(List<Item> items)
    {
        if ( items.Count == 1) {
            return false;
        }

        for (int i = 1; i < items.Count; i++)
        {
            if (items[i].SameTypeAs(items[0]))
            {
                return true;
            }
        }

        return false;
    }

    public static bool AllItemsAreSimilar(List<Item> items)
    {
        return items.TrueForAll(x => x.debug_name == items.First().debug_name) && !items[0].info.differenciate;
    }
}
