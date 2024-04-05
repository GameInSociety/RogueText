using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Phrase {

    [System.Serializable]
    public class PhraseType {
        public string key;
        public List<Phrase> phrases = new List<Phrase>();
    }
    public static List<PhraseType> phraseTypes = new List<PhraseType>();
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
            }
        }
        public string key = "";
        public int leftMin;
        public int rightMin;
        public List<string> variants = new List<string>();
        public List<string> pool = new List<string>();

        public static string GetRandom(int left, int right) {
            var part = parts.Find(x => left >= x.leftMin && right >= x.rightMin);
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
            return content;
        }
    }
    

    public static string GetPhrase(List<ItemSlot> input, out List<ItemSlot> output, ItemDescription.Options options) {

        var slotCount = options.groupedSlots ? input.Count : Random.Range(1, 4);

        slotCount = Mathf.Clamp(slotCount, 1, input.Count);
        List<ItemSlot> slots = new List<ItemSlot>();
        for (int i = 0; i < slotCount; i++) {
            slots.Add(input[i]);
            slots[i].defined = options.definite;
        }
        input.RemoveRange(0, slotCount);

        var result = "";

        var slots_multItems = slots.FindAll(x => x.items.Count > 1);
        var slots_singleItems = slots.FindAll(x => x.items.Count  == 1);

        int sepIndex = Random.Range(0, slotCount - 1);

        var left = sepIndex;
        var right = slots.Count - sepIndex;
        for (int i = 0; i < slots.Count; i++) {
            var slot = slots[i];
            if ( options.list) {
                result += $"{slot.ItemsToString()}.\n";
            } else {
                if (i == sepIndex)
                    result += $"{Part.GetRandom(left, right)} ";
                result += $"{slot.ItemsToString()}{TextUtils.GetCommas(i, slots.Count)}";
            }
        }
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