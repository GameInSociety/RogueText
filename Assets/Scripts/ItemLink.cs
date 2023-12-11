using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.UIElements;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemLink
{
    public static Item GetItem(string key) {

        var parentItem = (Item)null;
        if (key.Contains('.')) {
            // split
            var parts = key.Split('.');
            // get the parent item
            parentItem = GetItem(parts[0]);
            key = parts[1];
            if (parentItem == null)
                return null;
        }

        if ( parentItem != null )
            return GetItem(key, parentItem.GetChildItems(2));

        if ( ItemParser.GetCurrent == null )
            return GetItem(key, AvailableItems.currItems);
        
        // when the key starts with *, it search will only occur in the input items
        if (key.StartsWith('*')) {
            return GetItem(key.Substring(1), ItemParser.GetCurrent.GetOptionalItems());
        }

        var parserItem = GetItem(key, ItemParser.GetCurrent.GetOptionalItems());
        if (parserItem != null) {
            return parserItem;
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

    public static Property GetProperty(string key, Item item = null) {
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
