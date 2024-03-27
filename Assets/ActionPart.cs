using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Xml.Schema;
using UnityEngine;
using static UnityEditor.Progress;
public class ActionPart {
    public string text;
    public string log;
    public Item item;
    public Tile tile;
    public Property prop;
    public int value = -1;

    public bool HasItem() {
        return text.Contains('!');
    }

    public bool HasTile() {
        return text.Contains('[');
    }
    public bool HasProp() {
        return text.Contains('>');
    }
    public bool HasValue() {
        return value >= 0;
    }

    public ActionPart(string text) {
        this.text = text;
    }

    string[] operations = new string[2] {
        "+",
        "-",
    };

    public bool TryInit(Item defaultItem) {

        item = defaultItem;

        // replace prop links text
        text = ItemLink.ReplacePropLinks(item, text);


        // operation ?
        if (text.Contains('%')) {
            text = text.Remove(0, text.IndexOf('%') + 1).TrimStart(' ');
            var opValue = 0;
            int index = text.IndexOfAny(new char[2] {'+', '-'});
            if ( index >= 0) {
                var split = text.Split(text[index]);
                for (int i = 0; i < split.Length; i++) {
                    Function.LOG($"operation [{split[i]}]", Color.cyan);
                    var newPart = new ActionPart(split[i]);
                    if (!newPart.TryInit(defaultItem))
                        return false;

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

                Debug.LogError($"FINAL VALUE : {opValue}");
                value = opValue;
            }

            return true;
        }

        // if it's a tile, no item or prop
        if (HasTile()) {
            tile = FetchTile();
            item = tile;
            Function.ADDLOG(tile == null ? " [tile?] " : $" [tile:{tile.debug_name}] ", tile == null ? Color.red : Color.green);
            return tile != null;
        }

        // item
        if (HasItem()) {
            item = FetchItem(text);
            Function.ADDLOG(item == null ? " [item?] " : $" [item:{item.debug_name}] ", item == null ? Color.red : Color.green);
            if (item == null)
                return false;
        }

        // prop
        if (HasProp()) {
            prop = FetchProp(text, item);
            Function.ADDLOG(prop == null ? " [prop?] " : $" [prop:{prop.name}] ", prop == null ? Color.red : Color.green);
            if (prop == null)
                return false;
            if (prop.HasNumValue()) {
                value = prop.GetNumValue();
            }
        }

        // value
        var v = FetchValue();
        if (v >= 0) {
            value = v;
            Function.ADDLOG($" [value:{value}] ", Color.green);
        }
        return true;
    }

    int FetchValue() {
        int v = -1;
        if (text.Contains('{')) {
            string key = TextUtils.Extract('{', text, out text);
            switch (key) {
                case "input num":
                    if (ItemParser.GetCurrent != null) {
                        var numPart = System.Array.Find(ItemParser.GetCurrent.parts, x => x.number >= 0);
                        value = numPart == null ? 1 : numPart.number;
                    } else
                        value = 1;
                    break;
            }
        } else if (text.Contains('?')) {
            var split = text.Split('?');
            var min = int.Parse(split[0]);
            var max = int.Parse(split[1]);
            value = UnityEngine.Random.Range(min, max);
        } else if (int.TryParse(text, out v)) {
            value = v;
        } else
            v = -1;

        return v;
    }

    Item FetchItem(string key) {
        // key
        if (key.Contains('>'))
            key = key.Remove(key.IndexOf('>'));
        key = key.Remove(0, key.IndexOf('!') + 1);
        key = key.Trim(' ');
        // search
        var result = ItemLink.SearchItem(key);
        if (result == null)
            return null;
        return result;
    }
    Tile FetchTile() {
        var key = TextUtils.Extract('[', text, out _);
        var tilesetId = Player.Instance.tilesetId;
        // check for other tile set
        if (key.Contains('{')) {
            var tilesetKey = TextUtils.Extract('{', key, out key);
            if (tilesetKey.Contains('>')) {
                var tsIt = item;
                if (tilesetKey.Contains('!')) {
                    tsIt = FetchItem(tilesetKey);
                    if (tsIt == null)
                        return null;
                }
                var tilesetProp = FetchProp(tilesetKey, tsIt);
                tilesetId = tilesetProp.GetNumValue();
            } else {
                tilesetId = int.Parse(tilesetKey);
            }
        }

        // check for addition
        var split = key.Split('+');
        var coords = new Coords();
        foreach (var s in split) {
            // get maybe item
            var it = item;
            if (s.Contains('!')) {
                it = FetchItem(s);
                if (it == null)
                    return null;
            }

            // get maybe prop
            var newCoords = Coords.zero;
            if (s.Contains('>')) {
                var cProp = FetchProp(s, it);
                if (cProp == null)
                    return null;
                newCoords = Coords.PropToCoords(cProp, tilesetId);
                cProp.SetValue(Coords.CoordsToText(newCoords));
            } else
                newCoords = Coords.TextToCoords(s, tilesetId);
            coords += newCoords;
        }

        var result = TileSet.tileSets[tilesetId].GetTile(coords);
        if (result == null)
            return null;
        return result;
    }
    Property FetchProp(string key, Item it) {

        /*if (key.Contains('-')) {
            var split = 
            Debug.Log($"substraction, just getting {ke}");
        }*/

        key = key.Remove(0, key.IndexOf('>') + 1);
        key = key.Trim(' ');
        var result = ItemLink.GetProperty(key, it);
        return result;
    }
}