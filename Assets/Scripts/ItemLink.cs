using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemLink
{
    public static string failMessage;
    static Item sourceItem;
    public class ItemHistory {
        public static List<ItemHistory> list = new List<ItemHistory>();
        public static void Clear() {
            list.Clear();
        }
        public string key;
        public Item item;
        public ItemHistory(string  key, Item item) {
            this.key = key;
            this.item = item;
        }
    }

    public static Item SearchItem(string _key) {

        var histItem = ItemHistory.list.Find(x=>x.key == _key);
        if (histItem != null) {
            Function.LOG($"getting {histItem.item} from history ({histItem.key})", Color.white);
            return histItem.item;
        }

        failMessage = "";

        sourceItem = WorldAction.current.TargetItem();


        string key = _key;

        bool onlyInParser = false;
        bool random = false;
        if (key.StartsWith("INPUT ")) {
            key = key.Remove(0,"INPUT ".Length);
            onlyInParser = true;
        }
        if (key.StartsWith("RANDOM ")) {
            key = key.Remove(0, "RANDOM ".Length);
            random = true;
        }
       
        var results = ReadKey(key);

        if (results == null || results.Count == 0) {
            // if no fail message was there, set default one
            if (string.IsNullOrEmpty(failMessage))
                DisplayFail($"there's no {key} around");
            return null;
        }

        if ( ItemParser.GetCurrent != null) {
            var inParser = ItemParser.GetCurrent.GetItems().FindAll(x=> results.Contains(x));
            var strIt = "";
            foreach (var it in inParser)
                strIt += it.debug_name + " ";
            if ( inParser.Count > 0)
                return inParser[0];
            else if (onlyInParser) {
                DisplayFail($"specify something in the input");
                return null;
            }
        }
        var item = random ? results[Random.Range(0, results.Count)] : results[0];
        if (random) {
            Debug.Log($"picking random result : {item.debug_name}");
        }
        ItemHistory.list.Add(new ItemHistory(_key, item));
        return item;
    }

    public static string ReplacePropLinks(Item item, string key) {
        var propLinkIndex = key.IndexOf('{');
        while (propLinkIndex >= 0) {
            string propKey = TextUtils.Extract('{', key, out key);
            var newPart = new ActionPart(propKey);
            if (!newPart.TryInit(item)) {
            }

            var insertKey = newPart.prop.GetTextValue();
            key = key.Insert(propLinkIndex, insertKey);
            propLinkIndex = key.IndexOf('{');
        }
        return key;
    }

    public static List<Item> ReadKey(string key) {

        // first, check if the search will be in another item
        if (key.Contains('.')) {
            var range = AvailableItems.currItems;
            while (key.Contains('.')) {
                // split
                var parentKey = key.Split('.')[0];
                // get the parent item
                var parent = SearchItemsInRange(parentKey, range);
                if (parent == null || parent.Count == 0)
                    return null;
                range = parent[0].GetChildItems(2);
                key = key.Remove(0, key.IndexOf('.') + 1);
            }
            return SearchItemsInRange(key, range);
        }

        var results = SearchItemsInRange(key, AvailableItems.currItems);
        if ( results == null)
            return null;
        return results;
    }

    static void DisplayFail(string message, bool overrideMessage = false) {
        failMessage = message;
    }

    static void Error(string message) { Debug.LogError(message);}

    public static List<Item> SearchItemsInRange(string key, List<Item> range) {
        key = key.Trim(' ');

        Function.ADDLOG($"({key}) ", Color.gray);

        // this searches in the parser
        if (key.StartsWith("ANY"))
            return GetItemsWithProp(key, range);
        
        switch (key) {
            case "PARENT":
                return new List<Item>() { sourceItem.GetParent() };
            case "THIS":
                return new List<Item>() { sourceItem };
        }
        return range.FindAll(x => x.HasWord(key) && x != sourceItem);
    }

    public static Property GetProperty(string key, Item item) {


        var prop = (Property)null;
        if (key.StartsWith("ANY ")) {
            key = key.Remove(0, 4);
            prop = item.GetPropertyOfType("orientation");
        } else
            prop = item.GetProp(key);

        if ( prop == null) {
            DisplayFail($"{item.GetText("the dog")} doesn't have any {key}", true);
            return null;
        }
        return prop;
    }

    static void LOG(string message, Color c) {
        Function.LOG(message, c);
    }

    public static List<Item> GetItemsWithProp (string key, List<Item> range) {
        key = key.Remove(0, key.IndexOf("ANY") + "ANY".Length).Trim(' ');
        var results = range.FindAll(x => x.GetProp(key) != null && x != sourceItem);
        if (results.Count == 0)
            DisplayFail($"nothing here has {key}");
        return results;
    }
}
