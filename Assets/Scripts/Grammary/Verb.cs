using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;

[System.Serializable]
public class Verb {

    public int col = 0;

    public static List<Verb> verbs = new List<Verb>();
    public class Sequence {
        public int id;
        public string content;
    }
    public List<Sequence> cells = new List<Sequence>();

    public bool HasCell(Item item) {
        return cells.Find(x => x.id == item.dataIndex) != null;
    }

    public Sequence GetSequence(Item item) {
        return cells.Find(x => x.id == item.dataIndex);
    }

    public static void AddVerb(Verb verb) {
        verbs.Add(verb);
    }

    public string question = "";
    public string[] prepositions;

    public string helpPhrase = "";

    public bool universal = false;

    public string[] words;
    public int currentNameIndex = 0;
    public string GetFull => $"{getWord} {GetPreposition}";
    public string getWord => words[currentNameIndex];
    public string GetPreposition {
        get {
            if (currentNameIndex >= prepositions.Length) {
                return prepositions[0];
            }

            return prepositions[currentNameIndex];
        }
    }


    public void AddCell(int id, string content) {
        var cell = new Sequence();
        cell.id = id;
        cell.content = content;
        cells.Add(cell);
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

    public int getIndexInText(string text) {
        for (int i = 0; i < words.Length; i++) {
            string word = words[i];
            // get all the verbs
            if (Regex.IsMatch(text, @$"\b{word}\b")) {
                currentNameIndex = i;
                return text.IndexOf(word);
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