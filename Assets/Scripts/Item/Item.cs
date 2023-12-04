using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

[System.Serializable]
public class Item {

    public string debug_name;
    public int dataIndex;
    public ItemData GetData() { return ItemData.itemDatas[dataIndex]; }
    public int debug_randomID;

    public List<Property> props = new List<Property>();
    public int wordIndex = 0;
    [SerializeField]
    private List<Item> mChildItems;


    public virtual void Init() {
        debug_name = GetData().words[0].text; 
        foreach (var prop in GetData().properties) {
            props.Add(new Property(prop));
        }
    }

    /// <summary>
    /// HANDLING OF CHILD ITEMS CONTAINED IN THIS ITEMS
    /// </summary>
    /// <returns></returns>

    #region child items
    public bool HasChildItems() {
        return mChildItems != null && mChildItems.Count > 0;
    }
    public List<Item> GetChildItems() {
        return mChildItems;
    }
    public List<Item> GetChildItemsWithProp(string propertyFilter, string propertyValue = "") {
        
        var filteredItems = mChildItems.FindAll(x => x.HasProp(propertyFilter));
        if (!string.IsNullOrEmpty(propertyValue))
            filteredItems.RemoveAll(x => x.GetProp(propertyFilter).GetPart("value").text != propertyValue);

        return filteredItems;
    }
    public List<Item> GetChildItems(int depth) {
        var items = new List<Item> { this };
        if (depth == 0 || mChildItems == null)
            return items;
        foreach (var it in mChildItems)
            items.AddRange(it.GetChildItems(depth - 1));
        return items;
    }
    public bool IsAChildItemOf(Item item) {
        return item.hasItem(this);
    }
    public void GenerateChildItems(Property prop = null) {
        
        // juste an action
        if (prop == null) {
            if (!HasProp("contents"))
                return;
            prop = GetProp("contents");
            if (!prop.enabled)
                return;
            prop.enabled = false;
        }

        if (prop.parts == null) {
            Debug.Log($"no prop parts for {debug_name}");
            return;
        }

        if (mChildItems == null)
            mChildItems = new List<Item>();

        foreach (var part in prop.parts) {
            string it_name = part.key;

            int amount = 1;
            int percent = 100;
            if (part.key == "main") {
                it_name = part.text;
            } else {
                if (part.text.Contains('*')) {
                    var strs = part.text.Split(" * ");
                    try {
                        amount = int.Parse(strs[0]);
                        percent = int.Parse(strs[1].Remove(strs[1].Length - 1));
                    } catch (Exception ex) {
                        Debug.LogError($"error on item generation :{debug_name}" +
                            $"\n{ex.Message}");
                        Debug.LogError($"{debug_name} mw_prop parse error on : {it_name} ({strs[0]}) ({strs[1]})");
                    }
                } else {
                    if (part.text.Contains('%'))
                        percent = int.Parse(part.text.Remove(part.text.Length - 1));
                    else
                        amount = int.Parse(part.text);
                }
            }

            for (var i = 0; i < amount; i++) {
                var f = UnityEngine.Random.value * 100f;
                if (f < percent) {
                    _ = CreateChildItem(it_name);
                }
            }
        }
    }
    #endregion

    #region child item creation
    public Item CreateChildItem(string name) {
        var newItem = ItemData.Generate_Simple(name);
        return CreateChildItem(newItem);
    }
    public Item CreateChildItem(Item item) {
        // vraiment pas sûr que ça devrait être ici
        item.GenerateChildItems();
        AddChildItem(item);
        return item;
    }

    public void AddChildItem(Item item) {

        if (mChildItems == null)
            mChildItems = new List<Item>();

        mChildItems.Add(item);

        var weightProp = GetProp("weight");
        var oWeightProp = item.GetProp("weight");

        if ( weightProp != null && oWeightProp != null) {
            int value = weightProp.GetNumValue();
            weightProp.SetValue(value + oWeightProp.GetNumValue());
        }

        item.SetProp($"contained / search:{debug_name}");
    }

    // remove item
    public void RemoveItem(Item item) {
        if (!mChildItems.Contains(item)) {
            Debug.LogError($"{debug_name} doesn't contain {item.debug_name}");
        }
        var weightProp = GetProp("weight");
        var oWeightProp = item.GetProp("weight");

        if (weightProp != null && oWeightProp != null) {
            int value = weightProp.GetNumValue();
            weightProp.SetValue(value - oWeightProp.GetNumValue());
        }
        mChildItems.Remove(item);
    }
    public Item GetItem(string itemName) {
        if (mChildItems == null)
            return null;

        var tmpItem = mChildItems.Find(x => x.HasWord(itemName));

        return tmpItem;
    }

