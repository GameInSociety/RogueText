using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;

public static class Function {
    static WorldAction worldAction;
    static string[] prms = new string[0];
    static Item[] items;
    static Property[] props;
    static Item item;
    static bool continueOnFail;

    static Item GetItem(int i) {
        if (!HasPart(i))
            return item;
        var key = GetPart(i);
        if (key.Contains('>'))
            key = key.Remove(GetPart(i).IndexOf('>'));

        if (items[i] == null) {
            items[i] = ItemLink.SearchItem(key);
        }
        return items[i];
    }

    static bool HasProp(int i) {
        return GetProp(i) != null;
    }

    static Property GetProp(int i) {
        if (!HasPart(i))
            return null;
        if (props[i] == null) {
            props[i] = ItemLink.GetProperty(GetPart(i), item);
        }
        return props[i];
    }

    public static string GetPart(int i) {
        if (i >= prms.Length)
            return "no contents";
        return prms[i];
    }

    public static void Fail(string message) {
        if ( continueOnFail) {
            Debug.Log($"failing but continuing:");
            Debug.Log($"message : {message}");
            return;
        }
        WorldAction.current.Fail(message);
    }

    public static bool NoParts() { return prms.Length > 0; }
    public static bool HasPart(int i) { return i < prms.Length; }
    public static int PartCount() { return prms.Length; }
    public static int ParsePart(int i) { return int.Parse(prms[i]); }
    public static bool CanParse(int i) { int a = 0; return int.TryParse(prms[i], out a); }
    public static bool HasLinks(int i) { return prms[i].Contains('[') || prms[i].Contains('$'); }
    public static void TryCall(WorldAction _event, string line) {

        worldAction = _event;

        // search for []
        if (line.StartsWith('[')) {
            var itemKey = TextUtils.Extract('[', line);
            item = ItemLink.SearchItem(itemKey);
            line = line.Remove(0, itemKey.Length + 3);
        } else {
            item = worldAction.itemGroup.items.First();
        }

        var functionName = line;

        continueOnFail = false;
        if (line.StartsWith('*')) {
            Debug.Log($"[FUNCTION] : {line} / don't break sequence");
            continueOnFail = true;
            line = line.Substring(1);
        }

        // get options
        if (line.Contains('(')) {
            functionName = line.Remove(line.IndexOf('('));
            var paramerets_all = TextUtils.Extract('(', line);
            var stringSeparators = new string[] { ", " };
            prms = paramerets_all.Split(stringSeparators, StringSplitOptions.None);
            items = new Item[prms.Length];
            props = new Property[prms.Length];
        }
            
        MethodInfo info = typeof(Function).GetMethod(
            functionName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (info == null) {
            Debug.LogError($"no method named {functionName} found");
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
        } else if (HasPart(0)) {
            count = CanParse(0) ? ParsePart(0) : GetProp(0).GetNumValue();
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

        var itmText = ItemDescription.NewDescription(worldAction.itemGroup.items);

        // get target item
        if ( GetItem(0) == null) {
            Fail($"nowhere to put {itmText} in");
            return;
        }

        // this is just where you check the nextWeight, not add them.
        // sourceValue'll never need to check if at the initialization of the game.
        // I add nextWeight in the transfer to function
        // get target item weight

        // get target item pick up props
        var ci_wProp = item.GetProp("weight");
        int ti_weigh = GetItem(0).GetProp("weight").GetNumValue();
        int ti_cap = GetItem(0).GetProp("capacity").GetNumValue();
        // get current item group weight
        int ci_weight = ci_wProp.GetNumValue() * worldAction.itemGroup.items.Count;
        // check if weight goes above capicity
        if (ti_weigh + ci_weight>= ti_cap) {
            Fail($"{itmText} is too big or heavy for {GetItem(0).GetText($"the dog")} ");
            return;
        }

        foreach (var it in worldAction.itemGroup.items) {
            if (GetItem(0).hasItem(it)) {
                Fail($"{GetItem(0).GetText("the dog")} already contains {it.GetText("the dog")}");
                return;
            }
            it.TransferTo(GetItem(0));
        }

        if (!Verb.IsNull(ItemParser.GetCurrent.verb)) {
            TextManager.Write($"you {ItemParser.GetCurrent.verb.GetCurrentWord} {itmText} in {GetItem(0).GetText("the dog")}");
        } else {
            TextManager.Write($"{itmText} are now in {GetItem(0).GetText("the dog")}");
        }
    }

    static void destroy() {
        Item.Destroy(item);
    }

    static void createItem() {

        // createItem (WHAT ITEM, HOW MUCH)

        var amount = HasPart(1) ? ParsePart(1) : 1;

        var itemName = GetPart(0);
        if (itemName.StartsWith('.')) {
            var prop = item.GetProp(itemName.Substring(1));
            if (prop == null) Debug.LogError($"no prop named {itemName.Substring(1)}");
            itemName = prop.GetDescription();
            Debug.Log($"item name : {itemName}");
        }
        var newItem = worldAction.tile.CreateChildItem(itemName);
        Debug.Log($"creating item {newItem.debug_name}");

        for (var i = 1; i < amount; i++) {
            _ = worldAction.tile.CreateChildItem(itemName);
        }

        if (worldAction.tile == Tile.GetCurrent) {
            TextManager.Write($"{newItem.GetText("a dog")} was {ItemParser.GetCurrent.verb.GetCurrentWord}ed");
        } else {
            TextManager.Write($"target tile : {Tile.GetCurrent.coords.ToString()} / event tile : {worldAction.tile.coords.ToString()}");
        }
    }

    static void describe() {
        foreach (var prop in worldAction.itemGroup.linkedProps) {
            if (prop.HasPart("key")) {
                TextManager.Write($"{prop.GetDescription()}");
                return;
            }
        }

        if ( worldAction.itemGroup.items.Count == 1) {

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

        TextManager.Write($"{item.GetText("the dog")} is {ItemDescription.NewDescription(worldAction.itemGroup.items, "show props")}");
        return;


    }
    #endregion


    #region props
    static void disable() {
        GetProp(0).enabled = false;
    }

    static void enable() {
        var prop = GetProp(0);
        prop.enabled = true;
        Property_CheckEvents(prop);
        PropertyDescription.Add(item, prop);
    }

    static void trigger() {
        var prop = item.GetProp($"!{GetPart(0)}");
        string seq = prop.GetPart("main").content;
        var action = new WorldAction(item, worldAction.tileInfo, seq);
        action.Call(); 
    }

    static void check() {

        if (!HasProp(0))
            return;

        bool fail = false;
        switch (GetPart(1)) {
            case "max":
                fail = GetProp(0).GetNumValue() >= GetProp(0).GetNumValue("max");
                break;
            case "empty":
                fail = GetProp(0).GetNumValue() == 0;
                break;
            default:
                break;
        }

        if (fail) {
            Fail(HasPart(2) ? $"{GetPart(2)}" : $"check fail : {GetPart(0)} / {GetPart(1)}");
            return;
        }
    }

    static void add() {
        if (!HasProp(0))
            return;
        if (!GetProp(0).enabled)
            return;
        int addValue = CanParse(1) ? ParsePart(1) : GetProp(1).GetNumValue();
        int nextValue = 0;

        if ( CanParse(1) ) {
            addValue = ParsePart(1);
            nextValue = GetProp(0).GetNumValue() + addValue;
        } else {
            addValue = GetProp(1).GetNumValue();
            nextValue = GetProp(0).GetNumValue() + addValue;
            if (GetProp(0).HasPart("max")) {
                if (addValue == 0 && worldAction.source == WorldAction.Source.PlayerAction) {
                    Fail($"{GetItem(1).GetText("the dog")} doesn't have any {GetProp(1).name} left");
                    return;
                }
                int max = GetProp(0).GetNumValue("max");
                int dif = max - nextValue;
                GetProp(1).SetValue(-dif);
                PropertyDescription.Add(GetItem(1), GetProp(1));
            }
        }

        GetProp(0).SetValue(nextValue);
        Property_CheckEvents(GetProp(0));
        PropertyDescription.Add(GetItem(0), GetProp(0));
    }

    static void sub() {
        if (!HasProp(0))
            return;
        if (!GetProp(0).enabled)
            return;
        int subValue = CanParse(1) ? ParsePart(1) : GetProp(1).GetNumValue();
        if (CanParse(1)) {
            subValue = ParsePart(1);
        } else {
            subValue = GetProp(1).GetNumValue();
            if (subValue == 0 && worldAction.source == WorldAction.Source.PlayerAction) {
                Fail($"{GetItem(1).GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            GetProp(1).SetValue(subValue - GetProp(0).GetNumValue());
            PropertyDescription.Add(GetItem(1), GetProp(1));
        }

        GetProp(0).SetValue(GetProp(0).GetNumValue() - subValue);
        Property_CheckEvents(GetProp(0));
        PropertyDescription.Add(GetItem(0), GetProp(0));
    }

    static void set() {
        if (!GetProp(0).enabled)
            return;

        // getting target targetPropValue
        if (GetProp(0).GetPart("value").content == GetPart(1) && worldAction.source == WorldAction.Source.PlayerAction) {
            Fail($"{GetItem(0).GetText("the dog")} is already {GetProp(0).GetDescription()}");
            return;
        }
        GetProp(0).SetValue(GetPart(1));
        Property_CheckEvents(GetProp(0));
        PropertyDescription.Add(GetItem(0), GetProp(0));
    }
    static void Property_CheckEvents(Property prop) {
        
        var valueEvents = prop.parts.FindAll(x=>x.key == "OnValue");
        if(valueEvents.Count == 0) return;
        
        foreach (var valueEvent in valueEvents){
            bool call = false;
            var content = valueEvent.content;
            var returnIndex = content.IndexOf('\n');

            var targetValue = content.Remove(returnIndex);
            var sequence = content.Remove(0, returnIndex + 1);
            if (targetValue.StartsWith('>')) {
                int i = int.Parse(targetValue.Remove(0, 1));
                call = prop.GetNumValue() > i;
            } else if (targetValue.StartsWith('<')) {
                int i = int.Parse(targetValue.Remove(0, 1));
                call = prop.GetNumValue() < i;
            } else {
                try {
                    int i = int.Parse(targetValue);
                    call = prop.GetNumValue() == i;
                } catch {
                    Debug.Log($"could'nt parse {targetValue} in seqence {content}");
                }
            }

            if (call) {
                WorldAction tileEvent = new WorldAction(item, worldAction.tileInfo, sequence);
                tileEvent.Call();
            }
        }
    }
    #endregion

}