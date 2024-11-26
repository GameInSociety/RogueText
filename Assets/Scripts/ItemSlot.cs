using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;
using static UnityEngine.ParticleSystem;

/// <summary>
/// 
/// </summary>
[System.Serializable]
public class ItemSlot {
    // an item slot is items grouped by property TYPE ( property names : (material:wooden, orientation: left, size:small etc...)

    /// <summary>
    /// ??
    /// </summary>
    public string key;

    /// <summary>
    /// Index of the item
    /// </summary>
    public int dataIndex;

    /// <summary>
    /// The / A
    /// </summary>
    public bool definite;

    /// <summary>
    /// Described Items
    /// </summary>
    public List<Item> items = new List<Item>();

    /// <summary>
    /// Described Properties
    /// </summary>
    public List<Property> props = new List<Property>();

    /// <summary>
    /// The target grammar of the slot
    /// </summary>
    private Property grammar;

    public ItemSlot (string key, int dataIndex) {
        this.key = key;
        this.dataIndex = dataIndex;
    }

    public string GetText(bool debug = false) {

        grammar = RefItem.GetProp("grammar");

        string textprops = GetPropText(props) == "" ? "" : $" ({GetPropText(props)})";
        return $"{GetArticleText(GetItemText())} {GetItemText()}{textprops}";

        // get noun
        string item_name = GetItemText();
        // Get props that are not ajdectives ( "You're standing in a clearing" => you're standing in" )
        string props_hook = GetPropText(props.FindAll(x => x.HasPart("before article")), false);
        // Get props before word ( "Wooden door" );
        string props_before = GetPropText(props.FindAll(x => x.HasPart("before word")), true);
        // Get props after word ( "door is wooden" );
        string props_after = GetPropText(props.FindAll(x => !x.HasPart("before word")&& !x.HasPart("before article")), true);

        // Merge 
        props_before = string.IsNullOrEmpty(props_before) ? "" : $"{props_before} ";
        props_after= string.IsNullOrEmpty(props_after) ? "" : $" {props_after}";
        props_hook = string.IsNullOrEmpty(props_hook) ? "" : $"{props_hook} ";
        var phrase = $"{props_before}{item_name}{props_after}";
        phrase = phrase.Trim(' ');

        // get articles
        string article = GetArticleText(phrase);
        // put phrase together
        var text = $"{props_hook}{article} {phrase}";
        text = text.Trim(' ');

        DescriptionManager.Instance.describedItems.Add(RefItem.dataIndex);
        return text;
    }


    public string GetPropText(List<Property> props, bool showSetup = false) {
        if (props.Count == 0) return "";

        string text = "";

        // get text in properties
        var props_txt = props.Select(x => x.GetDisplayDescription()).ToList();
        props_txt.RemoveAll(x => string.IsNullOrEmpty(x));
        if (props_txt.Count == 0) return "";
        
        // add properties
        for (int i = 0; i < props_txt.Count; i++) {
            var setup = props[i].GetContent("description setup");
            if (setup == "start") {
                if ( items.First().dataIndex == Tile.GetCurrent.dataIndex) {
                    setup = "continue";
                }
            }

            // on verra
            if (setup == "is")
                setup = IsMultiple() ? "are" : "is";

            // trim for some reason
            props_txt[i]= props_txt[i].Trim(' ');

            // aggregate prop texts
            if (showSetup) {
                var b = showSetup && i > 0 && props[i - i].GetContent("description setup") == props[i].GetContent("description setup");
                string hook = b ? "" : $"{setup} ";
                text += $"{hook}{props_txt[i]}{TextUtils.GetCommas(i, props_txt.Count)}";
            } else {
                text += $"{props_txt[i]}{TextUtils.GetCommas(i, props_txt.Count)}";

            }
        }
        return text.Trim(' ');
    }

    bool IsMultiple() {
        if (grammar.HasPart("number"))
            return grammar.GetPart("number").content == "plural";
        return items.Count > 1;
    }

    string GetItemText() {
        // check self ref
        // example : the player => you
        if (grammar != null) {
            if (grammar.HasPart("self ref")) {
                var selfRef = grammar.GetContent("self ref");
                // THEN = first time described ( in current description ) and the other times
                // ex : (undead THEN he) => the undead walks, he hits you
                if (selfRef.Contains("THEN")) {
                    var split = selfRef.Split(" THEN ");
                    selfRef = FirstTimeDescribed() ? split[0] : split[1];
                }
                // X = nothing
                if (selfRef == "X")
                    return "";
                // normal = the default name ( ex:undead )
                if (selfRef != "normal")
                    return selfRef;
            }
        }

        string dog = IsMultiple() ? "dogs" : "dog";
        return items.First().GetText($"{dog}");
    }

    // ???
    bool FirstTimeDescribed() {
        return !DescriptionManager.Instance.describedItems.Contains(RefItem.dataIndex);
    }

    /// <summary>
    /// Getting the main item of the slots
    /// </summary>
    private Item RefItem {
        get {
            return items.First();
        }
    }

    private string GetArticleText(string text) {
        var article = "";
        var grammar = RefItem.GetProp("grammar");
        if (grammar.HasPart("article")) {
            article = grammar.GetContent("article");
            if (article.Contains(" THEN ")) {
                var split = article.Split(" THEN ");
                if (FirstTimeDescribed()) {
                    // first time
                    article = split[0];
                } else {
                    // second time
                    article = split[1];
                }
            }

            if (article == "X") {
                return "";
            }
            if (article != "normal") {
                return article;
            }
        }

        if (grammar.HasPart("definite"))
            definite = true;

        return definite ? "the" : (IsMultiple() ? Phrase.Part.GetRandom("article_mult") : (StartWithVowel(text)?"an":"a"));
    }

    private bool StartWithVowel(string text) {
        char[] vowels = new char[] {
            'a','e','i','o','u'
        };
        int i = text.Trim(' ')  .IndexOfAny(vowels);    
        return i == 0;
    }
}