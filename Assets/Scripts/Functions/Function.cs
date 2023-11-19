using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Contexts;
using System.Windows.Markup;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class Function {

    static WorldAction currentEvent;
    static string[] prms;
    static Item item;

    public static string getParam(int i) {
        if (i >= prms.Length)
            return "no contents";
        return prms[i];
    }

    public static bool noParams() { return prms.Length > 0; }
    public static bool hasParam(int i) { return i < prms.Length; }
    public static int paramLength() { return prms.Length; }
    public static int parseParam(int i) { return int.Parse(prms[i]); }
    public static void Call(WorldAction _event, string line) {

        currentEvent = _event;

        var functionName = line.Remove(line.IndexOf('('));
        Debug.Log($"function : {functionName}");
        var str = line.Remove(0, line.IndexOf('(') + 1).Remove(line.IndexOf(')'));
        var stringSeparators = new string[] { ", " };
        prms = str.Split(stringSeparators, StringSplitOptions.None);
        foreach (var content in prms)
            Debug.Log($"getParam : " + content);
        // switch case ?
        Type type = typeof(Function);
        MethodInfo info = type.GetMethod(
            functionName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        object value = info.Invoke(null, null);
    }

    #region write
    static void write() {
        switch (getParam(0)) {
            case "_north":
                var orientation = Coords.GetOrientationFromNorth(Player.Instance.currentCarnidal);
                TextManager.Write($"the north is on the {orientation.ToString()}");
                break;
            case "_tile":
                Tile.GetCurrent.Describe();
                break;
            case "_time":
                TimeManager.writeTimeOfDay();
                break;
            default:
                TextManager.Write(getParam(0));
                break;
        }
    }
    #endregion

    #region time
    static void wait() {
        int hours = ItemParser.GetCurrent.itemGroups[0].items.Count;
        TimeManager.Wait(hours);
    }
    #endregion

    #region player
    static void move() {

        Tile targetTile = item as Tile;
        if ( targetTile != null) {
            // move to tile
            Player.Instance.MoveToTile(currentEvent.item as Tile);
            return;
        }

        var prop = currentEvent.item.GetProp("direction");
        if ( prop != null ) {
            var orientation = Coords.GetOrientationFromString(prop.name);
            if (currentEvent.item.hasSpec("entrance"))
                TileSet.ChangeTileSet(0);
            else
                Player.Instance.Move(orientation);
        }

        var moveOrientation = (Player.Orientation)parseParam(1);
        Player.Instance.Move(Player.OrientationToCardinal(moveOrientation));
    }

    static void orient() {
        var lookOrientation = (Player.Orientation)parseParam(0);
        Player.Instance.Orient(lookOrientation);
    }

    static void equip() {
        Player.Instance.GetBody.Equip(currentEvent.item);
    }

    static void unequip() {
        Player.Instance.GetBody.Unequip(currentEvent.item);
    }
    #endregion

    #region interior
    static void enter() {
        var interior = currentEvent.item as Interior;
        interior.Enter();
    }
    #endregion

    #region items
    static void pickUp() {
        if (Player.Instance.Inventory.hasItem(currentEvent.item)) {
            TextManager.Write("inventory_pickUp_already", currentEvent.item);
            return;
        }

        currentEvent.item.RemoveFromTile();

        foreach (var item in ItemParser.GetCurrent.itemGroups[0].items) {
            Player.Instance.Inventory.CreateChildItem(currentEvent.item);
        }

        string feedback = ItemParser.GetCurrent.itemGroups[0].GetDescription();
        TextManager.Write($"you took {feedback}");
    }

    static void @throw() {
        var item = Player.Instance.Inventory.GetItem(currentEvent.item.GetWord().text);

        if (item == null) {
            TextManager.Write("inventory_throw_nothing");
            return;
        }

        // remove && add
        Player.Instance.Inventory.RemoveItem(item);
        _ = currentEvent.tile.CreateChildItem(currentEvent.item);

        TextManager.Write("inventory_throw_sucess", currentEvent.item);
    }


    static void destroy() {
        Item.Destroy(currentEvent.item);
    }

    static void createItem() {

        var amount = hasParam(1) ? parseParam(1) : 1;

        var itemType = getParam(0);
        var item = currentEvent.tile.CreateChildItem(itemType);

        for (var i = 1; i < amount; i++) {
            _ = currentEvent.tile.CreateChildItem(itemType);
        }

        if (currentEvent.tile == Tile.GetCurrent) {
            TextManager.Write("tile_addItem", item);
        }
    }

    static void require() {
        if (hasParam(1)) {
            var item_name = getParam(0);
            var targetItem = AvailableItems.Get.getItemOfName(item_name);

            if (targetItem == null) {
                // found no item in container, inventory or tile
                // break flow of actions
                TextManager.Write($"there's no {item_name} around");
                currentEvent.Stop();
                return;
            }
            return;
        }
    }

    static void describe() {

        ItemGroup group = ItemParser.GetCurrent.itemGroups[0];
        TextManager.Write($"{group.GetDescription()}");
    }
    #endregion

    #region props
    static void disable() {
        var line = getParam(0);
        var property = currentEvent.item.props.Find(x => x.name == line);
        currentEvent.item.DisableProperty(line);
    }

    static void enable() {
        var prop_name = getParam(0);
        var property = currentEvent.item.props.Find(x => x.name == prop_name);
        if (property == null) {
            Debug.LogError("ACTION_ENABLEPROPERY : did not find property : " + prop_name + " on " + currentEvent.item.debug_name);
            return;
        }
        currentEvent.item.EnableProperty(prop_name);
    }

    static void trigger() {
        var prop = currentEvent.item.GetProp($"!{getParam(0)}");
        string seq = prop.getPart("main").text;
        var action = new WorldAction(currentEvent.item, currentEvent.tileCoords, currentEvent.tileSetId, seq);
        action.Call(); 
    }

    static void check() {
        var propertyName = getParam(0);
        var property = currentEvent.item.GetProp(propertyName);

        if (property.getNum(propertyName) <= 0) {
            TextManager.Write("No " + property.name);
            currentEvent.Stop();
            return;
        }
    }

    static void update() {
        var propName = getParam(0);
        var property = currentEvent.item.GetProp(propName);
        if (!property.enabled) {
            Debug.Log($"trying to update prop {propName} but it's disabled... all good");
            return;
        }

        var line = getParam(1);
        property.update(line);

        if (property.hasPart("onValue")) {
            bool call = false;

            string[] contents = property.getText("onValue").Split(new char[1] {'\n'}, 1);


            string value = contents[0];
            Debug.Log($"ON PROP VALUE : {property.name} with value :{value} and text :{contents[1]}");

            if (value.StartsWith('>')) {
                int i = int.Parse(value.Remove(0, 1));
                call = property.getNum("value") > i;
            } else if (value.StartsWith('<')) {
                int i = int.Parse(value.Remove(0, 1));
                call = property.getNum("value") < i;
            } else {
                int i = int.Parse(value);
                call = property.getNum("value") == i;
            }

            if (call) {
                Debug.Log($"calling event on value prop of {property.name}");
                string sequence = contents[1];
                WorldAction tileEvent = new WorldAction(currentEvent.item, currentEvent.tileCoords, currentEvent.tileSetId, sequence);
                tileEvent.Call();
            }
        }

        PropertyDescription.Add(currentEvent.item, property);
    }
    #endregion

}