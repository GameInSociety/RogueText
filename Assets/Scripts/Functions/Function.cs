using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using UnityEngine.UIElements;

public class Function {

    static WorldAction currentEvent;
    static string[] prms;
    static Item item;

    public Item GetItem(string str) {
        return null;
    }

    public Property GetProp(string str) {
        return null;
    }


    public static string GetPart(int i) {
        if (i >= prms.Length)
            return "no contents";
        return prms[i];
    }

    static void Fail(string message) {
        TextManager.Write(message);
        currentEvent.Fail();
    }

    public static bool NoParts() { return prms.Length > 0; }
    public static bool HasPart(int i) { return i < prms.Length; }
    public static int PartCount() { return prms.Length; }
    public static int ParsePart(int i) { return int.Parse(prms[i]); }
    public static void Call(WorldAction _event, string line) {

        currentEvent = _event;

        // search for []
        // 
        if (line.StartsWith('[')) {
            var itemKey = TextUtils.Extract('[', line);
            Debug.Log($"content key {itemKey}");
            item = ItemLink.GetItem(itemKey);
            line = line.Remove(0, itemKey.Length + 3);
            Debug.Log($"new line {line}");
        } else {
            item = currentEvent.itemGroup.items.First();
        }

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

        if (info == null) {
            Debug.LogError($"no method named {functionName} found");
            TextManager.Write($"no method named {functionName} found");
            return;
        }
        info.Invoke(null, null);
    }

    #region write
    static void write() {
        switch (GetPart(0)) {
            case "_north":
                var orientation = Coords.GetOrientationFromNorth(Player.Instance.currentCarnidal);
                TextManager.Write($"the north is on the {orientation.ToString()}");
                break;
            case "_tile":
                Tile.GetCurrent.Describe();
                break;
            default:
                TextManager.Write(GetPart(0));
                break;
        }
    }
    #endregion

    #region time
    static void wait() {
        int count = 0;
        if (ItemParser.GetCurrent.integer >= 0) {
            count = ItemParser.GetCurrent.integer;
        } else {
            count = HasPart(0) ? ParsePart(0) : 1;
        }
        TimeManager.Wait(count);
        TextManager.Write($"{count} {TextUtils.WordWithNumber("hour", count)} pass by");
    }
    static void triggerEvent() {
        WorldEvent.TriggerEvent(GetPart(0));
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

        if (!HasPart(0)) {
            Fail($"can't go to {item.GetText("the dog")}");
            return;
        }

        var moveOrientation = (Humanoid.Orientation)ParsePart(1);
        Player.Instance.Move(Humanoid.OrientationToCardinal(moveOrientation));
    }

    static void orient() {
        var lookOrientation = (Player.Orientation)ParsePart(0);
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
            Fail($"{itmText} can't be moved");
            return;
        }

        var itemName = GetPart(0);
        var targetItem = ItemLink.GetItem(itemName);

        if ( targetItem == null) {
            Fail($"nowhere to put {itmText} in");
            return;
        }

        // this is just where you check the nextWeight, not add them.
        // sourceValue'll never need to check if at the initialization of the game.
        // I add nextWeight in the transfer to function
        var targetWeightProp = targetItem.GetProp("weight");
        if ( targetWeightProp != null) {
            var otherWeight = targetWeightProp.GetNumValue();
            var maxWeight = targetWeightProp.GetNumValue("max");
            var nextWeight = weightProp.GetNumValue() * currentEvent.itemGroup.items.Count;
            if ( otherWeight + nextWeight > maxWeight) {
                Fail($"{itmText} is too big or heavy for {targetItem.GetText($"the dog")} ");
                return;
            }
        }
        

        foreach (var it in currentEvent.itemGroup.items) {
            if (targetItem.hasItem(it)) {
                Fail($"{targetItem.GetText("the dog")} already contains {it.GetText("the dog")}");
                return;
            }
            it.TransferTo(targetItem);
        }

