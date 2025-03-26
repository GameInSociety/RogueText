using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This holds the reference to an item, property, ect... 
public class Slot {

    [Serializable]
    public class Data {
        public Type _type;
        public string _description;
    }


    public enum Type {
        None,

        Text,
        Value,
        Item,
        Property,
        Sequence,
        Check,
        Tile,
        Part,
    }
    
    public enum State {
        None,
        Done,
        Failed,
        Error,
    }

    private Data _data;
    public State state = State.None;

    // input text
    public string _input;
    // modified test
    public string _output;

    // input item
    public Item _sourceItem;

    // item results
    public List<Item> items = new List<Item>();
    // prop result
    public Property prop;
    // value result
    public int value = -1;

    // UNIQUEMENT DEBUG
    public List<string> errors = new List<string>();
    public List<string> linkLog = new List<string>();
    public static Slot current;
    public Slot parent;
    public static List<Slot> debug_slots = new List<Slot>();
    public List<Slot> children = new List<Slot>();
    public string _label = "";

    public Slot() {

    }

    public Slot(string input) {
        // check if line part is a child of another
        if (current != null) {
            parent = current;
            current.children.Add(this);
        } else {
            if (_label != "Sequence")
                debug_slots.Add(this);
        }
        current = this;
        _output = _input;

        // replace prop links text
        _output = Replace(_output);

        if (_output == "content parsing error") {
            ThrowFail("Content Parsing Fail");
            return;
        }

        TryOperations();
        TryItems();
        TryProperties();
        TryValue();

        if (value >= 0)
            _output = value.ToString();

        state = State.Done;

        // reset current to parent
        current = parent;
    }

    public static Slot Parse(string input, Item defaultItem, string label) {
        var newSlot = new Slot();
        newSlot._input = input;
        newSlot._sourceItem = defaultItem;
        newSlot._label = label;

        return newSlot;
    }

    #region ITEM
    void TryItems() {
        if (!_output.Contains('{') && !_output.Contains('!'))
            return;
        var key = _output;
        // search
        var result = ItemLink.SearchItem(key, _sourceItem);
        if (result == null) {
            ThrowFail($"item fail", key);
            return;
        }
        items.Add(result);

    }
    public bool HasItem() {
        return item != null;
    }
    public bool HasValue() => value >= 0;
    public Item item {
        get {
            if (items.Count == 0)
                return null;
            return items.First();
        }
    }
    #endregion

    #region COORDS / TILE
    public bool HasCoords(int tileset) {
        var c = GetCoords(tileset);
        return c.x != -1 && c.y != -1;
    }

    public Coords GetCoords(int tileset) {

        Coords c = new Coords(-1, -1);

        if (HasProp()) {
            c = Coords.PropToCoords(prop, tileset);
            if (c == Coords.none) {
                Debug.Log($"coords doesn't match");
            }
            return c;
        } else {
            if (_output.Contains('/')) {
                return Coords.TextToCoords(_output);
            } else {
                return c;
            }
        }
    }
    #endregion

    #region PROPERTY
    public bool HasProp() => prop != null;
    void TryProperties() {
        if (!_output.Contains('>'))
            return;

        // check for prop after tile index
        int startIndex = _output.Contains('{') ? _output.IndexOf('}') : 0;

        int propIndex = _output.IndexOf('>', startIndex);
        if (propIndex< 0)
            return;

        var key = _output.Remove(0, propIndex+1);
        key = key.Trim(' ');
        var it = item == null ? _sourceItem : item;
        prop = ItemLink.GetProperty(key, it);
        if (prop == null) {
            ThrowFail($"prop fail", key);
            return;
        }

        if (prop.HasNumValue()) {
            value = prop.GetNumValue();
            _output = value.ToString();
        } else if (prop.HasPart("value")) {
            _output = prop.GetTextValue();
        }
    }
    #endregion

    #region VALUE / OPERATIONS
    void TryValue() {
        if (item != null || prop != null || _output.Contains('/'))
            return;

        string key = ParseValue(_output);
        value = int.TryParse(key, out _) ? int.Parse(key) : -1;
    }

