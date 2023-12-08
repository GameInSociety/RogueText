using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditorInternal;
using UnityEngine;

public static class WorldData {
    private static readonly List<parameter> parameters = new List<parameter>();

    public static List<Item> globalItems = new List<Item>();

    public struct parameter {
        public string name;
        public string value;
        public parameter(string name, string value) {
            this.name = name;
            this.value = value;
        }
        public int intValue {
            get {
                return int.Parse(value);
            }
        }
    }
    public static void Init() {
        var file = Resources.Load<TextAsset>("world settings");
        var lines = file.text.Split('\n');
        for (int i = 0; i < lines.Length; i++) {
            if (string.IsNullOrEmpty(lines[i]))
                continue;
            var parts = lines[i].Split(':');
            parameters.Add(new parameter(parts[0], parts[1]));
        }

        var globalIndexes = ItemData.GetDatasOfType("universal");
        foreach (var index in globalIndexes) {
            var itemData = ItemData.itemDatas[index];
            Debug.Log($"adding universal item {itemData.debugName}");
            var newItem = ItemData.Generate_Simple(itemData.debugName);
            globalItems.Add(newItem);
        }

        
    }

    public static string get(string name) {
        parameter p = parameters.Find(x => x.name == name);
        if (string.IsNullOrEmpty(p.name)) {
            Debug.LogError($"WorldData : no GetPart named {name}");
            return "";
        }
        return p.value;
    }

    public static float getFloat(string name) {
        parameter p = parameters.Find(x => x.name == name);
        if (string.IsNullOrEmpty(p.name)) {
            Debug.LogError($"WorldData : no GetPart named {name}");
            return -1f;
        }
        float f = -1f;
        if (!float.TryParse(p.value, out f)) {
            Debug.LogError($"WorldData: cannot parse float {p.value} for key {p.name}");
        }
        return f;

    }

    public static int getInt(string name) {
        parameter p = parameters.Find(x => x.name == name);
        if (string.IsNullOrEmpty(p.name)) {
            Debug.LogError($"no world data GetPart named {name}");
            return -1;
        }
        return int.Parse(p.value);
    }
}
