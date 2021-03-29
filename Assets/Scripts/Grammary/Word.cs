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
                currentPreposition = Preposition.De;
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

    // ADJECTIVE
    // more logic if word have adjectives, and not items or location or tiles. fuck s
    public List<Adjective> adjectives = new List<Adjective>();
    public Adjective.Type adjectiveType;

    public Number currentNumber = Number.None;
    public Definition currentDefinition = Definition.Undefined;
    public Preposition currentPreposition = Preposition.None;

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
            default:
                Debug.LogError("pas trouvé de genre pour l'item : " + text + " ( content : " + str + ")");
                break;
        }
    }
    #endregion

    #region adjectives
    public void SetRandomAdjectives()
    {
        int count = 1;

        for (int i = 0; i < count; i++)
        {
            Adjective newAdjective = Adjective.GetRandom(adjectiveType);
            adjectives.Add(newAdjective);
        }
    }
    public void UpdateAdjectiveType(string str)
    {
        for (int i = 0; i < (int)Adjective.Type.Any; i++)
        {
            Adjective.Type a = (Adjective.Type)i;
            if (a.ToString() == str)
            {
                adjectiveType = a;
                return;
            }

        }

        Debug.LogError("pas trouvé adj type pour " + str);

    }
    #endregion

    public string GetArticle()
    {
        // l'espace est inclu dans l'article par qu'il 

        // NUMBER
        if (currentNumber == Number.Plural)
        {
            switch (currentPreposition)
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
                        if (currentDefinition == Definition.Defined)
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

        if (currentDefinition == Definition.Defined)
        {
            if (StartsWithVowel())
            {
                switch (currentPreposition)
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

                        switch (currentPreposition)
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

                        switch (currentPreposition)
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

                    switch (currentPreposition)
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

                    switch (currentPreposition)
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
    public string GetContentTest(string key_phrase)
    {
        string[] parts = key_phrase.Split(' ');

        switch (key_phrase)
        {
            case "route":
                return GetContent(ContentType.JustWord, Definition.Undefined, Preposition.None, Number.Singular);
            case "routes":
                return GetContent(ContentType.JustWord, Definition.Undefined, Preposition.None, Number.Plural);
            case "la route":
                return GetContent(ContentType.ArticleAndWord, Definition.Defined, Preposition.None, Number.Singular);
            case "les routes":
                return GetContent(ContentType.ArticleAndWord, Definition.Defined, Preposition.None, Number.Plural);
            case "des routes":
                return GetContent(ContentType.ArticleAndWord, Definition.Undefined, Preposition.De, Number.Plural);
            case "à la route":
                return GetContent(ContentType.ArticleAndWord, Definition.Defined, Preposition.A, Number.Singular);
            case "aux routes":
                return GetContent(ContentType.ArticleAndWord, Definition.Undefined, Preposition.A, Number.Plural);
            case "de la route":
                return GetContent(ContentType.ArticleAndWord, Definition.Defined, Preposition.De, Number.Singular);
            case "sur la route":
                return GetContent(ContentType.ArticleAndWord, Definition.Defined, Preposition.None, Number.Singular, true);
            case "sur une route":
                return GetContent(ContentType.ArticleAndWord, Definition.Undefined, Preposition.None, Number.Singular, true);
            case "sur les routes":
                return GetContent(ContentType.ArticleAndWord, Definition.Defined, Preposition.None, Number.Singular, true);
            case "sur des routes":
                return GetContent(ContentType.ArticleAndWord, Definition.Undefined, Preposition.None, Number.Singular, true);
            default:
                return "/// " + key_phrase + "///";
        }
    }
    public string GetContent(string item_key)
    {
        string[] parts = item_key.Split('/');

        // CONTENT TYPE
        ContentType contentType = ContentType.ArticleAndWord;

        switch (parts[0])
        {
            case "FULL":
                contentType = ContentType.FullGroup;
                break;
            case "JW":
                contentType = ContentType.JustWord;
                break;
            case "AOW":
                contentType = ContentType.ArticleOtherAndWord;
                break;
            case "AW":
                contentType = ContentType.ArticleAndWord;
                break;
        }
        //

        // DEFINITION
        Definition definition = Definition.Defined;

        switch (parts[1])
        {
            case "DEF":
                definition = Definition.Defined;
                break;
            case "UNDEF":
                definition = Definition.Undefined;
                break;
        }
        //

        // PREPOSITION
        Preposition preposition = Preposition.None;

        switch (parts[2])
        {
            case "A":
                preposition = Preposition.A;
                break;
            case "DE":
                preposition = Preposition.De;
                break;
            case "NONE":
                preposition = Preposition.None;
                break;
        }

        // NUMBER
        Number number = Number.None;

        switch (parts[3])
        {
            case "SING":
                number = Number.Singular;
                break;
            case "PLUR":
                number = Number.Plural;
                break;
        }
        //

        bool hasLocation = false;
        if ( parts.Length >= 5)
        {
            hasLocation = true;
        }
        else
        {

        }

        return GetContent(contentType, definition, preposition, number, hasLocation);
    }
    public string GetPlural()
    {
        string plural = text.ToLower();

        plural += "s";

        return plural;
    }
    public string GetContent(ContentType contentType, Definition _definition, Preposition _preposition, Number _number)
    {
        return GetContent(contentType, _definition, _preposition, _number, false);
    }

    public string GetContent(ContentType contentType, Definition _definition, Preposition _preposition, Number _number, bool location)
    {
        // set current params
        currentDefinition = _definition;
        currentPreposition = _preposition;
        currentNumber = _number;
        //

        
        /// WORD
        string word_str = text.ToLower();

        // set number
        if (currentNumber == Number.Plural)
        {
            word_str = GetPlural();
        }

        // ?
        if (word_str.Contains("("))
        {
            word_str = word_str.Remove(word_str.IndexOf("(") - 1);
        }

        word_str = "<color=lime>" + word_str + "</color>";

        /// ARTICLE
        string article = "";

        if (location)
        {
            string loc = GetLocationPrep;
            article = loc + " " + GetArticle();
        }
        else
        {
            article = GetArticle();
        }
        /// ARTICLE

        /// ADJECTIVES
        if (!Tile.SameAsPrevious())
        {

        }
        /// ADJECTIVES

        /// STRUCTURE
        switch (contentType)
        {
            case ContentType.JustWord:
                return word_str;
            case ContentType.ArticleAndWord:
                return article + word_str;
            case ContentType.FullGroup:

                Adjective adjective = adjectives[0];
                string adjective_str = adjectives[0].GetContent(genre, currentNumber);

                adjective_str = "<color=green>" + adjective_str + "</color>";

                // espace inclu dans l'article pour les 'd'" etc...
                if (adjective.beforeWord)
                {
                    return article + adjective_str + " " + word_str;
                }
                else
                {
                    return article + word_str + " " + adjective_str;
                }
            case ContentType.ArticleOtherAndWord:
                return article + "autre " + word_str;
            default:
                return "error : no word content type";
        }
        // STRUCTURE
    }
    #endregion

    public void SetText(string _text)
    {
        text = _text;
        text_parts = text.Split(' ');
    }
}