using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

public class Line {

    public static Line current;

    public List<LinePart> parts = new List<LinePart>();
    public WorldAction worldAction;
    public bool continueOnFail;
    private Item item;
    
    public bool failed = false;

    public string content;
    public string fail_feedback = "";

    public bool debug_selected = false;

    public Line(string _content, WorldAction worldAction) {
        content = _content;
        this.worldAction = worldAction;
        item = worldAction.TargetItem();
    }

    public void Parse(WorldAction worldAction) {

        current = this;
        this.worldAction = worldAction;

        // search for []
        var functionName = content.ToLower();

        // continue on fail
        continueOnFail = false;
        if (functionName.StartsWith('*')) {
            continueOnFail = true;
            functionName = functionName.Substring(1);
        }

        functionName = functionName.Trim(' ');
        if (functionName.Contains('(')) {
            functionName = functionName.Remove(functionName.IndexOf('(')).Trim(' ');
            if (!TryInitParts(content)) {
                Fail($"{ItemLink.failMessage}");
                return;
            }
        }

        MethodInfo info = typeof(Line).GetMethod(
            functionName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        
        try {
            info.Invoke(this, null);
        } catch (Exception e) {
            Debug.LogError($"<color=yellow>item: </color>{item.debug_name}\n" + 
                $"<color=magenta>line: </color>{content}");
            Debug.LogException(e);
            Fail($"Unity Error on line:\n{content}\nitem:{item.debug_name}");
        }
    }

    #region write
    void write() {
        TextManager.Write(GetText(0));
    }
    #endregion

    #region time
    void triggerevent() {
        WorldEvent.TriggerEvent(GetText(0));
    }
    void interupt() {
        WorldActionManager.Instance.interuptNextSequence = true;
    }
    #endregion

    #region items
    void transferto() {

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
            int ci_weight = ci_wProp.GetNumValue();
            // check if weight goes above capicity
            if (ti_weigh + ci_weight >= ti_cap) {
                Fail($"{targetItem.GetText("the dog")} is too big or heavy for {GetItem(0).GetText($"the dog")} ");
                return;
            }
        }

        if (container.HasItem(targetItem) && WorldAction.active.source == WorldAction.Source.PlayerAction ) {
            Fail($"{container.GetText("the dog")} already contains {targetItem.GetText("the dog")}");
            return;
        }
        targetItem.TransferTo(container);

        ItemDescription.AddItems("describe", new List<Item>() { targetItem }, $"start:{container.GetText("on the dog")}, ");
    }
    void destroy() {
        var targetItem = HasPart(0) ? GetItem(0) : item;
        TextManager.Write($"{targetItem.GetText("the dog")} disappeared");
        Item.Destroy(targetItem);
    }

    void createitem() {

        // createItem (WHAT ITEM, *HOW MUCH, *WHERE)
        var itemName = "";
        if (GetPart(0).HasProp())
            itemName = GetProp(0).GetTextValue();
        else
            itemName = GetText(0);

        var targetTile = item.GetTile();
        // tile
        if (GetPart(2).HasItem())
            targetTile = (Tile)GetPart(2).item;
        var newItem = targetTile.CreateChildItem(itemName);

        // amount
        var amount = parts.Count == 2 ? GetPart(1).value : 1;
        for (var i = 1; i < amount; i++)
            _ = targetTile.CreateChildItem(itemName);

        if (targetTile == Tile.GetCurrent) {
            ItemDescription.AddItems("new items", new List<Item>() { newItem });
        } else {
            // in other tile
            //TextManager.Write($"target tile : {Tile.GetCurrent.GetCoords().ToString()} / event tile : {WorldAction.current.tile.GetCoords().ToString()}");
        }
    }

