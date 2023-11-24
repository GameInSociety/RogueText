using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;
using Unity.Android.Types;
using System.Net.Configuration;
using static UnityEngine.ParticleSystem;
using System.Runtime.InteropServices;
using UnityEditor;

/// <summary>
/// this class is for action execution only.
/// it's used to specify item to interact with
/// NOT for description.
/// for description, see ItemDescription.cs
/// </summary>
[System.Serializable]
public class ItemGroup {
    public string debug_name;
    public ItemGroup(int index, Word.Number num) {
        this.index = index;
        this.num = num;
    }
    public int index;
    public Word.Number num;
    public List<Item> items = new List<Item>();
    public List<ItemGroup> childGroups = new List<ItemGroup>();
    public Item first => items[0];
    public string text;

    public bool TryInit() {

        if (items.Count == 1)
            return true;

        if (Regex.IsMatch(text, @$"\ball\b")) {
            // do nothing, not deleting any other item in the group
        }

        // check some 
        if (Regex.IsMatch(text, @$"\bsome\b")) {
            Debug.Log("found some in " + first.debug_name);
            float f = (float)items.Count / 2;
            int half = (int)Mathf.Clamp(Mathf.Round(f), 1, items.Count);
            if (half < items.Count)
                items.RemoveRange(half, items.Count - half);
        }

        // check for numerics
        foreach (var str in text.Split(' ')) {
            int count = 0;
            if (str.All(char.IsDigit) && int.TryParse(str, out count)) {
                Debug.Log($"found number {count} in item group of {first.debug_name}");
                for (int i = count; i < items.Count; i++)
                    items.RemoveAt(i);
                break;
            }
        }

        if (num == Word.Number.Singular) {

            if (first.HasProp("dif") ) {
                // check for distinct item
                Item specificItem = GetSpecificItem();
                if (specificItem != null)
                    items.RemoveAll(x => x != specificItem);
                else
                    return false;

            } else {
                if (items.Count > 1)
                    items.RemoveRange(1, items.Count - 1);
            }
        }

        return true;
    }


    private Item GetSpecificItem() {
        // try to find an item spec in the input
        AssignOrdinalProps();
        Property prop = null;
        string text = ItemParser.GetCurrent.lastInput;
        return items.Find(x => x.CheckPropsInText(text, out prop));
    }
    private void AssignOrdinalProps() {
        for (int i = 0; i < items.Count; i++) {
            string ordinal = GetOrdinal(i);
            var ordinal_prop = items[i].GetProp("ordinal");
            if (ordinal_prop != null) {
                /*ordinal_prop.searchValue = ordinal;
                ordinal_prop.displayValue = ordinal;*/
            } else {
                ordinal_prop = new Property();
                ordinal_prop.name = "ordinal";
                ordinal_prop.AddPart("search", ordinal);
            }
            items[i].SetProp($"ordinal / search:{ordinal}");
        }
    }

    public string GetOrdinal(int i) {
        var ordinals = new string[10]
        {
            "first",
            "second",
            "third",
            "fourth",
            "fifth",
            "sixth",
            "seventh",
            "eighth",
            "ninth",
            "tenth",
        };
        return ordinals[i];
    }


    public static List<ItemGroup> GetGroups(List<Item> targetItems, string filter = "") {
        var groups = new List<ItemGroup>();
        int index = 0;
        foreach (var item in targetItems) {
            Word.Number num = Word.Number.None;
            index = string.IsNullOrEmpty(filter) ? item.GetData().index : item.GetIndexInText(filter, out num);
            if (index >= 0) {
                var itemgroup = groups.Find(x => x.index == index);
                if (itemgroup == null) {
                    itemgroup = new ItemGroup(index, num);
                    itemgroup.debug_name = item.debug_name;
                    itemgroup.text = filter;
                    groups.Add(itemgroup);
                }
                itemgroup.items.Add(item);
            }
        }

        groups.Sort((a, b) => a.index.CompareTo(b.index));
        return groups;
    }

}