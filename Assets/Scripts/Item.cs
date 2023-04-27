using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[System.Serializable]
public class Item
{
    public string debug_name = "debug name";

    public static List<Item> dataItems = new List<Item>();

    /// <summary>
    /// declaration
    /// </summary>
    public int index;
    public int weight = 0;
    public bool usableAnytime = false;

    public string inputToFind = "str";

    /// <summary>
    /// exemples :
    /// dans une forêt : "derrière l'arbre", ou "dans un buisson"
    /// dans un sac : (donc autre objet) au fond du sac, etc...
    /// </summary>
    /// Je crois qu'il vaut mieux que ces trucs soient dans les sockets
    /// ( liste d'int dans le socket parce quand y'a pas de copy dans les sockets )
    ///c'est déjàle cas maisc'estpar rapport aux tiles
    public List<Socket> sockets = new List<Socket>();
    // pareil
    public List<AppearInfo> appearInfos = new List<AppearInfo>();

    //
    public List<Item> containedItems;

    /// <summary>
    /// WORD
    /// </summary>
    public int currentWordIndex = 0;
    public Word word
    {
        get
        {
            return words[currentWordIndex];
        }
    }
    public List<Word> words = new List<Word>();
    public bool stackable = false;

    /// <summary>
    /// container
    /// </summary>
    public bool opened = false;
    public bool emptied = false;

    /// <summary>
    /// properties
    /// </summary>
    public List<Property> properties = new List<Property>();

    public Item()
    {

    }

    #region container
    public bool ContainsItems()
    {
        //GenerateItems();

        bool b = containedItems != null && containedItems.Count > 0;
        Debug.Log("continas item ? " + b);
        return b;
    }
    public void GenerateItems()
    {
        if (ContainsItems())
        {
            return;
        }

        foreach (var appearInfo in appearInfos)
        {
            for (int i = 0; i < appearInfo.amount; i++)
            {
                if (Random.value * 100f < appearInfo.rate)
                {
                    ItemManager.Instance.CreateInItem(this, appearInfo.GetItemName());
                }
            }
        }

    }

    public void AddItem(Item item)
    {
        if (containedItems == null)
        {
            containedItems = new List<Item>();
        }

        containedItems.Add(item);

    }

    // remove item
    public void RemoveItem(Item item)
    {
        containedItems.Remove(item);
    }
    // item row : wtf ? le craft system est pas ouf
    public void RemoveItem(int itemRow)
    {
        RemoveItem(containedItems.Find(x => x.index == itemRow));
    }
    //

    public Item FindItem(string str)
    {
        Item item = containedItems.Find(x => x.HasWord(str));

        if (item == null)
            return null;

        return item;
    }

    public Item GetItem(string itemName)
    {
        return containedItems.Find(x => x.word.text == itemName);
    }

    public bool ContainsItem( Item item)
    {
        return containedItems.Contains(item);
    }

    /// <summary>
    ///  WEIGHT
    /// </summary>
    public int GetContainedItemWeight()
    {
        int w = 0;
        foreach (var item in containedItems)
        {
            w += item.weight;
        }
        return w;
    }
    ///

    /// <summary>
    /// OPEN / CLOSE
    /// </summary>
    public void Open()
    {
        Debug.Log("opening : " + debug_name);
        GenerateItems();

        Container.opened = true;
        Container.CurrentItem= this;
        WriteContainedDescription();
    }

    public void Close()
    {
        // toujours dans la classe inventory.cs pour l'intsant
        if (!Container.opened)
        {
            PhraseKey.WritePhrase("container_alreadyClosed");
            return;
        }

        Container.opened = false;
        PhraseKey.WritePhrase("container_clsose", Container.CurrentItem);
    }
    ///

    /// <summary>
    /// DESCRIPTION
    /// </summary>
    public void WriteContainedDescription()
    {
        if (!ContainsItems())
        {
            Debug.Log(debug_name + " is empty");
            PhraseKey.WritePhrase("container_empty");
            return;
        }

        PhraseKey.Renew();
        PhraseKey.WritePhrase("container_describe");
    }

    public bool SameTypeAs(Item otherItem)
    {
        return otherItem.index == index;
    }
    public bool ExactSameAs(Item otherItem)
    {
        return otherItem == this;
    }
    #endregion

    #region appear info
    [System.Serializable]
    public class AppearInfo
    {
        public int itemIndex = 0;
        public int rate = 0;
        public int amount = 1;

