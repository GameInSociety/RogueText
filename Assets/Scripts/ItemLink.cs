using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public static class ItemLink
{
    public static List<Item> itemHistory = new List<Item>();
    public static List<Property> propHistory = new List<Property>();
    public static void ClearHistory() {
        itemHistory.Clear(); propHistory.Clear();
    }
    public static void AddToHistory(Item item) {
        itemHistory.Add(item);
    }
    public static void AddToHistory(Property prop) {
        propHistory.Add(prop);
    }
    public static Item GetPreviousItem() {
        return itemHistory[0];
    }
    public static Property GetPreviousProp() {
        return propHistory[0];
    }


    public static Item SearchItem(string key) {

        if ( key == "pitem")
            return GetPreviousItem();
        var result = ReadKey(key);
        if (result == null)
            return null;

        itemHistory.Add(result);
        return result;
    }

    public static Item ReadKey(string key) {
        if (key.StartsWith('['))
            key = TextUtils.Extract('[', key);

        var parentItem = (Item)null;
        if (key.Contains('.')) {
            // split
            var parts = key.Split('.');
            // get the parent item
            parentItem = SearchItem(parts[0]);
            key = parts[1];
            if (parentItem == null) {
                Debug.LogError($"parent item {key} was not found");
                return null;
            }
        }

        // if parent, search in children
        if ( parentItem != null )
            return SearchItemInRange(key, parentItem.GetChildItems(2));

        // if no current item parser, only in surrnouding items ( for events )
        if ( ItemParser.GetCurrent == null )
            return SearchItemInRange(key, AvailableItems.currItems);
        
        // when the key starts with *, it search will only occur in the input items
        if (key.StartsWith('*')) {
            var inputItem = SearchItemInRange(key.Substring(1), ItemParser.GetCurrent.GetOptionalItems());
            if (inputItem == null ) {
                if (ItemParser.GetCurrent.HasSecondItem()) {
                    Fail($"{ItemParser.GetCurrent.GetOptionalItems().First().GetText("the dog")} can't do that");
                } else {
                    Fail($"specify in what you want to {ItemParser.GetCurrent.lastInput}");
                }
            }
            return inputItem;
        }

        // first serach in parsed items
        var parserItem = SearchItemInRange(key, ItemParser.GetCurrent.GetOptionalItems());
        if (parserItem != null)
            return parserItem;

        var surroundingItem = SearchItemInRange(key, AvailableItems.currItems);

        return surroundingItem;
    }

    static void Fail(string message) {
        Function.Fail(message);
        //WorldAction.current.Fail(message);
    }

    static void Error(string message) { Debug.LogError(message);}

    public static Item SearchItemInRange(string key, List<Item> range) {
        // this searches in the parser
        if (key.StartsWith("ANY ")) return GetItemWithProp(key, range);
        if (key == "tile") return Tile.GetCurrent;
        // exclude current item
        if (range.Contains(WorldAction.current.itemGroup.first)) {
            range.Remove(WorldAction.current.itemGroup.first);
        }
        var result = range.Find(x => x.HasWord(key));
        return result;
    }

    public static Property GetProperty(string key, Item item) {
        if (key == "pprop")
            return GetPreviousProp();

        var initItem = item;
        var targetItem = item;
        var prop = (Property)null;
        if (key.Contains('>')) {
            var split = key.Split('>');
            targetItem = SearchItem(split[0]);
            if (targetItem== null) {
                Fail($"[ITEM LINK] (GetProperty) : failed searching for item {split[0]} in {key}");
                return null;
            }
            key = split[1];

        }
        prop = targetItem.GetProp(key);
        if ( prop == null) {
            Fail($"[ITEM LINK] (GetProperty) : failed searching for prop in {key} / item : {targetItem.debug_name}");
            return null;
        }
        propHistory.Add(prop);
        return prop;
    }

    public static Item GetItemWithProp (string key, List<Item> range) {
        var propName = key.Remove(0,"ANY ".Length);
        // remove action item from range
        if (range.Contains(WorldAction.current.itemGroup.first)) {
            Debug.Log($"[{key}] : the range contains the current item");
            range.Remove(WorldAction.current.itemGroup.first);
        }
        return range.Find(x => x.GetProp(propName) != null);
    }
}
