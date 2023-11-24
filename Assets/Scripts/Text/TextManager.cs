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

    public static void Write(string str, Item _overrideItem) {
        addOverride(_overrideItem);
        Write(str);
    }
    public static void addOverride(Item item) {
        overrides.Add(item);
    }
    public static void Write(string str) {
        DisplayDescription.Instance.AddToDescription($"\n{str}\n", true);
    }

    public static void AddLink(int index, int l) {
        add(TextUtils.GetCommas(index, l));
    }

    public static void add(string str, Item _overrideItem) {
        addOverride(_overrideItem);
        add(str);
    }
    public static void add(string str) {
        DisplayDescription.Instance.AddToDescription(str, false);
    }
    #endregion

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