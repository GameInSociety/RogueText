using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[System.Serializable]
public class ItemSlot {
    // an item slot is items grouped by property TYPE ( property names : (material:wooden, orientation: left, size:small etc...)

    public string key;
    public bool definite;
    public bool overall;
    public List<Item> items = new List<Item>();
    public List<Property> props = new List<Property>();
    Property grammar;

    public ItemSlot (string key) {
        this.key = key;
    }

    public string Describe(bool debug = false) {

        grammar = RefItem.GetProp("grammar");

        // get noun
        var phrase = $"[{GetName()}] ({GetProps(props, true)})";
        phrase = phrase.Trim(' ');

        // get articles
        string article = GetArticle(phrase);
        // put phrase together
        var text = $"{article} {phrase}";
        text = text.Trim(' ');

        ItemDescription.describedItems.Add(RefItem.dataIndex);
        return text;
    }


    public string GetProps(List<Property> props, bool showHook = false) {
        if (props.Count == 0) return " ";

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
            if (showHook) {
                var b = showHook && i > 0 && props[i - i].GetContent("description setup") == props[i].GetContent("description setup");
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

    string GetName() {
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

    bool FirstTimeDescribed() {
        return !ItemDescription.describedItems.Contains(RefItem.dataIndex);
    }
    Item RefItem {
        get {
            return items.First();
        }
    }

    string GetArticle(string text) {
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

        return definite ? "the" : (IsMultiple() ? Phrase.Part.GetRandom("article_mult") : (startWithVowel(text)?"an":"a"));
    }

    bool startWithVowel(string text) {
        char[] vowels = new char[] {
            'a','e','i','o','u'
        };
        int i = text.Trim(' ')  .IndexOfAny(vowels);    
        return i == 0;
    }
}