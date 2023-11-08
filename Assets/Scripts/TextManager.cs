using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEditor.Progress;
public static class TextManager {
    static List<Humanoid.Orientation> overrideOrientations = new List<Humanoid.Orientation>();
    // PARAMS
    public static List<PhraseKey> phraseKeys = new List<PhraseKey>();
    // override c'est vraiment pas bien, il faut trouver une façon de faire ("&le chien sage (surrounding tile)&")
    private static List<Item> overrides = new List<Item>();

    public static void SetOverrideOrientation(Humanoid.Orientation orientation) {
        overrideOrientations = new List<Humanoid.Orientation> { orientation };
    }
    public static void SetOverrideOrientation(List<Humanoid.Orientation> _orientations) {
        overrideOrientations = _orientations;
    }
    public static string getItemWord(string key) {
        // check if there's a key phrase ( ex : item_pickUp )
        var str = GetPhraseKey(key);

        // ITEM ( il faut le faire ici AUSSI, pour la compil du display description
        str = KeyWords.ReplaceKeyWords(str);

        // &a good dog& => a charred road
        // &some dogs (main)& => some seeds
        str = getOverrideText(str);
        return str;
    }
    private static string GetPhraseKey(string key) {
        var phraseKey = phraseKeys.Find(x => x.key == key);
        if (phraseKey == null) {
            //Debug.LogError("phrase <color=red>" + key + "</color> does not exist, returning key");
            return key;
        }
        return phraseKey.values[Random.Range(0, phraseKey.values.Count)];
    }
    public static string getOverrideText(string text) {
        var safetyBreak = 0;
        if (text == null)
            Debug.LogError("extract item words : le text est null ?");

        // each "&le chien sage (itemcode)& iteration
        while (text.Contains("&")) {
            // bonjour je suis &le chien sage (main)& => &le chien sage (main)&
            var partToReplace = isolatePart(text);
            // "&the dog& => the dog
            var prms = trimPart(partToReplace);

            Item targetItem = null;
            if (prms.Contains('*')) {
                targetItem = GetItemInHistory(prms, out prms);
                if (targetItem == null) {
                    Debug.LogError("no override item for " + text);
                    return "!NO OVERRIDE ITEM!";
                }
            } else {
                // getting the item
                if (overrides.Count > 0) {
                    targetItem = overrides.Last();
                    overrides.Remove(targetItem);
                }
            }

            if (targetItem == null)
                return "!override text error!";

            // check if item is in player inventory or body
            if (Player.Instance.getRecursive(3).Find(x => x == targetItem) != null) {
                Debug.Log("the item " + targetItem.debug_name + "/" + targetItem.GetHashCode() + " is part of the player");
                prms = "your dog";
            }

            // get word from item
            var newPart = targetItem.getText(prms);

            // replace target part with word, and continue
            text = text.Replace(partToReplace, newPart);

            // safety break
            ++safetyBreak;
            if (safetyBreak >= 10) {
                Debug.LogError("item word detection reached safety break");
                break;
            }
        }
        return text;
        //
    }

    private static Item GetItemInHistory(string arg, out string newText) {
        // word code = le chien sage (main)
        // check if there tags
        if (!arg.Contains("*")) {
            // no key specified, so returning the main
            // not a bug, just flemme

            Debug.LogError("HISTORY ITEM : " + arg + " doesn't have *");
            newText = arg;
            return null;
        }
        // remove parentheses
        var searchKey = arg.Remove(0, arg.IndexOf('*'));
        newText = arg.Replace(searchKey, "dog");
        searchKey = searchKey.Remove(0, 1);
        searchKey = searchKey.Remove(searchKey.IndexOf('*'));
        var itemKey = ItemParser.GetCurrent.itemHistory.Find(x => x.key == searchKey);

        if (itemKey == null) {
            Debug.LogError("no item with key : " + searchKey + " in item history");
            return null;
        }

        var item = itemKey.item;

        return item;
    }
    // isolate "&" du texte
    static string isolatePart(string str) {
        var startIndex = str.IndexOf('&');
        var targetPart = str.Remove(0, startIndex);
        var endIndex = targetPart.Remove(0, 1).IndexOf('&') + 2;
        if (endIndex < targetPart.Length)
            targetPart = targetPart.Remove(endIndex);
        return targetPart;
    }
    static string trimPart(string str) {
        try {
            var wordCode = str.Remove(0, 1);
            wordCode = wordCode.Remove(wordCode.Length - 1);
            return wordCode;
        } catch {
            Debug.LogError("str in : "+ str);
            return "trim part error";
        }
    }

    public static List<Humanoid.Orientation> GetOverrideOrientations() {
        return overrideOrientations;
    }
    #region write phrase
    public static void Return() {
        DisplayDescription.Instance.text_target += "\n";
    }

    public static void write(string str, Item _overrideItem) {
        addOverride(_overrideItem);
        write(str);
    }
    public static void addOverride(Item item) {
        overrides.Add(item);
    }
    public static void write(string str) {
        var text = getItemWord(str);
        DisplayDescription.Instance.AddToDescription($"\n{text}\n", true);
    }

    public static void AddLink(int index, int l) {
        add(TextUtils.GetLink(index, l));
    }

    public static void add(string str, Item _overrideItem) {
        addOverride(_overrideItem);
        add(str);
    }
    public static void add(string str) {
        var text = getItemWord(str);
        DisplayDescription.Instance.AddToDescription(text, false);
    }
    #endregion
    public static void Renew() {

    }

    public static string itemNameToClassName(string str) {
        var parts = str.Split(' ');
        var className = "";
        foreach (var part in parts)
            className += part[0].ToString().ToUpper() + part.Substring(1);
        return className;
    }

    public static string linkText(List<string> list) {
        string str = "";
        for (int i = 0; i < list.Count; i++) {
            str += list[i];
            str += TextUtils.GetLink(i, list.Count);
        }
        return str;
    }

    // gets all the word of the items.
    public static string getItemWords(List <Item> items) {
        return "nique toi";
    }


    public static string ToLowercaseNamingConvention(this string s, bool toLowercase) {
        if (toLowercase) {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
            return r.Replace(s, " ").ToLower();
        } else
            return s;
    }

}