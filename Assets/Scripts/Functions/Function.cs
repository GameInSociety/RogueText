using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class Function {

    public static List<ActionPart> parts = new List<ActionPart>();
    static Item item;
    static bool continueOnFail;
    static bool failed = false;

    public static void TryCall( string _line, Item _item) {

        LOG($"[{_item.debug_name}] <color=grey>({_item.debug_Id})</color>", Color.magenta);
        LOG($"{_line}", Color.yellow);

        string line = _line;

        // target item
        item = _item;

        // search for []
        var functionName = line;

        // continue on fail
        continueOnFail = false;
        if (line.StartsWith('*')) {
            continueOnFail = true;
            line = line.Substring(1);
        }

        parts.Clear();
        // get options
        if (line.Contains('(')) {
            functionName = line.Remove(line.IndexOf('(')).Trim(' ');
            if (!TryInitParts(line)) {
                Fail($"{ItemLink.failMessage}");
                return;
            }
        }

        MethodInfo info = typeof(Function).GetMethod(
            functionName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        failed = false;

        try {
            info.Invoke(null, null);
        } catch (Exception e) {
            Debug.LogError($"<color=yellow>item: </color>{item.debug_name}\n" +
                $"<color=magenta>line: </color>{line}");
            Debug.LogException(e);
            Fail($"Unity Error on line:\n{line}\nitem:{item.debug_name}");
            
        }

        /*foreach (var part in parts) {
            if (part.prop != null) {
                var it = part.HasItem() ? part.item : item;
                Property_CheckEvents(it, part.prop);
                PropertyDescription.Add(it, part.prop, WorldAction.current.source, failed);
            }
        }*/
    }

    #region write
    static void write() {
        TextManager.Write(GetText(0));
    }
    #endregion

    #region time
    static void triggerEvent() {
        WorldEvent.TriggerEvent(GetText(0));
    }
    #endregion

    #region items
    static void transferTo() {

        if (!HasPart(0) || GetPart(0).item == null) {
            Fail($"no item for transferTo Action");
            return;
        }

        var targetItem = HasPart(1) ? GetItem(0) : item;
        var container = HasPart(1) ? GetItem(1) : GetItem(0);

        if (targetItem.HasProp("weight") && container.HasProp("weight") && container.HasProp("capacity")) {
            // get target item pick up props
            var ci_wProp = item.GetProp("weight");
            int ti_weigh = GetItem(0).GetProp("weight").GetNumValue();
            int ti_cap = GetItem(0).GetProp("capacity").GetNumValue();
            // get current item group weight
            int ci_weight = ci_wProp.GetNumValue() * WorldAction.current.GetItems().Count;
            // check if weight goes above capicity
            if (ti_weigh + ci_weight >= ti_cap) {
                Fail($"{targetItem.GetText("the dog")} is too big or heavy for {GetItem(0).GetText($"the dog")} ");
                return;
            }
        }

        if (container.hasItem(targetItem)) {
            Fail($"{container.GetText("the dog")} already contains {targetItem.GetText("the dog")}");
            return;
        }
        targetItem.TransferTo(container);

        TextManager.Write($"{targetItem.GetText("the dog")} is now in {container.GetText("the dog")}");
    }
    static void destroy() {
        var targetItem = HasPart(0) ? GetItem(0) : item;
        TextManager.Write($"{targetItem.GetText("the dog")} disappeared");
        Item.Destroy(targetItem);
    }

    static void createItem() {

        // createItem (WHAT ITEM, *HOW MUCH, *WHERE)
        var itemName = "";
        if (GetPart(0).HasProp())
            itemName = GetProp(0).GetTextValue();
        else
            itemName = GetText(0);

        var targetTile = WorldAction.current.tile;
        // tile
        if (GetPart(2).HasTile())
            targetTile = GetPart(2).tile;

        var newItem = targetTile.CreateChildItem(itemName);

        // amount
        var amount = parts.Count == 2 ? GetPart(1).value : 1;
        for (var i = 1; i < amount; i++)
            _ = targetTile.CreateChildItem(itemName);

        

        if (targetTile == Tile.GetCurrent) {
            TextManager.Write($"{newItem.GetText("a dog")} appeared");
        } else {
            TextManager.Write($"target tile : {Tile.GetCurrent.coords.ToString()} / event tile : {WorldAction.current.tile.coords.ToString()}");
        }
    }

    static void describe() {
        // the default describe function, 
        if (parts.Count == 0) {
            if (ItemParser.GetCurrent != null && ItemParser.GetCurrent.GetPart(0).properties != null) {
                ItemDescription.AddProperties("describe", item, ItemParser.GetCurrent.GetPart(0).properties, "list / definite");
                return;
            }
            if ( item.HasVisibleItems())
                ItemDescription.AddItems($"{item.debug_Id} description", item.GetVisibleItems(),$"start:{item.GetText("on the dog")}, ");
            else
                ItemDescription.AddItems("describe", new List<Item>() {item}, "list / definite");
            
            if (item.HasVisibleProps())
                ItemDescription.AddProperties("prop describe", item, item.GetVisibleProps(), "list / definite");
            return;
        }

        // the specific description of the tile, quite to complicated to do in data
        if (GetText(0) == "_TILE") {
            Tile.GetCurrent.Describe();
            return;
        }

        // describing a specific prop in the item parser {"how much does bag weigh"}
        if (GetPart(0).HasProp()) {
            Debug.Log($"has prop");
            var describedItem = GetPart(0).HasItem() ? GetItem(0) : item;
            TextManager.Write($"{describedItem.GetText("the lone dog")} {GetProp(0).GetCurrentDescription()}");
            return;
        }

        TextManager.Write("not supposed to go here in the describe action");
    }
    #endregion

    #region props
    static void disable() {
        var prop = GetProp(0);
        prop.enabled = false;
        prop.SetChanged();
    }

    static void enable() {
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var prop = GetProp(0);
        prop.enabled = true;
        prop.SetChanged();
    }

    static void trigger() {
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var actName = GetText(0);
        var itemAct = targetItem.GetData().acts.Find(x=> x.triggers[0] == actName);
        var action = new WorldAction(item, WorldAction.current.tileInfo, itemAct.seq);
        action.Call(); 
    }

    static void breakIf() {
        bool fail = false;

        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var targetProp = GetProp(0);
        var feedback = "";

        switch (GetText(1)) {
            case "enabled":
                fail = targetProp == null || targetProp != null && targetProp.enabled;
                feedback = $"can't, {targetItem.GetText("the dog")} is already {targetProp.GetCurrentDescription()}";
                break;
            case "disabled":
                fail = targetProp == null || targetProp != null && !targetProp.enabled;
                feedback = $"can't {targetItem.GetText("the dog")} isn't {targetProp.GetCurrentDescription()}";
                break;
            case "max":
                fail = targetProp.GetNumValue() >= targetProp.GetNumValue("max");
                break;
            case "empty":
                fail = targetProp.GetNumValue() == 0;
                break;
            case "not empty":
                fail = targetProp.GetNumValue() > 0;
                feedback = $"can't, {targetItem.GetText("the dog")} is already {targetProp.GetCurrentDescription()}";
                break;
            case "not":
                fail = targetProp.GetTextValue() != GetProp(2).GetTextValue();
                feedback = $"this {targetItem.debug_name}({targetProp.GetTextValue()})" +
                    $"doesn't match this {GetItem(2).debug_name}({GetProp(2).GetTextValue()})";
                break;
            case "equals":
                string checkValue = (GetPart(2).HasProp() ? GetProp(2).GetTextValue() : GetPart(2).text);
                fail = targetProp.GetTextValue() == checkValue;
                break;
            case "ABOVE":
                fail = targetProp.GetNumValue() > (GetPart(2).HasProp() ? GetProp(2).GetNumValue() : GetPart(2).value);
                break;
            case "BELOW":
                fail = targetProp.GetNumValue() < (GetPart(2).HasProp() ? GetProp(2).GetNumValue() : GetPart(2).value);
                break;
            case "PERCENT":
            case "NOT PERCENT":
                if (!targetProp.HasPart("max")) {
                    Debug.LogError($"{targetProp.name} doens't have max, can't do percent");
                }
                var max = targetProp.GetNumValue("max");
                var lerp = (float)targetProp.GetNumValue() / max;
                var minPercent = GetPart(2).value;
                var maxPercent = GetPart(3).value;
                var checkPercent = UnityEngine.Random.value * 100f;
                var propPercent = minPercent + (maxPercent - minPercent) * lerp;
                Debug.Log($"lerp : {lerp}");
                Debug.Log($"prop percent : {propPercent}");
                Debug.Log($"max percent : {maxPercent}");
                Debug.Log($"min percent : {minPercent}");
                fail = GetText(1) == "NOT PERCENT" ? checkPercent < propPercent : checkPercent > propPercent;
                Debug.Log($"fail ? : {fail}");
                feedback = GetPart(4).text;
                break;
            default:
                break;
        }

        if (fail) {
            if ( string.IsNullOrEmpty( feedback ) ) {
                feedback = $"can't, {targetItem.GetText("the dog")} is {targetProp.GetCurrentDescription()}";
            }
            var str = feedback;
            Fail($"{str}");
        }
    }
    static void IF() {
        Debug.Log($"IF");

        int value = GetProp(0).GetNumValue();
        bool success = TextUtils.GetCondition(GetPart(1).text, value);

        if (success) {
            Debug.Log($"success");
            // do nothing, just continue functions
        } else {
            Debug.Log($"fail, going to next ~~");
            // tell world action to wait for next ~~
            WorldAction.current.goToNextPart = true;
        }
    }

    static void add() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        if (!targetProp.enabled)
            return;

        int addValue = 0;
        int nextValue = 0;

        if (GetPart(1).HasValue()) {
            addValue = GetPart(1).value;
            nextValue = targetProp.GetNumValue() + addValue;
        } else {
            var sourceProp = GetProp(1);
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : item;

            addValue = sourceProp.GetNumValue();
            nextValue = targetProp.GetNumValue() + addValue;
            if (addValue == 0 && WorldAction.current.source == WorldAction.Source.PlayerAction) {
                Fail($"{sourceItem.GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            // source prop changing
            if (targetProp.HasPart("max")) {
                int max = targetProp.GetNumValue("max");
                int dif = max - nextValue;
                sourceProp.SetValue(-dif);
            } else {
                int dif = nextValue;
                sourceProp.SetValue(-dif);
            }
        }

        targetProp.SetValue(nextValue);
    }

    static void sub() {

        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        if (!targetProp.enabled)
            return;

        int subValue = 0;
        if (GetPart(1).HasValue()) {
            subValue = GetPart(1).value;
        } else {
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : item;
            var sourceProp = GetProp(1);

            subValue = sourceProp.GetNumValue();
            if (subValue == 0 && WorldAction.current.source == WorldAction.Source.PlayerAction) {
                Fail($"{sourceItem.GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            sourceProp.SetValue(subValue - targetProp.GetNumValue());
        }

        targetProp.SetValue(targetProp.GetNumValue() - subValue);
    }

    static void set() {

        // check if we're setting an abstract item
        if (GetPart(0).text.StartsWith('#')) {
                var abstractItem = GetPart(1).text == "this" ? item : GetItem(1);
            WorldData.SetAbstractItem(GetPart(0).text.Remove(0, 1), abstractItem);
            Debug.Log($"setting abstract item : {GetPart(0).text} to : {abstractItem.debug_name}");
            return;
        }

        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        if (!targetProp.enabled)
            return;

        // getting target targetPropValue
        /*if (targetProp.GetPart("value").content == GetText(1) && WorldAction.current.source == WorldAction.Source.PlayerAction) {
            Debug.Log($"item : {targetItem.debug_name}");
            Debug.Log($"prop : {targetProp.name}");
            Fail($"{targetItem.GetText("the dog")} is already {targetProp.GetDescription()}");
            return;
        }*/

        if (GetPart(1).HasProp()) {
            var sourceProp = GetProp(1);
            targetProp.SetValue(sourceProp.GetTextValue());
            // commenté parce que ça fourait la merde avec les coords, elles, qui ne se transf_re pas
            //sourceProp.SetValue(0);
        } else if (GetPart(1).HasValue())
            targetProp.SetValue(GetPart(1).value);
        else
            targetProp.SetValue(GetPart(1).text);

    }
    public enum OnValueCondition {
        Above,
        Below,
        Equals
    }
    static void Property_CheckEvents(Item targetItem, Property prop) {
        if (!prop.enabled)
            return;

        var valueEvents = prop.parts.FindAll(x=>x.key == "OnValue");
        if (valueEvents.Count == 0)
            return;

        foreach (var valueEvent in valueEvents){
            var content = valueEvent.content;
            
            // getting the condition
            var returnIndex = content.IndexOf('\n');
            var condition_text = content.Remove(returnIndex);
            // 

            var propValue = prop.GetNumValue();
            var targetValue = 0;

            bool call = false;

            var onValCondition = OnValueCondition.Equals;
            if (condition_text.StartsWith("ABOVE")) {
                onValCondition = OnValueCondition.Above;
                condition_text = condition_text.Remove(0, "ABOVE".Length);
            }
            if (condition_text.StartsWith("BELOW")) {
                onValCondition = OnValueCondition.Below;
                condition_text = condition_text.Remove(0, "BELOW".Length);
            }
            condition_text = condition_text.Trim(' ');

            if (condition_text.StartsWith('[')) {
                var key = TextUtils.Extract('[', condition_text, out condition_text);
                var onValuePart = new ActionPart(key);
                if (!onValuePart.TryInit(targetItem))
                    LOG($"[{targetItem.debug_name}/{prop.name}] OnValue link prop failed", Color.red);
                targetValue = onValuePart.prop.GetNumValue();
            } else {
                targetValue = int.Parse(condition_text);
            }


            switch (onValCondition) {
                case OnValueCondition.Above:
                    call = propValue > targetValue;
                    break;
                case OnValueCondition.Below:
                    call = propValue < targetValue;
                    break;
                case OnValueCondition.Equals:
                    call = propValue == targetValue;
                    break;
            }

            // getting the sequence
            var sequence = content.Remove(0, returnIndex + 1);
            if (call) {
                var tileEvent = new WorldAction(targetItem, WorldAction.current.tileInfo, sequence);
                tileEvent.Call();
            }
        }
    }
    #endregion
    static bool TryInitParts(string text) {
        var paramerets_all = TextUtils.Extract('(', text, out text);
        var stringSeparators = new string[] { ", " };
        var split = paramerets_all.Split(stringSeparators, StringSplitOptions.None);
        foreach (var s in split) {
            var part = new ActionPart(s.Trim(' '));
            parts.Add(part);
            ADDLOG($" [{part.text}] ", Color.white);
            if (!part.TryInit(item))
                return false;
        }
        return true;
    }
    static bool HasPart(int i) { return i < parts.Count; }

    static ActionPart GetPart(int i) { return parts[i]; }

    static Item GetItem(int i) {
        return parts[i].item;
    }
    static Property GetProp(int i) {
        return parts[i].prop;
    }
    public static string GetText(int i) {
        return parts[i].text;
    }

    public static void Fail(string message) {
        failed = true;
        if (continueOnFail)
            return;

        WorldAction.current.Fail(message);
        LOG($"[FAIL] {message}", Color.red);
    }

    public static string log;
    public static void ADDLOG(string message, Color color) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $"{txt_color}{message}</color>";
        log += str;
    }
    public static void LOG(string message, Color color) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $"\n{txt_color}{message}</color>";
        log += str;
    }
}