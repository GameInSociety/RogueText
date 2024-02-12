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
    [System.Serializable]
    public struct HistoryKey_Item {
        public HistoryKey_Item(Item item, string key) {
            this.item = item;
            this.key = key;
        }
        public Item item;
        public string key;
    }
    [System.Serializable]
    public struct HistoryKey_Prop {
        public HistoryKey_Prop(Property prop, string key) {
            this.prop = prop;
            this.key = key;
        }
        public Property prop;
        public string key;
    }
    public static List<HistoryKey_Item> history_Items = new List<HistoryKey_Item>();
    public static List<HistoryKey_Prop> history_props = new List<HistoryKey_Prop>();
    public static void ClearHistory() {
        history_props.Clear(); history_Items.Clear();
    }
    public static void AddToHistory(Item item, string key) {
        //history_Items.Add(new HistoryKey_Item(item, key));
    }
    public static void AddToHistory(Property prop, string key) {
        //history_props.Add(new HistoryKey_Prop(prop, key));
    }
    public static Property GetPropInHistory(string key) {
        return history_props.Find(x=> x.key == key).prop;
    }
    public static Item GetItemInHistory(string key) {
        return history_Items.Find(x=> x.key == key).item;
    }

    public static List<Item> SearchItems(string key) {
        // get in history
        /*var result = GetItemInHistory(key);

        if ( result != null)
            return result;*/

        var results = ReadKey(key);

        if (results.Count == 0)
            return null;

        foreach (var item in results) {
            AddToHistory(item, key);
        }
        return results;
    }

    public static Item SearchItem(string key) {
        // get in history
        /*var result = GetItemInHistory(key);

        if ( result != null)
            return result;*/

        var results = ReadKey(key);

        if (results.Count ==0)
            return null;

        var item = results[0];

        AddToHistory(item, key);
        return item;
    }

    public static List<Item> ReadKey(string key) {

        Debug.Log($"GET ITEM : in key {key}");

        // first, check if the search will be in another item
        var parentItem = (Item)null;
        if (key.Contains('.')) {
            // split
            var parts = key.Split('.');
            // get the parent item
            parentItem = ReadKey(parts[0])[0];
            key = parts[1];
            if (parentItem == null) {
                Debug.LogError($"parent item {key} was not found");
                return null;
            }
        }

        

        // if parent, search in children
        if ( parentItem != null )
            return SearchItemsInRange(key, parentItem.GetChildItems(2));

        // if no current item parser, only in surrnouding items ( for events )
        if ( ItemParser.GetCurrent == null )
            return SearchItemsInRange(key, AvailableItems.currItems);
        
        // when the key starts with *, it search will only occur in the input items
        if (key.StartsWith("!p")) {
            Debug.Log($"searching for key : {key} only in parser");
            if ( ItemParser.GetCurrent == null) {
                Debug.LogError($"no current item parser when searching for things");
                return null;
            }
            var inputItem = ItemParser.GetCurrent.SearchForItemsInParts(key);
            if (inputItem == null ) {
                Debug.Log($"no item found for key {key}");
                var verb = ItemParser.GetCurrent.verb;
                var currItem = ItemParser.GetCurrent.GetPart(0).items[0];
                Fail($"specify in what you want to {verb.GetCurrentWord} {currItem.GetText("the dog")} {verb.GetPreposition}");
            }
            return inputItem;
        }

        // first serach in parsed items
        var parserItem = ItemParser.GetCurrent.SearchForItemsInParts(key);
        if (parserItem != null)
            return parserItem;

        var surroundingItem = SearchItemsInRange(key, AvailableItems.currItems);

        return surroundingItem;
    }

    static void Fail(string message) {
            Function.Fail(message);
    }

    static void Error(string message) { Debug.LogError(message);}

    public static List<Item> SearchItemsInRange(string key, List<Item> range) {
        Debug.Log($"search item in range : in key : {key}");
        // first, remove symbols
        // you NEED the symbols tu m'entends, sinon non seulement on comprend rien mais en plus ça t'empeche de faire des trucs de check en amont
        int symbol = key.IndexOf('!');
        if (symbol < 0) {
            Debug.LogError($"no ! in key : {key}");
        } else {
            key = key.Remove(0, symbol + 2);
            Debug.Log($"no key : {key}");
        }

        // this searches in the parser
        if (key.StartsWith("ANY ")) {
            Debug.Log($"searching item with prop : {key}");
            return GetItemsWithProp(key, range);
        }
        if (key == "parent") {
            Debug.Log($"linking to the parent of the current action item");
            Debug.Log($"current action item : {WorldAction.current.TargetItem().debug_name}");
            Debug.Log($"item parent : {WorldAction.current.TargetItem().GetParent()}");
            return new List<Item>() { WorldAction.current.TargetItem().GetParent() };
        }
        if (key == "tile") return new List<Item>() { Tile.GetCurrent };
        var result = range.FindAll(x => x.HasWord(key) && x != WorldAction.current.TargetItem());
        return result;
    }

    public static Property GetProperty(string key, Item item) {

        /*var prop = GetPropInHistory(key);

        if (prop != null) {
            Debug.Log($"found prop in history : {prop.name} with key {key}");
            return prop;
        }*/

        var prop = (Property)null;

        var initItem = item;
        var targetItem = item;
        if (key.Contains('>')) {
            Debug.Log($"prop in other item");
            var split = key.Split('>');
            targetItem = SearchItem(split[0]);
            if (targetItem== null) {
                Debug.Log($"no target item : {split[0]}");
                //Fail($"[ITEM LINK] (GetProperty) : failed searching for item {split[0]} in {key}");
                //Fail($"there's no {split[0]} around");
                return null;
            }
            key = split[1];
            Debug.Log($"continuing");

        }

        if (key.StartsWith("ANY ")) {
            key = key.Remove(0, 4);
            prop = targetItem.GetPropertyOfType("orientation");
        } else
            prop = targetItem.GetProp(key);

        if ( prop == null) {
            Fail($"[ITEM LINK] (GetProperty) : failed searching for prop in {key} / item : {targetItem.debug_name}");
            return null;
        }

        AddToHistory(prop, key);
        return prop;
    }

    public static List<Item> GetItemsWithProp (string key, List<Item> range) {
        key = key.Remove(0, "ANY ".Length);
        Debug.Log($"GETTING ITEM WITH PROP : {key}");

        if ( key.StartsWith("SORT OF")) {
            key = key.Remove(0,"SORT OF ".Length);
            Debug.Log($"looking for item with a prop of type : {key}");
            return range.FindAll(x => x.GetPropertyOfType(key) != null && x != WorldAction.current.TargetItem());
        } else
            return range.FindAll(x => x.GetProp(key) != null && x != WorldAction.current.TargetItem());
    }
}
