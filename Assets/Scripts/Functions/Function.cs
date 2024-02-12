using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;
using UnityEngine.UIElements;

public static class Function {
    static string[] prms = new string[0];
    static Item[] items;
    static Property[] props;
    static Item item;
    static bool continueOnFail;

    static bool HasItem(int i) {
        if (!HasPart(i))
            return false;
        var key = GetPart(i);
        if (key.Contains('>'))
            key = key.Remove(GetPart(i).IndexOf('>'));
        return ItemLink.SearchItem(key)!=null;
    }

    static Item GetItem(int i) {
        if (!HasPart(i))
            return item;
        var key = GetPart(i);
        if (key.Contains('>'))
            key = key.Remove(GetPart(i).IndexOf('>'));
        if (items[i] == null) {
            var search = ItemLink.SearchItem(key);
            items[i] = search;
        }
        return items[i];
    }

    static Item GetPropItem(int i) {
        if (!HasPart(i))
            return item;
        var key = GetPart(i);
        if (key.Contains('>'))
            key = key.Remove(GetPart(i).IndexOf('>'));
        else {
            return item;
        }

        if (items[i] == null) {
            var search = ItemLink.SearchItem(key);
            if (search == null) search = item;
            items[i] = search;
        }
        return items[i];
    }

    static bool HasProp(int i) {
        if (!HasPart(i))
            return false;
        return ItemLink.GetProperty(GetPart(i), item) != null;
    }

    static Property GetProp(int i) {
        if (!HasPart(i))
            return null;
        if (props[i] == null)
            props[i] = ItemLink.GetProperty(GetPart(i), item);
        return props[i];
    }

    public static string GetPart(int i) {
        if (i >= prms.Length)
            return "no contents";
        return prms[i];
    }

    public static void Fail(string message) {
        if ( continueOnFail) {
            return;
        }
        Debug.Log($"failed : {message}");
        WorldAction.current.Fail(message);
    }