        if (!Verb.IsNull(ItemParser.GetCurrent.verb)) {
            TextManager.Write($"you {ItemParser.GetCurrent.verb.GetCurrentWord} {itmText} in {targetItem.GetText("the dog")}");
        } else {
            TextManager.Write($"{itmText} are now in {targetItem.GetText("the dog")}");
        }
    }

    static void destroy() {
        Item.Destroy(item);
    }

    static void createItem() {

        var amount = HasPart(1) ? ParsePart(1) : 1;

        var itemType = GetPart(0);
        var item = currentEvent.tile.CreateChildItem(itemType);

        for (var i = 1; i < amount; i++) {
            _ = currentEvent.tile.CreateChildItem(itemType);
        }

        if (currentEvent.tile == Tile.GetCurrent) {
            TextManager.Write("tile_addItem", item);
        }
    }

    static void describe() {

        Debug.Log($"content group name : {currentEvent.itemGroup.debug_name}");

        foreach (var prop in currentEvent.itemGroup.linkedProps) {
            if (prop.HasPart("key")) {
                TextManager.Write($"{prop.GetDescription()}");
                return;
            }
        }

        if ( currentEvent.itemGroup.items.Count == 1) {

            if (item.GetVisibleProps(1).Count == 0 && !item.HasChildItems()) {
                TextManager.Write($"nothing special about this {item.GetText("dog")}");
                return;
            }

            if ( item.GetVisibleProps(1).Count > 0)
                TextManager.Write($"{item.GetText("the dog")} is {Property.GetDescription(item.GetVisibleProps(1))}");

            if (item.HasChildItems()) {
                var description = ItemDescription.NewDescription(item.GetChildItems());
                TextManager.Write($"there's {description} inside");
            }
            return;
        }

        TextManager.Write($"{item.GetText("the dog")} is {ItemDescription.NewDescription(currentEvent.itemGroup.items, "show props")}");
        return;


    }
    #endregion

    #region props
    static void disable() {
        var line = GetPart(0);
        var property = item.props.Find(x => x.name == line);
        item.DisableProperty(line);
    }

    static void enable() {
        var prop_name = GetPart(0);
        var property = item.props.Find(x => x.name == prop_name);
        if (property == null) {
            Debug.LogError("ACTION_ENABLEPROPERY : did not find targetProp : " + prop_name + " on " + item.debug_name);
            return;
        }
        item.EnableProperty(prop_name);
    }

    static void trigger() {
        var prop = item.GetProp($"!{GetPart(0)}");
        string seq = prop.GetPart("main").text;
        var action = new WorldAction(item, currentEvent.tileCoords, currentEvent.tileSetId, seq);
        action.Call(); 
    }

    static void check() {
        var propertyName = GetPart(0);
        var property = item.GetProp(propertyName);

        if (property.GetNumValue() <= 0) {
            TextManager.Write("No " + property.name);
            currentEvent.Fail();
            return;
        }
    }

    static void update() {
        // getting the target property
        var propName = GetPart(0);
        var targetProp = (Property)null;
        // checking for link to other items
        if (propName.StartsWith('$'))
            targetProp = ItemLink.GetProperty(propName, item);
        else
            targetProp = item.GetProp(propName);

        if ( targetProp == null) {
            Debug.LogError($"no targetProp with the name {propName} on content {item.debug_name}");
            return;
        }
        if (!targetProp.enabled) {
            Debug.Log($"trying to Update targetProp {propName} but it's disabled... all good");
            return;
        }

        // getting the line to modify the target property with
        var line = GetPart(1);

        // check if substract, simple set or add
        Property.UpdateType updateType = Property.UpdateType.None;
        if (line.StartsWith('+'))
            updateType = Property.UpdateType.Add;
        else if (line.StartsWith('-'))
            updateType = Property.UpdateType.Substract;
        if (updateType != Property.UpdateType.None) line = line.Substring(1);
        //

        // getting target targetPropValue
        int sourceValue = 0;
        var sourceProp = (Property)null;
        // check if link to other targetProp
        if (line.StartsWith('$')) {
            sourceProp = ItemLink.GetProperty(line, item);
            sourceValue = sourceProp.GetNumValue();
            Debug.Log($"found source prop : {sourceProp.name}");
        } else {
            sourceValue = int.Parse( line );
        }

        int targetPropValue = targetProp.GetNumValue();
        switch (updateType) {
            case Property.UpdateType.None:
                // not doing the i here
                // not sure if i'm using this much or when i do a i is necessary
                targetProp.SetValue(sourceValue);
                break;
            case Property.UpdateType.Add:
                int i = targetPropValue + sourceValue;
                if (targetProp.HasPart("max") && sourceProp != null) {
                    Debug.Log($"applying changes on prop {sourceProp.name}");
                    int max = targetProp.GetNumValue("max");
                    int dif = i - max;
                    Debug.Log($"dif : {dif}");
                    sourceProp.SetValue(sourceValue - dif);
                    Debug.Log($"new value for : {sourceProp.name} : {sourceProp.GetNumValue()}");
                }
                targetProp.SetValue(i);
                break;
            case Property.UpdateType.Substract:
                if (sourceProp != null) {
                    Debug.Log($"SUBSTRACTION : applying changes on prop {sourceProp.name}");
                    Debug.LogFormat($"source value : {sourceValue}");
                    Debug.LogFormat($"target prop value : {targetPropValue}");
                    int dif = sourceValue - targetPropValue;
                    if ( dif >=  0 ) {
                        sourceProp.SetValue(dif);
                    }
                    Debug.Log($"dif : {dif}");
                    Debug.Log($"new value for : {sourceProp.name} : {sourceProp.GetNumValue()}");
                }
                targetProp.SetValue(targetPropValue - sourceValue);
                break;
        }

        property_checkEvents(targetProp);

        if (targetProp.HasPart("description")) {
            TextManager.Write(targetProp.GetDescription());
        }
    }
    static void property_checkEvents(Property prop) {
        if (prop.HasPart("onValue")) {
            bool call = false;
            var text = prop.GetPart("onValue").text;
            var returnIndex = text.IndexOf('\n');

            var value = text.Remove(returnIndex);
            var sequence = text.Remove(0, returnIndex + 1);
            Debug.Log($"ON PROP VALUE : {prop.name} with targetPropValue :{value} and seq :{sequence}");
            if (value.StartsWith('>')) {
                int i = int.Parse(value.Remove(0, 1));
                call = prop.GetNumValue() > i;
            } else if (value.StartsWith('<')) {
                int i = int.Parse(value.Remove(0, 1));
                call = prop.GetNumValue() < i;
            } else {
                int i = int.Parse(value);
                call = prop.GetNumValue() == i;
            }

            if (call) {
                Debug.Log($"calling event on targetPropValue targetProp of {prop.name}");
                WorldAction tileEvent = new WorldAction(item, currentEvent.tileCoords, currentEvent.tileSetId, sequence);
                tileEvent.Call();
            }
        }
    }
    #endregion

}