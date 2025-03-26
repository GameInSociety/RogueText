using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Step {

    #region inner classes
    [Serializable]
    public class Data {
        public Data(string name) {
            _name = name;
        }
        public string _name;
        // step category ( ite things, property things, engine things, flow things etc... à concevoir )
        public string _category;
        public List<Profile> _profiles = new List<Profile>();
    }

    [Serializable]
    public class Profile {
        public Profile(string description) {
            _description = description;
        }
        public string _description;
        public List<Slot.Data> _slotDatas = new List<Slot.Data>();
    }
    public enum State {
        None,
        Skipped,
        Broken,
        Error,
        Done,
    }
    #endregion

    #region static declaration
    // Keeps track of active step (?)
    public static Step s_current;
    public static List<Step.Data> _datas = new List<Data>();
    #endregion

    #region local declaration
    public Data _data;
    public string _stepType; // step type ( type of method )
    public string _content; // unparse, raw text input
    public List<Slot> _slots = new List<Slot>();    // All slots contained in step.
    public Sequence _sequence;                      // The sequence from where the step originated (?)
    public bool _continueOnFail;                    // Does the sequence continue if step fails/breaks ?
    private Item _item;                             // the main, target item of the step. dunno (?)
    public State state = State.None;
    public string debug_text = "";
    public string error_feedback = "";
    public string stop_feedback = "";
    string[] break_conditions = new string[] { "==", "!=", ">>", "<<" };
    string[] slot_Separators = new string[] { ",", "==", "!=", ">>", "<<" };
    #endregion

    #region constructor
    public Step(string content, Sequence sequence) {
        // set value
        _content = content;
        _sequence = sequence;
        _item = sequence.TargetItem();

        // ? a priori juste un safe break pour un loop ( pour parser les sockets )
        Slot.outB = 0;

        // Set Current step
        s_current = this;
        this._sequence = sequence;

        // trim
        _content = _content.Trim(' ');

        // check for cof & trim
        _continueOnFail = false;
        if (_content.StartsWith('*')) {
            _continueOnFail = true;
            _content = _content.Substring(1).Trim(' ');
        }

        // Getting function name
        _stepType = _content.ToLower();
        _stepType = _stepType.Trim(' ');

        // Check if sub sequence (?)
        if (_stepType.StartsWith('!')) {
            _stepType = _stepType.Substring(1);
            Debug.LogError($"Thing is a subsequence");
            return;
        }

        /// SLOT PARSING /// 
        if (_content.Contains('(')) {
            // Trim function name accordingly
            _stepType = _stepType.Remove(_stepType.IndexOf('(')).Trim(' ');
            var paramerets_all = TextUtils.Extract('(', _content, out _);
            var slots_txt = paramerets_all.Split(slot_Separators, StringSplitOptions.None);
            foreach (var s in slots_txt) {
                var newSlot = Slot.Parse(s.Trim(' '), _item, "Sequence");
                _slots.Add(newSlot);
            }
        }

        var targetData = _datas.Find(x => x._name == _stepType);
        _data = targetData;
    }
    #endregion

    public void Call() {
        // Calling here instead of from parsing.

        // Invoking step method 
        MethodInfo info = typeof(Step).GetMethod(
            _stepType,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        try {
            info.Invoke(this, null);
        } catch (Exception e) {
            Sequence.active.Error("Unity Error");

            if (Slot.current != null) {
                Debug.Log($"[CURRENT LINE PART ERROR] ({Slot.current._input})");
                s_current.Error("Unity Error");
                Slot.current.Error("Unity Error");
            } else {
                Debug.Log($"[NO ACTIVE LINE PARTS]");
            }

            TextManager.Write($"\n" +
                "Unity Error On Line" +
                "\n" +
                $"{s_current._content}" +
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
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : _item;
        var sequenceName = GetPart(0).HasItem() ? GetText(1) : GetText(0);
        var slotIndex = GetPart(0).HasItem() ? 2 : 1;

        // get act
        var sequence = targetItem.GetData().sequences.Find(x => x.triggers[0] == sequenceName);
        if (sequence == null) {
            Debug.LogError($"no item act named : {sequenceName} on item {targetItem._debugName}");
            return;
        }

        // set parameters
        List<Sequence.Param> prms = new List<Sequence.Param>();
        for (int i = slotIndex; i < sequence.triggers.Length; i++) {
            if (i >= _slots.Count) {
                Debug.LogError($"param : out of range {_content}");
            }
            var newParam = new Sequence.Param(sequence.triggers[i], GetText(i));
            prms.Add(newParam);
        }

        TriggerAction(sequence, targetItem, prms);

        // start sequence
        //action.StartSequence();
    }

    // this is for loop woops.
    private protected Sequence TriggerAction(Sequence sequence, Item targetItem, List<Sequence.Param> _parameters) {
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
                    Sequence.active = this._sequence;

                    var newParams = new List<Sequence.Param>(_parameters);
                    newParams[i] = new Sequence.Param(prm.key, $"{l}");
                    var loopwa = TriggerAction(sequence, targetItem, newParams);
                    /*if (loopwa.state == WorldAction.State.Broken) {
                        Debug.LogError($"FAILED WHILE IN LOOP");
                        Break("LOOP FAILED");
                        return null;
                    }*/
                    ++br;
                    if (br == 100) {
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
        var step = new Sequence(targetItem, sequence.mContent);
        step.parameters = _parameters;
        step.StartSequence(Sequence.Source.Event);
        return step;
    }
    void interupt() {
        SequenceManager.Instance.interuptNextSequence = true;
    }
    #endregion

    void wait() {

    }

    #region items
    void transferto() {

        if (!HasPart(0) || GetPart(0).item == null) {
            Error($"no item for transferTo Action");
            return;
        }

        var targetItem = HasPart(1) ? GetItem(0) : _item;
        var container = HasPart(1) ? GetItem(1) : GetItem(0);

        if (targetItem.HasProp("weight") && container.HasProp("weight") && container.HasProp("capacity")) {
            // get target item pick up props
            var ci_wProp = _item.GetProp("weight");
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

        if (container.HasItem(targetItem) && Sequence.active.source == Sequence.Source.PlayerAction) {
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
        var targetItem = HasPart(0) ? GetItem(0) : _item;
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

        var targetTile = _item.GetTile();
        // tile
        if (GetPart(2).HasItem())
            targetTile = (Tile)GetPart(2).item;
        var newItem = targetTile.CreateChildItem(itemName);

        // amount
        var amount = _slots.Count == 2 ? GetPart(1).value : 1;
        for (var i = 1; i < amount; i++)
            _ = targetTile.CreateChildItem(itemName);

        if (targetTile == Tile.GetCurrent) {
            var description_id = $"Create Item ({newItem._debugName})";
            DescriptionManager.Instance.AddItem(description_id, newItem);
        }
        AvailableItems.UpdateItems();

    }

    void describe() {

        // the default describe function, 
        if (_slots.Count == 0 || (_slots.Count > 0 && GetPart(0).HasItem())) {

            var description_options = "";
            if (HasPart(1))
                description_options = GetPart(1)._output;

            var targetItem = HasPart(0) ? GetPart(0).item : _item;
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
            var describedItem = GetPart(0).HasItem() ? GetItem(0) : _item;
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
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : _item;
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
            int index = _content.IndexOf(sep);
            if (index >= 0)
                condition = sep;
        }
        if (string.IsNullOrEmpty(condition)) {
            Debug.LogError($"BREAK : no condition in line : {_content}");
        }

        switch (condition) {
            case "present":
                match = GetProp(0) != null;
                break;
            case "!=":
            case "==":
                match = GetPart(0)._output == GetPart(1)._output;
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
            foreach (var part in _slots) {
                if (part._output.StartsWith("f:")) {
                    feedback = part._output.Substring(2);
                    Debug.Log($"custom feedback : {feedback}");
                    return;
                }
            }
            Break($"{feedback}");
        }
    }

    void si() {
        if (!ConditionMatches()) {
            Sequence.active.StartSkipping();
        }
    }

    void key() {
        //Debug.Log($"added : {GetItem(0).debug_name} to history");
    }

    void add() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : _item;

        int addValue = 0;
        int nextValue = 0;

        if (GetPart(1).prop == null) {
            addValue = GetPart(1).value;
            nextValue = targetProp.GetNumValue() + addValue;
        } else {
            var sourceProp = GetProp(1);
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : _item;

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
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : _item;

        int addValue = 0;
        int nextValue = 0;

        if (GetPart(1).HasValue()) {
            addValue = GetPart(1).value;
            nextValue = targetProp.GetNumValue() + addValue;
        } else {
            var sourceProp = GetProp(1);
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : _item;

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
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : _item;
            var sourceProp = GetProp(1);

            subValue = sourceProp.GetNumValue();
            if (subValue == 0 && Sequence.active.source == Sequence.Source.PlayerAction) {
                Error($"{sourceItem.GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            sourceProp.SetValue(subValue - targetProp.GetNumValue());
        }

        targetProp.SetValue(targetProp.GetNumValue() - subValue);
    }

    void set() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : _item;

        if (GetPart(1).HasProp()) {
            var sourceProp = GetProp(1);
            targetProp.SetValue(sourceProp.GetTextValue());
            // commenté parce que ça fourait la merde avec les coords, elles, qui ne se transf_re pas
            //sourceProp.SetValue(0);
        } else if (GetPart(1).HasValue()) {
            targetProp.SetValue(GetPart(1).value);
        } else {
            targetProp.SetValue(Slot.ParseValue(GetPart(1)._output));
        }

    }
    void addnew() {
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : _item;
        targetItem.AddProp(GetPart(1)._output);
    }
    void remove() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : _item;
        targetItem.RemoveProp(targetProp);
    }
    #endregion


    bool HasPart(int i) {
        return i < _slots.Count;
    }

    Slot GetPart(int i) {
        return _slots[i];
    }

    Item GetItem(int i) {
        return _slots[i].item;
    }
    Property GetProp(int i) {
        return _slots[i].prop;
    }
    public string GetText(int i) {
        return _slots[i]._output;
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