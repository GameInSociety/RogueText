using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Xml.Schema;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.Progress;
public class LinePart {
    public string text;
    public string log;
    public Item sourceItem;
    public Item item;
    public Property prop;
    public int value = -1;
    public bool fail = false;

    public bool HasItem() {
        return item != null;
    }

    public bool HasProp() {
        return prop != null;
    }
    public bool HasValue() {
        return value >= 0;
    }

    public LinePart(string text) {
        this.text = text;
    }

    public bool TryInit(Item defaultItem) {

        sourceItem = defaultItem;

        // replace prop links text
        text = ItemLink.ReplacePropLinks(sourceItem, text).Trim(' ');

        // operation ?
        if (text.Contains('%')) {
            TryOperations();
        } else {
            TryItems();
            TryProperties();
            TryValue();
        }
        

        return !fail;
    }

    void TryOperations() {
        text = text.Remove(0, text.IndexOf('%') + 1).TrimStart(' ');
        var opValue = 0;
        int index = text.IndexOfAny(new char[3] { '+', '-' , 'X'});
        if (index >= 0) {
            var split = text.Split(text[index]);
            for (int i = 0; i < split.Length; i++) {
                var newPart = new LinePart(split[i]);
                if (!newPart.TryInit(sourceItem)) {
                    Fail($"operation fail");
                    return;
                }

                if (i == 0)
                    opValue = newPart.value;
                else {
                    switch (text[index]) {
                        case '+':
                            opValue += newPart.value;
                            break;
                        case '-':
                            opValue -= newPart.value;
                            break;
                        case 'X':
                            opValue *= newPart.value;
                            break;
                    }
                }
            }
            value = opValue;
        }
        Suc("operation", value.ToString());
    }
    void TryItems() {

        if (!text.Contains('[') && !text.Contains('!'))
            return;

        var key = text;
        // search
        var result = ItemLink.SearchItem(key);
        item = result;
        if ( item == null) {
            Fail($"item fail", key);
            return;
        }
        Suc($"{item.debug_name}", key);
    }
    void TryProperties() {
        if (!text.Contains('>'))
            return;

        // check for prop after tile index
        int startIndex = text.Contains('[') ? text.IndexOf(']') : 0;

        int propIndex = text.IndexOf('>', startIndex);
        if( propIndex< 0)
            return;

        var key = text.Remove(0, propIndex+1);
        key = key.Trim(' ');
        var it = item == null ? sourceItem : item;
        prop = ItemLink.GetProperty(key, it);
        if (prop == null) {
            Fail($"prop fail", key);
            return;
        }
        
        Suc($"{prop.name}", key);
        if (prop.HasNumValue()) {
            value = prop.GetNumValue();
            Suc($"value:{value}");
        } else if (prop.HasPart("value")) {
            text = prop.GetTextValue();
            Suc($"text:{text}");
        }
    }
    void TryValue() {
        if (item != null || prop != null || text.Contains('/'))
            return;

        string key = ParseValue(text);
        if ( int.TryParse(key, out _))
        {
            value = int.Parse(key);
            Suc($"value:{value}");
        } else {
            value = -1;
        }
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


    public string debug_feedback;
    void Fail(string msg, string additionalInfo = "") {
        if ( additionalInfo == "") {
            debug_feedback += $"<color=red>{msg}</color> | ";
        } else {
            debug_feedback += $"<color=red>{msg}</color> ({additionalInfo}) | ";
        }
        fail = true;
    }
    void Suc(string msg, string additionalInfo = "") {
        if (additionalInfo == "") {
            debug_feedback += $"<color=green>{msg}</color> | ";
        } else {
            debug_feedback += $"<color=green>{msg}</color> ({additionalInfo}) | ";
        }
    }
}