
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class IP_Part {

    // Data
    public string startText;
    public string finalText;

    // ID
    public int index;
    
    // States
    public bool skip;
    public string problem;
    public bool used;

    public int number = -1;
    public List<Item> items = new List<Item>();
    public List<Property> properties = new List<Property>();
    private ItemParser parser;

    public enum SortType {
        Single,
        Plural,
    }

    public IP_Part(string inputText, ItemParser parser) {
        startText = inputText;
        SetText(startText);
        this.parser = parser;
    }

    public void Parse() {
        GetNumber();
        GetItems();
    }

    // This removes all of the text that was succefully USED in the input.
    public void ClearText() {
        foreach (var str in extracts) {
            SetText(Regex.Replace(GetText, @$"\b{str}\b", ""));
        }
    }


    public void GetNumber() {
        string str = Regex.Match(GetText, @"\d+").Value;
        if (string.IsNullOrEmpty(str))
            return;
        ExtractFromText(str);
        number = int.Parse(str);
    }


    /// <summary>
    /// Getting and sorting items
    /// </summary>
    #region ITEMS
    public void GetItems() {
        // Check if input contains reference to any available item.
        items = GetItemsFromText(AvailableItems.GetAll(), GetText);

        // If no result, check if input contains reference to a PROPERTY from an item in the AvailableItems.
        if (items.Count == 0)
            items = AvailableItems.GetAll().FindAll(x => GetPropsFromText(x).Count > 0);
    }

    public void SortItems() {
        // No sorting if lone item
        if (items.Count == 1)
            return;

        // the "some" keyword will select HALF of the items
        if (ExtractFromText("some")) {
            float f = (float)items.Count / 2;
            int half = (int)Mathf.Clamp(Mathf.Round(f), 1, items.Count);
            if (half < items.Count)
                items.RemoveRange(half, items.Count - half);
            return;
        }

        // Check input for particular number of items desired
        if (number >= 0) {
            if (number >= items.Count)
                return;
            for (int i = number; i < items.Count; i++)
                items.RemoveAt(i);
            return;
        }

        // Properties to get item by ordian ("first house, "fifth apple" etc...);
        AssignOrdinalProps(items);

        // Check if part refers to an item in ANOTHER part. The other part needs to not be used ( See GetItemKeys )
        // Example : "take apple from bag in left hand"
        // A checker. 
        var itemFromOtherPart = (Item)null;
        if (index == 0 && parser.parts.Length > 1) {
            var nextPart = parser.parts[index + 1];
            // In this cas, checks if the first item is contained ( with property "contained" ) in an item from another part.
            nextPart.GetPropsFromText(items.First());

            // For each item in this part
            foreach (var item in items) {
                // Check if a property is refered in the text of another part.
                var referedProperties = nextPart.GetPropsFromText(item);
                if (referedProperties.Count > 0) {
                    nextPart.SetUsed();
                    items.RemoveAll(x => x != item);
                    return;
                }
            }
        }

        // look for an specific item with a property
        var propertyReferedItems = items.FindAll(sampleItem => GetPropsFromText(sampleItem).Count > 0);
        if (propertyReferedItems.Count > 0)
            items = propertyReferedItems;

        // If the input fetched multiple items from the Availble Items, but the word is singular, return first.
        if (MainItem().GetWord().currentNumber == Word.Number.Plural) {
            // Do nothing.
        } else {
            // Select first of list
            items = new List<Item> { items.First() };
        }
    }
    #endregion

    public List<string> extracts = new List<string>();

    // This method checks ( strickly with regex ) if a text contains a string, the extracts it from the text to show it's been used.
    public bool ExtractFromText(string str) {
        if (Regex.IsMatch(GetText, @$"\b{str}\b")) {
            if (!extracts.Contains(str))
                extracts.Add(str);
            return true;
        }
        return false;
    }

    /// <summary>
    /// GET & SET
    /// </summary>
    /// <returns></returns>
    public bool HasItems() {
        return items.Count > 0;
    }
    public Item MainItem() {
        return items.First();
    }
    public void SetUsed() {
        used = true;
    }
    #region Item Getter

    public List<Item> GetItemsFromText(List<Item> range, string targetText) {
        return range.FindAll(x => IsItemReferedInText(x, targetText));
    }
    public bool IsItemReferedInText(Item item, string targetText) {
        int wIndex = 0;
        // Refered by item words.
        foreach (var word in item.GetData().words) {
            for (int i = 0; i < 2; i++) {
                var num = (Word.Number)i;
                string _word = word.getText(num);
                if (ExtractFromText(_word)) {
                    word.currentNumber = num;
                    item.wordIndex = wIndex;
                    return true;
                }
            }
            ++wIndex;
        }

        // Refered by type.
        if (!item.HasProp("types"))
            return false;
        var typeParts = item.GetProp("types").parts;
        foreach (var type in typeParts) {
            string bound = type.key;
            if (ExtractFromText(bound)) {
                Debug.Log($"Found item : {item.DebugName} with type {bound}");
                return true;
            }
        }

        return false;
    }
    #endregion

    // Property refered in text
    #region property text
    public List<Property> GetPropsFromText(Item sampleItem) {
        var result = new List<Property>();
        // Find reference to property description
        foreach (var prop in sampleItem.GetDescribableProps()) {
            var description = prop.GetDescription();
            // Refered by exacted current description ("Put FILLED bottle in case")
            if (ExtractFromText(description))
                result.Add(prop);
            // Refered by property's name ("Put flammable thing in water")
            if (ExtractFromText(prop.name))
                result.Add(prop);
        }

        // Refered by key ( invisible part in property only for matching )
        foreach (var prop in sampleItem.props.FindAll(x => x.HasPart("key"))) {
            var keys = prop.GetContent("key").Split('/').ToList();
            var key = keys.Find(x => ExtractFromText(x));
            if (key!=null)
                result.Add(prop);
        }
        return result;
    }
    #endregion

    /// <summary>
    /// ORDINAL PROPERTIES
    /// </summary>
    /// <param name="items"></param>
    #region ordinal properties
    void AssignOrdinalProps(List<Item> items) {
        foreach (var itemGroup in items.GroupBy(x => x.DebugName)) {
            for (int i = 0; i < itemGroup.Count(); i++) {
                var item = itemGroup.ToList()[i];
                string ordinal = GetOrdinal(i);
                var ordinal_prop = item.GetProp("ordinal");
                if (ordinal_prop == null) {
                    ordinal_prop = new Property();
                    ordinal_prop.name = "ordinal";
                    ordinal_prop.AddPart("key", ordinal);
                }
                item.SetProp($"ordinal | key:{ordinal}");
            }
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
        if (i >= ordinals.Length) {
            return "other";
        }
        return ordinals[i];
    }
    #endregion

    public string GetText {
        get {
            return finalText;
        }
    }
    public void SetText(string str) {
        finalText = str.Trim(' ');
    }
}