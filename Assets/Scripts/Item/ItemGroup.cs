using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

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
    public List<Property> linkedProps = new List<Property>();
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


        string digit_str = Regex.Match(text, @"\d+").Value;
        int digit = 0;
        if ( int.TryParse(digit_str, out digit)) {
            if ( digit >= items.Count) {
                TextManager.Write($"they are only {items.Count} {first.GetText("dogs")}");
                return false;
            }
            for (int i = digit; i < items.Count; i++)
                items.RemoveAt(i);
        }

        if (num == Word.Number.Singular) {

            // will look for a specific item any way
            Item specificItem = GetSpecificItem();
            if (specificItem != null) {
                Debug.Log($"found specific item {specificItem.debug_name}");
                items.RemoveAll(x => x != specificItem);
            } else {
                // but will return a probleme only if the item has dif
                if (first.HasProp("dif")) {
                    return false;
                } else {
                    Debug.Log($"removing all other {first.debug_name}");
                    items.RemoveRange(1, items.Count - 1);
                }
            }

            
        }

        return true;
    }


    private Item GetSpecificItem() {
        // try to find an item spec in the input
        AssignOrdinalProps();
        string text = ItemParser.GetCurrent.lastInput;
        var i = 0;
        foreach (var item in items) {
            var prop = item.GetPropInText(text, out i);
            if ( prop != null) {
                Debug.Log($"found prop {prop.name}");
                return item;
            }
        }
        Debug.Log($"no prop to distinguish {first.debug_name}");
        return null;
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
            "GetMainItem",
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

}