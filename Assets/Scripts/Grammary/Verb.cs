using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Verb {

    public static List<Verb> verbs = new List<Verb>();
    public static void AddVerb(Verb verb) {
        verbs.Add(verb);
    }

    public string debug_word;
    public int indexInText;
    public string question = "";
    public string[] prepositions;
    public string[] words;
    public int duration = 0;
    public int currentNameIndex = 0;

    public string GetFull => $"{GetCurrentWord} {GetPreposition}";
    public string GetCurrentWord => words[currentNameIndex];
    public string GetPreposition {
        get {
            if (currentNameIndex >= prepositions.Length)
                return prepositions[0];
            return prepositions[currentNameIndex];
        }
    }

    public static bool IsNull(Verb verb) {
        return verb == null || (verb != null && string.IsNullOrEmpty(verb.debug_word));
    }
    public string GetItemSequence(ItemData data, bool checkUndefined = true) {

        if (data == null)
            return null;

        // get all the sequences_debug int the data
        foreach (var sequence in data.verbSequences) {
            // see if one of the verbs of the sequenbe match this verb
            foreach (var potVerb in sequence.triggers) {
                if (words.Contains(potVerb))
                    return sequence.seq;
            }
        }

        return null;
    }

    public int[] getIndexesInText(string text) {
        List<int> ints = new List<int>();
        for (int i = 0; i < words.Length; i++) {
            string word = words[i];
            // get all the verbs
            if (Regex.IsMatch(text, @$"\b{word}\b")) {
                int startIndex = -1;
                while (true) {
                    startIndex = text.IndexOf(word, startIndex + 1);
                    if (startIndex < 0 || startIndex >= text.Length)
                        break;
                    currentNameIndex = i;
                    ints.Add(startIndex);
                }

            }
        }
        return ints.ToArray();
    }

    public int GetIndexInText(string text) {
        for (int i = 0; i < words.Length; i++) {
            string word = words[i];
            // get all the verbs
            if (Regex.IsMatch(text, @$"\b{word}\b")) {
                currentNameIndex = i;
                indexInText = text.IndexOf(word);
                return indexInText;
            }
        }
        return -1;
    }

    public static Verb FindInData(string str) {
        var verb = verbs.Find(x => x.words[0] == str);

        if (verb == null) {
            Debug.LogError("couldn't find verb in " + str);
            return null;
        }

        return verb;
    }
}