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
using System.Configuration;
using System.Runtime.InteropServices.WindowsRuntime;

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
        // initiating value
        if (HasPart("value")) {
            InitNumValue();
        }

        // enable / disable
        enabled = !name.StartsWith('*');
        if (!enabled) name = name.Remove(0, 1);
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
    public void AddPart(string key, string text) {
        AddPart(new Part(key, text));
    }
    public void AddPart(Part part) {
        parts.Add(part);
    }

    public void SetPart(string key, string text) {
        var part = GetPart(key);
        if (part == null) {
            part = new Part(key, text);
        } else {
            Debug.Log($"property {key} already has part{key} with {text}");
        }
        part.content = text;
    }

    public bool HasPart(string str) {
        return parts.Find(x => x.key == str) != null;
    }

    public List<Part> GetParts(string key) {
        return parts.FindAll(x=> x.key == key);
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

        // check link
        if (part.content.Contains('[')){
            var prop = ItemLink.GetProperty(part.content, null);
            part.content = prop.GetNumValue().ToString();
            return;
        }

        if (part.content.Contains('?')) {
            string[] prts = part.content.Split('?');
            int min = int.Parse(prts[0]);
            int max = int.Parse(prts[1]);
            int i = UnityEngine.Random.Range(min, max+1);
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
    public string GetDescription() {

        if (!HasPart("description"))
            return ""; 

        var newDescription = GetNewDescription();

        if (!HasPart("prevDescription"))
            AddPart("prevDescription", "");

        if (!HasPart("currDescription"))
            AddPart("currDescription", "");

        GetPart("prevDescription").content = GetPart("currDescription").content;
        GetPart("currDescription").content = newDescription;

        return newDescription;
    }
    public bool DescriptionChanged() {
        if (!HasPart("currDescription"))
            GetDescription();
        return GetPart("prevDescription").content != GetPart("currDescription").content;
    }
    private string GetNewDescription() {
        
        var description = GetPart("description").content;

        if (description.Contains("/")) {
            var start = "";
            if (description.StartsWith('(')) {
                start = TextUtils.Extract('(', description);
                description = description.Remove(0, description.IndexOf(')') + 1);
            }
            var prts = description.Split(" / ");
            int max = HasPart("max") ? GetNumValue("max") : 10;
            int i = GetNumValue();
            if (i == 0) return prts[0];
            if ( i == max ) return prts[prts.Length-1];
            var lerp = (float)i * prts.Length / max;
            int index = Math.Clamp((int)lerp, 1, prts.Length-2);
            return $"{start} {prts[index]}";
        }

        if (description.StartsWith('(')) {
            var strs = TextUtils.Extract('(', description).Split('=');
            switch (strs[0]) {
                case "type":
                    var typeIndex = ItemData.GetRandomDataOfType(strs[1]);
                    GetPart("description").content = ItemData.itemDatas[typeIndex].name;
                    Debug.Log($"changed value of {name} to {ItemData.itemDatas[typeIndex].name}");
                    return GetPart("description").content;
                case "spec":
                    GetPart("description").content = Spec.GetCat(strs[1]).GetRandomSpec();
                    return GetPart("description").content;
            }
        }

        if ( description.Contains("[value]"))
            description = description.Replace("[value]", GetPart("value").content);

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
}