    public static bool NoParts() { return prms.Length > 0; }
    public static bool HasPart(int i) { return i < prms.Length; }
    public static int PartCount() { return prms.Length; }
    public static int ParsePart(int i) { return int.Parse(prms[i]); }
    public static bool CanParse(int i) { int a = 0; return int.TryParse(prms[i], out a); }
    public static bool HasLinks(int i) { return prms[i].Contains('[') || prms[i].Contains('$'); }
    public static void TryCall( string _line, Item _item) {

        string line = _line;

        item = _item;

        // search for []
        var functionName = line;

        continueOnFail = false;
        if (line.StartsWith('*')) {
            continueOnFail = true;
            line = line.Substring(1);
        }

        // get options
        if (line.Contains('(')) {
            functionName = line.Remove(line.IndexOf('('));
            var paramerets_all = TextUtils.Extract('(', line, out line);
            var stringSeparators = new string[] { ", " };
            prms = paramerets_all.Split(stringSeparators, StringSplitOptions.None);
            items = new Item[prms.Length];
            props = new Property[prms.Length];
        } else {
            prms = new string[0];
            items = new Item[0];
            props = new Property[0];
        }
            
        MethodInfo info = typeof(Function).GetMethod(
            functionName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (info == null) {
            Debug.LogError($"no method named {functionName} found");
        }

        try {
            info.Invoke(null, null);
        } catch (Exception e) {
            Debug.LogError($"<color=yellow>item: </color>{item.debug_name}\n" +
                $"<color=magenta>line: </color>{line}");
            Debug.LogException(e);
        }
    }

    #region write
    static void write() {
                TextManager.Write(GetPart(0));
    }
    #endregion

    #region time
    static void wait() {
        int count = 0;
        if (ItemParser.GetCurrent.GetPart()?.number >= 0) {
            count = ItemParser.GetCurrent.GetPart().number;
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

        if (item.HasProp("entrance")) {
            Interior.Exit();
        }

        Tile targetTile = item as Tile;
        if ( targetTile != null) {
            // move to tile
            Player.Instance.MoveToTile(item as Tile);
            return;
        }

        if (!HasPart(0)) {
            Fail($"can't go to {item.GetText("the dog")}");
            return;
        }

        var target = GetProp(0);

        var coords = Player.Instance.coords + Humanoid.GetCoordsFromCardinal(target.GetTextValue());
        Player.Instance.Move(coords);
        TextManager.Write("move with player orientation here");
    }
    #endregion

    #region items
    static void transferTo() {

        var itmText = ItemDescription.NewDescription(WorldAction.current.GetItems());

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
        int ci_weight = ci_wProp.GetNumValue() * WorldAction.current.GetItems().Count;
        // check if weight goes above capicity
        if (ti_weigh + ci_weight>= ti_cap) {
            Fail($"{itmText} is too big or heavy for {GetItem(0).GetText($"the dog")} ");
            return;
        }

        foreach (var it in WorldAction.current.GetItems()) {
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
        var targetItem = item;
        if (HasPart(0))
            targetItem = GetItem(0);
        Item.Destroy(targetItem);
    }

    static void createItem() {

        // createItem (WHAT ITEM, *HOW MUCH, *WHERE)

        var amount = HasPart(1) ? ParsePart(1) : 1;

        var itemName = "";
        if (HasProp(0)) {
            itemName = GetProp(0).GetTextValue();
        } else {
            itemName = GetPart(0);
        }

        var newItem = WorldAction.current.tile.CreateChildItem(itemName);
        Debug.Log($"creating item {newItem.debug_name}");

        for (var i = 1; i < amount; i++) {
            _ = WorldAction.current.tile.CreateChildItem(itemName);
        }

        if (WorldAction.current.tile == Tile.GetCurrent) {
            TextManager.Write($"{newItem.GetText("a dog")} appeared");
            AvailableItems.UpdateItems();
        } else {
            TextManager.Write($"target tile : {Tile.GetCurrent.coords.ToString()} / event tile : {WorldAction.current.tile.coords.ToString()}");
        }
    }

    static void describe() {

        if ( HasPart(0)) {
            Debug.Log($"has second part");
            if (GetPart(0) == "tile") {
                Tile.GetCurrent.Describe();
                return;
            }

            if ( HasProp(0)) {
                Debug.Log($"has prop");
                var describedItem = HasItem(0) ? GetItem(0) : item;
                TextManager.Write($"{describedItem.GetText("the lone dog")} is {GetProp(0).GetDescription()}");
                return;
            }

        }

        //if ( WorldAction.current.GetItems().Count == 1) {
        /*if (true) {
            // NO PROPERTIES OR CHILD ITEMS
            if (item.GetVisibleProps().Count == 0 && !item.HasChildItems()) {
                TextManager.Write($"nothing special about this {item.GetText("dog")}");
                return;
            }

            //
            var vProps = item.GetVisibleProps();
            if ( vProps.Count > 0)
                TextManager.Write($"{item.GetText("a lone dog")}, {Property.GetDescription(vProps)}");
            else
                TextManager.Write($"a {item.GetText("a dog")}, {Property.GetDescription(vProps)}");

            // DISPLAY CHILD ITEMS
            if (item.HasChildItems()) {
                var description = ItemDescription.NewDescription(item.GetChildItems());
                TextManager.Write($"there's {description} inside");
            }
            return;
        }
        TextManager.Write($"{ItemDescription.NewDescription(WorldAction.current.GetItems(), "")}");
        */
        ItemDescription.DelayDescription(item);

        return;
    }
    #endregion


    #region props
    static void disable() {
        GetProp(0).enabled = false;
        var targetItem = HasItem(0) ? GetItem(0) : item;
        PropertyDescription.Add(targetItem, GetProp(0));
    }

    static void enable() {
        var prop = GetProp(0);
        prop.enabled = true;
        Property_CheckEvents(prop);
        Debug.Log($"checking events for : {prop.name}");
        var targetItem = HasItem(0) ? GetItem(0) : item;
        prop.SetChanged();
        PropertyDescription.Add(targetItem, prop);

    }

    static void trigger() {
        var prop = item.GetProp($"!{GetPart(0)}");
        string seq = prop.GetPart("main").content;
        var action = new WorldAction(item, WorldAction.current.tileInfo, seq);
        action.Call(); 
    }

    static void check() {

        bool not = false;
        if ( GetPart(0).StartsWith("NOT ")) {
            not = true;
            prms[0] = prms[0].Substring(4);
        }

        if (!HasProp(0)) {
            Fail($"no {item.GetText("dog")} {GetPart(0)}");
            return;
        }

        var defaultFeedback = "";
        bool fail = false;
        if (!HasPart(1)) {
            // simple check if the prop exists / is enable
            if ( not) {
                fail = !HasProp(0) || (HasProp(0) && !GetProp(0).enabled);
                defaultFeedback = $"{GetPropItem(0).GetText("the dog")} isn't {GetProp(0).GetDescription()}";
            } else {
                // ATTENTION ICI C'est le meme dilemme. il faut différencier si la prop est là ou si elle est activée
                fail = HasProp(0) ? GetProp(0).enabled : false;
                defaultFeedback = $"{GetPropItem(0).GetText("the dog")} is {GetProp(0).GetDescription()}";
            }
            
        } else {
            switch (GetPart(1)) {
                case "max":
                    fail = GetProp(0).GetNumValue() >= GetProp(0).GetNumValue("max");
                    defaultFeedback = $"the {GetProp(0).name} is already full";
                    break;
                case "empty":
                    fail = GetProp(0).GetNumValue() == 0;
                    defaultFeedback = $"the {GetProp(0).name} is empty";
                    break;
                case "not empty":
                    fail = GetProp(0).GetNumValue() > 0;
                    defaultFeedback = $"the {GetProp(0).name} is not empty";
                    break;
                case "match":
                    if (!HasProp(2)) {
                        defaultFeedback = "?";
                        fail = true;
                        break;
                    }
                    fail = GetProp(0).GetNumValue() != GetProp(2).GetNumValue();
                    defaultFeedback = $"this {GetPropItem(0).debug_name}({GetProp(0).GetNumValue()})" +
                        $"doesn't match this {GetPropItem(2).debug_name}({GetProp(2).GetNumValue()})";
                    break;
                default:
                    break;
            }
        }

        if (fail) {
            Fail($"{defaultFeedback}");
        }
    }

    static void add() {
        if (!HasProp(0))
            return;
        if (!GetProp(0).enabled) {
            return;
        }
        int addValue = CanParse(1) ? ParsePart(1) : GetProp(1).GetNumValue();
        int nextValue = 0;
        var targetItem = GetPropItem(0) == null ? item : GetPropItem(0);

        if (CanParse(1)) {
            addValue = ParsePart(1);
            nextValue = GetProp(0).GetNumValue() + addValue;
        } else {
            addValue = GetProp(1).GetNumValue();
            nextValue = GetProp(0).GetNumValue() + addValue;
            if (addValue == 0 && WorldAction.current.source == WorldAction.Source.PlayerAction) {
                Fail($"{GetPropItem(1).GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            if (GetProp(0).HasPart("max")) {
                int max = GetProp(0).GetNumValue("max");
                int dif = max - nextValue;
                GetProp(1).SetValue(-dif);
            } else {
                int dif = nextValue;
                GetProp(1).SetValue(-dif);
            }
            PropertyDescription.Add(GetPropItem(1), GetProp(1));
        }

        GetProp(0).SetValue(nextValue);
        Property_CheckEvents(GetProp(0));
        if (WorldAction.current.source == WorldAction.Source.PlayerAction)
            GetProp(0).SetPart("changed", "");
        PropertyDescription.Add(targetItem, GetProp(0));
    }

    static void sub() {
        if (!HasProp(0))
            return;
        if (!GetProp(0).enabled) {
            Debug.Log($"no updating {GetPart(0)}, it's disabled");
            return;
        }

        var targetItem = GetPropItem(0) == null ? item : GetPropItem(0);

        int subValue = CanParse(1) ? ParsePart(1) : GetProp(1).GetNumValue();
        if (CanParse(1)) {
            subValue = ParsePart(1);
        } else {
            subValue = GetProp(1).GetNumValue();
            if (subValue == 0 && WorldAction.current.source == WorldAction.Source.PlayerAction) {
                Fail($"{GetPropItem(1).GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            GetProp(1).SetValue(subValue - GetProp(0).GetNumValue());
            PropertyDescription.Add(GetPropItem(1), GetProp(1));
        }

        GetProp(0).SetValue(GetProp(0).GetNumValue() - subValue);
        Property_CheckEvents(GetProp(0));
        PropertyDescription.Add(targetItem, GetProp(0));
    }

    static void set() {
        if (!GetProp(0).enabled)
            return;

        var targetItem = GetPropItem(0) == null ? item : GetPropItem(0);

        // getting target targetPropValue
        if (GetProp(0).GetPart("value").content == GetPart(1) && WorldAction.current.source == WorldAction.Source.PlayerAction) {
            Debug.Log($"item : {GetPropItem(0).debug_name}");
            Debug.Log($"item : {GetProp(0).name}");
            Fail($"{GetPropItem(0).GetText("the dog")} is already {GetProp(0).GetDescription()}");
            return;
        }

        int i = GetProp(1).ParsePart("value");
        Debug.Log($"parsing part value of {GetProp(0).name}: {i}");
        if (HasProp(1) && i >= 0) {
            GetProp(0).SetValue(GetProp(1).GetNumValue());
            GetProp(1).SetValue(0);
        } else {
            GetProp(0).SetValue(GetProp(1).GetTextValue());
        }


        Property_CheckEvents(GetProp(0));
        PropertyDescription.Add(targetItem, GetProp(0));
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
                var tileEvent = new WorldAction(item, WorldAction.current.tileInfo, sequence);
                tileEvent.Call();
            }
        }
    }
    #endregion

}