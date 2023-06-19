using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEditor.Progress;
public static class TextManager
{
    static Item overrideItem = null;
    static List<Humanoid.Orientation> overrideOrientations = new List<Humanoid.Orientation>();
    // PARAMS
    public static List<PhraseKey> phraseKeys = new List<PhraseKey>();
    // override c'est vraiment pas bien, il faut trouver une façon de faire ("&le chien sage (surrounding tile)&")

    public static void SetOverrideOrientation(Humanoid.Orientation orientation)
    {
        overrideOrientations = new List<Humanoid.Orientation> { orientation };
    }
    public static void SetOverrideOrientation(List<Humanoid.Orientation> _orientations)
    {
        overrideOrientations = _orientations;
    }
    public static string GetPhrase(string key, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        return GetPhrase(key);
    }
    public static string GetPhrase(string key)
    {
        // check if there's a key phrase ( ex : item_pickUp )
        string str = GetPhraseKey(key);
        
        // ITEM ( il faut le faire ici AUSSI, pour la compil du display description
        str = KeyWords.ReplaceKeyWords(str);

        // &a good dog& => a charred road
        // &some dogs (main)& => some seeds
        str = ExtractItemWords(str);
        return str;
    }
    private static string GetPhraseKey(string key)
    {
        PhraseKey phraseKey = phraseKeys.Find(x => x.key == key);
        if (phraseKey == null)
        {
            //Debug.LogError("phrase <color=red>" + key + "</color> does not exist, returning key");
            return key;
        }
        return phraseKey.values[Random.Range(0, phraseKey.values.Count)];
    }
    public static string ExtractItemWords(string text)
    {
        int safetyBreak = 0;
        if (text == null)
        {
            Debug.LogError("extract item words : le text est null ?");
        }
        // each "&le chien sage (itemcode)& iteration
        while (text.Contains("&"))
        {
            // bonjour je suis &le chien sage (main)& => &le chien sage (main)&
            string targetPart = IsolatePart(text);
            // "&le chien sage (main)& => le chien sage (main)
            string wordInfo = TrimPart(targetPart);
            // le chien sage (main) => main
            string itemInfo = GetKey(wordInfo);

            // getting the item
            Item targetItem = GetItemFromCode(itemInfo);

            if (targetItem == null)
            {
                Debug.LogError("target item is null " + itemInfo + " text : " + text);
            }

            // get word from item
            string word = targetItem.word.GetInfo(wordInfo);
            // replace target part with word, and continue
            text = text.Replace(targetPart, word);

            // safety break
            ++safetyBreak;
            if (safetyBreak >= 10)
            {
                Debug.LogError("item word detection reached safety break");
                break;
            }
        }
        return text;
        //
    }
    private static string GetKey(string _key)
    {
        // word code = le chien sage (main)
        // check if there tags
        if (!_key.Contains("("))
        {
            // no key specified, so returning the main
            // not a bug, just flemme

            //Debug.LogError("word code : " + wordCode + " doesn't contain item code");
            return "main";
        }
        int startIndex = _key.IndexOf('(');
        string tmpItemCode = _key.Remove(0, startIndex + 1);
        tmpItemCode = tmpItemCode.Remove(tmpItemCode.Length - 1);
        _key = _key.Remove(startIndex - 1);
        // assign
        return tmpItemCode;
    }
    // isolate "&" du texte
    static string IsolatePart(string str)
    {
        // remove &s
        int startIndex = str.IndexOf('&');
        string targetPart = str.Remove(0, startIndex);
        int endIndex = targetPart.Remove(0, 1).IndexOf('&') + 2;
        if (endIndex < targetPart.Length)
        {
            targetPart = targetPart.Remove(endIndex);
        }
        return targetPart;
    }
    static string TrimPart(string str)
    {
        string wordCode = str.Remove(0, 1);
        wordCode = wordCode.Remove(wordCode.Length - 1);
        return wordCode;
    }
    static Item GetItemFromCode(string itemCode)
    {
        if ( overrideItem == null)
        {
            Debug.LogError("override item is null");
        }

        return overrideItem;
    }
    public static List<Humanoid.Orientation> GetOverrideOrientations()
    {
        return overrideOrientations;
    }
    #region write phrase
    public static void Return()
    {
        DisplayDescription.Instance.text_target += "\n";
    }
    public static void Write(string str, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        Write(str);
    }
    public static void Write(string str)
    {
        string text = GetPhrase(str);
        DisplayDescription.Instance.AddToDescription(text, true);
    }

    public static void AddLink (int index, int l)
    {
        Add(TextUtils.GetLink(index, l));
    }

    public static void Add(string str, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        Add(str);
    }
    public static void Add (string str)
    {
        string text = GetPhrase(str);
        DisplayDescription.Instance.AddToDescription(text, false);
    }
    #endregion
    public static void Renew()
    {
        
    }

    public static string ItemNameToClassName(string str)
    {
        string[] parts = str.Split(' ');

        string className = "";

        foreach (var part in parts)
        {
            className += part[0].ToString().ToUpper() + part.Substring(1);
        }

        return className;
    }

    public static string ToLowercaseNamingConvention(this string s, bool toLowercase)
    {
        if (toLowercase)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return r.Replace(s, " ").ToLower();
        }
        else
            return s;
    }

}