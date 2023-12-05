using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.UIElements;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemLink
{
    public static Item GetItem(string key) {

        if (key.Contains('.')) {
            // split
            var parts = key.Split('.');
            // get the parent item
            var parentItem = GetItem(parts[0]);
            if (parentItem == null)
                return null;
            var inputItem = GetItem(parts[1], ItemParser.GetCurrent.GetOptionalItems());
            if (inputItem != null) {
                return inputItem;
            }
            // search the item 
            return GetItem(parts[1], parentItem.GetChildItems(2));

        }

        if (key.StartsWith('*')) {
            return GetItem(key.Substring(1), ItemParser.GetCurrent.GetOptionalItems());
        }

        var parserItems = GetItem(key, ItemParser.GetCurrent.GetOptionalItems());
        if (parserItems != null) {
            return parserItems;
        }
        return GetItem(key, AvailableItems.currItems);
    }

    public static Item GetItem(string key, List<Item> range) {
        // this searches in the parser
        if (key.StartsWith('$')) return GetItemWithProp(key, range);
        if (key == "tile") return Tile.GetCurrent;
        var result = range.Find(x => x.HasWord(key));
        return result;
    }

    public static Property GetProperty(string key, Item item) {
        key = key.TrimStart('$');
        if (key.StartsWith('[')) {
            string itemKey = TextUtils.Extract('[', key);
            item = GetItem(itemKey);
            if (item == null) {
                Debug.LogError($"[GETTING PROP!] no item with key {itemKey}");
                return null;
            }
        }
        string propertyKey = key.Remove(0, key.LastIndexOf('.') + 1);
        var prop = item.GetProp(propertyKey);
        if ( prop == null) {
            Debug.LogError($"no property with name {propertyKey} in item {item.debug_name}");
            return null;
        }
        return prop;
    }

    public static Item GetItemWithProp (string key, List<Item> range) {
        var propName = key.Substring(1);
        var parts = propName.Split('/');
        propName = parts[0];

        var it = range.Find(x => x.GetProp(propName) != null);

        if ( parts.Length > 1 && it != null && it.GetProp(propName).HasPart(parts[1])) {
            return it;
        } else {
            return it;
        }
    }
}
