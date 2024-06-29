using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LinePart {

    /// <summary>
    ///  input parameters
    /// </summary>
    // input text
    public string input;
    // modified test
    public string output;
    // input item
    public Item defaultItem;

    public enum State {
        None,
        Done,
        Failed,
        Error,
    }
    public State state = State.None;

    /// <summary>
    /// OUTPUTS
    /// </summary>
    // output items
    public List<Item> items = new List<Item>();
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

    public bool HasCoords(int tileset) {
        var c = GetCoords(tileset);
        return c.x != -1 && c.y != -1;
    }

    public Coords GetCoords(int tileset) {

        Coords c = new Coords(-1, -1);


        if (HasValue()) {
            Debug.Log($"value : {value}");
        }

        if (HasProp()) {
            c = Coords.PropToCoords(prop, tileset);
            if (c == Coords.none) {
                Debug.Log($"coords doesn't match");
            }
            return c;
        } else {
            if (output.Contains('/')) {
                return Coords.TextToCoords(output);
            } else {
                return c;
            }
        }
    }

    // output properties
    public Property prop;
    public bool HasProp() => prop != null;
    // output values    
    public int value = -1;

    // debug
    public List<string> errors = new List<string>();
    public List<string> linkLog = new List<string>();
    public static List<LinePart> debug_lineParts = new List<LinePart>();
    public static LinePart current;
    public LinePart parent;
    public List<LinePart> children = new List<LinePart>();
    public string label = "";

    public static LinePart Parse( string input, Item defaultItem, string label) {


        var newLinePart = new LinePart();
        newLinePart.input = input;
        newLinePart.defaultItem = defaultItem;
        newLinePart.label = label;

        newLinePart.Init();

        return newLinePart;
    }

    /// <summary>
    /// IMPORTANT : J'init ici, aprce que si je fais tout dans la constructor, elle ne se crée pas et donc ne s'affiche dans le debug pour voir d'ou vient le bug
    /// </summary>
    public void Init() {
        // check if line part is a child of another
        if (current != null) {
            parent = current;
            current.children.Add(this);
        } else {
            if (label != "Sequence") 
                debug_lineParts.Add(this);
        }
        current = this;
        output = input;

        // replace prop links text
        output = Replace(output);

        if (output == "content parsing error") {
            ThrowFail("Content Parsing Fail");
            return;
        }

        TryOperations();
        TryItems();
        TryProperties();
        TryValue();

        if (value >= 0)
            output = value.ToString();

        state = State.Done;

        // reset current to parent
        current = parent;

    }


    

    void TryOperations() {

        var operations = new string[4] { " + ", " - ", " X ", " DIS " };
        var operation = "";
        for (int i = 0; i < operations.Length; i++) {
            if (output.Contains(operations[i])) {
                operation = operations[i];
            }
        }
        if (string.IsNullOrEmpty(operation))
            return;

        int TEMP_symbolIndex = output.IndexOf('%');
        if ( TEMP_symbolIndex >= 0) {
            output = output.Remove(0, TEMP_symbolIndex + 1).TrimStart(' ');
            Debug.Log($"{input} still has öperation symbol % left");
        } else {
            output = output.Trim(' ');
        }

        var split = output.Split(operation);
        var part1 = LinePart.Parse(split[0], defaultItem, "Operation: First Part");
        var part2 = LinePart.Parse(split[1], defaultItem, "Operation: Second Part");

        switch (operation) {
            case " + ":
                if (part1.HasCoords(0)) {
                    var newCoords = part1.GetCoords(0) + part2.GetCoords(0);
                    output = $"{newCoords.x}/{newCoords.y}";
                } else {
                    value = part1.value + part2.value;
                }
                break;
            case " - ":
                value = part1.value - part2.value;
                output = value.ToString();
                break;
            case " X ":
                if (part1.HasCoords(0)) {
                    var newCoords = part1.GetCoords(0) * part2.value;
                    output = $"{newCoords.x}/{newCoords.y}";
                } else {
                    value = part1.value * part2.value;
                }

                break;
            case " DIS ":
                var c1 = Coords.TextToCoords(part1.output);
                var c2 = Coords.TextToCoords(part2.output);
                float distanceBetweenTiles = Mathf.Pow(c2.x - c1.x, 2) + Mathf.Pow(c2.y - c1.y, 2);
                distanceBetweenTiles = Mathf.Sqrt(distanceBetweenTiles);
                distanceBetweenTiles = Mathf.RoundToInt(distanceBetweenTiles);
                value = (int)distanceBetweenTiles;
                output = value.ToString();
                break;
        }

    }
    void TryItems() {
        if (!output.Contains('{') && !output.Contains('!'))
            return;
        var key = output;
        // search
        var result = ItemLink.SearchItem(key, defaultItem);
        if ( result == null) {
            ThrowFail($"item fail", key);
            return;
        }
        items.Add(result);

    }
    void TryProperties() {
        if (!output.Contains('>'))
            return;

        // check for prop after tile index
        int startIndex = output.Contains('{') ? output.IndexOf('}') : 0;

        int propIndex = output.IndexOf('>', startIndex);
        if( propIndex< 0)
            return;

        var key = output.Remove(0, propIndex+1);
        key = key.Trim(' ');
        var it = item == null ? defaultItem : item;
        prop = ItemLink.GetProperty(key, it);
        if (prop == null) {
            ThrowFail($"prop fail", key);
            return;
        }
        
        if (prop.HasNumValue()) {
            value = prop.GetNumValue();
            output = value.ToString();
        } else if (prop.HasPart("value")) {
            output = prop.GetTextValue();
        }
    }
    void TryValue() {
        if (item != null || prop != null || output.Contains('/'))
            return;

        string key = ParseValue(output);
        value = int.TryParse(key, out _) ? int.Parse(key) : -1;
    }

    public static string ParseValue(string str) {
        string result = "";
        result = ParseValue(str, ""); ;
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


    #region text replacement
    public static string ParseBrackets(string input, Item item) {

        var b = 0;
        string key = input;
        var index = key.IndexOf('[');
        while (index >= 0) {
            var extract = TextUtils.Extract('[', key, out key);
            var contentParsing = LinePart.Parse(extract, item, "Content Parsing");
            key = key.Insert(index, contentParsing.output);
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

            var param = WorldAction.active.parameters.Find(x => x.key == content);
            if (string.IsNullOrEmpty(param.key)) {
                var linePart = LinePart.Parse(content, defaultItem, "Content Parsing");
                content = linePart.output;
            } else
                content = param.value;

            input = input.Remove(startIndex, extracted.Length + 2);
            input = input.Insert(startIndex, content);
            /*int valueIndex = keys.FindIndex(x => x == extracted);
            if (valueIndex >= 0) {
                input = input.Remove(startIndex, extracted.Length + 2);
                input = input.Insert(startIndex, values[valueIndex]);
            } else {
                input = input.Remove(startIndex, extracted.Length + 2);
                input = input.Insert(startIndex, replace);
            }*/

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
        output = msg;

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