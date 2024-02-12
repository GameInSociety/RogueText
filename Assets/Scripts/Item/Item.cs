using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Jobs;

[System.Serializable]
public class Item {

    public string debug_name;
    public int dataIndex;
    public ItemData GetData() { return ItemData.itemDatas[dataIndex]; }
    public int debug_Id;

    public Tile.Info TileInfo;

    public List<Property> props = new List<Property>();
    public int wordIndex = 0;
    [SerializeField]
    private Item mParentItem;
    [SerializeField]
    private List<Item> mChildItems;


    public virtual void Init() {
        debug_name = GetData().words[0].GetText;

        foreach (var seq in GetData().events) {
            // trying alternative method for events.
            var content = $"{seq.triggers[0]}\n{seq.seq}";
            WorldEvent.SubscribeItem(this, content);
        }
        foreach (var dataProp in GetData().properties) {
            AddProp(dataProp);
        }
    }

    public Property AddProp(string name) {
        return AddProp(Property.GetDataProp(name));
    }
    public Property AddProp(Property prop) {
        var newProp = new Property();
        newProp.Init(prop);
        props.Add(newProp);
        return newProp;
    }

    public void RemoveProp(string name) {
        var i = props.FindIndex(x => x.name == name);
        if (i < 0) {
            Debug.LogError($"removing prop from item {debug_name} : no props named {name} found");
            return;
        }
        props.RemoveAt(i);
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
            filteredItems.RemoveAll(x => x.GetProp(propertyFilter).GetPart("value").content != propertyValue);

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

        if (mChildItems == null)
            mChildItems = new List<Item>();

        foreach (var part in prop.parts) {
            string it_name = part.key;

            int amount = 1;
            int percent = 100;
            if (part.key == "main") {
                it_name = part.content;
            } else {
                if (part.content.Contains('*')) {
                    var strs = part.content.Split('*');
                    try {
                        var str_amount = strs[0].Trim(' ');
                        amount = int.Parse(str_amount);
                        var str_percent = strs[1].Trim(' ');
                        percent = int.Parse(str_percent.Remove(str_percent.Length - 1));
                    } catch (Exception ex) {
                        Debug.LogError($"error on item generation :{debug_name}" +
                            $"\n{ex.Message}");
                        Debug.LogError($"{debug_name} mw_prop parse error on : {it_name} ({strs[0]}) ({strs[1]})");
                    }
                } else {
                    if (part.content.Contains('%'))
                        percent = int.Parse(part.content.Remove(part.content.Length - 1));
                    else
                        amount = int.Parse(part.content);
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

    #region parent
    public void SetParent(Item item) {
        mParentItem = item;
    }
    public Item GetParent() {
        if (mParentItem == null) {
            Debug.LogError($"{debug_name} has no parent");
            return null;
        }
        return mParentItem;
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
        item.TileInfo = TileInfo;
        AddChildItem(item);
        return item;
    }

    public void AddChildItem(Item item) {

        if (mChildItems == null)
            mChildItems = new List<Item>();

        mChildItems.Add(item);

        item.SetParent(this);

        var weightProp = GetProp("weight");
        var oWeightProp = item.GetProp("weight");

        if (weightProp != null && oWeightProp != null) {
            int value = weightProp.GetNumValue();
            weightProp.SetValue(value + oWeightProp.GetNumValue());
        }

        // commented to resolve weight found every where bug
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
        var text = prms.Contains("dog") ? GetWord().GetText : "";
        if (prms.Contains("dogs") && GetWord().defaultNumber != Word.Number.Plural)
            text = $"{text}s";

        // add props descriptions that are always displayed
        if (!prms.Contains("lone")) {
            var props = GetVisibleProps("always");
            if (props.Count > 0) {
                // add describable props
                var before = props.FindAll(x => !x.HasPart("after word"));
                var after = props.FindAll(x => x.HasPart("after word"));
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
        return $"{text}{(DebugManager.Instance.displayItemID ? debug_Id : "")}";
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
        wordIndex = GetData().words.FindIndex(x => x.GetText == _text);
        return wordIndex >= 0;
    }

    public bool ContainedInText(string text) {
        return GetIndexInText(text) >= 0;
    }
    public int GetIndexInText(string text) {
        var wIndex = 0;
        foreach (var word in GetData().words) {
            for (int i = 0; i < 2; i++) {
                var num = (Word.Number)i;
                string _word = word.getText(num);
                if (Regex.IsMatch(text, @$"\b{_word}\b")) {
                    word.currentNumber = num;
                    wordIndex = wIndex;
                    return text.IndexOf(_word);
                }
            }
            ++wIndex;
        }
        return -1;
    }
    public Property GetPropInText(string text) {
        int index = 0;
        return GetPropInText(text, out index);
    }
    public Property GetPropInText(string text, out int textIndex) {
        textIndex = -1;
        foreach (var prop in GetVisibleProps()) {
            var description = prop.GetDescription();
            if (Regex.IsMatch(text, @$"\b{description}\b")) {
                textIndex = text.IndexOf(description);
                return prop;
            }
        }

        // search ( not visible but searchable )
        foreach (var prop in props.FindAll(x => x.HasPart("key"))) {
            var keyPart = prop.parts.Find(x => Regex.IsMatch(text, @$"\b{x.content}\b"));
            if (keyPart != null) {
                textIndex = text.IndexOf(keyPart.content);
                return prop;
            }
        }

        return null;
    }
    #endregion

    #region properties
    /// <summary>
    /// CREATION
    /// </summary>
    /// <param name="prms"></param>
    /// <returns></returns>

    // adding unchangeable, static data to the item (ex : direction etc.. )
    public void AddDataProp(string name) {
        var prop = Property.GetDataProp(name);
        props.Add(prop);
    }

    public Property SetProp(string prms) {

        var split = prms.Split(" / ");
        var name = split[0];

        // check if prop is already here
        var prop = GetProp(name);
        if (prop != null) {
            for (int i = 1; i < split.Length; i++) {
                var strs = split[i].Split(':');
                var part = prop.GetPart(strs[0]);
                if (part == null)
                    prop.AddPart(strs[0], strs[1]);
                else
                    part.content = strs[1];
            }
            prop.Init();
            return prop;
        }

        prop = new Property();
        prop.name = name;

        for (int i = 1; i < split.Length; i++) {
            var part = split[i].Split(':');
            prop.AddPart(part[0], part[1]);
        }

        prop.Init();
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
    public Property GetPropertyOfType(string type) {
        return props.Find(x => x.GetPart("type")?.content == type);
    }
    public Property GetProp(string name) {
        return props.Find(x => x.name == name);
    }
    public bool HasItemWithProp(string propertyName) {
        return GetChildItems().Find(x => x.HasProp(propertyName)) != null;
    }
    //

    #endregion

    #region visible props ?
    public bool HasVisibleProps() {
        return GetVisibleProps().Count > 0;
    }
    public List<Property> GetVisibleProps(string filters = "") {
        var visibleProps = props.FindAll(x => x.enabled && x.Visible());
        if (!string.IsNullOrEmpty(filters)) {
            var split = filters.Split(", ").ToList();
            foreach (var filter in split) {
                if (filter.StartsWith('!'))
                    visibleProps.RemoveAll(x => x.GetPart("description type").content == filter.Substring(1));
            }
            visibleProps.RemoveAll(x => !filters.Contains(x.GetPart("description type").content));
        }
        return visibleProps;
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