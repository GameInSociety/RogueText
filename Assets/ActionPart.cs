using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Xml.Schema;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.Progress;
public class ActionPart {
    public string text;
    public string log;
    public Item sourceItem;
    public Item item;
    public Property prop;
    public int value = -1;
    bool fail = false;

    public bool HasItem() {
        return item != null;
    }

    public bool HasProp() {
        return prop != null;
    }
    public bool HasValue() {
        return value >= 0;
    }

    public ActionPart(string text) {
        this.text = text;
    }

    

    public bool TryInit(Item defaultItem) {

        sourceItem = defaultItem;

        // replace prop links text
        text = ItemLink.ReplacePropLinks(sourceItem, text);

        // operation ?
        TryOperations();
        TryItems();
        TryProperties();
        TryValue();

        return !fail;
    }

    void TryOperations() {
        string[] operations = new string[2] {
            "+",
            "-",
        };
        if (!text.Contains('%'))
            return;

        text = text.Remove(0, text.IndexOf('%') + 1).TrimStart(' ');
        var opValue = 0;
        int index = text.IndexOfAny(new char[2] { '+', '-' });
        if (index >= 0) {
            var split = text.Split(text[index]);
            for (int i = 0; i < split.Length; i++) {
                var newPart = new ActionPart(split[i]);
                if (!newPart.TryInit(sourceItem)) {
                    Fail($"operation ({text}):fail");
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
                    }
                }
            }
            value = opValue;
        }
        Suc($"operation ({text})");
    }
    void TryItems() {

        if (!text.Contains('[') && !text.Contains('!'))
            return;

        var key = text;
        // search
        var result = ItemLink.SearchItem(key);
        item = result;
        if ( item == null) {
            Fail($"item ({key}):null");
            return;
        }
        Suc($"item ({key}):{item.debug_name}");
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
            Fail($"prop ({key}):null");
            return;
        }
        
        Suc($"prop ({key}):{prop.name}");
        if (prop.HasNumValue()) {
            value = prop.GetNumValue();
            Suc($"(prop value :{value})");
        }
    }
    void TryValue() {
        if (item != null || prop != null || text.Contains('/'))
            return;
        int v = -1;
        if (text == "INPUT NUM") {
            if (ItemParser.GetCurrent != null) {
                var numPart = System.Array.Find(ItemParser.GetCurrent.parts, x => x.number >= 0);
                v = numPart == null ? 1 : numPart.number;
            } else
                v = 1;
        } else if (text.Contains('?')) {
            Debug.Log($"RANDOM IN {text}");
            var split = text.Split('?');
            var min = int.Parse(split[0]);
            var max = int.Parse(split[1]);
            v = UnityEngine.Random.Range(min, max);
        } else if (int.TryParse(text, out v)) {
            
        } else
            v = -1;

        if (v>= 0) {
            value = v;
            Suc($"value ({text}):{value}");
        }
    }

    void Fail(string msg) {
        Function.ADDLOG($" | {msg}", Color.red);
        fail = true;
    }
    void Suc(string msg) {
        Function.ADDLOG($" | {msg}", Color.green);
    }
}