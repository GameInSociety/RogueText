using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Word {
    // data //
    public string text = "";
    public string locationPrep = "";
    public Number number;
    public bool defined = false;

    public Word() {

    }

    public Word(Word copy) {
        this.locationPrep = copy.locationPrep;
        this.text = copy.text;
        this.number = copy.number;
        this.defined = copy.defined;
    }

    #region number
    public void UpdateNumber(string str) {
        var parts = str.Split('\n');

        switch (parts[0]) {
            case "s":
                number = Number.Singular;
                break;
            case "p":
                number = Number.Plural;
                break;
            default:
                Debug.LogError("pas trouvé de nombre pour l'item : " + text + " ( content : " + str + ")");
                break;
        }

        if (parts.Length > 1) {
            if (parts[1] == "d") {
                defined = true;
            }
        }
    }
    #endregion

    public bool startWithVowel() {
        if (
            text[0] == 'a'
            ||
            text[0] == 'e'
            ||
            text[0] == 'i'
            ||
            text[0] == 'o'
            ||
            text[0] == 'u') {
            return true;
        }

        return false;
    }

    #region getter

    /// <summary>
    /// FORME : THE GOOD DOG
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string get(string str) {

        // location "on a dog" => in the armory
        var locBound = @$"\bon\b";
        if (Regex.IsMatch(text, locBound))
            str = str.Replace(locBound, locationPrep);

        if (defined) {
            // if the word is always defined ( north, left, sky etc...)
            var articleBound = @$"\ba\b";
            str = str.Replace(articleBound, "the");
        } else {
            // a dog => some mittens
            if ( number == Number.Plural) {
                var articleBound = @$"\ba\b";
                str = Regex.Replace(str, articleBound, "some");
            } else {
                // a dog = an armory
                if (startWithVowel()) {
                    var articleBound = @$"\ba\b";
                    str = Regex.Replace(str, articleBound, "an");
                }
            }
        }

        str = str.Replace("dog", text);

        if (DebugManager.Instance.colorWords) {
            return "<color=green>" + str + "</color>";
        } else {
            return str;
        }
    }
    #endregion

    public void SetText(string _text) {
        text = _text;
    }

    public string getText(Number num = Number.Singular) {
        return num == Number.Plural ? getPlural() : text;
    }
    public string getPlural() {
        var plural = text.ToLower();

        if (!text.EndsWith("s"))
            plural += "s";

        return plural;
    }

    #region enums
    public enum Number {
        Singular,
        Plural,

        Any,
        None,
    }
    #endregion
}