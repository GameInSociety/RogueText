using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Linq;
using static UnityEditor.Progress;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;

[System.Serializable]
public class Property {


    public static List<Property> datas = new List<Property>();

    public string name;
    public bool changed = false;
    public bool enabled = false;
    public bool destroy = false;
    public List<Part> parts = new List<Part>();
    private Item _linkedItem;

    // in data
    [System.Serializable]
    public class Part {
        public string key;
        public string content;
        public Part(string k, string v) {
            key = k;
            content = v;
        }
    }

    public static Property GetDataProp(string name) {
        var prop = datas.Find(x=> x.name == name);
        if ( name == "contents") {
            Debug.Log($"pourquoi ?");
        }
        if (prop == null) {
            Debug.Log($"no prop with the name : {name}");
        }
        return prop;
    }

    public static void AddPropertyData(Property prop) {
        datas.Add(prop);
    }


    public void Init(Item item) {

        _linkedItem = item;

        // enable / disable
        if (name.StartsWith('*')) {
            name = name.Substring(1);
            if (name.StartsWith('?')) {
                var percent = TextUtils.Extract('?', name, out name);
                float r = UnityEngine.Random.value * 100f;
                enabled = r < int.Parse(percent.Trim('%'));
            } else {
                enabled = false;
            }
        } else {
            enabled = true;
        }

        if (name.Contains('/')) {
            var split = name.Split('/');
            name = split[0];
            for (int i = 1; i < split.Length; i++) {
                AddPart("key", split[i].Trim(' '));
            }
        }

        InitValue("value");
        InitDescription();
    }

    public void Parse(string[] lines) {
        name = lines[0];
        // no parts, only name
        if (lines.Length == 1) return;

        char[] chars = { '\r', '\t', '\b', '\n', ' ' };
        for (int i = 1; i < lines.Length; i++) {
            try {
                // skip empty lines
                if (string.IsNullOrEmpty(lines[i])) continue;

                if (lines[i].Contains(':')) {
                    // new part
                    var strs = lines[i].Split(':');
                    var text = strs[1].Trim(chars);
                    AddPart(strs[0], text);
                } else {
                    // continue part
                    var str = lines[i].Trim(chars);
                    var part = parts[parts.Count - 1];
                    part.content += string.IsNullOrEmpty(part.content) ? str : $"\n{str}";
                }
            } catch (Exception e) {
                TextManager.Write($"[Error Loading Properties] at part [ {lines[i]} ]", Color.red);
                Debug.LogException(e);
                
            }
        }
    }

    #region parts
    // adding
    public Part AddPart(string key, string text) {
        return AddPart(new Part(key, text));
    }
    public Part AddPart(Part part) {
        parts.Add(part);
        return part;
    }

    // setting
    public void SetPart(string key, string text) {
        var part = GetPart(key);
        if (part == null) {
            part = AddPart(key, text);
        } else {
            //Debug.Log($"property {key} already has part{key} with {text}");
        }
        part.content = text;
    }

    // removing
    public void RemovePart(string str) {
        var p = GetPart(str);
        parts.Remove(p);
    }


    // checking
    public bool HasPart(string str) {
        return parts.Find(x => x.key == str) != null;
    }
    public int ParsePart(string str) {
        int i = -1;
        if (int.TryParse(GetPart(str).content, out i))
            return i;
        else
            return -1;
    }

    // getting
    public List<Part> GetParts(string key) {
        return parts.FindAll(x=> x.key == key);
    }
    public Part GetPart(string key) {
        return parts.Find(x => x.key == key);
    }
    #endregion

    #region value
    public void InitValue(string key) {

        if (!HasPart(key))
            return;

        Part part = GetPart(key);

        if ( !part.content.StartsWith('?') && part.content.Contains('?') ) {
            string[] prts = part.content.Split('?');
            int min = int.Parse(prts[0]);
            int max = int.Parse(prts[1]);
            int i = UnityEngine.Random.Range(min, max);
            part.content = $"{i}";
        }
        if (part.content.Contains('m')) {
            var split = part.content.Split('m');
            part.content = split[0];
            SetPart("max", split[1]);
        }
        if (part.content.StartsWith('{')) {
            string outText = "";
            var s = TextUtils.Extract('{', part.content, out outText);
            var parts = s.Split('/');
            switch (parts[0]) {
                case "type":
                    string type = ItemData.GetItemData(ItemData.GetRandomDataOfType(parts[1])).name;
                    Debug.Log($"setting value of property {name} to {type}");
                    part.content = type;
                    break;
                case "adj":
                    break;
                case "new tileset":
                    int tilesetId = TileSet.tileSets.Count;
                    part.content = tilesetId.ToString();
                    TileSet.tileSets.Add(Interior.InitTileSet(_linkedItem, tilesetId));
                    break;
                default:
                    break;
            }
        }
    }
    public string GetTextValue(string key = "value") {
        return GetPart(key).content;
    }
    // value can be number or seq. but it's alway dynamic
    public bool HasNumValue(string key = "value") {
        return HasPart(key) && int.TryParse(GetPart(key).content, out _);
    }
    public int GetNumValue(string key = "value") {
        Part part = GetPart(key);
        if ( part == null) {
            return -1;
        }

        // check link
        if (part.content.Contains('[')) {
            var propKey = TextUtils.Extract('[', part.content, out _);
            var linkPart = new ActionPart(propKey);
            if (!linkPart.TryInit(_linkedItem))
                Function.LOG($"[{_linkedItem.debug_name}/{name}] OnValue link prop failed", Color.red);
            part.content = linkPart.prop.GetNumValue().ToString();
            return linkPart.prop.GetNumValue();
        }

        int num = 0;
        if (!int.TryParse(part.content, out num)) {
            Debug.LogError($"GET NUM VALUE : value of {name} ({part.content}) can't be parsed");
            return -1;
        }
        return num;
    }
    public void SetValue(string text, string part = "value", bool setChanged = true) {
        GetPart(part).content = text;
        if ( setChanged)
            SetChanged();

    }
    public void SetValue(int num, string part = "value") {
        if (HasPart("max"))
            num = Math.Clamp(num, 0, GetNumValue("max"));
        if (num < 0)
            num = 0;

        GetPart(part).content = num.ToString();
        SetChanged();
    }
    public void SetChanged() {
        changed = true;
        CheckValueEvents();
        if ( WorldAction.current != null) {
            PropertyDescription.Add(_linkedItem, this, WorldAction.current.source, false);
        }
    }
    #endregion

