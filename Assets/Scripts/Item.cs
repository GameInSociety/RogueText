using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Item {

    // DEBUG THINGS
    // the debug name, without string function for debug purposes
    // only for serialization
    public string debug_name = "debug name";
    // the random id, to store hashcode
    public int debug_randomID;

    // SHOULD BE IN ITEMDATA CLASS (no class yet)
    // this class should only encapsulate the item data
    public static List<AppearInfo> appearInfos = new List<AppearInfo>();
    public static List<Item> dataItems = new List<Item>();
    // the class name of the item, it will serialize a new one if not null
    public string className;
    public int dataIndex;
    public List<Word> words = new List<Word>();

    // word parameters not like the data item words.


    /// <summary>
    /// properties
    /// </summary>
    /// complicated, ITEM DATA should have properties ( mabye only string )
    /// that way, the data properties would be there
    /// but still, we could add, remove and handle properties in game
    public List<Property> properties = new List<Property>();

    // INFOS is a bit of a duplicate of property. but less complicated.
    // not sure, maybe it could be in the item data class, but 
    public List<string> infos = new List<string>();

    /// <summary>
    /// WORD the recetn index of the word used
    /// </summary>
    public int currentWordIndex = 0;

    // les spécificit's de l'objet.
    // à reset à chaque passe.
    // droite / gauche / bleu, rouge...
    // mais aussi second, first, third etc...
    public List<Spec> specs;

    // interior
    // maybe the interior class would be a item type
    // INTERIOR SHOULD BE CLASSES
    public Interior interior = null;

    

    #region info
    public void AddInfo(string str) {
        if (infos.Contains(str))
            return;

        infos.Add(str);
    }
    public bool HasInfo(string str) {
        return infos.Contains(str);
    }
    #endregion
    #region specs
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
    public Spec getSpecWithKey(string key) {
        if (specs == null)
            return null;
        return specs.Find(x => x.searchValue == key);
    }
    public Item getItemWithSpec(string key) {
        return getContainedItems.Find(x => x.hasSpecWithKey(key));
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
    public Spec setSpec(string search,string display = "", string key = "none", bool visible = true) {
        if (specs == null)
            specs = new List<Spec>();

        if (string.IsNullOrEmpty(display))
            display = search;
        if (string.IsNullOrEmpty(key))
            key = search;

        Spec spec = specs.Find(x => x.key != "none" && x.key == key);
        if ( spec == null) {
            spec = new Spec(search, display, key, visible);
            specs.Add(spec);
        } else {
            spec.searchValue = search;
            spec.displayValue = display;
            spec.key = key;
            spec.visible = visible;
        }
        return spec;
    }
    #endregion

    public AppearInfo GetAppearInfo() {
        return AppearInfo.GetAppearInfo(dataIndex);
    }
    public virtual void Init(Item copy) {
        foreach (var prop_copy in copy.properties) {
            var newProp = CreateProperty(prop_copy);
            properties.Add(newProp);
        }
        if (HasInfo("dif")) {
            //Debug.Log($"{debug_name} needs to be differenciated, adding specs");
            Spec.Category cat = Spec.GetCat("color");
            string specName = cat.GetRandomSpec();
            Spec newSpec = setSpec(specName, specName, cat.name );
            //Debug.Log($"adding spec {newSpec.displayValue} to {debug_name}");
        }

    }
    #region contained items
    public bool containsItems() {
        var b = mContainedItems != null && mContainedItems.Count > 0;
        return b;
    }

    public virtual void generateItems() {
        if (HasInfo("generated items"))
            return;
        AddInfo("generated items");
        foreach (var itemInfo in GetAppearInfo().itemInfos) {
            for (var i = 0; i < itemInfo.amount; i++) {
                var f = UnityEngine.Random.value * 100f;
                if (f < itemInfo.chance) {
                    _ = addItem(itemInfo.name);
                }
            }
        }
    }
    #endregion
    #region item creation
    public Item addItem(string name) {
        var newItem = Generate_Simple(name);
        return addItem(newItem);
    }
    public Item addItem(Item item) {
        if (mContainedItems == null)
            mContainedItems = new List<Item>();

        mContainedItems.Add(item);

        item.setSpec(debug_name, $">{getText("in a dog")}", "container", false);

        // vraiment pas sûr que ça devrait être ici
        item.generateItems();

        return item;
    }

    // remove item
    public void removeFromTile() {
        var tmp = Tile.GetCurrent.getRecursive(4);
        foreach (var item in tmp) {
            if (item.hasItem(this)) {
                Debug.Log(item.debug_name + " has item : " + debug_name);
                item.RemoveItem(this);
                break;
            }
        }
    }
    public void RemoveItem(Item item) {
        if (!mContainedItems.Contains(item)) {
            Debug.LogError($"{debug_name} doesn't contain {item.debug_name}");
        }
        mContainedItems.Remove(item);
    }
    public Item GetItem(string itemName) {
        if (mContainedItems == null)
            return null;

        var tmpItem = mContainedItems.Find(x => x.HasWord(itemName));

        return tmpItem;
    }

    public bool hasItem(string itemName) {
        if (mContainedItems == null)
            return false;
        return mContainedItems.Find(x => x.word.text == itemName) != null;
    }

    public bool hasItem(Item item) {
        return mContainedItems != null && mContainedItems.Contains(item);
    }

    public bool SameTypeAs(Item otherItem) {
        if (otherItem == null) {
            return false;
        }

        return otherItem.dataIndex == dataIndex;
    }
    public bool ExactSameAs(Item otherItem) {
        if (otherItem == null) {
            Debug.Log("no previous tile");
            return false;
        }

        return otherItem == this;
    }
    #endregion
    #region socket
    /// <summary>
    /// DESCRIPTION
    /// </summary>
    [SerializeField]
    private List<Item> mContainedItems;
    public List<Item> getContainedItems {
        get {
            if (mContainedItems == null)
                mContainedItems = new List<Item>();
            return mContainedItems;
        }
    }
    public List<Item> getItems() {
        return mContainedItems.FindAll(x =>
            x as Humanoid == null
        );
    }
    public List<Item> getEnemies() {
        return mContainedItems.FindAll(x => (x as Zombie) != null);
    }
    public bool containedIn(Item item) {
        return item.hasItem(this);
    }
    public virtual void writeContainedItems(bool describeContainers = false) {
        string list = TextManager.NewItemDescription(getItems());
        string description = $"there's {list} {getText("in a dog")}";
        TextManager.write(description);
        AddInfo("discovered");
        return;
    }

    public string getText(string prms) {

        // FORM
        // a special dog
        // preposition/article/specs/name

        // preposition "on a dog" => in the armory
        var prepBound = @$"\bon\b";
        if (Regex.IsMatch(word.text, prepBound))
            prms = prms.Replace(prepBound, word.preposition);

        // name + specs (getting before article to see if it starts with a vowel)
        var name_group = word.text;
        var specBound = @$"\bspecial\b";
        if (hasSpecs() && Regex.IsMatch(prms, specBound)) {
            int specIndex = prms.IndexOf("special");
            prms = prms.Remove(specIndex , "special".Length + 1);
            var front_specs = specs.FindAll(x => !x.displayValue.StartsWith('>'));
            var back_specs = specs.FindAll(x => x.displayValue.StartsWith('>'));
            if (front_specs.Count > 0) name_group = name_group.Insert(0, $"{getSpecText(front_specs)}");
            if (back_specs.Count > 0) name_group = name_group.Insert(name_group.Length, $" {getSpecText(back_specs)}");
        }

        // article
        if (word.defined || HasInfo("definite")) {
            var articleBound = @$"\ba\b";
            prms = Regex.Replace(prms, articleBound, "the");
        } else {
            // a dog => some mittens
            if (word.defaultNumber == Word.Number.Plural) {
                var articleBound = @$"\ba\b";
                prms = Regex.Replace(prms, articleBound, "some");
            } else {
                // a dog = an armory
                if (Word.startWithVowel(name_group)) {
                    var articleBound = @$"\ba\b";
                    prms = Regex.Replace(prms, articleBound, "an");
                }
            }
        }

        

        prms = prms.Replace("dog", name_group);
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
            str += $"{spectext} {TextUtils.GetLink(i, list.Count, false)}";
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
        if (depth == 0 || mContainedItems == null)
            return items;
        foreach (var it in mContainedItems)
            items.AddRange(it.getRecursive(depth - 1));
        return items;
    }
    #endregion
    #region search
    public Word word => words[currentWordIndex];
    public bool HasWord(string _text) {
        currentWordIndex = words.FindIndex(x => x.text == _text);
        return currentWordIndex >= 0;
    }
    public bool containedInText(string _text) {
        Word.Number num = Word.Number.None;
        return getIndexInText(_text, out num) >= 0;
    }
    public int getIndexInText(string text, out Word.Number num) {
        num = Word.Number.None;
        foreach (var word in words) {
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

    #region properties
    /// properties /// <summary>
    /// properties ///
    /// </summary>
    public virtual void writeDescription() {
        writeDescription("it's", "a special dog");
    }
    public virtual void writeDescription(string starter, string prms) {

        TextManager.write($"{starter} {getText(prms)}");

        if (hasProperties())
            WriteProperties();

        if (containsItems())
            writeContainedItems(true);

        if (!hasProperties() && !containsItems()) {
            if (GetAppearInfo().CanContainItems())
                TextManager.write("it's empty");
        }
    }

    // adds whole new, simple property
    public Property addProperty(string line, bool describe = false) {
        var newProperty = new Property();
        var parts = line.Split(" / ");
        newProperty.type = parts[0];
        newProperty.name = parts[1];
        if (parts.Length > 2)
            newProperty.SetValue(parts[2]);
        newProperty.Init();

        if (describe)
            PropertyDescription.Add(this, newProperty);

        properties.Add(newProperty);
        return newProperty;
    }

    public void UpdateProperty(string propName, string line) {
        var property = GetProperty(propName);
        property.Update(line);
        PropertyDescription.Add(this, property);
    }

    /// <summary>
    ///  WHAT THE FUCK JAI FAIS ça a 3 heures c'est pas bon mais je l'ai mis parce que ça permet
    ///  de réfléchir à une autre solution
    /// </summary>
    // add proper property, from the data, with events and all
    public Property CreateProperty(Property property_data) {
        var newProperty = new Property(property_data);
        newProperty.Init();
        if (newProperty.eventDatas != null) {
            foreach (var eventData in newProperty.eventDatas) {
                // juste un objet par evetn en fait
                var itemEvent = ItemEvent.list.Find(x => x.item == this);
                if (itemEvent == null)
                    itemEvent = ItemEvent.New(this, Tile.GetCurrent);
            }
        }
        return newProperty;
    }
    public void DeleteProperty(string propertyName) {
        var property = GetProperty(propertyName);
        if (property == null) {
            Debug.LogError("DeleteProperty : property " + propertyName + " does not exist in " + debug_name);
            return;
        }
        _ = properties.Remove(property);
    }
    public bool hasProperties() {
        return properties.FindAll(x => x.enabled).Count > 0;
    }
    public bool HasVisibleProperties() {
        return properties.FindAll(x => x.enabled && x.type != "hidden").Count > 0;
    }
    public bool HasEnabledProperty(string name) {
        var property = properties.Find(x => x.name == name && x.enabled);
        return property != null;
    }
    public bool hasProperty(string name) {
        var property = properties.Find(x => x.name == name);
        return property != null;
    }
    public bool HasPropertyOfType(string type) {
        var property = properties.Find(x => x.type == type);
        return property != null;
    }

    public List<Property> GetEnabledProperties() {
        // ici "type == container" veut dire que la battery ou water des items est décrite meme si le truc est disabled,
        // à terme, mettre un parametre qui fait que la prop est décrite meme quand l'item est disabled
        // ou un autre truc mieux
        return properties.FindAll(x => x.enabled || x.type == "container");
    }

    public void EnableProperty(string propertyName) {
        var prop = properties.Find(x => x.name == propertyName);
        if (prop == null) {
            Debug.LogError("ENABLE PROP : Could not find property " + propertyName);
            return;
        }

        prop.enabled = true;
        ItemEvent.callEventOnProp("subEnable", prop);
        PropertyDescription.Add(this, prop);


    }

    public void DisableProperty(string propertyName) {
        var prop = properties.Find(x => x.name == propertyName);
        if (prop == null) {
            Debug.LogError("DISABLE PROP : Could not find property " + propertyName);
            return;
        }

        prop.enabled = false;

        PropertyDescription.Add(this, prop);
    }

    public Property GetProperty(string name) {
        var property = properties.Find(x => x.name == name);

        if (property == null) {
            Debug.LogError("property : " + name + " doesn't exist in item " + word.text);
            return null;
        }

        return property;
    }
    public List<Property> GetPropertiesOfType(string type) {
        return properties.FindAll(x => x.type == type);
    }
    public Property GetPropertyOfType(string type) {
        var property = properties.Find(x => x.type == type);

        if (property == null) {
            Debug.LogError("property if type : " + type + " doesn't exist in item " + word.text);
            return null;
        }

        return property;
    }
    public bool HasItemWithProperty(string propertyName) {
        return getContainedItems.Find(x => x.hasProperty(propertyName)) != null;
    }

    public void WriteProperties() {

        var enabledPropCount = GetEnabledProperties().Count;
        if (enabledPropCount == 0)
            return;
        
        TextManager.add("it's ");
        for (var i = 0; i < enabledPropCount; i++) {
            var property = GetEnabledProperties()[i];
            TextManager.add(property.GetDescription());
            TextManager.AddLink(i, enabledPropCount);
        }
    }
    #endregion

    #region remove & destroy
    public static void Destroy(Item item) {
        ItemEvent.Remove(item);
        foreach (var prop in item.properties)
            prop.Destroy();
        AvailableItems.Get.removeFromWorld(item);
    }
    public void TransferTo(Item item) {
        AvailableItems.Get.removeFromWorld(this);
        item.addItem(this);
    }
    #endregion

    #region tools
    public static Item GetDataItem(string key) {
        Item item;
        if (key.StartsWith("type:")) {
            key = key.Remove(0, 5);
            var items = dataItems.FindAll(x => x.HasInfo(key));
            item = items[UnityEngine.Random.Range(0, items.Count)];
        } else {
            item = dataItems.Find(x => x.HasWord(key));
        }
        if (item == null) {
            Debug.LogError("no " + key + " in item datas");
            return null;
        }
        return item;
    }

    public static Item Generate_Simple(string name) {
        var copy = GetDataItem(name);
        var item = Generate(copy);
        item.Init(copy);
        return item;
    }

    public static object Generate_Special(string name) {
        var copy = GetDataItem(name);

        if (string.IsNullOrEmpty(copy.className)) {
            Debug.Log("request " + name + " : no class name in data");
            return null;
        }

        var ItemType = Type.GetType(copy.className);
        if (ItemType == null) {
            Debug.LogError("pas d'item type pour " + copy.debug_name);
            return null;
        }

        var item = Generate(copy);

        var serializedParent = JsonConvert.SerializeObject(item);
        var obj = JsonConvert.DeserializeObject(serializedParent, ItemType);

        ((Item)obj).Init(copy);

        return obj;
    }

    /// create a new item by name
    private static Item Generate(Item copy) {
        var newItem = new Item();

        // common to all
        newItem.debug_name = copy.debug_name;
        newItem.debug_randomID = newItem.GetHashCode();
        newItem.dataIndex = copy.dataIndex;
        newItem.className = copy.className;

        newItem.infos = new List<string>(copy.infos);
        // the word never changes, non ? pourquoi en copy
        newItem.words = copy.words;

        return newItem;
    }

    public static List<Item> FindItemsWithProperty(string propName) {
        return dataItems.FindAll(x => x.hasProperty(propName));
    }
    #endregion



}