using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Word {
    // data //
    public string text = "";
    public string preposition = "";
    public Number defaultNumber;
    public bool defined = false;

    public Word() {

    }

    public Word(Word copy) {
        this.preposition = copy.preposition;
        this.text = copy.text;
        this.defaultNumber = copy.defaultNumber;
        this.defined = copy.defined;
    }

    #region number
    public void UpdateNumber(string str) {
        var parts = str.Split('\n');

        switch (parts[0]) {
            case "s":
                defaultNumber = Number.Singular;
                break;
            case "p":
                defaultNumber = Number.Plural;
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

    public static bool StartsWithVowel(string str) {
        if (
            str[0] == 'a'
            ||
            str[0] == 'e'
            ||
            str[0] == 'i'
            ||
            str[0] == 'o'
            ||
            str[0] == 'u') {
            return true;
        }

        return false;
    }

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
        Plural,
        Singular,

        Any,
        None,
    }
    #endregion
}