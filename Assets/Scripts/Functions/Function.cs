using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Contexts;
using System.Windows.Markup;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditorInternal;
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

    static void Fail(string message) {
        TextManager.Write(message);
    }

    public static bool noParams() { return prms.Length > 0; }
    public static bool hasParam(int i) { return i < prms.Length; }
    public static int paramLength() { return prms.Length; }
    public static int parseParam(int i) { return int.Parse(prms[i]); }
    public static void Call(WorldAction _event, string line) {

        currentEvent = _event;

        // search for []
        // 
        item = currentEvent.itemGroup.items.First();

        var functionName = line;

        // get options
        if (line.Contains('(')) {
            functionName = line.Remove(line.IndexOf('('));
            var paramerets_all = TextUtils.Extract('(', line);
            var stringSeparators = new string[] { ", " };
            prms = paramerets_all.Split(stringSeparators, StringSplitOptions.None);
        }
            
        MethodInfo info = typeof(Function).GetMethod(
            functionName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        info.Invoke(null, null);
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
        int hours = currentEvent.itemGroup.items.Count;
        TimeManager.Wait(hours);
    }
    #endregion

    #region player
    static void move() {

        if (item.HasProp("rooms")) {
            Interior.Enter(item);
            return;
        }

        Tile targetTile = item as Tile;
        if ( targetTile != null) {
            // move to tile
            Player.Instance.MoveToTile(item as Tile);
            return;
        }

        var prop = item.GetProp("direction");
        if ( prop != null ) {
            var orientation = Coords.GetOrientationFromString(prop.name);
            if (item.HasProp("entrance"))
                TileSet.ChangeTileSet(0);
            else
                Player.Instance.Move(orientation);
        }

        if (!hasParam(0)) {
            TextManager.Write($"can't go to {item.GetText("the dog")}");
            return;
        }

        var moveOrientation = (Humanoid.Orientation)parseParam(1);
        Player.Instance.Move(Humanoid.OrientationToCardinal(moveOrientation));
    }

    static void orient() {
        var lookOrientation = (Player.Orientation)parseParam(0);
        Player.Instance.Orient(lookOrientation);
    }

    static void equip() {
        Player.Instance.GetBody.Equip(item);
    }

    static void unequip() {
        Player.Instance.GetBody.Unequip(item);
    }
    #endregion

    #region items
    static void transferTo() {

        var itmText = ItemDescription.NewDescription(currentEvent.itemGroup.items);

        var weightProp = item.GetProp("weight");
        if ( weightProp == null) {
            TextManager.Write($"{itmText} can't be moved");
            return;
        }

        var targetItem = ItemLink.GetItem("$contents");
        if (targetItem == null) {
            var itemName = getParam(0);
            targetItem = ItemLink.GetItem(itemName);
        }

        if ( targetItem == null) {
            TextManager.Write($"nowhere to put {itmText} in");
            return;
        }

        // this is just where you check the nextWeight, not add them.
        // i'll never need to check if at the initialization of the game.
        // I add nextWeight in the transfer to function
        var targetWeightProp = targetItem.GetProp("weight");
        if ( targetWeightProp != null) {
            var otherWeight = targetWeightProp.GetNumValue();
            var maxWeight = targetWeightProp.GetNumValue("maxWeight");
            var nextWeight = weightProp.GetNumValue() * currentEvent.itemGroup.items.Count;
            if ( otherWeight + nextWeight > maxWeight) {
                TextManager.Write($"{item.GetText($"the dog")} is too big or heavy for {itmText}");
                return;
            }
        }

        foreach (var it in currentEvent.itemGroup.items) {
            it.TransferTo(targetItem);
        }

        if (!Verb.IsNull(ItemParser.GetCurrent.verb)) {
            TextManager.Write($"you {ItemParser.GetCurrent.verb.GetCurrentWord} {itmText} {targetItem.GetText("in the dog")}");
        } else {
            TextManager.Write($"{itmText} are now {targetItem.GetText("in the dog")}");
        }
    }
    static void pickUp() {
        if (Player.Instance.Inventory.hasItem(item)) {
            TextManager.Write("inventory_pickUp_already", item);
            return;
        }

        item.RemoveFromTile(); 

        foreach (var item in currentEvent.itemGroup.items) {
            Player.Instance.Inventory.CreateChildItem(item);
        }

        string feedback = ItemDescription.NewDescription(currentEvent.itemGroup.items);
        TextManager.Write($"you took {feedback}");
    }

    static void @throw() {
        if (item == null) {
            TextManager.Write("inventory_throw_nothing");
            return;
        }

        // remove && add
        Player.Instance.Inventory.RemoveItem(item);
        _ = currentEvent.tile.CreateChildItem(item);

        TextManager.Write("inventory_throw_sucess", item);
    }


    static void destroy() {
        Item.Destroy(item);
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
            var targetItem = AvailableItems.getItemOfName(item_name);

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

        if ( currentEvent.itemGroup.items.Count == 1) {

            if (item.GetVisibleProps(1).Count == 0 && !item.HasChildItems()) {
                TextManager.Write($"nothing special about this {item.GetText("dog")}");
                return;
            }

            if ( item.GetVisibleProps(1).Count > 0)
                TextManager.Write($"it's {Property.GetDescription(item.GetVisibleProps(1))}");

            if (item.HasChildItems()) {
                var description = ItemDescription.NewDescription(item.GetChildItems());
                TextManager.Write($"there's {description} inside");
            }
            return;
        }

        TextManager.Write($"it's {ItemDescription.NewDescription(currentEvent.itemGroup.items, "show props")}");
        return;

        for (int i = 0; i < currentEvent.itemGroup.items.Count; i++) {
            var it = currentEvent.itemGroup.items[i];
            var start = i == 0 ? "there's one," : "another,";
            TextManager.Write($"{start} {Property.GetDescription(it.GetVisibleProps(1))}");
        }


    }
    #endregion

    #region props
    static void disable() {
        var line = getParam(0);
        var property = item.props.Find(x => x.name == line);
        item.DisableProperty(line);
    }

    static void enable() {
        var prop_name = getParam(0);
        var property = item.props.Find(x => x.name == prop_name);
        if (property == null) {
            Debug.LogError("ACTION_ENABLEPROPERY : did not find property : " + prop_name + " on " + item.debug_name);
            return;
        }
        item.EnableProperty(prop_name);
    }

    static void trigger() {
        var prop = item.GetProp($"!{getParam(0)}");
        string seq = prop.GetPart("main").text;
        var action = new WorldAction(item, currentEvent.tileCoords, currentEvent.tileSetId, seq);
        action.Call(); 
    }

    static void check() {
        var propertyName = getParam(0);
        var property = item.GetProp(propertyName);

        if (property.GetNumValue() <= 0) {
            TextManager.Write("No " + property.name);
            currentEvent.Stop();
            return;
        }
    }

    static void update() {
        var propName = getParam(0);
        var property = item.GetProp(propName);
        if (!property.enabled) {
            Debug.Log($"trying to update prop {propName} but it's disabled... all good");
            return;
        }

        var line = getParam(1);
        property.update(line);

        if (property.HasPart("onValue")) {
            bool call = false;

            string[] contents = property.GetPart("onValue").text.Split(new char[1] {'\n'}, 1);


            string value = contents[0];
            Debug.Log($"ON PROP VALUE : {property.name} with value :{value} and text :{contents[1]}");

            if (value.StartsWith('>')) {
                int i = int.Parse(value.Remove(0, 1));
                call = property.GetNumValue() > i;
            } else if (value.StartsWith('<')) {
                int i = int.Parse(value.Remove(0, 1));
                call = property.GetNumValue() < i;
            } else {
                int i = int.Parse(value);
                call = property.GetNumValue() == i;
            }

            if (call) {
                Debug.Log($"calling event on value prop of {property.name}");
                string sequence = contents[1];
                WorldAction tileEvent = new WorldAction(item, currentEvent.tileCoords, currentEvent.tileSetId, sequence);
                tileEvent.Call();
            }
        }

        PropertyDescription.Add(item, property);
    }
    #endregion

}