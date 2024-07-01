using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;

[System.Serializable]
public class Phrase {

    [System.Serializable]
    public class PhraseType {
        public string key;
        public List<Phrase> phrases = new List<Phrase>();
    }
    public static List<Part> parts = new List<Part>();

    public string text;
    public int groupCount;
    public int multItemsCount = 0;
    public int singleItemsCount = 0;

    public Phrase (string text) {
        this.text = text;
    }

    [System.Serializable]
    public class Part {
        public Part(string key) {
            this.key = key;
            if (key.Contains('/')) {
                var split = key.Split('/');
                leftMin = int.Parse(split[0]);
                rightMin = int.Parse(split[1]);
            } else {
                leftMin = 100;
                rightMin = 100;
            }
        }
        public string key = "";
        public int leftMin;
        public int rightMin;
        public List<string> variants = new List<string>();
        public List<string> pool = new List<string>();

        public static string GetRandom(int left, int right) {
            var _ptrs = parts.FindAll(x => left >= x.leftMin && right >= x.rightMin);
            var i = Random.Range(0, _ptrs.Count);
            var part = _ptrs[i];
            if ( part == null) {
                Debug.LogError($"no part with left {left} and right {right}");
                return "part error";
            }

            return GetRandom(part.key);
        }

        public static string GetRandom(string key) {
            var part = parts.Find(x => x.key == key);
            if ( part == null) {
                Debug.LogError($"no part with key : {key}");
                return "part_error";
            }
            if ( part.pool.Count == 0 ) {
                foreach (var s in part.variants)
                    part.pool.Add(s);
            }
            int i = Random.Range(0, part.pool.Count);
            var content = part.pool[i];
            part.pool.RemoveAt(i);
            if ( content == "x") {
                return "";
            }
            return $"{content}";
        }
    }
    

    public static string GetPhrase(List<ItemSlot> input, out List<ItemSlot> output, DescriptionGroup.Options options) {

        var slotCount = options.groupedSlots ? input.Count : Random.Range(1, 4);
        //slotCount = Mathf.Clamp(slotCount, 1, input.Count);
        slotCount = input.Count;

        List<ItemSlot> slots = new List<ItemSlot>();
        for (int i = 0; i < slotCount; i++) {
            slots.Add(input[i]);
            slots[i].definite = options.definite;
        }
        input.RemoveRange(0, slotCount);


        var slots_multItems = slots.FindAll(x => x.items.Count > 1);
        var slots_singleItems = slots.FindAll(x => x.items.Count  == 1);

        var sepIndex = Random.Range(0,slots.Count+1);
        bool after = Random.value > 0.5f;
        
        var results = new List<string>();
        for (int i = 0; i < slots.Count; i++) {

            string itemText = options.list ? slots[i].Describe() : slots[i].Describe();
            string end = options.list ? "\n" : TextUtils.GetCommas(i, i < sepIndex ? sepIndex : slots.Count);
            results.Add($"{itemText}{end}");

        }
        if (!options.list) {
            var insert = $" {Part.GetRandom(sepIndex, slots.Count - sepIndex)} ";
            results.Insert(sepIndex, insert);
        }


        var result = string.Join("", results);
        output = input;
        return result.Trim(' ');

    }

    static string ReplaceKeys(string text, string key, List<string> strs) {
        foreach (var str in strs) {
            var kIndex = text.IndexOf(key);
            text = text.Remove(kIndex, key.Length);
            text = text.Insert(kIndex, str);
        }
        return text;
    }

}