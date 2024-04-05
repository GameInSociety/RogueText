using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

public static class ItemLink
{
    public static string failMessage;
    static Item sourceItem;

    public static Item SearchItem(string _key) {

        failMessage = "";

        sourceItem = WorldAction.current.TargetItem();

        // KEY TRIM //
        string key = _key;
        
        // SET START INDEX ( IF TILE )
        int startIndex = key.Contains('[') ? key.IndexOf(']') : 0;

        // remove prop > symbols
        int propIndex = key.IndexOf('>', startIndex);
        if ( propIndex >= 0)
            key = key.Remove(propIndex);

        // remove item ! symbol
        int itemIndex = key.IndexOf('!', startIndex);
        if (  itemIndex >= 0 ) {
            key = key.Remove(0, startIndex);
            key = key.Remove(0, itemIndex+1);
        }
        key = key.Trim(' ');

        // search in history
        var itemInHistory = ItemLink.CheckForItemInHistory(key);
        if (itemInHistory != null) {
            Function.ADDLOG($"(h:{itemInHistory.debug_name})", Color.yellow);
            return itemInHistory;
        }

        // special searches
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
       
        // get result
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

        history.Add(new ItemHistory(key, item));

        return item;
    }

    public static string ReplacePropLinks(Item item, string key) {
        var propLinkIndex = key.IndexOf('{');
        while (propLinkIndex >= 0) {
            string propKey = TextUtils.Extract('{', key, out key);
            Debug.Log($"prop key : {propKey}");
            var newPart = new ActionPart(propKey);
            if (!newPart.TryInit(item)) {
            }
            Debug.Log($"prop : {newPart.prop.name}");
            var insertKey = newPart.prop.GetTextValue();
            key = key.Insert(propLinkIndex, insertKey);
            propLinkIndex = key.IndexOf('{');
        }
        return key;
    }

    public static List<Item> ReadKey(string key) {

        Debug.Log($"KEY:{key}");
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
                Function.ADDLOG($">in parent {parent[0].debug_name}", Color.gray);
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

        if (key.StartsWith('['))
            return new List<Item>() { GetTile(key) };

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

    static Item GetTile(string key) {
        key = TextUtils.Extract('[', key, out _);
        var tilesetId = Player.Instance.tilesetId;
        // check for other tile set
        if (key.Contains('{')) {
            var tilesetKey = TextUtils.Extract('{', key, out key);
            if (tilesetKey.Contains('>')) {
                var tsIt = sourceItem;
                if (tilesetKey.Contains('!')) {
                    tsIt = SearchItem(tilesetKey);
                    if (tsIt == null)
                        return null;
                }
                var tilesetProp = GetProperty(tilesetKey, tsIt);
                tilesetId = tilesetProp.GetNumValue();
            } else {
                tilesetId = int.Parse(tilesetKey);
            }
        }

        // check for addition
        var split = key.Split('+');
        var coords = new Coords();
        foreach (var s in split) {

            var ap = new ActionPart(s);
            if(!ap.TryInit(sourceItem)) {
                Debug.LogError($"action part fail in : {s}");
                return null;
            }
            var newCoords = Coords.zero;
            if (ap.HasProp()) {
                newCoords = Coords.PropToCoords(ap.prop, tilesetId);
                ap.prop.SetValue(Coords.CoordsToText(newCoords));
            } else {
                newCoords = Coords.TextToCoords(s, tilesetId);
            }
            coords += newCoords;
        }

        var result = TileSet.tileSets[tilesetId].GetTile(coords);
        if (result == null) {
            Debug.LogError($"no tile for {key}");
            return null;
        }
        return result;
    }

    static void LOG(string message, Color c) {
        Function.LOG(message, c);
    }

    public static List<Item> GetItemsWithProp (string key, List<Item> range) {
        key = key.Remove(0, key.IndexOf("ANY") + "ANY".Length).Trim(' ');

        bool highest = false;
        if ( key.StartsWith("H ")) {
            highest = true;
            key = key.Substring(2).Trim(' ');
        }

        var results = range.FindAll(x => x.GetProp(key) != null && x != sourceItem);
        if (results.Count == 0)
            DisplayFail($"nothing here has {key}");

        if(highest) {
            var hItem = results[0];
            for (int i = 1; i < results.Count; i++) {
                if (results[i].GetProp(key).GetNumValue() > hItem.GetProp(key).GetNumValue()) {
                    hItem = results[i];
                }
            }
            return new List<Item> { hItem };
        }

        return results;
    }
    public static Item CheckForItemInHistory(string key) {
        return history.Find(x => x.key == key).item;
    }

    public struct ItemHistory {
        public ItemHistory(string k, Item i) {
            key = k;
            item = i;
        }
        public string key;
        public Item item;
    }
    public static List<ItemHistory> history = new List<ItemHistory>();
}
