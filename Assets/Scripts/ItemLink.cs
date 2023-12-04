using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.UIElements;
using UnityEngine;

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

        Debug.Log($"sarch everywhere");
        Debug.Log($"GetMainItem in parser");
        var dfdfd = GetItem(key, ItemParser.GetCurrent.GetOptionalItems());
        if (dfdfd != null) {
            return dfdfd;
        }
        Debug.Log($"found no {key} in input, searching in av items");
        return GetItem(key, AvailableItems.currItems);
    }

    public static Item GetItem(string key, List<Item> range) {
        // this searches in the parser
        if (key.StartsWith('$')) return GetItemWithProp(key, range);
        if (key == "tile") return Tile.GetCurrent;
        var result = range.Find(x => x.HasWord(key));
        return result;
    }

    public static int GetPropertyValue(string key, Item item) {
        Debug.Log($"get property in value : {key}");
        if (key.StartsWith('[')) {
            string itemKey = TextUtils.Extract('[', key);
            Debug.Log($"item key : {itemKey}");
            item = GetItem(itemKey);
            if (item == null) {
                Debug.Log($"no item with key {itemKey}");
                return -1;
            }
        }

        Debug.Log($"item : {item.debug_name}");
        string propertyKey = key.Remove(0, key.LastIndexOf('.')+1);
        Debug.Log($"property key : {propertyKey}");

        var prop = item.GetProp(propertyKey);
        Debug.Log($"found property : {propertyKey}");

        return prop.GetNumValue();
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
