using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using TreeEditor;
using System;

[System.Serializable]
public class Property {

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
    // peut être tout le temps mettre "*" quand il est disabled
    // juste sauver un peu de mémoire mais bon...
    public bool enabled = false;
    public List<Part> parts;

    // à revoir
    public bool changed = false;

    public Property() {

    }

    public Property(Property copy) {
        name = copy.name;
        enabled = !name.StartsWith('*');
        if (!enabled) name = name.Remove(0, 1);
        if (copy.parts == null)
            return;
        foreach (var part in copy.parts)
            AddPart(new Part(part.key, part.text));
    }

    public Property(string cell) {

        var lines = cell.Split('\n');
        name = lines[0];

        // no parts, only name
        if (lines.Length == 1) return;

        var partCount = cell.Split(':').Length - 1;
        if (partCount == 0) return;

        parts = new List<Part>();
        char[] chars = { '\r', '\t', '\b', '\n', ' ' };

        for (int i = 1; i < lines.Length; i++) {
            // skip empty lines
            if (string.IsNullOrEmpty(lines[i])) continue;

            if (lines[i].Contains(':')) {
                // new part
                var strs = lines[i].Split(':');
                var text = strs[1].Trim(chars);
                parts.Add(new Part(strs[0], text));
            } else {
                try {
                    // add to current part
                    var str = lines[i].Trim(chars);
                    var part = parts[parts.Count - 1];
                    part.text += string.IsNullOrEmpty(part.text) ? str : $"\n{str}";
                } catch (Exception e) {
                    Debug.Log($"error loading propety : {name}");
                    Debug.Log($"line : {lines[i]}");
                    Debug.LogException(e);
                }
            }
        }
    }

    public void AddPart(Part part) {
        if ( parts == null)
            parts = new List<Part>();
        parts.Add(part);
    }

    public bool hasPart(string str) {
        return parts.Find(x => x.key == str) != null;
    }

    public Part getPart(string key) {
        Part part = parts.Find(x => x.key == key);
        if (part == null) {
            Debug.LogError($"{name} has no part with key {key}");
            return null;
        }
        return part;
    }

    public string getText(string key) {
        return getPart(key).text;
    }
    public void setText(string key, string str) {
        getPart(key).text = str;
    }
    public int getNum(string key) {
        Part part = getPart(key);

        int num = 0;
        if (!int.TryParse(part.text, out num)) {
            Debug.LogError($"{name} hasPart part {part.key} but {part.text} can't be parsed");
            return -1;
        }

        return num;
    }
    public void setNum(int num) {
        getText("value");
    }

    #region description
    public string GetDescription() {
        return $"(description of {name})";
    }
    #endregion

    public bool destroy = false;
    public void Destroy() {
        destroy = true;
    }

    #region events
    public string getEvent(string key) {
        Part p = getPart(key);
        return p.text;
    }
    #endregion

    #region update
    public void update(string newContent) {

        if (!hasPart("value")) {
            Debug.LogError($"property {name} hasPart no value, can't update");
            return;
        }

        var add = false;
        var remove = false;

        if (newContent.StartsWith('+')) {
            newContent = newContent.Remove(0, 1);
            add = true;
        }

        if (newContent.StartsWith('-')) {
            newContent = newContent.Remove(0, 1);
            remove = true;
        }

        int dif;
        int num = getNum("value");
        if (int.TryParse(newContent, out dif)) {
            var newValue = dif;
            if (add)
                newValue = num + dif;
            else if (remove)
                newValue = num - dif;
            setNum(newValue);
        }
    }
    #endregion
}


