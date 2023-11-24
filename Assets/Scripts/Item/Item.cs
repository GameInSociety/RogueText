using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using UnityEditor;
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
    public List<Item> GetChildItems(string propertyFilter, string propertyValue = "") {
        
        var filteredItems = mChildItems.FindAll(x => x.HasProp(propertyFilter));
        if (!string.IsNullOrEmpty(propertyValue))
            filteredItems.RemoveAll(x => x.GetProp(propertyFilter).GetPart("value").text != propertyValue);

        return filteredItems;
    }
    public Item GetChildItem(string propertyFilter, string propertyValue = "") {
        var item = mChildItems.Find(x => x.HasProp(propertyFilter));

        if (item != null && !string.IsNullOrEmpty(propertyValue)) {
            if (item.GetProp(propertyFilter).GetTextValue() == propertyValue)
                return item;
            return null;
        }

        return item;
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

        if ( weightProp != null && oWeightProp != null)
            weightProp.SetValue(oWeightProp.GetNumValue());

        item.SetProp($"contained / search:{debug_name}");
    }

    // remove item
    public void RemoveFromTile() {
        var tmp = Tile.GetCurrent.GetChildItems(4);
        foreach (var item in tmp) {
            if (item.hasItem(this)) {
                Debug.Log(item.debug_name + " HasPart item : " + debug_name);
                item.RemoveItem(this);
                break;
            }
        }
    }
    public void RemoveItem(Item item) {
        if (!mChildItems.Contains(item)) {
            Debug.LogError($"{debug_name} doesn't contain {item.debug_name}");
        }
        mChildItems.Remove(item);
    }
    public Item GetItem(string itemName) {
        if (mChildItems == null)
            return null;

        var tmpItem = mChildItems.Find(x => x.HasWord(itemName));

        return tmpItem;
    }

    public bool hasItem(string itemName) {
        if (mChildItems == null)
            return false;
        return mChildItems.Find(x => x.GetWord().text == itemName) != null;
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
        return GetData().words[wordIndex];
    }
    public bool HasWord(string _text) {
        wordIndex = GetData().words.FindIndex(x => x.text == _text);
        return wordIndex >= 0;
    }
    public bool IsContainedInText(string _text) {
        Word.Number num = Word.Number.None;
        return GetIndexInText(_text, out num) >= 0;
    }
    public int GetIndexInText(string text, out Word.Number num) {
        num = Word.Number.None;
        foreach (var word in GetData().words) {
            for (int i = 0; i < 2; i++) {
                num = (Word.Number)i;
                string _word = word.getText(num);
                if (Regex.IsMatch(text, @$"\b{_word}\b"))
                    return text.IndexOf(_word);
            }
        }

        foreach (var prop in GetVisibleProps(10)) {
            if (Regex.IsMatch(text, @$"\b{prop.GetDescription()}\b")){
                Debug.Log($"found item {debug_name} with mw_prop description {prop.GetDescription()}");
                return text.IndexOf(prop.GetDescription());
            }

            var searchProp = prop.GetPart("search");
            if (searchProp != null && Regex.IsMatch(text, @$"\b{searchProp.text}\b")) {
                Debug.Log($"found item {debug_name} with mw_prop search input {searchProp.text}");
                return text.IndexOf(searchProp.text);
            }
        }

        num = Word.Number.None;
        return -1;
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
    public bool CheckPropsInText(string text, out Property prop) {
        if (!HasProps()) {
            prop = null;
            return false;
        }
        prop = props.Find(x=> x.HasPart("search") && text.Contains(x.GetPart("search").text));
        if (prop != null) return true;
        prop = props.Find(x => text.Contains(x.GetDescription()));
        if(prop!= null) return true;

        return false;
    }
    public bool HasVisibleProps() {
        return GetVisibleProps().Count > 0;
    }
    public List<Property> GetVisibleProps(int layer = 0) {
        var visibleProps = props.FindAll(x => x.HasPart("description") && !x.HasPart("layer"));
        if ( layer > 0) {
            var layeredProps = props.FindAll(x => x.HasPart("layer") && x.GetNumValue("layer") <= layer);
            visibleProps.AddRange(layeredProps);
        }
        return visibleProps;
    }
    public Property GetVisibleProp(int i) {
        var visibleProps = GetVisibleProps();
        if (visibleProps.Count >= i)
            return visibleProps[i];

        return null;

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
        item.AddChildItem(this);
        AvailableItems.RemoveFromWorld(this);
    }
    #endregion



}