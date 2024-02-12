using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.UIElements;

[System.Serializable]
public class Property {

    public static List<Property> datas = new List<Property>();

    public static Property GetDataProp(string name) {
        var prop = datas.Find(x=> x.name == name);
        if (prop == null) {
            Debug.Log($"no prop with the name : {name}");
        }
        return prop;
    }

    public static void AddPropertyData(Property prop) {
        datas.Add(prop);
    }

    // in data
    [System.Serializable]
    public class Part {
        public string key;
        public string content;
        public Part(string k, string v) {
            key = k;
            content = v;
            //Debug.Log($"new part(key:{k} value:{v})");
        }
    }

    public string name;
    public bool enabled = false;
    public bool destroy = false;
    public List<Part> parts = new List<Part>();

    public void Init(Property copy) {
        name = copy.name;

        // first, get all data specific to the item
        // if there's none, it might be a one liner, or a fully copied data prop
        foreach (var part in copy.parts)
            AddPart(new Part(part.key, part.content));

        // searching a prop in data for copying
        var dataProp = datas.Find(x => x.name == name);
        if (dataProp != null) {
            // adding implicit parts from the data
            foreach (var part in dataProp.parts) {
                if (parts.Find(x => x.key == part.key) != null)
                    continue;
                AddPart(part.key, part.content);
            }
        }

        // fetch events
        Init();
    }

    public void Init() {
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

        if (HasPart("key")) {
            var split = GetPart("key").content.Split(", ");
            if ( split.Length > 1) {
                RemovePart("key");
                foreach (var s in split) {
                    AddPart("key", s);
                }
            }
        }

        InitValue();
        InitDescription();
    }

    public void Parse(string[] lines) {
        name = lines[0];
        // no parts, only name
        if (lines.Length == 1) return;

        char[] chars = { '\r', '\t', '\b', '\n', ' ' };
        for (int i = 1; i < lines.Length; i++) {
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
    public void InitValue(string key = "value") {

        if (!HasPart("value"))
            return;

        Part part = GetPart(key);
        if (part == null) {
            Debug.LogError($"INIT NUM VALUE : prop {name} doesn't have part with key {key}");
            return;
        }

        // check link
        if (part.content.Contains('[')) {
            var prop = ItemLink.GetProperty(part.content, null);
            part.content = prop.GetNumValue().ToString();
            return;
        }

        if (part.content.Contains('?')) {
            string[] prts = part.content.Split('?');
            int min = int.Parse(prts[0]);
            int max = int.Parse(prts[1]);
            int i = UnityEngine.Random.Range(min, max + 1);
            part.content = $"{i}";
            AddPart("max", max.ToString());
        }

        if (part.content.Contains('m')) {
            string[] prts = part.content.Split('m');
            int min = int.Parse(prts[0]);
            int max = int.Parse(prts[1]);
            part.content = min.ToString();
            AddPart("max", max.ToString());
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
                default:
                    break;
            }
        }
    }
    public string GetTextValue(string key = "value") {
        return GetPart(key).content;
    }
    // value can be number or seq. but it's alway dynamic
    public int GetNumValue(string key = "value") {
        Part part = GetPart(key);
        if ( part == null) {
            Debug.LogError($"GET NUM VALUE : prop {name} doesn't have part with key {key}"); 
            return 0;
        }

        int num = 0;
        if (!int.TryParse(part.content, out num)) {
            Debug.LogError($"GET NUM VALUE : value of {name} ({part.content}) can't be parsed");
            return -1;
        }
        return num;
    }
    public void SetValue(string text, string part = "value") {
        GetPart(part).content = text;

    }
    public void SetValue(int num, string part = "value") {
        if (HasPart("max"))
            num = Math.Clamp(num, 0, GetNumValue("max"));
        GetPart(part).content = num.ToString();

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
            SetPart("description", description);
        } else if (!HasPart("description type")) {
            SetPart("description type","none");
        }
        //
        // SETUP //
        if (description.StartsWith('[')) {
            var setup = TextUtils.Extract('[', description, out description);
            SetPart("description", description);
            SetPart("description setup", setup);
        }
        //
        
    }
    public string GetDescription() {
        if (!HasPart("description"))
            return "";


        var newDescription = GetNewDescription();
        if (HasPart("cDes") && newDescription != GetPart("cDes").content)
            SetChanged();
        SetPart("cDes", newDescription);
        return newDescription;
    }
    public bool DescriptionChanged() {
        return HasPart("changed");
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
    public void SetChanged() {
        SetPart("changed", "");
    }

    private string GetNewDescription() {
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

    public static string GetDescription(List<Property> props) {
        string str = "";
        Debug.Log($"describing props");
        for (int i = 0; i < props.Count; i++) {
            Debug.Log($"{props[i].name}/{props[i].GetDescription()}");
            str += $"{props[i].GetDescription()}{TextUtils.GetCommas(i, props.Count, false)}";
        }
        return str;
    }
    #endregion

    public void Destroy() {
        destroy = true;
    }
}


