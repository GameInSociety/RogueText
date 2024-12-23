using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


public class Line {

    public static Line current;

    public List<LinePart> parts = new List<LinePart>();
    public WorldAction worldAction;
    public bool continueOnFail;
    private Item item;

    public enum State {
        None,
        Skipped,
        Broken,
        Error,
        Done,
    }
    public State state = State.None;

    public string content;
    public string debug_text = "";
    public string error_feedback = "";
    public string stop_feedback = "";

    public bool debug_selected = false;
    string[] break_conditions = new string[] { "==", "!=", ">>", "<<" };
    string[] part_Separators = new string[] { ",", "==", "!=", ">>", "<<" };

    public Line(string _content, WorldAction worldAction) {
        content = _content;
        this.worldAction = worldAction;
        item = worldAction.TargetItem();
    }

    public void Parse(WorldAction worldAction) {

        LinePart.outB = 0;

        current = this;
        this.worldAction = worldAction;

        // search for []
        var functionName = content.ToLower();

        // continue on fail
        continueOnFail = false;
        if (functionName.StartsWith('*')) {
            continueOnFail = true;
            functionName = functionName.Substring(1).Trim (' ');
        }

        functionName = functionName.Trim(' ');

        // check if sub sequence
        if ( functionName.StartsWith('!')) {
            functionName = functionName.Substring(1);
            return;
        }
        // init parameters
        if (functionName.Contains('(')) {
            functionName = functionName.Remove(functionName.IndexOf('(')).Trim(' ');
            debug_text = $"<color=white>{functionName} (</color>";
            var paramerets_all = TextUtils.Extract('(', content, out _);
            var split = paramerets_all.Split(part_Separators, StringSplitOptions.None);
            int index = 0;
            foreach (var s in split) {
                try {
                    var newLinePart = LinePart.Parse(s.Trim(' '), item, "Sequence");
                    string colorstr = newLinePart.state == LinePart.State.Done ? "green" : newLinePart.state == LinePart.State.Error ? "red" : "yellow";
                    debug_text += $"<color={colorstr}> {newLinePart.output}{TextUtils.GetCommas(index, split.Length, false)} </color>";
                    parts.Add(newLinePart);
                } catch (LinePart.LineFailException le) {
                    current.parts.Add(LinePart.current);
                    current.Break(le.Message);
                }

                ++index;
            }

            debug_text += "<color=white> )</color>";
        }

        MethodInfo info = typeof(Line).GetMethod(
            functionName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        Debug.Log($"Invoke LIne : {current.content}");

        try {
            info.Invoke(this, null);
        } catch (Exception e) {
            Debug.Log($"[CURRENT LINE ERROR] ({current.content})");
            WorldAction.active.Error("Unity Error");

            if ( LinePart.current != null) {
                current.parts.Add(LinePart.current);
                Debug.Log($"[CURRENT LINE PART ERROR] ({LinePart.current.input})");
                current.Error("Unity Error");
                LinePart.current.Error("Unity Error");
            } else {
                Debug.Log($"[NO ACTIVE LINE PARTS]");
            }

            TextManager.Write($"\n" +
                "Unity Error On Line" +
                "\n" +
                $"{current.content}" +
                "\n", Color.red);
            Debug.LogException(e);
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
    void start() {


        // get parts
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var actName = GetPart(0).HasItem() ? GetText(1) : GetText(0);
        var partIndex = GetPart(0).HasItem() ? 2 : 1;
        
        // get act
        var sequence = targetItem.GetData().acts.Find(x => x.triggers[0] == actName);
        if ( sequence == null) {
            Debug.LogError($"no item act named : {actName} on item {targetItem._debugName}");
            return;
        }

        // set parameters
        List<WorldAction.Param> prms = new List<WorldAction.Param>();
        for (int i = partIndex; i < sequence.triggers.Length; i++) {
            if ( i >= parts.Count ) {
                Debug.LogError($"param : out of range {content}");
            }
            var newParam = new WorldAction.Param(sequence.triggers[i], GetText(i));
            prms.Add(newParam);
        }

        TriggerAction(sequence, targetItem, prms);

        // start sequence
        //action.StartSequence();
    }
    private protected WorldAction TriggerAction(Sequence sequence, Item targetItem, List<WorldAction.Param> _parameters) {


        // create action
        for (int i = 0; i < _parameters.Count; i++) {
            var prm = _parameters[i];

            // look for loop 
            if (prm.value.Contains('@')) {
                // get min / max
                var split = prm.value.Split('@');
                int a = 0;
                int b = 0;
                if (!int.TryParse(split[0], out a)) {
                    Debug.LogError($"Loop Param Parse Problem (SPLIT A)");
                    continue;
                }
                if (!int.TryParse(split[1], out b)) {
                    Debug.LogError($"Loop Param Parse Problem (SPLIT B)");
                    continue;
                }

                // loop sequences
                int br = 0;
                int dir = b > a ? 1 : -1;
                for (int l = a; l < b; l += dir) {
                    WorldAction.active = worldAction;

                    var newParams = new List<WorldAction.Param>(_parameters);
                    newParams[i] = new WorldAction.Param(prm.key, $"{l}");
                    var loopwa = TriggerAction(sequence, targetItem, newParams);
                    /*if (loopwa.state == WorldAction.State.Broken) {
                        Debug.LogError($"FAILED WHILE IN LOOP");
                        Break("LOOP FAILED");
                        return null;
                    }*/
                    ++br;
                    if ( br == 100) {
                        Debug.LogError($"breaking item act loop ");
                        return null;
                    }
                }
                return null;
            }
        }

        string param_text = "";
        foreach (var prm in _parameters) {
            param_text += $"{prm.key}({prm.value}) /";
        }
        var action = new WorldAction(targetItem, sequence.content, $"start() : {sequence.triggers[0]} ({param_text})");
        action.parameters = _parameters;
        action.StartSequence(WorldAction.Source.Event);
        return action;
    }
    void interupt() {
        WorldActionManager.Instance.interuptNextSequence = true;
    }
    #endregion

    #region items
    void transferto() {

        if (!HasPart(0) || GetPart(0).item == null) {
            Error($"no item for transferTo Action");
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
                Error($"{targetItem.GetText("the dog")} is too big or heavy for {GetItem(0).GetText($"the dog")} ");
                return;
            }
        }

        if (container.HasItem(targetItem) && WorldAction.active.source == WorldAction.Source.PlayerAction ) {
            Error($"{container.GetText("the dog")} already contains {targetItem.GetText("the dog")}");
            return;
        }
        targetItem.TransferTo(container);

        string DESCRIPTION_OPTIONS = $"start:{container.GetText("on the dog")}, ";
        var description_id = $"Transfer ({targetItem._debugName})";
        DescriptionManager.Instance.AddItem(description_id, targetItem);

        AvailableItems.UpdateItems();

    }
    void destroy() {
        var targetItem = HasPart(0) ? GetItem(0) : item;
        TextManager.Write($"{targetItem.GetText("the dog")} disappeared");
        Item.Destroy(targetItem);
        AvailableItems.UpdateItems();

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
            var description_id = $"Create Item ({newItem._debugName})";
            DescriptionManager.Instance.AddItem(description_id, newItem);
        } else {
            // in other tile
            //TextManager.Write($"target tile : {Tile.GetCurrent.GetCoords().ToString()} / event tile : {WorldAction.current.tile.GetCoords().ToString()}");
        }
        AvailableItems.UpdateItems();

    }

    void describe() {

        // the default describe function, 
        if (parts.Count == 0 || (parts.Count > 0 && GetPart(0).HasItem()) ) {

            var description_options = "";
            if (HasPart(1))
                description_options = GetPart(1).output;

            var targetItem = HasPart(0) ? GetPart(0).item : item;
            // je sais plus à quoi ça sert mais ça avait l'air important
            /*if (ItemParser.Instance != null && ItemParser.Instance.GetPart(0).properties != null) {
                DescriptionGroup.AddProperties($"Property From Input ({targetItem._debugName})", targetItem, ItemParser.Instance.GetPart(0).properties, "list / definite");
                return;
            }*/

            // Create id for item description
            string groupID = $"{targetItem._debugName}";
            // Adding item to descrition group.
            DescriptionManager.Instance.AddItem(groupID, new List<Item>() { targetItem });

            // Adding non constant props to description.
            if (targetItem.HasVisibleProps())
                DescriptionManager.Instance.AddProperty(groupID, targetItem, targetItem.GetVisibleProps());
            return;
        }

        // describing a specific prop in the item parser {"how much does bag weigh"}
        if (GetPart(0).HasProp()) {
            Debug.Log($"has prop");
            var describedItem = GetPart(0).HasItem() ? GetItem(0) : item;
            TextManager.Write($"{describedItem.GetText("the lone dog")} {GetProp(0).GetDescription()}");
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
    #endregion

    #region conditions
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

    bool ConditionMatches() {
        bool match = false;
        var targetProp = GetPart(0).prop != null ? GetProp(0) : null;
        var condition = "";
        foreach (var sep in break_conditions) {
            int index = content.IndexOf(sep);
            if (index >= 0)
                condition = sep;
        }
        if (string.IsNullOrEmpty(condition)) {
            Debug.LogError($"BREAK : no condition in line : {content}");
        }

        switch (condition) {
            case "present":
                match = GetProp(0) != null;
                break;
            case "!=":
            case "==":
                match = GetPart(0).output == GetPart(1).output;
                /*if (GetPart(1).text == "enabled")
                    match = targetProp != null && targetProp.enabled;
                else if (GetPart(1).text == "disabled")
                    match = targetProp != null && !targetProp.enabled;
                else
                    match = targetProp.GetTextValue() == GetPart(1).text;*/

                if (condition == "!=") {
                    match = !match;
                }
                break;
            case ">>":
                match = GetPart(0).value > GetPart(1).value;
                break;
            case "<<":
                match = GetPart(0).value < GetPart(1).value;
                break;
            case "contains":
            case "ncontains":
                if (condition.StartsWith("n"))
                    match = !GetPart(0).item.HasItem(GetText(1));
                else
                    match = GetPart(0).item.HasItem(GetText(1));
                break;

            default:
                Break($"Sequence Break : {condition} Is no condition");
                break;
        }
        return match;
    }

    void x() {

        string feedback = "";
        if (ConditionMatches()) {
            foreach (var part in parts) {
                if (part.output.StartsWith("f:")) {
                    feedback = part.output.Substring(2);
                    Debug.Log($"custom feedback : {feedback}");
                    return;
                }
            }
            Break($"{feedback}");
        }
    }

    void si() {
        if (!ConditionMatches()) {
            WorldAction.active.StartSkipping();
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
                Error($"{sourceItem.GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            sourceProp.SetValue(subValue - targetProp.GetNumValue());
        }

        targetProp.SetValue(targetProp.GetNumValue() - subValue);
    }

    void set() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        if (GetPart(1).HasProp()) {
            var sourceProp = GetProp(1);
            targetProp.SetValue(sourceProp.GetTextValue());
            // commenté parce que ça fourait la merde avec les coords, elles, qui ne se transf_re pas
            //sourceProp.SetValue(0);
        } else if (GetPart(1).HasValue()) {
            targetProp.SetValue(GetPart(1).value);
        } else {
            targetProp.SetValue(LinePart.ParseValue(GetPart(1).output));
        }

    }
    void addnew() {
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        targetItem.AddProp(GetPart(1).output);
    }
    void remove() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        targetItem.RemoveProp(targetProp);
    }
    #endregion
    
   
    bool HasPart(int i) { return i < parts.Count; }

    LinePart GetPart(int i) { return parts[i]; }

    Item GetItem(int i) {
        return parts[i].item;
    }
    Property GetProp(int i) {
        return parts[i].prop;
    }
    public string GetText(int i) {
        return parts[i].output;
    }

    public void Break(string message) {
        state = State.Broken;
        stop_feedback = message;
    }

    public void Error(string message) {
        state = State.Error;
        error_feedback = message;
    }
}