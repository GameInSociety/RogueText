using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public string DescribePropsToString() {
        if (describeProps.Count == 0) return " ";
        string text = Property.GetDescription(describeProps, true);
        if (string.IsNullOrEmpty(text)) return " ";
        return $" {text} ";
    }

    public string NestedPropsToString() {
        if (nestedProps.Count == 0) return " ";
        string text = Property.GetDescription(nestedProps, true);
        if (string.IsNullOrEmpty(text)) return " ";
        return $" {text} ";
    }

    public string ItemsToString() {

        var fItem = items.First();

        string dog = items.Count > 1 ? "dogs" : "dog";
        string article = defined ? "the" : (items.Count > 1 ? Phrase.Part.GetRandom("article_mult") : "a");
        var parents = fItem.GetParents();
        var refItem = parents.Find(x => x.HasProp("refer"));
        if (refItem != null)
            article = refItem.GetProp("refer").GetTextValue();

        var text = $"{article}{NestedPropsToString()}{items.First().GetText($"{dog}")}{DescribePropsToString()}";
        text = text.Trim(' ');
        return text;
    }
}