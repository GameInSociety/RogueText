using UnityEngine;

[System.Serializable]
public class Word {
    // data //
    public string _text = "";
    public string preposition = "";
    public Number defaultNumber = Number.Singular;
    public Number currentNumber;
    public bool defined = false;
    public bool main = false;

    public Word() {

    }

    public Word(Word copy) {
        this.preposition = copy.preposition;
        this._text = copy._text;
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
                defaultNumber = Number.Singular;
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

    public void SetText(string str) {
        if (str.StartsWith('[')) {
            TextUtils.Extract('[', str, out _text);
            Debug.Log($"changed {str} to {_text}");
        } else {
            _text = str;
        }

    }


    public string GetText {
        get {
            return _text;
        }
    }

    public string getText(Number num = Number.Singular) {
        return num == Number.Plural ? getPlural() : GetText;
    }
    public string getPlural() {
        var plural = GetText.ToLower();

        if (!GetText.EndsWith("s"))
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