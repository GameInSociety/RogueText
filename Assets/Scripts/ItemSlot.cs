using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[System.Serializable]
public class ItemSlot {
    // an item slot is items grouped by property TYPE ( property names : (material:wooden, orientation: left, size:small etc...)

    public string key;
    public bool defined;
    public bool overall;
    public List<Item> items = new List<Item>();
    public List<Property> nestedProps = new List<Property>();
    public List<Property> describeProps = new List<Property>();

    public ItemSlot (string key) {
        this.key = key;
    }

    public string PropsToString(List<Property> props) {
        if (props.Count == 0) return " ";

        string text = "";

        // get text in properties
        var props_txt = props.Select(x => x.GetDisplayDescription()).ToList();
        props_txt.RemoveAll(x => string.IsNullOrEmpty(x));
        if (props_txt.Count == 0) return "";
        
        // add properties
        for (int i = 0; i < props_txt.Count; i++) {
            var currSetup = props[i].GetPart("description setup").content;

            // on verra
            if (currSetup == "is")
                currSetup = IsMultiple() ? "are" : "is";

            // aggregate prop texts
            var b = i > 0 && props[i - i].GetPart("description setup").content == props[i].GetPart("description setup").content;
            string hook = b ? "" : $"{currSetup} ";
            text += $"{hook}{props_txt[i]}{TextUtils.GetCommas(i, props_txt.Count)}";
        }
        if (string.IsNullOrEmpty(text)) return " ";
        return $" {text} ";
    }

    bool IsMultiple() {
        return items.Count > 1 || items.First().GetWord().defaultNumber == Word.Number.Plural;
    }

    public string ItemsToString() {

        

        // get noun
        string selfRef = GetSelfRef();
        var phrase = $"{PropsToString(nestedProps)}{selfRef}{PropsToString(describeProps)}";

        // get articles
        string article = GetArticle(phrase);
        // put phrase together
        var text = $"{article}{phrase}";
        text = text.Trim(' ');

        ItemDescription.describedItems.Add(RefItem.dataIndex);
        return text;
    }


    string GetSelfRef() {
        var grammar = RefItem.GetProp("grammar");
        if (grammar != null) {
            if (grammar.HasPart("self ref")) {
                var selfRef = grammar.GetPart("self ref").content;
                var split = selfRef.Split(" THEN ");
                if ( split.Length > 1) {
                    if (FirstTimeDescribed()) {
                        // first time
                        selfRef = split[0];
                    } else {
                        // second time
                        selfRef = split[1];
                    }
                }

                if (selfRef == "X")
                    return "";

                if (selfRef == "normal") {

                } else {
                    return selfRef;
                }
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
        if (grammar != null) {
            if (grammar.HasPart("article")) {
                article = grammar.GetPart("article").content;
                if (article.Contains(" THEN ")) {
                    var split = article.Split(" THEN ");
                    if ( FirstTimeDescribed() ) {
                        // first time
                        article = split[0];
                    } else {
                        // second time
                        article = split[1];
                    }
                }

                if (article == "X") {
                    return "";
                } if (article != "normal") {
                    return article;
                }
            }
        }

        return defined ? "the" : (IsMultiple() ? Phrase.Part.GetRandom("article_mult") : (startWithVowel(text)?"an":"a"));
    }

    bool startWithVowel(string text) {
        char[] vowels = new char[] {
            'a','e','i','o','u'
        };
        int i = text.Trim(' ')  .IndexOfAny(vowels);    
        return i == 0;
    }
}