    void describe() {
        // the default describe function, 
        if (parts.Count == 0) {
            if (ItemParser.Instance != null && ItemParser.Instance.GetPart(0).properties != null) {
                ItemDescription.AddProperties("describe", item, ItemParser.Instance.GetPart(0).properties, "list / definite");
                return;
            }
            if ( item.HasVisibleItems())
                ItemDescription.AddItems($"{item.debug_Id} description", item.GetVisibleItems(),$"start:{item.GetText("on the dog")}, ");
            
            if (item.HasVisibleProps())
                ItemDescription.AddProperties("prop describe", item, item.GetVisibleProps(), "list / definite");
            else
                ItemDescription.AddItems("describe", new List<Item>() { item }, "list / definite");

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
    void disable() {
        var prop = GetProp(0);
        prop.Disable();
    }

    void enable() {
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var prop = GetProp(0);
        prop.Enable();
        prop.SetChanged();
    }

    void trigger() {
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var actName = GetText(0);
        var itemAct = targetItem.GetData().acts.Find(x=> x.triggers[0] == actName);
        var action = new WorldAction(item, itemAct.seq, $"Item Act:{actName}");
        action.StartSequence(); 
    }

    // marche pas avec les action parts
    List<string> _Conditions = new List<string>() {
        "present",
        "enabled",
        "disabled",
        "max",
        "different",
        "above",
        "below",
        "contains",
        "doesn't contain",
    };

    void x() {
        bool fail = false;

        var targetItem = GetPart(0).item!=null ? GetItem(0) : item;
        var targetProp = GetPart(0).prop != null ? GetProp(0) : null;
        var feedback = "";

        var condition = "";
        foreach (var sep in break_conditions) {
        int index = content.IndexOf(sep);
            if ( index >= 0) {
                condition = sep;
            }
        }
        if (string.IsNullOrEmpty(condition)) {
            Debug.LogError($"BREAK : no condition in line : {content}");
        }

        switch (condition) {
            case "present":
                fail = GetProp(0) != null;
                break;
            case "!=":
                if ( GetPart(1).text == "enabled")
                    fail = targetProp == null || targetProp != null && !targetProp.enabled;
                else if (GetPart(1).text == "disabled")
                    fail = targetProp == null || targetProp != null && targetProp.enabled;
                else
                    fail = targetProp.GetTextValue() != GetPart(1).text;
                break;
            case "==":
                if (GetPart(1).text == "enabled")
                    fail = targetProp != null && targetProp.enabled;
                else if (GetPart(1).text == "disabled")
                    fail = targetProp != null && !targetProp.enabled;
                else {
                    string checkValue = (GetPart(1).HasProp() ? GetProp(1).GetTextValue() : GetPart(1).text);
                    fail = targetProp.GetTextValue() == checkValue;
                }
                break;
            case ">>":
                Debug.Log($"part 1 value : {GetPart(0).value}");
                Debug.Log($"part 1 value : {GetPart(1).value}");
                fail = GetPart(0).value > GetPart(1).value;
                break;
            case "<<":
                Debug.Log($"part 1 value : {GetPart(0).value}");
                Debug.Log($"part 1 value : {GetPart(1).value}");
                fail = GetPart(0).value < GetPart(1).value;
                break;
            case "contains":
            case "ncontains":
                if (condition.StartsWith("n"))
                    fail = !GetPart(0).item.HasItem(GetText(1));
                else
                    fail = GetPart(0).item.HasItem(GetText(1));
                break;

            default:
                Fail($"Sequence Break : {condition} Is no condition");
                break;
        }

        if (fail) {
            foreach (var part in parts) {
                if (part.text.StartsWith("f:")) {
                    feedback = part.text.Substring(2);
                    Debug.Log($"custom feedback : {feedback}");
                    return;
                }
            }

            //var str = feedback;
            var str = $"Condition : {condition}\n";
            foreach (var pr in parts)
                str += $"{(pr.item ==null? $"[{item.debug_name}] " : $"[{pr.item.debug_name}] ")}{(pr.prop==null?"":$"{pr.prop.name}:{pr.prop.GetTextValue()}")} || ";

            Fail($"{str}");
        }
    }

    void key() {
        //Debug.Log($"added : {GetItem(0).debug_name} to history");
    }

    void add() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        int addValue = 0;
        int nextValue = 0;

        if (GetPart(1).prop == null) {
            addValue = GetPart(1).value;
            nextValue = targetProp.GetNumValue() + addValue;
        } else {
            var sourceProp = GetProp(1);
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : item;

            addValue = GetPart(1).value;
            nextValue = GetPart(0).value + addValue;
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

    // not made yet, maybe just don't do it ( add >bucket, ! ANY water>water) ET remove (>! any water>water, ? )
    void transfer() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        int addValue = 0;
        int nextValue = 0;

        if (GetPart(1).HasValue()) {
            addValue = GetPart(1).value;
            nextValue = targetProp.GetNumValue() + addValue;
        } else {
            var sourceProp = GetProp(1);
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : item;

            addValue = GetPart(1).value;
            nextValue = GetPart(0).value + addValue;
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

    void sub() {

        var targetProp = GetProp(0);
        int subValue = 0;
        if (GetPart(1).HasValue()) {
            subValue = GetPart(1).value;
        } else {
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : item;
            var sourceProp = GetProp(1);

            subValue = sourceProp.GetNumValue();
            if (subValue == 0 && WorldAction.active.source == WorldAction.Source.PlayerAction) {
                Fail($"{sourceItem.GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            sourceProp.SetValue(subValue - targetProp.GetNumValue());
        }

        targetProp.SetValue(targetProp.GetNumValue() - subValue);
    }

    void set() {


        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

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
            targetProp.SetValue(LinePart.ParseValue(GetPart(1).text));

    }
    #endregion
    
    string[] break_conditions = new string[] { "==", "!=", ">>" , "<<" };
    string[] part_Separators = new string[] { ",", "==", "!=", ">>" , "<<" };
    bool TryInitParts(string text) {
        var paramerets_all = TextUtils.Extract('(', text, out text);
        var split = paramerets_all.Split(part_Separators, StringSplitOptions.None);
        foreach (var s in split) {
            var part = new LinePart(s.Trim(' '));
            parts.Add(part);
            if (!part.TryInit(item))
                return false;
        }
        return true;
    }
    bool HasPart(int i) { return i < parts.Count; }

    LinePart GetPart(int i) { return parts[i]; }

    Item GetItem(int i) {
        return parts[i].item;
    }
    Property GetProp(int i) {
        return parts[i].prop;
    }
    public string GetText(int i) {
        return parts[i].text;
    }

    public void Fail(string message) {
        failed = true;
        if (continueOnFail)
            return;
        fail_feedback = message;
    }
}