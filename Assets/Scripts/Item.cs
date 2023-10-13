using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Item
{
    // the debug name, without string function for debug purposes
    public string debug_name = "debug name";
    // the random id, to store hashcode
    public int debug_randomID;
    // the class name of the item, it will serialize a new one if not null
    public string className;

    // un peu oublié à quoi ça ser ça
    public List<string> infos = new List<string>();

    public bool temporary = false;

    // les spécificit's de l'objet.
    // à reset à chaque passe.
    // droite / gauche / bleu, rouge...
    // mais aussi second, first, third etc...
    public List<string> specs = new List<string>();

    // interior
    // maybe the interior class would be a item type
    public Interior interior = null;

    /// <summary>
    /// declaration
    /// </summary>
    public int dataIndex;

    /// <summary>
    /// WORD
    /// </summary>
    public int currentWordIndex = 0;
    public List<Word> words = new List<Word>();

    /// <summary>
    /// properties
    /// </summary>
    public List<Property> properties = new List<Property>();

    #region info
    public void AddInfo(string str)
    {
        if (infos.Contains(str))
            return;

        infos.Add(str);
    }

    public bool HasInfo(string str)
    {
        return infos.Contains(str);
    }
    #endregion

    #region specs
    public void AddSpec(string str)
    {
        if (specs.Contains(str))
            return;

        specs.Add(str);
    }
    #endregion

    public AppearInfo GetAppearInfo()
    {
        return AppearInfo.GetAppearInfo(dataIndex);
    }

    public virtual void Init(Item copy)
    {
        // unique
        foreach (var prop_copy in copy.properties)
        {
            Property newProp = CreateProperty(prop_copy);
            properties.Add(newProp);

        }
    }

    #region contained items
    public bool ContainsItems()
    {
        bool b = mContainedItems != null && mContainedItems.Count > 0;
        return b;
    }

    
    public virtual void TryGenerateItems()
    {
        if (HasInfo("generated items"))
            return;
        GenerateItems();
    }

    public virtual void GenerateItems()
    {
        AddInfo("generated items");

        foreach (var itemInfo in GetAppearInfo().itemInfos)
        {
            for (int i = 0; i < itemInfo.amount; i++)
            {
                float f = UnityEngine.Random.value * 100f;
                if (f < itemInfo.chanceAppear)
                {
                    CreateItem(itemInfo.name);
                }
            }
        }
    }
    #endregion

    #region item creation
    public Item CreateItem(string name)
    {
        Item newItem = Generate_Simple(name);
        return AddItem(newItem);
    }

    public Item AddTemporaryItem(Item item)
    {
        item.temporary = true;
        return AddItem(item);
    }

    public Item AddItem(Item item)
    {
        if (mContainedItems == null)
            mContainedItems = new List<Item>();

        mContainedItems.Add(item);

        // vraiment pas sûr que ça devrait être ici
        item.TryGenerateItems();

        return item;
    }

    // remove item
    public void RemoveItem(Item item)
    {
        mContainedItems.Remove(item);
    }
    public Item GetItem(string itemName)
    {
       if (mContainedItems == null)
            return null;

        Item tmpItem = mContainedItems.Find(x => x.HasWord(itemName));

        return tmpItem;
    }

    public bool HasItem(string itemName)
    {
        if (mContainedItems == null)
            return false;

        return mContainedItems.Find(x => x.word.text == itemName) != null;
    }

    public bool HasItem(Item item)
    {
        if (mContainedItems == null)
            return false;

        return GetAllItems().Contains(item);
    }

    public bool SameTypeAs(Item otherItem)
    {
        if ( otherItem == null)
        {
            return false;
        }

        return otherItem.dataIndex == dataIndex;
    }
    public bool ExactSameAs(Item otherItem)
    {
        if (otherItem == null)
        {
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
    public List<Item> GetContainedItems
    {
        get
        {
            if (mContainedItems == null)
                mContainedItems = new List<Item>();

            return mContainedItems;
        }
    }

    public bool IsTile()
    {
        return this as Tile != null;
    }

    public bool IsHumanoid()
    {
        return this as Humanoid != null;
    }

    public List<Item> GetUnanimatedItems()
    {
        return mContainedItems.FindAll(x =>
            
            !x.IsHumanoid()

        );
    }
    public List<Item> GetEnemies()
    {
        return mContainedItems.FindAll(x => x as Zombie != null);
    }

    public bool ContainedIn(Item item)
    {
        return item.HasItem(this);
    }

    public virtual void WriteContainedItems(bool describeContainers=false) {
        // Get Groups for description
        List<Group> allGroups = new List<Group>();
        foreach (var item in GetUnanimatedItems())
        {
            Group group = allGroups.Find(x => x.item.dataIndex == item.dataIndex);

            if (group == null)
            {
                // Create new group
                Group newGroup = new Group();
                newGroup.amount = 1;
                newGroup.item = item;
                allGroups.Add(newGroup);

                item.word.defined = false;
                continue;

            }

            // Found group, adding amount
            ++group.amount;

        }

        List<Group> containerGroups = new List<Group>();
        List<Group> itemGroups = new List<Group>();
        if (describeContainers)
            containerGroups = allGroups.FindAll(x => x.item.ContainsItems() && x.item.HasInfo("invisible"));

        itemGroups = allGroups.FindAll(x => !x.item.HasInfo("invisible"));

        if (containerGroups.Count > 0 && !containerGroups.First().item.HasInfo("invisible"))
            itemGroups.Insert(0, containerGroups.First());

        if (itemGroups.Count > 0 )
        {
            if (describeContainers)
                TextManager.Write("there's ");
            else
                TextManager.Write("");

            // Describe groups
            int index = 0;
            foreach (var group in itemGroups)
            {
                if (group.item.word.defined)
                {
                    TextManager.Add("&the dog&", group.item);
                }
                else
                {
                    TextManager.Add("&a dog&", group.item);
                    group.item.word.defined = true;
                }
                TextManager.AddLink(index, itemGroups.Count);

                ++index;
            }

            if (this != Tile.GetCurrent && itemGroups.Count > 0)
            {
                if (word.defined)
                {
                    TextManager.Add(" &on the dog&", this);
                }
                else
                {
                    TextManager.Add(" &on a dog&", this);
                    word.defined = true;
                }
            }
        }

        foreach (var group in containerGroups)
        {
            if (group.item.ContainsItems())
                group.item.WriteContainedItems();
        }

        AddInfo("discovered");

    }

    [System.Serializable]
    public class Group
    {
        public Item item;
        public int amount;
    }
    // description depth here
    public List<Item> GetAllItems()
    {
        var items = new List<Item> {this};
        if (mContainedItems == null)
            return items;
        foreach (var item in mContainedItems)
           items.AddRange(item.GetAllItems());
        return items;
    }
    #endregion


    #region search
    public Word word {
        get { return words[currentWordIndex];}
    }
    public bool HasWord(string _text) {
        currentWordIndex = words.FindIndex(x => x.text == _text);
        return currentWordIndex >= 0;
    }
    public bool ContainedInText(string text) {
        int index = 0;
        return ContainedInText(text, out index);
    }
    public bool ContainedInText(string text, out int index) {
        foreach (var word in words) {
            // find singular
            string bound = @$"\b{word.text}\b";
            if (Regex.IsMatch(text, bound))
            {
                word.number = Word.Number.Singular;
                index = text.IndexOf(word.text);
                return true;
            }

            bound = @$"\b{word.GetPlural()}\b";
            if (Regex.IsMatch(text, bound)) {
                word.number = Word.Number.Plural;
                index = text.IndexOf(word.GetPlural());
                return true;
            }
        }

        index = -1;
        return false;
    }
    #endregion

    #region properties
    /// properties /// <summary>
    /// properties ///
    /// </summary>
    public virtual void WriteDescription()
    {
        if (HasProperties())
            WriteProperties();

        if (ContainsItems())
            WriteContainedItems(true);

        if (!HasProperties() && !ContainsItems()) {
            if (GetAppearInfo().CanContainItems())
                TextManager.Write("It's empty");
            else
                TextManager.Write("It's just &a dog&", this);
        }
    }

    // adds whole new, simple property
    public Property AddProperty(string line, bool describe = false) {
        Property newProperty = new Property();
        string[] parts = line.Split(" / ");
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
        Property property = GetProperty(propName);
        property.Update(line);
        PropertyDescription.Add(this, property);
    }

    /// <summary>
    ///  WHAT THE FUCK JAI FAIS ça a 3 heures c'est pas bon mais je l'ai mis parce que ça permet
    ///  de réfléchir à une autre solution
    /// </summary>
    // add proper property, from the data, with events and all
    public Property CreateProperty(Property property_data) {
        Property newProperty = new Property(property_data);
        newProperty.Init();
        if (newProperty.eventDatas != null) {
            foreach (var eventData in newProperty.eventDatas) {
                // juste un objet par evetn en fait
                ItemEvent itemEvent = ItemEvent.list.Find(x => x.item == this);
                if (itemEvent == null)
                    itemEvent = ItemEvent.New(this, Tile.GetCurrent);
            }
        }
        return newProperty;
    }
    public void DeleteProperty(string propertyName) {
        Property property = GetProperty(propertyName);
        if (property == null)
        {
            Debug.LogError("DeleteProperty : property " + propertyName + " does not exist in " + debug_name);
            return;
        }
        properties.Remove(property);
    }
    public bool HasProperties() {
        return properties.FindAll(x => x.enabled).Count > 0;
    }
    public bool HasVisibleProperties() {
        return properties.FindAll(x => x.enabled && x.type != "hidden").Count > 0;
    }
    public bool HasEnabledProperty(string name) {
        Property property = properties.Find(x => x.name == name && x.enabled);
        return property != null;
    }
    public bool HasProperty(string name) {
        Property property = properties.Find(x => x.name == name);
        return property != null;
    }
    public bool HasPropertyOfType(string type) {
        Property property = properties.Find(x => x.type == type);
        return property != null;
    }

    public List<Property> GetEnabledProperties() {
        // ici "type == container" veut dire que la battery ou water des items est décrite meme si le truc est disabled,
        // à terme, mettre un parametre qui fait que la prop est décrite meme quand l'item est disabled
        // ou un autre truc mieux
        return properties.FindAll(x => x.enabled || x.type == "container");
    }

    public void EnableProperty(string propertyName) {
        Property prop = properties.Find(x => x.name == propertyName);
        if (prop == null) {
            Debug.LogError("ENABLE PROP : Could not find property " + propertyName);
            return;
        }

        prop.enabled = true;
        ItemEvent.CallEventOnProp("subEnable", prop);
        PropertyDescription.Add(this, prop);


    }

    public void DisableProperty(string propertyName)
    {
        Property prop = properties.Find(x => x.name == propertyName);
        if (prop == null)
        {
            Debug.LogError("DISABLE PROP : Could not find property " + propertyName);
            return;
        }

        prop.enabled = false;

        PropertyDescription.Add(this, prop);
    }

    public Property GetProperty(string name)
    {
        Property property = properties.Find(x => x.name == name);

        if (property == null)
        {
            Debug.LogError("property : " + name + " doesn't exist in item " + word.text);
            return null;
        }

        return property;
    }
    public List<Property> GetPropertiesOfType(string type)
    {
        return properties.FindAll(x => x.type == type);
    }
    public Property GetPropertyOfType(string type)
    {
        Property property = properties.Find(x => x.type == type);

        if (property == null)
        {
            Debug.LogError("property if type : " + type + " doesn't exist in item " + word.text);
            return null;
        }

        return property;
    }
    public bool HasItemWithProperty(string propertyName)
    {
        return GetContainedItems.Find(x => x.HasProperty(propertyName)) != null;
    }

    public void WriteProperties()
    {

        int enabledPropCount = GetEnabledProperties().Count;

        if (enabledPropCount > 0)
        {
            TextManager.Write("&the dog& is ", this);

            for (int i = 0; i < enabledPropCount; i++)
            {
                Property property = GetEnabledProperties()[i];

                TextManager.Add(property.GetDescription());
                TextManager.AddLink(i, enabledPropCount);

            }
        }
    }
    #endregion

    ///
    /// <summary>
    /// TOOLS
    /// </summary>
    ///

    #region remove & destroy
    public static void Destroy(Item item)
    {
        Debug.Log("destroying " + item.debug_name);
        ItemEvent.Remove(item);

        foreach (var prop in item.properties)
        {
            prop.Destroy();
        }

        RemoveFromContainer(item);
    }
    public void TransferTo(Item item)
    {
        RemoveFromContainer(this);
        item.AddItem(this);
    }
    public static void RemoveFromContainer(Item targetItem)
    {

        // then in tile
        foreach (var item in AvailableItems.GetItems)
        {
            if (item.HasItem(targetItem))
            {
                item.RemoveItem(targetItem);
                return;
            }
        }
        Debug.LogError("removing item : " + targetItem.word.text + " failed : not in container, tile or inventory");
    }
    #endregion

    #region tools
    public static List<Item> dataItems = new List<Item>();

    public static Item GetDataItem(string key)
    {
        Item item;
        if (key.StartsWith("type:"))
        {
            key = key.Remove(0, 5);
            List<Item> items = dataItems.FindAll(x => x.HasInfo(key));

            item = items[UnityEngine.Random.Range(0, items.Count)];
        }
        else
        {
            item = dataItems.Find(x => x.HasWord(key));
        }

        if (item == null)
        {
            Debug.LogError("no " + key + " in item datas");
            return null;
        }

        return item;
    }

    public static List<AppearInfo> appearInfos = new List<AppearInfo>();

    public static Item Generate_Simple(string name)
    {
        Item copy = GetDataItem(name);

        Item item = Generate(copy);

        item.Init(copy);

        return item;
    }

    public static object Generate_Special(string name)
    {
        Item copy = GetDataItem(name);

        if (string.IsNullOrEmpty(copy.className))
        {
            Debug.Log("request " + name + " : no class name in data");
            return null;
        }

        Type ItemType = Type.GetType(copy.className);
        if (ItemType == null)
        {
            Debug.LogError("pas d'item type pour " + copy.debug_name);
            return null;
        }

        Item item = Generate(copy);

        var serializedParent = JsonConvert.SerializeObject(item);
        object obj = JsonConvert.DeserializeObject(serializedParent, ItemType);

        ((Item)obj).Init(copy);

        return obj;
    }

    /// create a new item by name
    private static Item Generate(Item copy)
    {
        Item newItem = new Item();

        // common to all
        newItem.debug_name = copy.debug_name;
        newItem.debug_randomID = newItem.GetHashCode();
        newItem.dataIndex = copy.dataIndex;
        newItem.className = copy.className;

        newItem.infos = copy.infos;
        // the word never changes, non ? pourquoi en copy
        newItem.words = copy.words;

        return newItem;
    }

    public static List<Item> FindItemsWithProperty(string propName)
    {
        return dataItems.FindAll(x => x.HasProperty(propName));
    }
    #endregion



}