    #region on value
    public enum OnValueCondition {
        Above,
        Below,
        Equals
    }
    void CheckValueEvents() {
        if (!enabled)
            return;

        var valueEvents = parts.FindAll(x => x.key == "OnValue");
        if (valueEvents.Count == 0)
            return;

        foreach (var valueEvent in valueEvents) {
            var content = valueEvent.content;

            // getting the condition
            var returnIndex = content.IndexOf('\n');
            var condition_text = content.Remove(returnIndex);
            // 

            var propValue = GetNumValue();
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
                if (!onValuePart.TryInit(_linkedItem))
                    Function.LOG($"[{_linkedItem.debug_name}/{name}] OnValue link prop failed", Color.red);
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
                var tileEvent = new WorldAction(_linkedItem, sequence);
                tileEvent.Call();
            }
        }
    }
    #endregion

    #region description
    public void InitDescription() {
        if (!HasPart("description"))
            return;
        var description = GetPart("description").content;

        // TYPE //
        if (description.StartsWith('(')) {
            var type = TextUtils.Extract('(', description, out description);
            SetPart("description type", type);
        } else
            SetPart("description type","none");
        

        // SETUP //
        if (description.StartsWith('[')) {
            var setup = TextUtils.Extract('[', description, out description);
            SetPart("description setup", setup);
        } else if (!HasPart("description setup"))
            SetPart("description setup", "is");
        //

        SetPart("description", description.Trim(' ', '\n'));

    }
    public string GetDisplayDescription() {

        if (!enabled) {
            if (changed)
                return $"no longer {GetCurrentDescription()}";
            else return "";
        }

        if (!changed)
            return GetCurrentDescription();

        var newDescription = GetCurrentDescription();
        var result = "";
        if (HasPart("current description")) {
            if (newDescription != GetPart("current description").content)
                result = $"now {newDescription}";
            else
                result = $"still {newDescription}";
        } else
            result = $"{newDescription}";
        SetPart("current description", newDescription);
        changed = false;
        return result;
    }

    public bool CanBeDescribed() {

        if (!HasPart("description"))
            return false;

        if (!enabled && !changed) {
            PropertyDescription.LOG($"[{name}] Skip (Disabled and Unchanged)", Color.red);
            return false;
        }

        if (!HasPart("description type")) {
            PropertyDescription.LOG($"[{name}] Skip (No Description Type)", Color.green);
            return true;
        }

        var type = GetPart("description type").content;
        if (type.EndsWith("%")) {
            float currentPercent = Mathf.Round((float)GetNumValue() / GetNumValue("max") * 100f);
            int targetPercent = 0;
            bool below = false;
            if (type.StartsWith('<')) {
                type = type.Substring(1);
                below = true;
            }

            if (!int.TryParse(type.TrimEnd('%'), out targetPercent)) {
                Debug.LogError($"could't parse prop description type {type}");
                return false;
            }
            if (below ? currentPercent <= targetPercent : currentPercent >= targetPercent) {
                PropertyDescription.LOG($"[{type}] {currentPercent}/{targetPercent} / value={GetNumValue()}] Describing", Color.green);
                return true;
            } else {
                PropertyDescription.LOG($"[{type}] {currentPercent}/{targetPercent} / value={GetNumValue()}] Skipping", Color.red);
                return false;
            }
        }
        switch (type) {
            case "always":
                PropertyDescription.LOG($"[ALWAYS] Describing", Color.gray);
                return true;
            case "on change":
                if (HasPart("changed")) {
                    PropertyDescription.LOG($"[ON CHANGE] Describing (changed)", Color.green);
                    return true;
                }
                PropertyDescription.LOG($"[ON CHANGE] Skipping (hasn't changed)", Color.red);
                return false;
            case "never":
                PropertyDescription.LOG($"[NEVER] Only context, skipping", Color.red);
                return false;
            default:
                PropertyDescription.LOG($"[NO TYPE] Describing", Color.green);
                return true;
        }


    }

    public string GetCurrentDescription() {
        var description = GetPart("description").content;
        if (description.Contains("/")) {
            var prts = description.Split(" / ");
            int max = HasPart("max") ? GetNumValue("max") : GetNumValue();
            int i = GetNumValue();
            if (i == 0) return prts[0];
            if ( i == max ) return prts[prts.Length-1];
            var lerp = (float)i * prts.Length / max;
            int index = Math.Clamp((int)lerp, 1, prts.Length-2);
            return $"{GetPart("description start")?.content}{prts[index]}";
        }
        description = description.Replace("{value}", GetPart("value")?.content);
        return description;
    }

    public bool Visible() {

        if (!HasPart("description"))
            return false;

        if (HasPart("condition")) {
            var condition = GetPart("condition").content;
            var split = condition.Split('/');
            var prop = Player.Instance.GetProp(split[0]);
            if (prop != null && split[1] != prop.GetTextValue())
                return false;
        }
        return true;
    }

    #endregion

    public void Destroy() {
        destroy = true;
    }
}


