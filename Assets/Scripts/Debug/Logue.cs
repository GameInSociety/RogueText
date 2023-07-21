using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Logue
{

    public class Entry
    {
        public Entry (string n, string t, Color c)
        {
            name = n;
            title = t;
            color = c;
        }

        public string name;
        public string title;
        public string content;
        public Color color;
    }
    public static List<Entry> entries = new List<Entry>();

    public static void New(string n, string str, Color c)
    {
        entries.Add(new Entry(n, str, c));
    }

    public static void Add(string str)
    {
        Entry lastEntry = entries[entries.Count - 1];

        lastEntry.content += str + "\n";
    }

    public static void Clear()
    {
        entries.Clear();
    }
}
