using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Word
{
    // data //
    public string locationPrep = "";
    public string text = "";
    public Number defaultNumber = Number.Singular;
    public bool defaultDefined = false;

    // ADJECTIVE
    // more logic if word have adjectives, and not items or location or tiles. fuck s
    private Adjective adjective;

    public Info currentInfo;

    public Word()
    {

    }

    public Word (Word copy)
    {
        this.locationPrep = copy.locationPrep;
        this.text = copy.text;
        this.defaultNumber = copy.defaultNumber;
        this.currentInfo = copy.currentInfo;
    }
    
    #region location prep
    public string GetLocationPrep
    {

        get
        {
            string str = locationPrep;

            return str;
        }
    }

    public void SetLocationPrep(string str)
    {
        locationPrep = str;
    }
    #endregion

    #region genre
    public void UpdateNumber(string str)
    {
        string[] parts = str.Split('\n');

        switch (parts[0])
        {
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

        if ( parts.Length > 1)
        {
            if (parts[1] == "d")
            {
                defaultDefined = true;
            }
        }
    }
    #endregion

    #region article
    public string GetArticle()
    {
        // l'espace est inclu dans l'article par qu'il 
        if (defaultNumber == Number.Plural)
        {
            currentInfo.plural = true;
        }

        if (currentInfo.amount > 1)
        {
            currentInfo.plural = true;
            return "some ";
        }

        // NUMBER
        if (currentInfo.plural)
        {
            switch (currentInfo.preposition)
            {
                case Preposition.None:

                    if (currentInfo.defined)
                    {
                        return "the ";
                    }
                    else
                    {
                        return "some ";
                    }
                        

                case Preposition.Of:
                    // there's no water
                    if (currentInfo.defined)
                        return "the ";
                    else
                        return "";
                case Preposition.To:
                    return "to the";
                default:
                    break;
            }

        }

        if (currentInfo.defined)
        {
            switch (currentInfo.preposition)
            {
                case Preposition.None:
                    return "the ";
                case Preposition.Of:
                    return "of ";
                case Preposition.To:
                    return "to ";
                default:
                    break;
            }

        }
        else
        {
            switch (currentInfo.preposition)
            {
                case Preposition.None:
                    if (StartsWithVowel())
                    {
                        return "an ";
                    }
                    else
                    {
                        return "a ";
                    }
                case Preposition.Of:
                    return "of the'";
                case Preposition.To:
                    return "to a";
                default:
                    break;
            }

        }

        return "pas trouvé d'aricle";
    }

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
    #endregion

    #region getter
    public struct Info
    {
        public bool article;
        public bool defined;
        public bool adjective;
        public bool other;
        public bool location;
        public bool plural;
        public int amount;
        public Preposition preposition;
    }

    /// <summary>
    /// FORME : THE GOOD DOG
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string GetInfo(string str)
    {
        //Info currentInfo = this.currentInfo;

        // article
        if (str.Contains("the "))
        {
            currentInfo.article = true;
            currentInfo.defined = true;
        }

        if (str.Contains("a ") || str.Contains("some "))
        {
            currentInfo.article = true;
            currentInfo.defined = false;
        }

        // PLURAL
        if (str.Contains("dogs"))
        {
            currentInfo.plural = true;
        }

        if (str.Contains("to "))
        {
            currentInfo.article = true;
            currentInfo.preposition = Preposition.To;
        }

        if (str.Contains("of "))
        {
            currentInfo.article = true;
            currentInfo.preposition = Preposition.Of;
            currentInfo.defined = true;
        }

        if (str.Contains("other "))
        {
            currentInfo.other = true;
        }

        if (str.Contains("good "))
        {
            currentInfo.adjective = true;
        }

        if (str.Contains("on "))
        {
            currentInfo.article = true;
            currentInfo.location = true;
        }

        if (defaultDefined)
        {
            currentInfo.defined = true;
        }

        /*Debug.Log("info input :");
        Debug.Log("article " + info.article);
        Debug.Log("location " + info.location);
        Debug.Log("other " + info.other);
        Debug.Log("defined " + info.defined);
        
        Debug.Log("result : " + GetContent(info));*/

        return GetContent(currentInfo);
    }

    //public string GetContent(ContentType contentType, Definition _definition, Preposition _preposition, Number _number, bool location)
    private string GetContent(Info info)
    {
        // set current params
        currentInfo = info;
        //

        /// WORD
        string str = text.ToLower();

        // set number
        if (currentInfo.plural || currentInfo.amount > 1)
        {
            str = GetPlural();
        }

        if (info.adjective)
        {
            // adjective
            string adjective_str = GetAdjective.GetContent(currentInfo.plural);
            str = adjective_str + " " + str;
        }

        if (info.other)
        {
            str = "other " + str;
        }

        /// ARTICLE
        string article = "";

        /// ARTICLE
        if (info.article)
        {
            if (info.location)
            {
                string loc = GetLocationPrep;
                article = loc + " " + GetArticle();
            }
            else
            {
                article = GetArticle();
            }

            str = article + str;
        }

        // reset info
        currentInfo = new Info();

        if (DebugManager.Instance.colorWords)
        {
            return "<color=green>" + str + "</color>";
        }
        else
        {
            return str;
        }
        //return str;
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

    #region adjective
    public bool HasAdjective()
    {
        return adjective != null;
    }
    public Adjective GetAdjective
    {
        get
        {
            if (adjective == null)
            {
                SetAdjective(Adjective.GetRandom());
            }

            return adjective;
        }
    }

    public void SetAdjective(Adjective adj)
    {
        adjective = adj;
    }
    #endregion

    public bool Compare(string input_part)
    {
        return
            (text.StartsWith(input_part));
    }

    #region enums
    public enum Number
    {
        None,

        Singular,
        Plural,
    }

    public enum ContentType
    {
        JustWord,
        ArticleAndWord,
        FullGroup,
        ArticleOtherAndWord,
    }

    public enum Definition
    {
        Defined,
        Undefined
    }

    public enum Preposition
    {
        None,
        Of,
        To,
    }
    #endregion
}



// OLD FRENCH PART
/*
 * public string GetContent(string str)
    {

        Info info = new Info();

        // article
        if (str.Contains("le") || str.Contains("les"))
        {
            info.article = true;
            info.defined = true;
        }

        if (str.Contains("un") || str.Contains("des"))
        {
            info.article = true;
            info.defined = false;
        }

        if (str.Contains("chiens"))
        {
            info.plural = true;
        }

        if (str.Contains("au") || str.Contains("aux"))
        {
            info.article = true;
            info.preposition = Preposition.A;
        }

        if (str.Contains("du") || str.Contains("des"))
        {
            info.article = true;
            info.preposition = Preposition.De;
            info.defined = true;
        }

        if (str.Contains("de") || str.Contains("des"))
        {
            info.article = true;
            info.preposition = Preposition.De;
            info.defined = false;
        }

        if (str.Contains("autre"))
        {
            info.other = true;
        }

        if (str.Contains("sage"))
        {
            info.adjective = true;
        }

        if (str.Contains("sur"))
        {
            info.article = true;
            info.location = true;
        }

        return GetContent(info);
    }
*/