        public string GetItemName()
        {
            return Item.dataItems[itemIndex].debug_name;
        }

        public bool CanAppear()
        {
            return false;
        }
    }
    #endregion

    ///
    /// <summary>
    /// TOOLS
    /// </summary>
    ///


    public void PickUp()
    {
        if (Inventory.Instance.bag_Item.GetContainedItemWeight() + weight > Inventory.Instance.maxWeight)
        {
            PhraseKey.WritePhrase("inventory_TooHeavy");
            return;
        }

        Item.Remove(this);

        Inventory.Instance.AddItem(this);

        PhraseKey.WritePhrase("inventory_PickUp");
    }

    #region remove & destroy
    public static void Destroy(Item item){
        
        Remove(item);
    }
    public static void Remove(Item targetItem)
    {
        // first search thing in opened container
        if (Container.opened)
        {
            if (Container.CurrentItem.containedItems.Contains(targetItem))
            {
                Container.CurrentItem.RemoveItem(targetItem);
            }
            return;
        }

        // then in tile
        if (Tile.GetCurrent.items.Contains(targetItem))
        {
            Tile.GetCurrent.RemoveItem(targetItem);

            //DisplayDescription.Instance.UpdateDescription();
            return;
        }

        // then in inventory
        if (Inventory.Instance.bag_Item.ContainsItem(targetItem))
        {
            Inventory.Instance.RemoveItem(targetItem);
            return;
        }

        Debug.LogError("removing item : " + targetItem.word.text + " failed : not in container, tile or inventory");
    }
    #endregion

    #region search
    public bool HasWord (string _text)
    {
        currentWordIndex = words.FindIndex(x => x.Compare(_text));

        if ( currentWordIndex < 0)
        {
            currentWordIndex = 0;
            return false;
        }

        return true;
    }
    public static Item FindInWorld(string str)
    {
        Item item = null;

        bool foundInContainer = false;

        // dans un container
        if (Container.opened)
        {
            item = Container.CurrentItem.FindItem(str);
            if ( item != null)
            {
                foundInContainer = true;
            }
        }


        // chercher une premiere fois dans l'inventaire s'il est ouvert
        if (Inventory.Instance.IsOpened)
        {
            item = Inventory.Instance.FindItem(str);
            if (item != null)
            {
                return item;
            }
        }


        // is the item one of the surrounding tiles ?
        if (item == null)
        {
            item = FindInTile(str);
        }

        // et en dernier s'il est fermé
        if (item == null)
        {
            item = Inventory.Instance.FindItem(str);
        }


        if (item == null)
        {
            item = FindUsableAnytime(str);
        }

        // pour éviter que le container reste ouvert si on fait une autre action
        if ( item != null && Container.opened && !foundInContainer)
        {
            Container.CurrentItem.Close();
        }

        return item;
    }

    private static Item FindInTile(string str)
    {
        // is the item the exact same tile as the one we're in ?
        if (Tile.GetCurrent.tileItem.HasWord(str))
        {
            return Tile.GetCurrent.tileItem;
        }

        //List<Item> items = Player.Instance.SurroundingTiles().FindAll();

        // is the item one of the surrounding tiles ?
        foreach (var tile in Player.Instance.SurroundingTiles())
        {
            if (tile.tileItem.word.Compare(str))
            {
                return tile.tileItem;
            }
        }

        List<Item> items = Tile.GetCurrent.items.FindAll(x => x.word.Compare(str));

        Debug.Log("");

        /// ADJECTIVES ///

        /// chercher les adjectifs pour différencier les objets ( porte bleu, porte rouge )
        if (items.Count > 0)
        {
            foreach (var inputPart in InputInfo.Instance.parts)
            {
                foreach (var item in items)
                {
                    if (!item.word.HasAdjective())
                    {
                        continue;
                    }

                    string adjSTR = item.word.GetAdjective.GetContent(false);

                    if (adjSTR == inputPart)
                    {
                        return item;
                    }
                }
            }

            return items[0];

        }

        return null;
    }

    private static Item FindUsableAnytime(string str)
    {
        return dataItems.Find(x => x.HasWord(str) && x.usableAnytime);
        //return items.Find(x => x.word.Compare(str) );
    }

