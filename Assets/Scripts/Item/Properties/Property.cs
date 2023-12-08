using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using TreeEditor;
using System;
using System.Security.Cryptography;
using System.Threading;
using JetBrains.Annotations;
using System.Xml.Schema;
using UnityEditor;
using System.Text;

[System.Serializable]
public class Property {

    public static List<Property> datas = new List<Property>();

    public static void AddPropertyData(Property prop) {
        datas.Add(prop);
    }

    // in data
    [System.Serializable]
    public class Part {
        public string key;
        public string text;
        public Part(string k, string v) {
            key = k;
            text = v;
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
            AddPart(new Part(part.key, part.text));

        // searching a prop in data for copying
        var dataProp = datas.Find(x => x.name == name);
        if (dataProp != null) {
            // adding implicit parts from the data
            foreach (var part in dataProp.parts) {
                if (parts.Find(x => x.key == part.key) != null)
                    continue;
                AddPart(part.key, part.text);
            }
        }

        // fetch events

        Init();
        
    }

    public void Init() {
        // initiating value
        if (HasPart("value")) {
            InitNumValue();
        }

        // enable / disable
        enabled = !name.StartsWith('*');
        if (!enabled) name = name.Remove(0, 1);
    }

    public void Parse(string cell) {
        var lines = cell.Split('\n');
        name = lines[0];

        // adding the value part to the prop
        // will not ovverride by data props
        if (name.Contains(':')) {
            var split = name.Split(':');
            name = split[0];
            AddPart("value", split[1]);
            // remove the first ':'
            cell = cell.Remove(0, cell.IndexOf(':')+1);
        }

        // no parts, only name
        if (lines.Length == 1) return;

        var partCount = cell.Split(':').Length - 1;
        if (partCount == 0) return;

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
                var str = lines[i].Trim(chars);
                var part = parts[parts.Count - 1];
                part.text += string.IsNullOrEmpty(part.text) ? str : $"\n{str}";
            }
        }
    }

    #region parts
    public void AddPart(string key, string text) {
        AddPart(new Part(key, text));
    }
    public void AddPart(Part part) {
        parts.Add(part);
    }

    public bool HasPart(string str) {
        return parts.Find(x => x.key == str) != null;
    }

    public Part GetPart(string key) {
        return parts.Find(x => x.key == key);
    }
    #endregion

    #region value
    public void InitNumValue(string key = "value") {
        Part part = GetPart(key);
        if (part == null) {
            Debug.LogError($"INIT NUM VALUE : prop {name} doesn't have part with key {key}");
            return;
        }
        if (part.text.Contains('?')) {
            string[] prts = part.text.Split('?');
            int min = int.Parse(prts[0]);
            int max = int.Parse(prts[1]);
            int i = UnityEngine.Random.Range(min, max);
            part.text = $"{i}";
            AddPart("max", max.ToString());
        }

        if (part.text.Contains('m')) {
            string[] prts = part.text.Split('m');
            int min = int.Parse(prts[0]);
            int max = int.Parse(prts[1]);
            part.text = min.ToString();
            AddPart("max", max.ToString());
        }
    }
    // value can be number or seq. but it's alway dynamic
    public int GetNumValue(string key = "value") {
        Part part = GetPart(key);
        if ( part == null) {
            Debug.LogError($"GET NUM VALUE : prop {name} doesn't have part with key {key}");
            return 0;
        }

        int num = 0;
        if (!int.TryParse(part.text, out num)) {
            Debug.LogError($"GET NUM VALUE : value of {name} ({part.text}) can't be parsed");
            return -1;
        }
        return num;
    }
    public string GetTextValue() {
        var part = GetPart("name");
        if (part == null)
            return $"property {name} has no part (name)";
        return part.text;

    }
    public void SetValue(int num, string part = "value") {
        if (HasPart("max")) {
            num = Math.Clamp(num, 0, GetNumValue("max"));
        }
        GetPart("value").text = num.ToString();
    }
    #endregion

    #region description
    public string GetDescription() {

        if (!HasPart("description")) return "error : no item description";
        
        var description = GetPart("description").text;

        if (description.Contains("/")) {
            var start = "";
            if (description.StartsWith('(')) {
                start = TextUtils.Extract('(', description);
                description = description.Remove(0, description.IndexOf(')') + 1);
            }
            var prts = description.Split(" / ");
            int max = HasPart("max") ? GetNumValue("max") : 10;
            var lerp = (float)GetNumValue() / max * prts.Length;
            int index = Math.Clamp((int)lerp, 0, prts.Length-1);
            return $"{start} {prts[index]}";
        }

        if (description.StartsWith('(')) {
            var strs = TextUtils.Extract('(', description).Split('=');
            switch (strs[0]) {
                case "type":
                    var itenName = ItemData.GetItemData(strs[1]).name;
                    description = itenName;
                    Debug.Log($"changed value of {name} to {itenName}");
                    break;
                case "spec":
                    description = Spec.GetCat(strs[1]).GetRandomSpec();
                    break;
            }
        }

        if ( description.Contains("[value]"))
            description = description.Replace("[value]", GetNumValue().ToString());
        if ( description.Contains("[name]"))
            description = description.Replace("[name]", GetTextValue().ToString());

        if (description.Contains('/'))
            return "didn't implemant split description yet";
        return description;
    }
    public static string GetDescription(List<Property> props) {
        string str = "";
        for (int i = 0; i < props.Count; i++) {
            str += $"{props[i].GetDescription()}{TextUtils.GetCommas(i, props.Count)}";
        }
        return str;
    }
    #endregion

    public void Destroy() {
        destroy = true;
    }
    #region update
    public enum UpdateType {
        None,
        Add,
        Substract,
    }
    #endregion
}


