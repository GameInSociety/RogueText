using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.Progress;
public class LinePart {
    // input text
    public string input;
    // modified test
    public string output;
    // input item
    public Item defaultItem;

    [Serializable]
    public class LineFailException : Exception {
        public LineFailException() : base() { }
        public LineFailException(string message) : base(message) { }
        public LineFailException(string message, Exception inner) : base(message, inner) { }
    }

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
        Debug.LogError($"New line part : {input}");
        current = this;
        output = input;

        // replace prop links text
        output = Replace(output);

        if (output == "content parsing error") {
            ThrowFail("Content Parsing Fail");
            return;
        }

        // operation ?
        if (output.Contains('%')) {
            TryOperations();
        } else {
            TryItems();
            TryProperties();
            TryValue();
        }

        if (value >= 0)
            output = value.ToString();

        state = State.Done;

        // reset current to parent
        current = parent;

    }


    

    void TryOperations() {
        output = output.Remove(0, output.IndexOf('%') + 1).TrimStart(' ');

        var operations = new string[4] { " + ", " - ", " X ", " DIS " };

        for (int i = 0; i < operations.Length; i++) {
            if (output.Contains(operations[i])) {
                var operation = operations[i];
                var split = output.Split(operation);
                var part1 = LinePart.Parse(split[0], defaultItem, "Operation: First Part");
                var part2 = LinePart.Parse(split[1], defaultItem, "Operation: Second Part");

                switch (operation) {
                    case " + ":
                        value = part1.value + part2.value;
                        break;
                    case " - " :
                        value = part1.value - part2.value;
                        break;
                    case " X ":
                        Debug.Log($"multiplication");
                        Debug.Log($"{part1.value} * {part2.value}");
                        value = part1.value * part2.value;
                        break;
                    case " DIS ":
                        var c1 = Coords.TextToCoords(part1.output);
                        var c2 = Coords.TextToCoords(part2.output);
                        float distanceBetweenTiles = Mathf.Pow(c2.x - c1.x, 2) + Mathf.Pow(c2.y - c1.y, 2);
                        distanceBetweenTiles = Mathf.Sqrt(distanceBetweenTiles);
                        distanceBetweenTiles = Mathf.RoundToInt(distanceBetweenTiles);
                        value = (int)distanceBetweenTiles;
                        break;
                }
                return;
            }
        }
        ThrowFail("Line Part Operation : Found no operation Symbol");
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

    string ParseActParams(string input) {
        if (WorldAction.active?.parameters == null)
            return input;

        var b = 0;
        string output = input;
        var index = output.IndexOf('[');
        while (index >= 0) {
            var key = TextUtils.Extract('[', output, out output);
            var param = WorldAction.active.parameters.Find(x => x.key == key);
            if (string.IsNullOrEmpty(param.key))
                return input;

            output = output.Insert(index, param.value);
            index = output.IndexOf("[", index);

            ++b;
            if (b >= 10) {
                break;
            }
        }
        return output;
    }


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

            Debug.Log($"input : {input}");
            Debug.Log($"content to insert : {content }");
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


    /*public static string ParseBrackets (string input, Item item) {
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
    }*/

    public void Error(string msg, string additionalInfo = "") {
        state = State.Error;
        errors.Add($"<color=red>{msg}</color>");
    }

    public void ThrowFail(string msg, string additionInfo = "") {
        state = State.Failed;
        throw new LineFailException(msg);
    }
}