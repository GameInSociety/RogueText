using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Word
{
    //
    public string GetLocationPrep{

        get
        {
            string str = locationPrep;

            if (str.Contains("/DE"))
            {
                currentInfo.preposition = Preposition.De;
                str = str.Replace("/DE", "");
            }

            return str;
        }
    }

    public void SetLocationPrep(string str)
    {
        locationPrep = str;
    }

    private string locationPrep = "";
    //

    public string text = "";
    public string[] text_parts;

    public bool hasBeenInteractedWith = false;

    // GENRE
    public Genre genre;
    public Number defaultNumber = Number.Singular;

    // ADJECTIVE
    // more logic if word have adjectives, and not items or location or tiles. fuck s
    private Adjective adjective;
    public string adjectiveType;

    public Info currentInfo;

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
        De,
        A,
    }

    #region genre
    public enum Genre
    {
        None,

        Masculine,
        Feminine,
    }

    public void UpdateGenre(string str)
    {
        switch (str)
        {
            case "ms":
                genre = Genre.Masculine;
                break;
            case "fs":
                genre = Genre.Feminine;
                break;
            case "mp":
                genre = Genre.Masculine;
                defaultNumber = Number.Plural;
                break;
            case "fp":
                genre = Genre.Feminine;
                defaultNumber = Number.Plural;
                break;
            default:
                Debug.LogError("pas trouvé de genre pour l'item : " + text + " ( content : " + str + ")");
                break;
        }
    }
    #endregion
    
    public string GetArticle()
    {
        // l'espace est inclu dans l'article par qu'il 
        if ( defaultNumber == Number.Plural)
        {
            currentInfo.plural = true;
        }
        

        // NUMBER
        if (currentInfo.plural)
        {
            switch (currentInfo.preposition)
            {
                case Preposition.None:
                    return "des ";
                case Preposition.De:
                    if (StartsWithVowel())
                    {
                        return "d'";
                    }
                    else
                    {
                        if (currentInfo.defined)
                            return "des ";
                        else
                            return "de ";
                    }
                case Preposition.A:
                    return "aux ";
                default:
                    break;
            }

        }

        if (currentInfo.defined)
        {
            if (StartsWithVowel())
            {
                switch (currentInfo.preposition)
                {
                    case Preposition.None:
                        return "l'";
                    case Preposition.De:
                        return "de l'";
                    case Preposition.A:
                        return "à l'";
                    default:
                        break;
                }

            }
            else
            {
                switch (genre)
                {
                    case Genre.Masculine:

                        switch (currentInfo.preposition)
                        {
                            case Preposition.None:
                                return "le ";
                            case Preposition.De:
                                return "du ";
                            case Preposition.A:
                                return "au ";
                            default:
                                break;
                        }
                        break;

                    case Genre.Feminine:

                        switch (currentInfo.preposition)
                        {
                            case Preposition.None:
                                return "la ";
                            case Preposition.De:
                                return "de la ";
                            case Preposition.A:
                                return "à la ";
                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }

        }
        else
        {

            switch (genre)
            {
                case Genre.Masculine:

                    switch (currentInfo.preposition)
                    {
                        case Preposition.None:
                            return "un ";
                        case Preposition.De:
                            if (StartsWithVowel())
                                return "de l'";
                            else
                                return "du ";
                        case Preposition.A:
                            return "à un ";
                        default:
                            break;
                    }

                    break;
                case Genre.Feminine:

                    switch (currentInfo.preposition)
                    {
                        case Preposition.None:
                            return "une ";
                        case Preposition.De:
                            return "d'une ";
                        case Preposition.A:
                            return "à une ";
                        default:
                            break;
                    }

                    break;
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
            text[0] == 'u'
            ||
            text[0] == 'é')
        {
            return true;
        }

        return false;
    }

    #region getter
    public struct Info
    {
        public bool article;
        public bool defined;
        public bool adjective;
        public bool other;
        public bool location;
        public bool plural;
        public Preposition preposition;
    }

    public string GetContent(string str)
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

    //public string GetContent(ContentType contentType, Definition _definition, Preposition _preposition, Number _number, bool location)
    private string GetContent(Info info)
    {
        // set current params
        currentInfo = info;
        //
        
        /// WORD
        string str = text.ToLower();

        // set number
        if (currentInfo.plural)
        {
            str = GetPlural();
        }

        // ?
        if (str.Contains("("))
        {
            str = str.Remove(str.IndexOf("(") - 1);
        }

        if ( info.adjective)
        {
            // adjectif
            string adjective_str = GetAdjective.GetContent(genre, currentInfo.plural);

            if (GetAdjective.beforeWord)
            {
                str = adjective_str + " " + str;
            }
            else
            {
                str = str + " " + adjective_str;
            }
        }

        if ( info.other)
        {
            str = "autre " + str;
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

        return str;
    }
    #endregion

    public void SetText(string _text)
    {
        text = _text;
        text_parts = text.Split(' ');
    }

    public string GetPlural()
    {
        string plural = text.ToLower();

        plural += "s";

        return plural;
    }

    #region adjective
    public Adjective GetAdjective
    {
        get
        {
            if (adjective == null)
            {
                SetAdjective(Adjective.GetRandom(adjectiveType));
            }

            return adjective;
        }
    }

    public void SetAdjective ( Adjective adj)
    {
        adjective = adj;
    }
    #endregion

    public bool Compare(string input_part)
    {
        return
            (text.StartsWith(input_part))
            ||
            (GetPlural().StartsWith(input_part));
    }
}