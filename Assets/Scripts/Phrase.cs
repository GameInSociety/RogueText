using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phrase
{
    public string key = "";
    public List<string> values = new List<string>();

    public static Item item;
    public static Player.Orientation orientation;
    public static List<Player.Orientation> orientations = new List<Player.Orientation>();

    // PARAMS

    public static List<Phrase> phrases = new List<Phrase>();

    public static string GetPhrase(string key)
    {
        Phrase phrase = phrases.Find(x => x.key == key);

        if (phrase == null)
        {
            Debug.LogError("phrase <color=red>" + key + "</color> does not exist");
            return null;
        }

        // ITEM
        string str = phrase.values[Random.Range(0, phrase.values.Count)];

        string[] parts = str.Split(' ');

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Contains("/"))
            {
                string word = item.word.GetContent(parts[i]);
                str = str.Replace(parts[i], word);
            }
        }
        //

        // ORIENTATIONS
        str = str.Replace("ORIENTATIONS", Coords.GetOrientationText(orientations));
        // 

        // ORIENTATION
        str = str.Replace("ORIENTATION", Coords.GetOrientationText(orientation));
        //

        return str;
    }
}
