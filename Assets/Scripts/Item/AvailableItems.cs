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
        Debug.LogError("removing item : " + targetItem.GetWord().GetText + " failed : not in container, tile or inventory");
    }


    public static void UpdateItems() {

        currItems.Clear();
        log = "";

        // the tile and all it's contained items

        var tileItems = Tile.GetCurrent.GetChildItems(2);
        currItems.AddRange(tileItems);
        LOG("Current Tile", tileItems);
        // it's important that it's after the tile, otherwise the parser will search the inventory GetMainItem (ex: take plate, you already have it)
        // add spec "my" ou "inventory"// add different keys to specs (my+inventory+in bag)
        // ah non. parce que �a peut �tre un container aussi. "take field plate". "take bag plate". �a y est toujorus �a ?
        // the surrouning tiles

        var adjTiles = Tile.GetCurrent.GetAdjacentTiles();
        currItems.AddRange(adjTiles);
        LOG("Adjacent Tiles", adjTiles);

        // the player and all it's things

        var playerItems = Player.Instance.GetChildItems(5);
        currItems.AddRange(playerItems);
        LOG("Player Items", playerItems);

        var globalItems = WorldData.globalItems;
        currItems.AddRange(globalItems);
        LOG("Global Items", globalItems);

        var abstractItems = WorldData.GetAbstractItems();
        currItems.AddRange(abstractItems);
        LOG("Abstract Items", abstractItems);
    }

    public static string log;
    public static void LOG(string message, Color color) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $"\n{txt_color}{message}</color>";
        log += str;
    }
    public static void ADDLOG(string message, Color color) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $" {txt_color}{message}</color>";
        log += str;
    }
    public static void LOG(string cat, List<Item> items) {
        LOG($"\n\n[{cat.ToUpper()}]", Color.cyan);
        foreach (var item in items) {
            string str = "";
            var parent = item.HasParent() ? item.GetParent() : null;
            while (parent != null) {
                str += ">    ";
                parent = parent.HasParent() ? parent.GetParent() : null;
            }
            str += $"[{item.debug_name}]";
            LOG(str, Color.yellow);
            int a = 0;

            foreach (var prop in item.props) {
                if (prop.enabled) {
                    if (prop.HasPart("value")) {
                        ADDLOG($"[{prop.name}]", Color.magenta);
                        ADDLOG($"{prop.GetPart("value").content}", Color.Lerp(Color.magenta, Color.black, 0.3f));
                    } else {
                        ADDLOG($"[{prop.name}]", Color.white);
                    }
                    if (prop.HasPart("description")) {
                        ADDLOG($"[{prop.GetDescription()}]", Color.green);
                    }
                } else {
                    if (prop.HasPart("value")) {
                        ADDLOG($"[{prop.name}]", Color.gray);
                        ADDLOG($"{prop.GetPart("value").content}", Color.Lerp(Color.gray, Color.black, 0.3f));
                    } else
                        ADDLOG($"[{prop.name}]", Color.gray);
                }
                ++a;
            }
        }
    }
}
