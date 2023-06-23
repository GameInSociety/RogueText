using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class ItemGroup
{
    public static List<ItemGroup> debug_groups = new List<ItemGroup>();

    public string text;
    public List<Item> items = new List<Item>();
    public Item pendingItem;
    public bool waitForItem = false;

    public ItemGroup()
    {

    }
    public ItemGroup(ItemGroup copy)
    {
        items = copy.items;
        pendingItem = copy.pendingItem;
        waitForItem = copy.waitForItem;
        text = copy.text;

        debug_groups.Add(this);

    }

    public List<Item> GetItems
    {
        get
        {
            return items;
        }
    }

    public bool Empty
    {
        get
        {
            return items.Count == 0;
        }
    }

    public void Clear()
    {
        items.Clear();
        waitForItem = false;
    }

    public static ItemGroup New()
    {
        ItemGroup itemGroup = new ItemGroup();

        debug_groups.Add(itemGroup);

        return itemGroup;

    }
    public Verb.Sequence GetSequence()
    {
        Item cellItem = null;

        foreach (Item item in items)
        {
            if (Verb.GetCurrent.HasCell(item))
            {
                cellItem = item;
                break;
            }
        }

        if ( cellItem == null)
        {
            return null;
        }

        items.Remove(cellItem);
        items.Insert(0, cellItem);
        return Verb.GetCurrent.GetSequence(cellItem);
    }

    public void Init(string _text)
    {
        text = _text;

        List<Item> availableItems = AvailableItems.Get;
        List<Item> tmpItems = new List<Item>(items);
        
        List<Item> range = availableItems.FindAll(x => x.ContainedInText(text));

        if (Regex.IsMatch(text, @$"\bit\b") || Regex.IsMatch(text, @$"\bthem\b"))
        {
            if (pendingItem != null)
            {
                range = new List<Item>() { pendingItem };
            }
        }

        items.AddRange(range);


        // try verb alone
        if (items.Count == 0 && Verb.HasCurrent)
        {
            Item verbItem = Item.GetDataItem("no item");
            if (Verb.GetCurrent.HasCell(verbItem))
            {
                items.Add(verbItem);
            }
        }

        if (items.Count > 1 /*&& */)
        {
            HandleMultipleItems();
        }

    }

    void HandleMultipleItems()
    {
        if (GetSimilarItems().Count == 0)
        {
            // no similar items, keep all
            return;
        }

        if (!Differenciate(items))
        {
            // Don't differenciate item, throwing any
            // ex there are some plates
            if (items[0].word.currentNumber == Word.Number.Plural)
            {
                // leave because taking all plates
                return;
            }
            else
            {
                // just the first plate if the word is singular
                items.RemoveRange(1, items.Count - 1);
                return;
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
        foreach (var cItem in items)
        {
            foreach (var item in items)
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
                items.Remove(containerItem);
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
            }
            else if (items.Contains(pendingItem))
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

    Item GetNumberItem()
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

    void SetSpecificItem(Item item)
    {
        pendingItem = item;

        waitForItem = false;

        /*int specItemIndex = list.IndexOf(item);
        Debug.Log("index of : " + item.debug_name + " is " +  specItemIndex);*/

        items.RemoveAll(x => x.SameTypeAs(item));
        items.Insert(0, item);
    }


    public void WaitForSpecificItem(string message)
    {
        if (waitForItem)
        {
            Debug.LogError("already waiting for an item");
            InputInfo.Instance.Reset();
            return;
        }

        if ( items.Count == 0 )
        {
            TextManager.Write(message);
        }
        else
        { 
            TextManager.Write(message, GetSimilarItems()[0]);
        }

        waitForItem = true;
    }

    public List<Item> GetSimilarItems()
    {
        List<Item> items = new List<Item>();

        if ( this.items.Count == 1) {
            return items;
        }

        foreach (var item in this.items)
        {
            List<Item> ts = this.items.FindAll(x => x.SameTypeAs(item));
            if ( ts.Count > 1)
            {
                items = ts;
                break;
            }
        }

        return items;
    }

    public bool HasItem(string itemName)
    {
        return FindOfType(itemName) != null;
    }

    public Item GetItem(int i = 0)
    {
        if (i >= items.Count)
        {
            Debug.LogError("Function GetItem : outside range ");
            return null;
        }

        return items[i];
    }

    public Item FindOfType(string itemName)
    {
        return items.Find(x => x.debug_name == itemName);
    }

    public bool Differenciate(List<Item> items)
    {
        bool containerPresent = items.Find(x => x.ContainsItems()) != null;

        bool allIdentical = items.TrueForAll(x =>
        x.debug_name == items.First().debug_name
        &&
        !x.ContainsItems());

        /*Debug.Log("all identical : " + allIdentical);
        Debug.Log("container present : " + containerPresent);*/

        return !allIdentical && containerPresent;
    }
}
