using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phrase
{
    public string key = "";
    public List<string> values = new List<string>();

    public static Player.Orientation orientation;
    public static List<Player.Orientation> orientations = new List<Player.Orientation>();

    static Item overrideItem = null;

    // PARAMS

    public static List<Phrase> phrases = new List<Phrase>();

    public static string GetPhrase(string key, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        return GetPhrase(key);
    }

    public static string GetPhrase(string key)
    {
        Phrase phrase = phrases.Find(x => x.key == key);

        if (phrase == null)
        {
            Debug.LogError("phrase <color=red>" + key + "</color> does not exist");
            return null;
        }

        // get random
        string str = phrase.values[Random.Range(0, phrase.values.Count)];

        // ITEM ( il faut le faire ici AUSSI, pour la compil du display description
        str = ReplaceItemInString(str);

        // ORIENTATIONS
        str = str.Replace("ORIENTATIONS", Coords.GetOrientationText(orientations));
        // 

        // ORIENTATION
        str = str.Replace("ORIENTATION", Coords.GetOrientationText(orientation));
        //

        return str;
    }

    public static string ReplaceItemInString(string str)
    {
        int a = 0;

        while (str.Contains("&"))
        {
            int startIndex = str.IndexOf('&');

            string word_code = str.Remove(0, startIndex);
            int endIndex = word_code.Remove(0, 1).IndexOf('&') + 2;

            if (endIndex < word_code.Length)
            {
                word_code = word_code.Remove(endIndex);
            }

            // "&le chien sage& => le chien sage
            string clean_word_code = word_code.Remove(0, 1);
            clean_word_code = clean_word_code.Remove(clean_word_code.Length - 1);

            // get item & word
            Item targetItem = null;

            if (overrideItem != null)
            {
                targetItem = overrideItem;
                overrideItem = null;

            }
            else
            {
                // check if word contains item
                if (clean_word_code.Contains("chapeau"))
                {
                    targetItem = InputInfo.GetCurrent.items[0];

                    if (clean_word_code.Contains("2"))
                    {
                        clean_word_code = clean_word_code.Remove(word_code.Length - 1);
                        targetItem = InputInfo.GetCurrent.items[1];
                        Debug.Log("second item : " + word_code);
                    }

                    clean_word_code = word_code.Replace("chapeau", "chien");
                    clean_word_code = word_code.Replace("chic", "sage");
                }

                // check if word contains tile item
                if (clean_word_code.Contains("bois"))
                {
                    targetItem = Tile.GetCurrent.tileItem;

                    clean_word_code = clean_word_code.Replace("bois", "chien");
                    clean_word_code = clean_word_code.Replace("calme", "sage");
                }

                
            }

            string word = targetItem.word.GetContent(clean_word_code);

            //
            str = str.Replace(word_code, word);
            
            // safety break
            ++a;

            if ( a >= 10)
            {
                Debug.LogError("item word detection reached safety break");
                break;
            }

        }

        return str;
        //
    }

}
