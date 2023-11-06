using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;
using Unity.Android.Types;

[System.Serializable]
public class ItemGroup {
    public ItemGroup(int index, Word.Number num) {
        this.index = index;
        this.num = num;
    }
    public int index;
    public Word.Number num;
    public List<Item> items = new List<Item>();
    public string text;

    public bool tryInit() {
        if (Regex.IsMatch(text, @$"\ball\b")) {
            // do nothing, not deleting any other item in the group
        }

        // check some 
        if (Regex.IsMatch(text, @$"\bsome\b")) {
            Debug.Log("found some in " + getFirst().debug_name);
            float f = (float)items.Count / 2;
            int half = (int)Mathf.Clamp(Mathf.Round(f), 1, items.Count);
            if (half < items.Count)
                items.RemoveRange(half, items.Count - half);
        }

        // check for numerics
        foreach (var str in text.Split(' ')) {
            int count = 0;
            if (str.All(char.IsDigit) && int.TryParse(str, out count)) {
                Debug.Log($"found number {count} in item group of {getFirst().debug_name}");
                for (int i = count; i < items.Count; i++)
                    items.RemoveAt(i);
                break;
            }
        }

        if (num == Word.Number.Singular) {

            bool specMatch = items.TrueForAll(x => x.specMatch(getFirst()));
            if (getFirst().HasInfo("dif") && !specMatch) {
                // check for distinct item
                Item specificItem = getSpecific();
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

    public bool ItemAreTheSame() {
        bool specMatch = items.TrueForAll(x => x.specMatch(getFirst()));
        return !getFirst().HasInfo("dif") && specMatch;
    }

    Item getSpecific() {
        // try to find an item spec in the input
        assignOrdinates();
        Spec spec = null;
        string text = ItemParser.GetCurrent.lastInput;
        return items.Find(x => x.textHasSpecs(text, out spec));
    }
    private void assignOrdinates() {
        for (int i = 0; i < items.Count; i++) {
            string ordinal = GetOrdinal(i);
            Spec ordinalSpec = items[i].getKeyInfo("ordinal");
            if (ordinalSpec != null) {
                ordinalSpec.searchValue = ordinal;
                ordinalSpec.displayValue = ordinal;
            } else
                items[i].setSpec(ordinal, ordinal, "ordinal", false);
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

    public Item getFirst() { return items[0]; }

    public static List<ItemGroup> getItemGroups(List<Item> targetItems, string filter = "") {
        var groups = new List<ItemGroup>();
        int index = 0;
        foreach (var item in targetItems) {
            Word.Number num = Word.Number.None;
            index = string.IsNullOrEmpty(filter) ? item.dataIndex : item.getIndexInText(filter, out num);
            if (index >= 0) {
                var itemgroup = groups.Find(x => x.index == index);
                if (itemgroup == null) {
                    itemgroup = new ItemGroup(index, num);
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