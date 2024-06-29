using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

public static class ItemLink
{
    public static string failMessage;
    static Item sourceItem;

    public static Item SearchItem(string _key, Item _sourceItem) {

        failMessage = "";
        sourceItem = _sourceItem;

        // KEY TRIM //
        string key = _key;
        Log($"Item Key : {key}");
        // SET START INDEX ( IF TILE )
        int startIndex = key.Contains('{') ? key.IndexOf('}') : 0;
        // remove prop > symbols
        if ( startIndex < key.Length && startIndex >= 0) {
            int propIndex = key.IndexOf('>', startIndex);
            if (propIndex >= 0)
                key = key.Remove(propIndex);
        } else {
            Debug.LogError($"start index is at the end");
        }

        // remove item ! symbol
        int itemIndex = key.IndexOf('!', startIndex);
        if (itemIndex >= 0) {
            key = key.Remove(0, startIndex);
            key = key.Remove(0, itemIndex + 1);
        }
        key = key.Trim(' ');
        Log($"Trimed : {key}");
        // search in history
        var searchKey = key.Trim(new char[] { '{', '}' });
        var itemInHistory = CheckForItemInHistory(searchKey);
        if (itemInHistory != null) {
            Log($"From History ({searchKey}) : {itemInHistory.DebugName}");
            return itemInHistory;
        }

        Log($"nothing in history");
        var HKey = "";
        if (key.Contains(':')) {
            var split = key.Split(':');
            HKey = split[0];
            key = split[1];
            Log($"Create History Key : {key}");
        } else {
            HKey = key;
        }

        // special searches
        bool onlyInParser = false;
        if (key.StartsWith("INPUT ")) {
            key = key.Remove(0,"INPUT ".Length);
            Log($"Only In Parser : {key}");
            onlyInParser = true;
        }

        // get result
        var results = ReadKey(key);

        if (results == null || results.Count == 0) {
            ThrowFail($"No Item Found (key:{key})");
            // if no fail message was there, set default one
            if (string.IsNullOrEmpty(failMessage)) {
                DisplayFail($"there's no {key} around");
            }

            return null;
        }
        Log($"Item Range : {string.Join('/', results.Select(x => x._debugName))}");

        if ( ItemParser.Instance != null) {
            var inParser = ItemParser.Instance.GetItems().FindAll(x=> results.Contains(x));
            if ( inParser.Count > 0) {
                Log($"Found in Parser : {inParser.First().DebugName}");
                return inParser[0];
            } else if (onlyInParser) {
                ThrowFail($"None in Parser");
                DisplayFail($"specify something in the input");
                return null;
            }
        }

        if ( results.Count == 1)
            Log($"Only One Result");
        else
            Log($"Selecting Random");

        var item = results[Random.Range(0, results.Count)];
        HKey = HKey.Trim(' ');
        history.Add(new ItemHistory(HKey, item));
        return item;
    }

    public static List<Item> ReadKey(string key) {
        // first, check if the search will be in another item
        if (key.Contains('.')) {
            Log($"Searching In Parent");
            var range = AvailableItems.GetAll();
            while (key.Contains('.')) {
                // split
                var parentKey = key.Split('.')[0];
                // get the parent item
                var parent = SearchItemsInRange(parentKey, range);
                if (parent == null || parent.Count == 0) {
                    ThrowFail($"No parent Found");
                    return null;
                }
                range = parent[0].GetChildItems(2);
                key = key.Remove(0, key.IndexOf('.') + 1);
                Log($"Child New Key : {key}");
            }
            return SearchItemsInRange(key, range);
        }

        Log($"Searching in Available Items");
        var results = SearchItemsInRange(key, AvailableItems.GetAll());
        if ( results == null) {
            return null;
        }
        return results;
    }

    static void DisplayFail(string message, bool overrideMessage = false) {
        failMessage = message;
    }


    public static List<Item> SearchItemsInRange(string key, List<Item> range) {
        key = key.Trim(' ');

        if (key.StartsWith('{')) {
            var tile_result = GetTile(key);
            if (tile_result == null) {
                ThrowFail($"Found No Tile : {key}");
                return null;
            }
            return new List<Item>() {tile_result};
        }

        // this searches in the parser
        if (key.StartsWith("ANY"))
            return GetItemsWithProp(key, range);
        
        switch (key) {
            case "PARENT":
                var parent = sourceItem.GetParent();
                Log($"Getting The Parent of {sourceItem.DebugName} : {parent.DebugName}");
                return new List<Item>() { parent };
            case "THIS":
                Log($"Getting the source Item : {sourceItem.DebugName}");
                return new List<Item>() { sourceItem };
        }

        return range.FindAll(x => x.HasWord(key) );
    }

