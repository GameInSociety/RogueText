using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Word
{
    // data //
    public string locationPrep = "";
    public string text = "";
    public Number number;
    public bool defined = false;

    public Word()
    {

    }

    public Word (Word copy)
    {
        this.locationPrep = copy.locationPrep;
        this.text = copy.text;
        this.number = copy.number;
        this.defined = copy.defined;
    }

    #region number
    public void UpdateNumber(string str)
    {
        string[] parts = str.Split('\n');

        switch (parts[0])
        {
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

        if ( parts.Length > 1)
        {
            if (parts[1] == "d")
            {
                defined = true;
            }
        }
    }
    #endregion

    public bool StartsWithVowel()
    {
        if (
            text[0] == 'a'
            ||
            text[0] == 'e'
            ||
            text[0] == 'i'
            ||
            text[0] == 'o'
            ||
            text[0] == 'u')
        {
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
    public string GetInfo(string str)
    {

        string locBound = @$"\bon\b";
        if (Regex.IsMatch(text, locBound))
        {
            str = str.Replace(locBound, locationPrep);
        }

        if (defined)
        {
            string articleBound = @$"\ba\b";
            str = str.Replace(articleBound, "the");
        }
        else
        {
            if (StartsWithVowel())
            {
                string articleBound = @$"\ba\b";
                str= Regex.Replace(str, articleBound, "an");
        }
        }

        str = str.Replace("dog", text);

        if (DebugManager.Instance.colorWords)
        {
            return "<color=green>" + str + "</color>";
        }
        else
        {
            return str;
        }
    }
    #endregion

    public void SetText(string _text)
    {
        text = _text;
    }

    public string GetPlural()
    {
        string plural = text.ToLower();

        if (!text.EndsWith("s"))
            plural += "s";

        return plural;
    }

    #region enums
    public enum Number
    {
        None,

        Singular,
        Plural,
    }
    #endregion
}