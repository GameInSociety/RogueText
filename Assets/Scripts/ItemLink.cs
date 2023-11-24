using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLink
{
    public static Item GetItem(string str) {


        if (str.Contains('.')) {
            // repeat in other item
        }

        if ( str == "tile") {
            return Tile.GetCurrent;
            // return tile
        }

        if (str.StartsWith('$')) {
            var propName  = str.Substring(1);
            Debug.Log($"{propName}");
            // search for a property in parser / available items
            foreach (var item in ItemParser.GetCurrent.itemGroups) {
               if (item.first.GetProp(propName) != null) {
                    Debug.Log($"found item {item.debug_name} that has prop contents in input");
                    return item.first;
                }
            }
        }



         

        return null;
    }
}
