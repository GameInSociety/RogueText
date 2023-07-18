using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Windows;

public static class TextManager
{
    static List<Humanoid.Orientation> overrideOrientations = new List<Humanoid.Orientation>();
    // PARAMS
    public static List<PhraseKey> phraseKeys = new List<PhraseKey>();
    // override c'est vraiment pas bien, il faut trouver une façon de faire ("&le chien sage (surrounding tile)&")
    private static List<Item> overrides = new List<Item>();

    public static void SetOverrideOrientation(Humanoid.Orientation orientation)
    {
        overrideOrientations = new List<Humanoid.Orientation> { orientation };
    }
    public static void SetOverrideOrientation(List<Humanoid.Orientation> _orientations)
    {
        overrideOrientations = _orientations;
    }
    public static string GetPhrase(string key)
    {
        // check if there's a key phrase ( ex : item_pickUp )
        string str = GetPhraseKey(key);
        
        // ITEM ( il faut le faire ici AUSSI, pour la compil du display description
        str = KeyWords.ReplaceKeyWords(str);

        // &a good dog& => a charred road
        // &some dogs (main)& => some seeds
        str = GetOverrideItem (str);
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
    public static string GetOverrideItem(string text)
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
            string partToReplace = IsolatePart(text);

            // "&le chien sage (main)& => le chien sage (main)
            string prms = TrimPart(partToReplace);


            Item targetItem = null;

            if (prms.Contains('*'))
            {
                targetItem = GetItemInHistory(prms, out prms);

                if (targetItem == null)
                {
                    Debug.LogError("no override item for " + text);
                    return "!NO OVERRIDE ITEM!";
                }
            }
            else
            {
                // getting the item
                if (overrides.Count > 0)
                {
                    targetItem = overrides.Last();
                    overrides.Remove(targetItem);
                }
            }

            

            if ( targetItem == null)
            {
                Debug.LogError("no target item for : " + prms);
                return "!override text error!";
            }


            if (Player.Instance.GetAllItems().Find(x => x == targetItem) != null)
            {
                Debug.Log("the item " + targetItem.debug_name + "/" + targetItem.GetHashCode() + " is part of the player");
                prms = "your dog";
            }

            // get word from item
            string newPart = targetItem.word.GetInfo(prms);
            // replace target part with word, and continue
            text = text.Replace(partToReplace, newPart);

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

    private static Item GetItemInHistory(string arg, out string newText)
    {
        // word code = le chien sage (main)
        // check if there tags
        if (!arg.Contains("*"))
        {
            // no key specified, so returning the main
            // not a bug, just flemme

            Debug.LogError("HISTORY ITEM : " + arg + " doesn't have *");
            newText = arg;
            return null;
        }
        // remove parentheses
        string searchKey = arg.Remove(0, arg.IndexOf('*'));
        newText = arg.Replace(searchKey, "dog");
        searchKey = searchKey.Remove(0, 1);
        searchKey = searchKey.Remove(searchKey.IndexOf('*'));
        ItemParser.ItemKey itemKey = ItemParser.history.Find(x => x.key == searchKey);

        if ( itemKey == null)
        {
            Debug.LogError("no item with key : " + searchKey + " in item history");
        }

        Item item = itemKey.item;

        return item;
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
        AddOverride(_overrideItem);
        Write(str);
    }
    public static void AddOverride(Item item)
    {
        overrides.Add(item);
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
    public static string GetLink(int index, int l)
    {
        return TextUtils.GetLink(index, l);
    }

    public static void Add(string str, Item _overrideItem)
    {
        AddOverride(_overrideItem);
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