    public bool hasItem(Item item) {
        return mChildItems != null && mChildItems.Contains(item);
    }

    public bool SimilarPropsAs(Item otherItem) {
        bool similarProps = false;
        if (GetVisibleProps().Count > 0 && otherItem.GetVisibleProps().Count > 0) {
            similarProps = GetVisibleProps()[0].GetDescription() == otherItem.GetVisibleProps()[0].GetDescription();
            if (similarProps) {
                Debug.Log($"{debug_name} is very similar to {otherItem.debug_name}");
            }
        }
        return otherItem.dataIndex == dataIndex == similarProps;
    }

    public bool sameTypeAs(Item otherItem) {
        if (otherItem == null)
            return false;
        return otherItem.GetData().index == GetData().index;
    }
    public bool exactSameAs(Item otherItem) {
        if (otherItem == null) {
            Debug.Log("no previous tile");
            return false;
        }

        return otherItem == this;
    }
    #endregion

    #region description

    public string GetText(string prms, int propLayer = 0) {

        // FORM
        // a special dog
        // preposition/article/specs/name

        // name : dog / dogs
        var text = GetWord().text;
        if (prms.Contains("dogs") && GetWord().defaultNumber != Word.Number.Plural)
            text = $"{text}s";

        // props : special dog
        var propskeybound = @$"\bspecial\b";
        if (Regex.IsMatch(prms, propskeybound)) {
            if (GetVisibleProps(propLayer).Count > 0) {
                // add describable props
                var before = GetVisibleProps(propLayer).FindAll(x => !x.HasPart("after word"));
                var after = GetVisibleProps(propLayer).FindAll(x => x.HasPart("after word"));
                if (before.Count > 0) text = text.Insert(0, $"{Property.GetDescription(before)} ");
                if (after.Count > 0) text = text.Insert(text.Length, $" {Property.GetDescription(after)}");
            }

        }

        // article
        var articlebound = @$"\ba\b";
        if (Regex.IsMatch(prms, articlebound)) {
            var article = "a";
            if (GetWord().defined || HasProp("definite")) {
                article = "the";
            } else if (GetWord().defaultNumber == Word.Number.Plural)
                article = "some";
            else if (Word.StartsWithVowel(text))
                article = "an";
            text = text.Insert(0, $"{article} ");
        } else if (Regex.IsMatch(prms, @$"\bthe\b")) {  
            text = text.Insert(0, $"the ");
        }

        // preposition "on a dog" => in the armory
        var prepBound = @$"\bon\b";
        if (Regex.IsMatch(prms, prepBound))
            text = text.Insert(0, $"{GetWord().preposition} ");


        return text;
    }

    public virtual void WriteDescription() {
        WriteDescription("it's", "a special dog");
    }
    public virtual void WriteDescription(string starter, string prms) {

        TextManager.Write($"{starter} {GetText(prms)}");

        if (HasChildItems())
            WriteChildItems();
    }
    public virtual void WriteChildItems() {
        TextManager.Write(ItemDescription.NewDescription(GetChildItems()));
    }
    #endregion

    #region word
    public Word GetWord() {
        return wordIndex < 0 ? GetData().words[0] : GetData().words[wordIndex];
    }
    public bool HasWord(string _text) {
        wordIndex = GetData().words.FindIndex(x => x.text == _text);
        return wordIndex >= 0;
    }
    public int GetIndexInText(string text, out Word.Number num) {
        num = Word.Number.None;
        var wIndex = 0;
        foreach (var word in GetData().words) {
            for (int i = 0; i < 2; i++) {
                num = (Word.Number)i;
                string _word = word.getText(num);
                if (Regex.IsMatch(text, @$"\b{_word}\b")) {
                    wordIndex = wIndex;
                    return text.IndexOf(_word);
                }
            }
            ++wIndex;
        }

        num = Word.Number.None;
        return -1;
    }
    public Property GetPropInText(string text, out int textIndex) {
        foreach (var prop in GetAllVisibleProps()) {
            var description = prop.GetDescription();
            if (Regex.IsMatch(text, @$"\b{description}\b")) {
                textIndex = text.IndexOf(description);
                return prop;
            }

            var searchProp = prop.GetPart("search");
            if (searchProp != null && Regex.IsMatch(text, @$"\b{searchProp.text}\b")) {
                textIndex = text.IndexOf(searchProp.text);
                return prop;
            }
        }
        textIndex = -1;
        return null;
    }
    #endregion

