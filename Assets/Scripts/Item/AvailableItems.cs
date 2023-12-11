using System.Collections.Generic;
using UnityEngine;

public static  class AvailableItems {
    public static List<Item> currItems = new List<Item>();
    public static List<Item> testItems = new List<Item>();
    private static Item noItem;
    public static Item NoItem() {

        if ( noItem == null)
            noItem = ItemData.Generate_Simple("no item");
        return noItem;
    }

    public static Item getItemOfProperty(string property) {
        return currItems.Find(x => x.HasProp(property));
    }
    public static Item getItemOfName(string name) {
        return currItems.Find(x => x.HasWord(name));
    }

    public static void RemoveFromWorld(Item targetItem) {
        foreach (var item in currItems) {
            if (item.hasItem(targetItem)) {
                item.RemoveItem(targetItem);
                return;
            }
        }
        Debug.LogError("removing item : " + targetItem.GetWord().text + " failed : not in container, tile or inventory");
    }


    public static void UpdateItems() {

        currItems.Clear();

        // the tile and all it's contained items
        currItems.AddRange(Tile.GetCurrent.GetChildItems(2));
        // it's important that it's after the tile, otherwise the parser will search the inventory GetMainItem (ex: take plate, you already have it)
        // add spec "my" ou "inventory"// add different keys to specs (my+inventory+in bag)
        // ah non. parce que �a peut �tre un container aussi. "take field plate". "take bag plate". �a y est toujorus �a ?
        // the surrouning tiles
        currItems.AddRange(Tile.GetCurrent.GetAdjacentTiles());
        // the player and all it's things
        currItems.AddRange(Player.Instance.GetChildItems(5));

        currItems.AddRange(WorldData.globalItems);
    }
}
