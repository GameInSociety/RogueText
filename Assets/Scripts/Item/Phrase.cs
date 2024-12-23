using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;

/// <summary>
/// This class merges all the item slots into a phrase, binding them with location text ("There's", "Between" etc... )
/// Phrase structure :
/// "Between the [SLOT 1] and the [SLOT 2]
/// </summary>
[System.Serializable]
public class Phrase {

    [System.Serializable]
    public class PhraseType {
        public List<Phrase> phrases = new List<Phrase>();
    }
    public static List<Part> parts = new List<Part>();

    /// <summary>
    /// The content of the phrase
    /// </summary>
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
    

    public static string GetPhrase(List<ItemDescription> input_Slots, out List<ItemDescription> output_Slots) {

        Description.Options options = new Description.Options("");

        var results = new List<string>();

        // (Debug) Very important. Return a simple list with items sorted by properties & number, without the phrase merge.
        if (DebugManager.Instance.Description_DebugList())
        {
            for (int i = 0; i < input_Slots.Count; i++)
            {
                string itemText = input_Slots[i].GetText();
                results.Add($"{itemText}");
            }
            var list = string.Join("\n", results);
            input_Slots.Clear();
            output_Slots = input_Slots;
            return list.Trim(' ');
        }

        // Selecting the number of slots merged in the next text. ( "surrounding a TREE, some MUSHROOMS", "In front of you, a dog" )
        var slotCount = options.groupedSlots ? input_Slots.Count : Random.Range(1, 4);
        // ?? 
        slotCount = input_Slots.Count;

        List<ItemDescription> slots = new List<ItemDescription>();
        for (int i = 0; i < slotCount; i++) {
            slots.Add(input_Slots[i]);
            slots[i].definite = options.definite;
        }
        input_Slots.RemoveRange(0, slotCount);


        var slots_multItems = slots.FindAll(x => x._infos.Count > 1);
        var slots_singleItems = slots.FindAll(x => x._infos.Count  == 1);

        var sepIndex = Random.Range(0,slots.Count+1);
        bool after = Random.value > 0.5f;
        
        for (int i = 0; i < slots.Count; i++) {

            string itemText = slots[i].GetText();
            string end = options.list ? "\n" : TextUtils.GetCommas(i, i < sepIndex ? sepIndex : slots.Count);
            results.Add($"{itemText}{end}");
        }
        if (!options.list) {
            var insert = $" {Part.GetRandom(sepIndex, slots.Count - sepIndex)} ";
            results.Insert(sepIndex, insert);
        }


        var result = string.Join("", results);
        output_Slots = input_Slots;
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