    public static Item GetDataItem(string _name)
    {
        Item item = TryGetItem(_name);

        if ( item == null)
        {
            Debug.LogError("couldn't find item : " + _name);
        }

        return item;
        
    }
    public bool IsAnItem(string item_name)
    {
        return TryGetItem(item_name) != null;
    }
    public static Item TryGetItem(string _name)
    {
        _name = _name.ToLower();

        // to find the word on ly
        //Item item = dataItems.Find(x => x.word.text.ToLower() == _name);

        // to search also in syonyms
        Item item = dataItems.Find(x =>
        x.words.Find(
            x => x.text.ToLower() == _name
            ) != null
            );

        if (item == null)
        {
            // find plural
            item = dataItems.Find(x => x.word.GetPlural() == _name);

            if (item != null)
            {
                // found the plural
                //Debug.Log("found plural");
                InputInfo.Instance.actionOnAll = true;
                return item;
            }
        }

        return item;
    }
    public static List<Item> FindAllByName(string str)
    {
        str = str.ToLower();

        return dataItems.FindAll(x => x.word.text.StartsWith(str));
    }
    public static Item FindByName(string str){
        str = str.ToLower();
        return dataItems.Find(x => x.word.text.StartsWith(str));

    }
    #endregion

    #region list
    public enum ListSeparator
    {
        Return,
        Commas,
    }
    public static string ItemListString(List<Item> _items, ListSeparator listSeparator, bool displayWeight)
    {
        string text = "";

        int i = 0;

        foreach (var item in _items)
        {
            text += item.word.GetContent("a dog");

            // pour l'instant en pause parce que pas forcément besoin, et systeme de poids un peu laissé de côté
            /*if (displayWeight)
            {
                text += " (w:" + (item.weight) + ")";
            }*/

            if (_items.Count > 1 && i < _items.Count - 1)
            {
                if (listSeparator == ListSeparator.Return)
                {
                    text += "\n";
                }
                else
                {
                    if (_items.Count > 2)
                    {
                        if (i == _items.Count - 2)
                        {
                            text += " and ";
                        }
                        else
                        {
                            text += ", ";
                        }
                    }
                    else
                    {
                        text += " and ";
                    }
                }

            }

            ++i;
        }

        return text;
    }
    #endregion

    #region actions
    public void Describe()
    {
        if (CanBeDescribed())
        {
            PhraseKey.WritePhrase("item_description");
        }
        else
        {
            PhraseKey.WritePhrase("item_noDescription");
        }
    }

    public bool CanBeDescribed()
    {
        int count = 0;

        if (HasProperties())
        {
            ++count;
        }

        foreach (var verb in Verb.GetVerbs)
        {
            foreach (var combination in verb.combinations)
            {
                if (combination.itemIndex == index)
                    ++count;

            }

        }
        return count != 0;
    }

    public string GetVerbsDescription()
    {
        string str = "You can ";

        List<Verb> verbList = new List<Verb>();

        foreach (var verb in Verb.GetVerbs)
        {
            foreach (var combination in verb.combinations)
            {
                if (combination.itemIndex == index)
                {
                    verbList.Add(verb);
                }

            }
        }

        for (int i = 0; i < verbList.Count; i++)
        {
            str += verbList[i].GetName;
            if (!string.IsNullOrEmpty(verbList[i].GetProposition))
            {
                str += " " + verbList[i].GetProposition;
            }

            if (verbList.Count > 1)
            {
                if (i == verbList.Count - 2)
                {
                    str += " and ";
                }
                else if (i < verbList.Count - 2)
                {
                    str += ", ";
                }
            }

        }

        str += " &the dog&";

        return str;
       
    }

    public string GetDescription()
    {
        if (HasProperties())
        {
            return GetPropertiesDescription();
        }
        else
        {
            return GetVerbsDescription();
        }
    }
    public string GetPropertiesDescription()
    {
        string str = "";

        if (HasProperties())
        {
            str += "\nIt's ";

            int enabledPropCount = GetEnabledProperties().Count;
            for (int i = 0; i < enabledPropCount; i++)
            {
                Property property = GetEnabledProperties()[i];

                if (!property.enabled)
                {
                    continue;
                }

                str += property.GetDescription();

                if (enabledPropCount > 1)
                {
                    if (i == enabledPropCount - 2)
                    {
                        str += " and ";
                    }
                    else if (i <enabledPropCount - 2)
                    {
                        str += ", ";
                    }
                }
            }
        }

        return str = TextUtils.FirstLetterCap(str);
    }
    #endregion

    /// properties ///
    
    #region properties
    // pour plus tard, update les propriétes sur l'objet
    // plutot que sur les prop ellesmemes ( pour le temps par ex )
    public void UpdateProperties()
    {
        foreach (var property in properties)
        {
            //property.UpdateParts();
        }
    }