    #region properties
    /// <summary>
    /// CREATION
    /// </summary>
    /// <param name="prms"></param>
    /// <returns></returns>
    public Property SetProp(string prms) {

        var parts = prms.Split(" / ");
        var name = parts[0];

        var prop = GetProp(name);
        if (prop != null) {
            for (int i = 1; i < parts.Length; i++) {
                try {
                    var strs = parts[i].Split(':');
                    var part = prop.GetPart(strs[0]);
                    if (part == null)
                        prop.AddPart(strs[0], strs[1]);
                    else
                        part.text = strs[1];
                } catch (Exception e) {
                    Debug.LogError($"error when setting mw_prop {name} : part {parts[i]}");
                    Debug.LogError(e.Message);
                }
            }
            return prop;
        }

        prop = new Property();
        prop.name = name;

        for (int i = 1; i < parts.Length; i++) {
            var part = parts[i].Split(':');
            prop.AddPart(part[0], part[1]);
        }

        props.Add(prop);

        return prop;
    }
    //

    // DELETE
    public void DeleteProperty(string propertyName) {
        var property = GetProp(propertyName);
        if (property == null) {
            Debug.LogError("DeleteProperty : property " + propertyName + " does not exist in " + debug_name);
            return;
        }
        _ = props.Remove(property);
    }
    //

    // CHECK
    public bool HasProps() {
        return props.FindAll(x => x.enabled).Count > 0;
    }
    public bool HasVisibleProperties() {
        return props.FindAll(x => x.enabled).Count > 0;
    }
    public bool HasEnabledProperty(string name) {
        var property = props.Find(x => x.name == name && x.enabled);
        return property != null;
    }
    public bool HasProp(string name) {
        var property = props.Find(x => x.name == name);
        return property != null;
    }
    //

    // GET
    public List<Property> GetProps(string name, bool enabled = true) {
        return enabled ? props.FindAll(x => x.name == name && x.enabled) : props.FindAll(x => x.name == name);
    }
    public List<Property> GetProps(bool enabled = true) {
        return enabled ? props.FindAll(x => x.enabled) : props;
    }
    public Property GetProp(string name) {
        return props.Find(x => x.name == name);
    }
    public bool HasItemWithProp(string propertyName) {
        return GetChildItems().Find(x => x.HasProp(propertyName)) != null;
    }
    //

    // CHANGE
    public void EnableProperty(string propertyName) {
        var prop = props.Find(x => x.name == propertyName);
        if (prop == null) {
            Debug.LogError("ENABLE PROP : Could not find property " + propertyName);
            return;
        }

        prop.enabled = true;
        PropertyDescription.Add(this, prop);
    }

    public void DisableProperty(string propertyName) {
        var prop = props.Find(x => x.name == propertyName);
        if (prop == null) {
            Debug.LogError("DISABLE PROP : Could not find property " + propertyName);
            return;
        }

        prop.enabled = false;

        PropertyDescription.Add(this, prop);
    }
    #endregion

    #region visible props ?
    public bool HasVisibleProps() {
        return GetVisibleProps().Count > 0;
    }
    public List<Property> GetAllVisibleProps() {
        return props.FindAll(x => x.HasPart("description") || x.HasPart("search"));
    }
    public List<Property> GetVisibleProps(int layer =0) {
        var visibleProps = props.FindAll(x => x.HasPart("description") && !x.HasPart("layer") && !x.HasPart("key"));
        if (layer > 0) {
            var layeredProps = props.FindAll(x => x.HasPart("layer"));
            foreach (var prop in layeredProps) {
                int l = 0;
                if (int.TryParse(prop.GetPart("layer").text, out l) && l <= layer)
                    visibleProps.Add(prop);
            }
        }
        return visibleProps;
    }
    public Property GetVisibleProp(int i) {
        return GetVisibleProps()[i];

    }
    #endregion

    #region remove & destroy
    public static void Destroy(Item item) {
        WorldEvent.RemoveWorldEventsWithItem(item);
        foreach (var prop in item.props)
            prop.Destroy();
        AvailableItems.RemoveFromWorld(item);
    }
    public void TransferTo(Item item) {
        AvailableItems.RemoveFromWorld(this);
        item.AddChildItem(this);
    }
    #endregion



}