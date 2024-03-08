using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemLink
{
    public static string failMessage;

    public static Item SearchItem(string key) {

        failMessage = "";

        LOG($"NEW ITEM SEARCH [{key}]", Color.white);

        baseItem = WorldAction.current.TargetItem();
        var results = ReadKey(key);

        if (results == null || results.Count == 0) {

            // if no fail message was there, set default one
            if (string.IsNullOrEmpty(failMessage))
                DisplayFail($"there's no {key} around");

            LOG(failMessage, Color.red);
            Function.Fail(failMessage);
            return null;
        }

        var item = results[0];
        return item;
    }

    static Item baseItem;

    public static List<Item> ReadKey(string key) {

        // first, check if the search will be in another item
        var parent_item = (Item)null;
        if (key.Contains('.')) {
            // split
            var parts = key.Split('.');
            LOG($"Look for parent with key : [{parts[0]}]", Color.grey);

            // get the parent item
            var results = ReadKey(parts[0]);
            if (results == null || results.Count == 0) {
                LOG($"No parent item with key [{key}] was not found", Color.grey);
                return null;
            }
            key = parts[1];
            baseItem = parent_item;
        }

        // if parent, search in children
        if ( parent_item != null) {
            LOG($"Looking inside parent : {parent_item.debug_name}", Color.grey);
            return SearchItemsInRange(key, parent_item.GetChildItems(2));
        }

        // when the key starts with *, it search will only occur in the input items
        if (key.StartsWith("p ")) {
            LOG($"Only looking inside Parser", Color.gray);
            if ( ItemParser.GetCurrent == null) {
                LOG($"Searching inside Parser in key [{key}], but no Parser Available", Color.red);
                return null;
            }
            var newKey = key.Remove(0, 2).TrimStart(' ');
            var itemResults = ItemParser.GetCurrent.SearchForItemsInParts(newKey);
            if (itemResults == null || itemResults.Count ==0 ) {
                    var verb = ItemParser.GetCurrent.verb;
                    var currItem = ItemParser.GetCurrent.GetPart(0).items[0];
                if (ItemParser.GetCurrent.parts.Length < 2)
                    DisplayFail($"specify something with {newKey}", true);
                else
                    DisplayFail($"{ItemParser.GetCurrent.GetPart(1).items.First().GetText("the dog")} doesn't have {newKey}", true);
            }
            return itemResults;
        }

        // if no current item parser, only in surrnouding items ( for events )
        if (ItemParser.GetCurrent == null) {
            LOG($"No Parser, Looking In Available Items", Color.grey);
            return SearchItemsInRange(key, AvailableItems.currItems);
        }

        // first serach in parsed items
        LOG($"looking in parser : {key}", Color.gray );
        var parserItem = ItemParser.GetCurrent.SearchForItemsInParts(key);
        if (parserItem != null) {
            LOG($"Found : {parserItem.First().debug_name}", Color.grey);
            return parserItem;
        } else {
            LOG($"found nothing in parser", Color.grey);
        }

        LOG($"Looking In Available Items", Color.grey);
        var surroundingItem = SearchItemsInRange(key, AvailableItems.currItems);

        if ( surroundingItem == null) {
            DisplayFail($"there's no {key} around");
            return null;
        }

        return surroundingItem;
    }

    static void DisplayFail(string message, bool overrideMessage = false) {

        failMessage = message;
        
    }

    static void Error(string message) { Debug.LogError(message);}

    public static List<Item> SearchItemsInRange(string key, List<Item> range) {
        key = key.Trim(' ');
        // this searches in the parser
        if (key.StartsWith("ANY")) {
            return GetItemsWithProp(key, range);
        }
        switch (key) {
            case "parent":
                LOG($"Special Key : Parent", Color.gray);
                return new List<Item>() { baseItem.GetParent() };
        }
        var result = range.FindAll(x => x.HasWord(key) && x != baseItem);
        return result;
    }

    public static Property GetProperty(string key, Item item) {
        LOG($"NEW PROP SEARCH : [{key}]", Color.white);
        var prop = (Property)null;
        if (key.StartsWith("ANY ")) {
            key = key.Remove(0, 4);
            LOG($"Getting ANY Property OF TYPE: [{key}]", Color.grey);
            prop = item.GetPropertyOfType("orientation");
        } else {
            LOG($"Getting Property with name [{key}]", Color.grey);
            prop = item.GetProp(key);
        }

        if ( prop == null) {
            LOG($"Failed Looking for Property : [{key}]", Color.grey);
            DisplayFail($"{item.GetText("the dog")} doesn't have any {key}", true);
            return null;
        }
        return prop;
    }

    static void LOG(string message, Color c) {
        Function.LOG(message, c);
    }

    public static List<Item> GetItemsWithProp (string key, List<Item> range) {
        LOG($"prop items : {key}", Color.red);
        key = key.Remove(0, key.IndexOf("ANY") + "ANY".Length);
        key = key.Trim(' ');
        LOG($"after handle : {key}", Color.red);

        if ( key.StartsWith("SORT OF")) {
                LOG($"Looing for any Item with a property OF TYPE : [{key}]", Color.gray);
            key = key.Remove(0,"SORT OF ".Length);
            var results = range.FindAll(x => x.GetPropertyOfType(key) != null && x != baseItem);
            if (results.Count ==0)
                DisplayFail($"nothing here has {key}");
            return results;
        } else {
            Debug.Log($"search for any {key}");
            LOG($"Looking for any Item with the property : [{key}]", Color.gray);
            //var results = range.FindAll(x => x.GetProp(key) != null && x != baseItem);
            // ici, x != baseITme a été viré parce que ça marche pas avec le any item.
            // trouver une autre solution quand le probleme se repose
            var results = range.FindAll(x => x.GetProp(key) != null && x != baseItem);
            if (results.Count == 0)
                DisplayFail($"nothing here has {key}");
            return results;
        }
    }
}