    // adds whole new, simple property
    public Property CreateProperty(string line)
    {
        Property newProperty = new Property();
        string[] parts = line.Split(" / ");
        newProperty.type = parts[0];
        newProperty.name = parts[1];
        if ( parts.Length > 2)
        {
            newProperty.value = parts[2];
        }

        newProperty.Init();

        properties.Add(newProperty);
        return newProperty;
    }

    /// <summary>
    ///  WHAT THE FUCK JAI FAIS ça a 3 heures c'est pas bon mais je l'ai mis parce que ça permet
    ///  de réfléchir à une autre solution
    /// </summary>

    bool subbedToHours = false;
    bool subbedToRain = false;
    // add proper property, from the data, with events and all
    public Property CreateProperty(Property property_data)
    {
        Property newProperty = new Property(property_data);

        newProperty.Init();

        foreach (var propEvent in newProperty.events)
        {
            Debug.Log("subscribing " + newProperty.name + " to " + propEvent.name);

            switch (propEvent.name)
            {
                case "subHours":
                    if (subbedToHours)
                    {
                        continue;
                    }
                    TimeManager.GetInstance().onNextHour += HandleOnNextHour;
                    subbedToHours = true;
                    break;
                case "subRain":
                    if ( subbedToRain)
                    {
                        continue;
                    }
                    TimeManager.GetInstance().onRaining += HandleOnRaining;
                    subbedToRain=true;
                    break;
                default:
            Debug.Log("no event : " + propEvent.name);
                    break;
            }
        }
        properties.Add(newProperty);

        return newProperty;
    }
    public void DeleteProperty(string propertyName)
    {
        Property property = GetProperty(propertyName);
        if ( property == null)
        {
            Debug.LogError("DeleteProperty : property " + propertyName + " does not exist in " + debug_name);
            return;
        }
        properties.Remove(property);
    }
    public bool HasProperties()
    {
        return properties.FindAll(x=>x.enabled).Count > 0;
    }
    public bool HasProperty(string name)
    {
        return properties.Find(x => x.name == name && x.enabled) != null;
    }

    public List<Property> GetEnabledProperties()
    {
        return properties.FindAll(x=>x.enabled);
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
    #endregion

    #region events
    public List<Property> FindPropertyWithEvents(string eventName)
    {
        return properties.FindAll(x => x.ContainsEvent(eventName) && x.enabled);
    }
    public void HandleOnNextHour()
    {
        foreach (Property property in FindPropertyWithEvents("subHours"))
        {

            // decrease time
            int timeLeft = property.GetInt();
            --timeLeft;
            property.SetInt(timeLeft);

            // don't do anything if the time left is above 0
            if (timeLeft > 0)
            {
                continue;
            }

            EventManager.instance.CallEvent(property, "subHours", this);

            property.Disable();
        }
    }
    public void HandleOnRaining()
    {
        Debug.Log("raining");

        foreach (Property property in FindPropertyWithEvents("subRain"))
        {
            EventManager.instance.CallEvent(property, "subRain", this);
        }
    }
    #endregion

    public static Item GetItemOfType(string type)
    {
        Item[] items = Item.dataItems.FindAll(x => x.HasProperty(type)).ToArray();

        Item item = items[Random.Range(0, items.Length)];

        return item;
    }

    public string GetRelativePosition()
    {
        if (!HasProperty("direction"))
        {
            return "";
        }

        string itemPosition = "";

        Player.Orientation fac = Player.Orientation.None;

        switch (GetProperty("direction").value)
        {
            case "north":
                fac = Player.Instance.CardinalToOrientation(Cardinal.North);
                break;
            case "south":
                fac = Player.Instance.CardinalToOrientation(Cardinal.South);
                break;
            case "east":
                fac = Player.Instance.CardinalToOrientation(Cardinal.East);
                break;
            case "west":
                fac = Player.Instance.CardinalToOrientation(Cardinal.West);
                break;
            default:
                break;
        }

        switch (fac)
        {
            case Player.Orientation.Front:
                return PhraseKey.GetPhrase("position_front");
            case Player.Orientation.Right:
                return PhraseKey.GetPhrase("position_right");
            case Player.Orientation.Back:
                return PhraseKey.GetPhrase("position_back");
            case Player.Orientation.Left:
                return PhraseKey.GetPhrase("position_left");
            default:
                break;
        }

        return itemPosition;
    }

}