    public static Property GetProperty(string key, Item item) {

        Log($"Getting Property : {key} on Item {item.DebugName}");

        
        var prop = (Property)null;

        // get the name of an item
        if (key.StartsWith("N ")) {
            prop = new Property();
            prop.name = "Name";
            key = key.Remove(0, 2);
            string item_text = item.GetText(key);
            prop.SetPart("value", item_text);
            return prop;
        }


        if (key.StartsWith("ANY ")) {
            key = key.Remove(0, 4);
            Log($"Any type of {key}");
            prop = item.GetPropertyOfType("orientation");
        } else
            prop = item.GetProp(key);

        if ( prop == null) {
            ThrowFail($"No Property with type : {key} on {item.DebugName}");
            DisplayFail($"{item.GetText("the dog")} doesn't have any {key}", true);
            return null;
        }
        Log($"Found : {prop.name}");
        return prop;
    }


    static Item GetTile(string key) {
        key = TextUtils.Extract('{', key, out key);
        var tilesetId = Player.Instance.tilesetId;
        // check for other tile set

        Log($"Searching a Tile : {key}");

        if (key.Contains('|')) {
            Log($"Targeting Tile Set : {key}");
            var tsSplit = key.Split('|');
            var tilesetKey = tsSplit[0];
            key = tsSplit[1];
            var tilesetPart = LinePart.Parse(tilesetKey, sourceItem, "Tile Set");
            Log($"Found Tileset : {tilesetPart.value}");
        }

        var linePart = LinePart.Parse(key, sourceItem, "Tile Fetching");
        var coords = linePart.GetCoords(tilesetId);
        var result = TileSet.tileSets[tilesetId].GetTile(coords);
        if (result == null)
            return null;
        Log($"Tile Result : {result.DebugName} (Coords:{coords})");

        return result;

        /// LES OPERATION NE SERVENT PLUS A RIEN DANS LES TILES ?
        /*var operations = new string[4] { " + ", " - ", " X ", " DIS " };
        for (int i = 0; i < operations.Length; i++) {
            if (!key.Contains(operations[i])) {
                continue;
            }
            var operation = operations[i];
            Debug.Log($"coords operation : {operation}");
            var split = key.Split(operation);
            Debug.Log($"first part: {split[0]}");
            Debug.Log($"second part: {split[1]}");

            var part_1 = LinePart.Parse(split[0], sourceItem, "Coords Operation (1)");
            var part_2 = LinePart.Parse(split[1], sourceItem, "Coords Operation (2)");

            switch (operation) {
                case " + ":
                    Debug.Log($"addition");
                    coords = part_1.GetCoords(tilesetId) + part_2.GetCoords(tilesetId);
                    Debug.Log($"resutl : {coords}");
                    break;
                case " X ":
                    Debug.Log($"multiplcation");
                    coords = part_1.GetCoords(tilesetId) * part_2.value;
                    Debug.Log($"result : {coords}");
                    break;
                default:
                    break;
            }
        }*/


    }

    public static List<Item> GetItemsWithProp (string key, List<Item> range) {
        key = key.Remove(0, key.IndexOf("ANY") + "ANY".Length).Trim(' ');

        Log($"Getting Items With Prop {key}");

        bool highest = false;
        if ( key.StartsWith("H ")) {
            Log($"(H) Getting Property with highest value");
            highest = true;
            key = key.Substring(2).Trim(' ');
        }

        var results = range.FindAll(x => x.GetProp(key) != null && x != sourceItem);
        if (results.Count == 0) {
            Log($"No Results");
            DisplayFail($"nothing here has {key}");
            return null;
        }

        if (highest) {
            var hItem = results[0];
            for (int i = 1; i < results.Count; i++) {
                if (results[i].GetProp(key).GetNumValue() > hItem.GetProp(key).GetNumValue()) {
                    hItem = results[i];
                }
            }
            Log($"Item With Highest Property : {hItem.DebugName}");
            return new List<Item> { hItem };
        }

        return results;
    }

    public static void ThrowFail( string message) {
        LinePart.current.linkLog.Add($"<color=red>{message}</color>");
    }
    public static void Log(string message) {
        LinePart.current.linkLog.Add($"<color=white>{message}</color>");
    }

    public static Item CheckForItemInHistory(string key) {
        return history.Find(x => x.key == key).item;
    }

    [System.Serializable]
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
