using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Item {

    public Tile GetTile() {
        return TileSet.GetTileSet(GetTileSet()).GetTile(GetCoords());
    }
    public int GetTileSet() {
        var p = GetProp("tileset");
        if (p == null) {
            return 0;
        }

        return p.GetNumValue();
    }
    public Coords GetCoords() {
        var p = GetProp("coords");
        if( p == null) {
            return Coords.zero;
        }
        return Coords.PropToCoords(p);
    }
    public string _debugName = "";
    public string DebugName {
        get {
            return _debugName;
            //return $"{_debugName} [l:{_debugName.Length}] [id:{debug_Id}]";
        }
    }
    public int dataIndex;
    public ItemData GetData() { return ItemData.itemDatas[dataIndex]; }
    public int debug_Id;

    public List<Property> props = new List<Property>();
    public int wordIndex = 0;
    [SerializeField]
    private Item mParentItem;
    [SerializeField]
    private List<Item> mChildItems;


    public virtual void Init() {
        _debugName = GetData().words[0].GetText;

        foreach (var dataProp in GetData().properties) {
            AddProp(dataProp, false);
        }

        foreach (var prop in props)
            prop.Init(this);
       
        var actName = "OnCreate";
        var itemAct = GetData().acts.Find(x => x.triggers[0] == actName);
        if ( itemAct != null) {
            var action = new WorldAction(this, itemAct.content, "On Create Event");
            action.InvokeSequence();
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
    public bool HasVisibleItems() {
        return HasChildItems() && (GetVisibleItems() != null && GetVisibleItems().Count > 0);
    }
    public List<Item> GetVisibleItems() {
        return mChildItems.FindAll(x => !x.HasProp("hidden"));
    }
    public List<Item> GetChildItems() {
        return mChildItems;
    }
    public List<Item> GetChildItemsWithProp(string propertyFilter, string propertyValue = "") {

        var filteredItems = mChildItems.FindAll(x => x.HasProp(propertyFilter));
        if (!string.IsNullOrEmpty(propertyValue))
            filteredItems.RemoveAll(x => x.GetProp(propertyFilter).GetContent("value") != propertyValue);

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
        return item.HasItem(this);
    }
    public void GenerateChildItems(Property prop = null) {

        // juste an action
        if (prop == null) {
            if (!HasProp("contents"))
                return;
            prop = GetProp("contents");
            //RemoveProp("contents");
        }

        if (mChildItems == null)
            mChildItems = new List<Item>();

        foreach (var part in prop.parts) {
            string content = part.content;
            try {
                string it_name = part.key;
                int amount = 1;
                int percent = 100;

                if (content.Contains('*')) {
                    var strs = content.Split('*');
                    var str_amount = strs[0].Trim(' ');
                    amount = int.Parse(str_amount);
                    var str_percent = strs[1].Trim(' ');
                    percent = int.Parse(str_percent.Remove(str_percent.Length - 1));
                } else {
                    if (content.Contains('%'))
                        percent = int.Parse(content.Remove(content.Length - 1));
                    else
                        amount = int.Parse(content);
                }

                for (var i = 0; i < amount; i++) {
                    var f = UnityEngine.Random.value * 100f;
                    if (f < percent) {
                        _ = CreateChildItem(it_name);
                    }
                }
            } catch (Exception e) {
                TextManager.Write($"Error loading item content\n[{part.key}:{content}]\n in item \n[{DebugName}]", Color.red);
                Debug.LogException(e);
            }
        }
    }
    #endregion

    #region parent
    public void SetParent(Item item) {
        mParentItem = item;
    }
    public bool HasParent() {
        return mParentItem != null;
    }
    public Item GetParent() {
        if (mParentItem == null)
            return null;
        return mParentItem;
    }
    public List<Item> GetParents() {
        var parents = new List<Item>();
        var parent = GetParent();
        int b = 0;
        while ( parent != null) {
            parents.Add(parent);
            parent = parent.GetParent();
            ++b;
            if (b == 10) {
                Debug.LogError($"parent loop break");
                break;
            }
        }
        return parents;
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
        item.SetProp($"coords|value:{Coords.CoordsToText(GetCoords())}");
        item.SetProp($"tileset|value:{GetTileSet()}");
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
        var gprop = GetProp("grammar");
        var keys = GetData().words.Select(x => x._text).ToList();
        if (gprop.HasPart("child keys")) {
            keys.AddRange(gprop.GetContent("child keys").Split('/'));
        }
        var str = string.Join('/', keys);
        item.SetProp($"contained | key:{str}");

        if (gprop.HasPart("child article")) {
            var childGrammar = item.GetProp("grammar");
            var childArticle = gprop.GetContent("child article");
            childArticle = childArticle.Replace("name", $"{GetText("the dog")}'s");
            childGrammar.SetPart("article", childArticle);
        }
    }

    // remove item
    public void RemoveItem(Item item) {
        if (!mChildItems.Contains(item)) {
            Debug.LogError($"{DebugName} doesn't contain {item.DebugName}");
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

    public bool HasItem(string name) {
        return GetItem(name) != null;
    }
    public bool HasItem(Item item) {
        return mChildItems != null && mChildItems.Contains(item);
    }

    public bool SimilarPropsAs(Item otherItem) {
        bool similarProps = false;
        if (GetVisibleProps().Count > 0 && otherItem.GetVisibleProps().Count > 0) {
            similarProps = GetVisibleProps()[0].GetCurrentDescription() == otherItem.GetVisibleProps()[0].GetCurrentDescription();
            if (similarProps) {
                Debug.Log($"{DebugName} is very similar to {otherItem.DebugName}");
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

    // Getting the description of the item 
    #region description
    public string GetText(string prms, int propLayer = 0) {

        // FORM
        // a special dog
        // preposition/article/properties/name
        var grammar = GetProp("grammar");

        // name : dog / dogs
        var name = GetWord().GetText;
        if ( grammar.HasPart("number")) {
            if (grammar.GetPart("number").content == "plural")
                name = $"{name}s";
        } else if (prms.Contains("dogs"))
            name = $"{name}s";

        // article
        var article = "";
        if (grammar.HasPart("definite")) {
            article = "the";
        } else if (Regex.IsMatch(prms, @$"\ba\b")) {
            article = "a";
             if (grammar.GetPart("number")?.content == "plural")
                article = "some";
            else if (Word.StartsWithVowel(name))
                article = "an";
            name = name.Insert(0, $"{article} ");
        } else if (Regex.IsMatch(prms, @$"\bthe\b")) {
            name = name.Insert(0, $"the ");
        }

        // preposition "on a dog" => in the armory
        var prepBound = @$"\bon\b";
        if (Regex.IsMatch(prms, prepBound))
            name = name.Insert(0, $"{GetWord().preposition} ");
        return $"{name}";
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
        // WORD //
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
        // TYPE //
        if (!HasProp("types")) return -1;
        var typeParts = GetProp("types").parts;
        foreach (var type in typeParts) {
            string bound = type.key;
            if (Regex.IsMatch(text, @$"\b{bound}\b")) {
                Debug.Log($"Found item : {DebugName} with type {bound}");
                return text.IndexOf(bound);
            }
        }
        return -1;
    }
    public List<Property> GetPropsInText(string text) {
        var result = new List<Property>();
        foreach (var prop in GetDescribableProps()) {
            var description = prop.GetCurrentDescription();
            if (Regex.IsMatch(text, @$"\b{description}\b") || Regex.IsMatch(text, @$"\b{prop.name}\b"))
                result.Add(prop);
        }

        // search ( not visible but searchable )
        foreach (var prop in props.FindAll(x => x.HasPart("key"))) {
            var keys = prop.GetContent("key").Split('/').ToList();
            var key = keys.Find(x => Regex.IsMatch(text, @$"\b{x}\b"));
            if (key!=null) {
                Debug.Log($"[item:{DebugName}], found key {key} on property {prop.name}");
                result.Add(prop);
            }
        }
        return result.Count > 0 ? result : null;
    }
    #endregion

    #region properties
    /// <summary>
    /// CREATION
    /// </summary>
    /// <param name="prms"></param>
    /// <returns></returns>

    // from content data
    public static Property New(Property copy) {
        var p = new Property ();
        p.name = copy.name;
        foreach (var part in copy.parts) {
            p.AddPart(part.key, part.content);
        }

        // remove enabled / Disabled symbol
        var str = p.name.TrimStart('*');

        // check if override
        if (str.StartsWith("new")) {
            // if override, don't add the parts from the data
            p.name = p.name.Remove(0, p.name.StartsWith('*') ? 5 : 4);
            return p;
        }
        // check if prop exists in content, to copy data from a higher property
        var dataProp = Property.sharedProps.Find(x => x.name == str);
        // le check de contents un peu flou en soi, ça va arriver avec d'autres prop donc il faudra réfléchir
        if (dataProp != null && dataProp.name != "contents") {
            // adding implicit parts from the data
            foreach (var part in dataProp.parts) {
                if (p.parts.Find(x => x.key == part.key) != null)
                    continue;
                p.AddPart(part.key, part.content);
            }
        }
        return p;
    }

    public Property AddProp(string name) {
        return AddProp(Property.GetDataProp(name));
    }
    public Property AddProp(Property prop, bool init = true) {
        var newProp = New(prop);
        props.Add(newProp);
        if (init) newProp.Init(this);
        return newProp;
    }

    public void RemovePropOfType(string type) {
        var i = props.FindIndex(x => x.HasPart("type") && x.GetContent("type").Equals(type));
        if (i < 0) {

        } else {
            props.RemoveAt(i);
        }
    }

    public void RemoveProp(Property prop) {
        props.Remove(prop);
    }
    public void RemoveProp(string name) {
        var i = props.FindIndex(x => x.name == name);
        if (i < 0) {
            Debug.LogError($"removing prop from item {DebugName} : no props named {name} found");
            return;
        }
        props.RemoveAt(i);
    }

    public Property SetProp(string prms) {

        var split = prms.Split('|');
        for (int i = 0; i < split.Length; i++) {
            split[i] = split[i].Trim(' ');
        }


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
            prop.Init(this);
            return prop;
        }

        prop = new Property();
        prop.name = name;
        for (int i = 1; i < split.Length; i++) {
            var part = split[i].Split(':');
            prop.AddPart(part[0], part[1]);
        }

        prop.Init(this);
        props.Add(prop);    

        return prop;
    }
    //

    // DELETE
    public void DeleteProperty(string propertyName) {
        var property = GetProp(propertyName);
        if (property == null) {
            Debug.LogError("DeleteProperty : property " + propertyName + " does not exist in " + DebugName);
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
        if ( props == null) {
            Debug.LogError($"crotte");
            return false;
        }
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
        return props.Find(x => x.HasPart("type") && x.GetContent("type") == type);
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

    // get all props that have a description, for when the parser searches through them
    public List<Property> GetDescribableProps() {
        return props.FindAll(x => x.enabled && x.Visible());
    }
    public List<Property> GetVisibleProps(string filters = "") {
        var visibleProps = props.FindAll(x => x.enabled && x.Visible() && /*remove none essential properties*/ x.GetContent("description type") != "on key" && x.GetContent("description type") != "always");
        if (!string.IsNullOrEmpty(filters)) {
            var split = filters.Split(", ").ToList();
            foreach (var filter in split) {
                if (filter.StartsWith('!'))
                    visibleProps.RemoveAll(x => x.GetContent("description type") == filter.Substring(1));
            }
            visibleProps.RemoveAll(x => !filters.Contains(x.GetContent("description type")));
        }
        return visibleProps;
    }
    #endregion

    #region remove & destroy
    public static void Destroy(Item item) {
        WorldEvent.RemoveWorldEventsWithItem(item);
        foreach (var prop in item.props)
            prop.Destroy();
        if (item.HasParent())
            item.GetParent().RemoveItem(item);
    }
    public void TransferTo(Item item) {
        if (HasParent()) {
            GetParent().RemoveItem(this);
        }
        item.AddChildItem(this);
    }
    #endregion



}