    public static string ParseValue(string str) {
        string result = "";
        result = ParseValue(str, "");
        ;
        return result;
    }
    public static string ParseValue(string str, string n) {
        if (str == "_InputValue") {
            if (ItemParser.Instance != null) {
                var numPart = System.Array.Find(ItemParser.Instance.parts, x => x.number >= 0);
                return numPart == null ? "1" : numPart.number.ToString();
            } else
                return "1";
        }

        if (str == "_?Percent") {
            float f = UnityEngine.Random.value * 100f;
            Debug.Log($"random percent : {f}");
            return f.ToString();
        }

        if (str.Contains('?')) {
            var split = str.Split('?');
            if (int.TryParse(split[0], out _)) {
                var min = int.Parse(split[0]);
                var max = int.Parse(split[1]);
                Debug.Log($"Numeric Value Range : {min}/{max}");
                return UnityEngine.Random.Range(min, max).ToString();
            }
            var s = split[UnityEngine.Random.Range(0, split.Length)];
            Debug.Log($"Text Value Range: {s}");
            return s;
        }

        return str;
    }
    void TryOperations() {

        var operations = new string[4] { " + ", " - ", " X ", " DIS " };
        var operation = "";
        for (int i = 0; i < operations.Length; i++) {
            if (_output.Contains(operations[i])) {
                operation = operations[i];
            }
        }
        if (string.IsNullOrEmpty(operation))
            return;

        int TEMP_symbolIndex = _output.IndexOf('%');
        if ( TEMP_symbolIndex >= 0) {
            _output = _output.Remove(0, TEMP_symbolIndex + 1).TrimStart(' ');
        } else {
            _output = _output.Trim(' ');
        }

        var split = _output.Split(operation);
        var part1 = Slot.Parse(split[0], _sourceItem, "Operation: First Part");
        var part2 = Slot.Parse(split[1], _sourceItem, "Operation: Second Part");

        switch (operation) {
            case " + ":
                if (part1.HasCoords(0)) {
                    var newCoords = part1.GetCoords(0) + part2.GetCoords(0);
                    _output = $"{newCoords.x}/{newCoords.y}";
                } else {
                    value = part1.value + part2.value;
                }
                break;
            case " - ":
                value = part1.value - part2.value;
                _output = value.ToString();
                break;
            case " X ":
                if (part1.HasCoords(0)) {
                    var newCoords = part1.GetCoords(0) * part2.value;
                    _output = $"{newCoords.x}/{newCoords.y}";
                } else {
                    value = part1.value * part2.value;
                }

                break;
            case " DIS ":
                var c1 = Coords.TextToCoords(part1._output);
                var c2 = Coords.TextToCoords(part2._output);
                float distanceBetweenTiles = Mathf.Pow(c2.x - c1.x, 2) + Mathf.Pow(c2.y - c1.y, 2);
                distanceBetweenTiles = Mathf.Sqrt(distanceBetweenTiles);
                distanceBetweenTiles = Mathf.RoundToInt(distanceBetweenTiles);
                value = (int)distanceBetweenTiles;
                _output = value.ToString();
                break;
        }

    }
    #endregion

    #region text replacement
    public static string ParseBrackets(string input, Item item) {

        var b = 0;
        string key = input;
        var index = key.IndexOf('[');
        while (index >= 0) {
            var extract = TextUtils.Extract('[', key, out key);
            var contentParsing = Slot.Parse(extract, item, "Content Parsing");
            key = key.Insert(index, contentParsing._output);
            index = key.IndexOf("[", index);

            ++b;
            if (b >= 10) {
                Debug.LogError($"break error ");
                break;
            }
        }

        return key;
    }
    public static int outB = 0;
    string Replace(string input) {
        int startIndex = 0;
        while ((startIndex = input.IndexOf('[', startIndex)) != -1) {

            ++outB;
            if (outB >= 10) {
                Debug.LogError($"out of look");
                return "";
            }

            int endIndex = FindClosingParenthesis(input, startIndex);
            if (endIndex == -1)
                break;

            string extracted = input.Substring(startIndex + 1, endIndex - startIndex - 1);
            string content = extracted;
            if (extracted.Contains('[')) {
                content = Replace(extracted);
            }

            var param = Sequence.active.parameters.Find(x => x.key == content);
            if (string.IsNullOrEmpty(param.key)) {
                var linePart = Slot.Parse(content, _sourceItem, "Content Parsing");
                content = linePart._output;
            } else
                content = param.value;

            input = input.Remove(startIndex, extracted.Length + 2);
            input = input.Insert(startIndex, content);

            startIndex = FindClosingParenthesis(input, startIndex) + 1;
        }

        return input;
    }

    static int FindClosingParenthesis(string input, int openIndex) {
        int closeIndex = openIndex;
        int counter = 1;

        while (counter > 0 && ++closeIndex < input.Length) {
            if (input[closeIndex] == '[')
                counter++;
            else if (input[closeIndex] == ']')
                counter--;
        }

        return counter == 0 ? closeIndex : -1;
    }
    #endregion

    #region error handling
    /// <summary>
    /// fails & erros
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="additionalInfo"></param>
    public void Error(string msg, string additionalInfo = "") {
        state = State.Error;
        errors.Add($"<color=red>{msg}</color>");
    }
    public void ThrowFail(string msg, string additionInfo = "") {
        state = State.Failed;
        _output = msg;

        throw new LineFailException(msg);
    }


    /// <summary>
    /// EXCEPTION
    /// </summary>
    /// 
    [Serializable]
    public class LineFailException : Exception {
        public LineFailException() : base() { }
        public LineFailException(string message) : base(message) { }
        public LineFailException(string message, Exception inner) : base(message, inner) { }
    }
    #endregion
}