using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Item {

    public string debug_name;
    public int dataIndex;
    public ItemData GetData() { return ItemData.itemDatas[dataIndex]; }
    public int debug_randomID;

    public List<Property> props = new List<Property>();
    public List<Spec> specs;
    public int wordIndex = 0;
    [SerializeField]
    private List<Item> mChildItems;


    public virtual void Init() {
        debug_name = GetData().words[0].text; 
        foreach (var prop in GetData().properties) {
            props.Add(new Property(prop));
        }
    }

    #region specs
    public static bool AllSpecMatch(List<Item> items) {
        return items.TrueForAll(x => x.specMatch(items.First()));
    }
    public Spec specContainedIn(string str) {
        Spec targetSpec = specs.Find(x => str.Contains(x.displayValue));
        return targetSpec;
    }

    public Spec getKeyInfo(string str) {
        if ( specs == null)
            return null;
        return specs.Find(x => x.key == str || x.displayValue == str || x.searchValue == str);
    }

    public bool hasSpecWithKey(string key) {
        return getSpecWithKey(key) != null;
    }
    public Spec getSpec(int i) {
        return specs == null || i >= specs.Count ? null : specs[i];
    }
    public Spec getSpecWithKey(string key) {
        if (specs == null)
            return null;
        return specs.Find(x => x.searchValue == key);
    }
    public Item getItemWithSpec(string key) {
        return GetChildItems().Find(x => x.hasSpecWithKey(key));
    }
    public bool hasSpec(string spec) {
        return hasSpecs() && specs.Find(x => x.key == spec) != null;
    }
    public bool hasSpecs() {
        return specs != null;
    }
    public bool specMatch(Item it) {

        if (specs == null)
            return false;
        
        if ( it.specs.Count != specs.Count){
            return false;
        }

        // ça va jamais être égale parce que c'est pas des structs mais des objets

        for (int i = 0; i < it.specs.Count; i++) {
            if (it.specs[i].searchValue != specs[i].searchValue || it.specs[i].key != specs[i].key)
                return false;
        }
        return true;
    }
    public bool textHasSpecs(string text, out Spec spec) {
        if (!hasSpecs()) {
            spec = null;
            return false;
        }
        spec = specs.Find(x => text.Contains(x.searchValue));
        return spec != null;
    }
    public Spec setSpec(Spec spec) {
        if (specs == null)
            specs = new List<Spec>();
        specs.Add(spec);
        return spec;
    }
    public Spec setSpec(string display,string search = "", string key = "none", bool visible = true) {
        if (specs == null)
            specs = new List<Spec>();

        if (string.IsNullOrEmpty(search))
            search = display;
        if (string.IsNullOrEmpty(key))
            key = display;

        Spec spec = specs.Find(x => x.key != "none" && x.key == key);
        if ( spec == null) {
            spec = new Spec(display, search, key, visible);
            specs.Add(spec);
        } else {
            Debug.LogError($"{debug_name} already hasPart spec with key {key}");
            spec.displayValue = display;
            spec.searchValue = search;
            spec.key = key;
            spec.visible = visible;
        }
        return spec;
    }
    #endregion

    /// <summary>
    /// HANDLING OF CHILD ITEMS CONTAINED IN THIS ITEMS
    /// </summary>
    /// <returns></returns>

    #region contained items
    public bool HasChildItems() {
        return mChildItems != null && mChildItems.Count > 0;
    }


    public List<Item> GetChildItems() {
        return mChildItems;
    }
    public List<Item> GetChildItems(string propertyFilter, string propertyValue = "") {
        
        var filteredItems = mChildItems.FindAll(x => x.HasProp(propertyFilter));
        foreach (var item in filteredItems)
            Debug.Log($"found item with property {propertyFilter}");
        if (!string.IsNullOrEmpty(propertyValue))
            filteredItems.RemoveAll(x => x.GetProp(propertyFilter).getPart("value").text != propertyValue);

        return filteredItems;
    }
    public bool IsAChildItemOf(Item item) {
        return item.hasItem(this);
    }
    public virtual void WriteChildItems(bool describeContainers = false) {
        foreach (ItemGroup group in ItemGroup.GetGroups(GetChildItems()))
            TextManager.Write(group.GetDescription());
    }

    public void GenerateChildItems(Property prop = null) {
        
        // juste an action
        if (prop == null) {
            if (!HasProp("contents"))
                return;
            prop = GetProp("contents");
            props.Remove(prop);
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
                        Debug.LogError($"{debug_name} contents parse error on : {it_name} ({strs[0]}) ({strs[1]})");
                        Debug.LogError(ex.Message);
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
    #region item creation
    public Item CreateChildItem(string name) {
        var newItem = ItemData.Generate_Simple(name);
        return CreateChildItem(newItem);
    }
    public Item CreateChildItem(Item item) {
        if (mChildItems == null)
            mChildItems = new List<Item>();

        mChildItems.Add(item);

        if (item.HasProp("dif")) {
            //Debug.Log($"{data.name} needs to be differenciated, adding specs");
            Spec.Category cat = Spec.GetCat("color");
            string spec_str = cat.GetRandomSpec();
            Spec newSpec = item.setSpec(spec_str, spec_str, "main");
            //Debug.Log($"adding spec {newSpec.displayValue} to {data.name}");
        }


        item.setSpec($">{GetText("in a dog")}", debug_name, "container", false);

        // vraiment pas sûr que ça devrait être ici
        item.GenerateChildItems();

        return item;
    }

    // remove item
    public void RemoveFromTile() {
        var tmp = Tile.GetCurrent.getRecursive(4);
        foreach (var item in tmp) {
            if (item.hasItem(this)) {
                Debug.Log(item.debug_name + " hasPart item : " + debug_name);
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

    #region socket

    public string GetText(string prms) {

        // FORM
        // a special dog
        // preposition/article/specs/name

        // preposition "on a dog" => in the armory
        var prepBound = @$"\bon\b";
        if (Regex.IsMatch(GetWord().text, prepBound))
            prms = prms.Replace(prepBound, GetWord().preposition);

        // name + specs (getting before article to see if it starts with a vowel)
        var name_group = GetWord().text;
        var specBound = @$"\bspecial\b";
        if (hasSpecs() && Regex.IsMatch(prms, specBound)) {
            int specIndex = prms.IndexOf("special");
            prms = prms.Remove(specIndex , "special".Length + 1);
            var front_specs = specs.FindAll(x => !x.displayValue.StartsWith('>'));
            var back_specs = specs.FindAll(x => x.displayValue.StartsWith('>'));
            if (front_specs.Count > 0) name_group = name_group.Insert(0, $"{getSpecText(front_specs)} ");
            if (back_specs.Count > 0) name_group = name_group.Insert(name_group.Length, $" {getSpecText(back_specs)}");
        }

        // article
        if (GetWord().defined || HasProp("definite")) {
            var articleBound = @$"\ba\b";
            prms = Regex.Replace(prms, articleBound, "the");
        } else {
            // a dog => some mittens
            if (GetWord().defaultNumber == Word.Number.Plural) {
                var articleBound = @$"\ba\b";
                prms = Regex.Replace(prms, articleBound, "some");
            } else {
                // a dog = an armory
                if (Word.StartsWithVowel(name_group)) {
                    var articleBound = @$"\ba\b";
                    prms = Regex.Replace(prms, articleBound, "an");
                }
            }
        }

        // plural / singular : name group
        if (prms.Contains("dogs")) {
            prms = prms.Replace("dogs", GetWord().defaultNumber == Word.Number.Plural ? name_group : $"{name_group}s") ;
        } else {
            prms = prms.Replace("dog", name_group);
        }


        return prms;
    }
    public string getSpecText(List<Spec> specs) {
        if (specs == null)
            return "";
        string str = "";
        List<Spec> list = specs.FindAll(x=>x.visible);
        for (int i = 0; i < list.Count; i++) {
            string spectext = list[i].displayValue;
            if (spectext.StartsWith('>')) spectext = spectext.Remove(0, 1);
            str += $"{spectext}{TextUtils.GetLink(i, list.Count, false)}";
        }
        return str;
    }

    [System.Serializable]
    public class Group {
        public Item item;
        public int amount;
    }
    // description depth here
    public List<Item> getRecursive(int depth) {
        var items = new List<Item> { this };
        if (depth == 0 || mChildItems == null)
            return items;
        foreach (var it in mChildItems)
            items.AddRange(it.getRecursive(depth - 1));
        return items;
    }

    #endregion
    #region search
    #endregion

    #region search
    public Word GetWord() {
        return GetData().words[wordIndex];
    }
    public bool HasWord(string _text) {
        wordIndex = GetData().words.FindIndex(x => x.text == _text);
        return wordIndex >= 0;
    }
    public bool containedInText(string _text) {
        Word.Number num = Word.Number.None;
        return getIndexInText(_text, out num) >= 0;
    }
    public int getIndexInText(string text, out Word.Number num) {
        num = Word.Number.None;
        foreach (var word in GetData().words) {
            for (int i = 0; i < 2; i++) {
                num = (Word.Number)i;
                string _word = word.getText(num);
                if (Regex.IsMatch(text, @$"\b{_word}\b"))
                    return text.IndexOf(_word);
            }
        }
        num = Word.Number.None;
        return -1;
    }
    #endregion

    #region description
    public virtual void WriteDescription() {
        WriteDescription("it's", "a special dog");
    }
    public virtual void WriteDescription(string starter, string prms) {

        TextManager.Write($"{starter} {GetText(prms)}");

        Debug.Log("need new way of writing properties");

        if (HasChildItems())
            WriteChildItems(true);
    }
    #endregion

    #region properties
    // ADD
    public void AddProp(string name) {
        Property prop = new Property();
        prop.name = name;
        props.Add(prop);
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
        var property = props.Find(x => x.name == name);

        if (property == null) {
            Debug.LogError("property : " + name + " doesn't exist in item " + GetWord().text);
            return null;
        }

        return property;
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

    public void WriteProperties() {

        var enabledPropCount = GetProps().Count;
        if (enabledPropCount == 0)
            return;
        
        TextManager.add("it's ");
        for (var i = 0; i < enabledPropCount; i++) {
            var property = GetProps()[i];
            TextManager.add($"{property.GetDescription()} {TextUtils.GetLink(0, enabledPropCount)}");
        }
    }
    #endregion

    #region remove & destroy
    public static void Destroy(Item item) {
        WorldEvent.RemoveWorldEventsWithItem(item);
        foreach (var prop in item.props)
            prop.Destroy();
        AvailableItems.Get.removeFromWorld(item);
    }
    public void TransferTo(Item item) {
        AvailableItems.Get.removeFromWorld(this);
        item.CreateChildItem(this);
    }
    #endregion



}