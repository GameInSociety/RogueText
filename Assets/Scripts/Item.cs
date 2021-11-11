using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int value = 0;
    public bool usableAnytime = false;

    public string inputToFind = "str";

    public List<AppearInfo> appearInfos = new List<AppearInfo>();

    /// <summary>
    /// exemples :
    /// une carrote ou bouteille d'eau se trouverait souvent sur une table ou quoi
    /// une flaque se trouverait souvent par terre
    /// </summary>
    private Socket socket;

    /// <summary>
    /// exemples :
    /// dans une forêt : "derrière l'arbre", ou "dans un buisson"
    /// dans un sac : (donc autre objet) au fond du sac, etc...
    /// </summary>
    public List<Socket> sockets = new List<Socket>();

    public List<Item> containedItems;

    public Word word;
    public bool stackable = false;

    /// <summary>
    /// container
    /// </summary>
    public bool emptied = false;

    /// <summary>
    /// properties
    /// </summary>
    public List<Property> properties = new List<Property>();

    public Item()
    {

    }

    #region container
    public void GenerateItems()
    {
        if (containedItems != null)
        {
            return;
        }

        containedItems = new List<Item>();

        foreach (var appearInfo in appearInfos)
        {
            for (int i = 0; i < appearInfo.amount; i++)
            {
                if (Random.value * 100f < appearInfo.rate)
                {
                    Item newItem = Item.CreateNew(appearInfo.GetItem());

                    Debug.Log("new item hash code : " + newItem.GetHashCode());

                    containedItems.Add(newItem);
                }
            }
        }

    }


    public void Open()
    {
        Item item = InputInfo.GetCurrent.MainItem;
        item.GenerateItems();

        Container.opened = true;
        Container.CurrentItem = item;

        Container.Describe();
    }

    public void Close()
    {
        // toujours dans la classe inventory.cs pour l'intsant
    }

    public void WriteContainedDescription()
    {
        if (containedItems.Count == 0)
        {
            Phrase.Write("Il n'y a rien dans &le chien (main item)&");
            return;
        }

        Phrase.Write("Dans &le chien (main item)& : vous voyez : \n" +
            "" + Item.ItemListString(containedItems, true, true));
    }

    public void RemoveItem(Item item)
    {
        containedItems.Remove(item);
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
    public class AppearInfo
    {
        public int itemIndex = 0;
        public int rate = 0;
        public int amount = 0;

        public Item GetItem()
        {
            return Item.dataItems[itemIndex];
        }

        public bool CanAppear()
        {
            return false;
        }
    }

    public Socket GetSocket()
    {
        if (socket == null)
        {
            socket = Socket.GetRandomSocket(this);
        }

        return socket;
    }
    #endregion

    ///
    /// <summary>
    /// TOOLS
    /// </summary>
    ///

    #region tools
    public static Item CreateNewItem(string name)
    {
        Item item = GetDataItem(name);
        return CreateNew(item);
    }

    public static Item CreateNew(Item copy)
    {
        // common to all
        Item newItem = new Item();

        newItem.debug_name = copy.debug_name;
        newItem.index = copy.index;
        newItem.weight = copy.weight;
        newItem.value = copy.value;
        newItem.usableAnytime = copy.usableAnytime;
        newItem.appearInfos = copy.appearInfos;
        newItem.socket = copy.socket;
        newItem.sockets = copy.sockets;
        newItem.word = new Word(copy.word);
        newItem.stackable = copy.stackable;

        // unique
        newItem.properties = new List<Property>(copy.properties);

        foreach (var _property in newItem.properties)
        {
            _property.Init(newItem);
        }

        return newItem;
    }
    #endregion

    public void PickUp()
    {
        if (Inventory.Instance.weight + weight > Inventory.Instance.maxWeight)
        {
            string str = "&le chien (main item)& est trop lourd pour le sac, il ne rentre pas";
            Phrase.Write(str);
            return;
        }

        Item.Remove(this);

        Inventory.Instance.AddItem(this);

        Phrase.Write("Vous avez pris : &le chien (main item)&");
    }

    #region remove
    public static void Remove(Item targetItem)
    {
        // first search thing in opened container
        if (Container.opened)
        {
            if (Container.CurrentItem.containedItems.Contains(targetItem))
            {
                Container.CurrentItem.RemoveItem(targetItem);
            }

            Container.Describe();

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
        if (Inventory.Instance.items.Contains(targetItem))
        {
            Inventory.Instance.RemoveItem(targetItem);

            Inventory.Instance.WriteDescription();
            return;
        }

        Debug.LogError("removing item : " + targetItem.word.text + " failed : not in container, tile or inventory");
    }
    #endregion

    #region search
    public static Item FindInWorld(string str)
    {
        Item item = null;

        // dans un container
        if (Container.opened)
        {
            item = Container.CurrentItem.containedItems.Find(x => x.word.Compare(str));
        }

        // chercher une premiere fois dans l'inventaire s'il est ouvert
        if (Inventory.Instance.opened)
        {
            item = FindInInventory(str);
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
            item = FindInInventory(str);
        }

        if (item == null)
        {
            item = FindUsableAnytime(str);
        }

        if (item == null)
        {
            //
        }

        return item;
    }

    private static Item FindInTile(string str)
    {
        // is the item the exact same tile as the one we're in ?
        if (Tile.GetCurrent.tileItem.word.Compare(str))
        {
            return Tile.GetCurrent.tileItem;
        }

        // is the item one of the surrounding tiles ?
        foreach (var tileGroup in TileGroupDescription.tileGroups)
        {
            if (tileGroup.tile.tileItem.word.Compare(str))
            {
                return tileGroup.tile.tileItem;
            }
        }

        List<Item> items = Tile.GetCurrent.items.FindAll(x => x.word.Compare(str));

        /// ADJECTIVES ///

        /// chercher les adjectifs pour différencier les objets ( porte bleu, porte rouge )
        if (items.Count > 0)
        {
            foreach (var inputPart in InputInfo.parts)
            {
                foreach (var item in items)
                {
                    if (!item.word.HasAdjective())
                    {
                        continue;
                    }

                    string adjSTR = item.word.GetAdjective.GetContent(item.word.genre, false);

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
        return dataItems.Find(x => x.word.Compare(str) && x.usableAnytime);
        //return items.Find(x => x.word.Compare(str) );
    }

    private static Item FindInInventory(string str)
    {
        Item item = Inventory.Instance.items.Find(x => x.word.Compare(str));

        if (item == null)
            return null;

        return item;
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
    static Item TryGetItem(string _name)
    {
        _name = _name.ToLower();

        Item item = dataItems.Find(x => x.word.text.ToLower() == _name);

        if (item == null)
        {
            // find plural
            item = dataItems.Find(x => x.word.GetPlural() == _name);

            if (item != null)
            {
                // found the plural
                //Debug.Log("found plural");
                InputInfo.GetCurrent.actionOnAll = true;
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
    #endregion

    #region list
    public static string ItemListString(List<Item> _items, bool separate, bool displayWeight)
    {
        string text = "";

        int i = 0;

        foreach (var item in _items)
        {
            text += item.word.GetContent("chien");

            if (displayWeight)
            {
                text += " (w:" + (item.weight) + ")";
            }

            if (_items.Count > 1 && i < _items.Count - 1)
            {
                if (separate)
                {
                    text += "\n";
                }
                else
                {
                    if (_items.Count > 2)
                    {
                        if (i == _items.Count - 2)
                        {
                            text += " et ";
                        }
                        else
                        {
                            text += ", ";
                        }
                    }
                    else
                    {
                        text += " et ";
                    }
                }

            }

            ++i;
        }

        return text;
    }
    public static string ItemListString(List<ItemGroup> _itemSockets, bool separateWithLigns, bool displayWeight)
    {
        string text = "";

        int i = 0;

        foreach (var itemSocket in _itemSockets)
        {
            text += itemSocket.GetWordGroup();

            if (displayWeight)
            {
                text += " (w:" + (itemSocket.item.weight * itemSocket.count) + ")";
            }

            if (_itemSockets.Count > 1 && i < _itemSockets.Count - 1)
            {
                if (separateWithLigns)
                {
                    text += "\n";
                }
                else
                {
                    if (_itemSockets.Count > 2)
                    {
                        if (i == _itemSockets.Count - 2)
                        {
                            text += " et ";
                        }
                        else
                        {
                            text += ", ";
                        }
                    }
                    else
                    {
                        text += " et ";
                    }
                }

            }

            ++i;
        }

        return text;
    }
    #endregion

    #region actions
    public static void Describe (Item item)
    {
        string str = "";

        int count = 0;

        if (item.HasProperties())
        {
            foreach (var property in item.properties)
            {
                str += property.GetPhrase() + "\n";
            }

            ++count;
        }

        str += "\nVous pouvez ";

        foreach (var verb in Verb.GetVerbs)
            {
                foreach (var combination in verb.combinations)
                {
                    if (combination.itemIndex == item.index)
                    {
                        str += verb.names[0] + ", ";
                        ++count;
                    }

                }

            }

        if (count == 0)
        {
            str = "Vous ne vous pouvez pas faire grand chose avec &le chien (main item)&";
        }
        else
        {
            str += " &le chien (main item)&";
        }

        Phrase.Write(str);
    }
    #endregion

    /// properties ///
    
    #region properties
    /// <summary>
    /// static functions
    /// </summary>
    /// <param name="propName"></param>
    /// <param name="newValue"></param>
    public static void ChangeProperty()
    {
        ChangeProperty( PlayerAction.GetCurrent.GetContent(0) , PlayerAction.GetCurrent.GetContent(0));
    }
    public static void ChangeProperty(string propName, string newValue)
    {
        Item targetItem = InputInfo.GetCurrent.MainItem;

        if (!targetItem.HasProperty(propName))
        {
            Debug.LogError(targetItem.debug_name + " doesn't have the property " + newValue);
            return;
        }

        if (targetItem.GetProperty(propName).GetContent() == newValue)
        {
            Phrase.Write("&le chien (main item)& est déjà " + targetItem.GetProperty(propName).GetText());
            return;
        }

        targetItem.GetProperty(propName).SetContent(newValue);

        string str = targetItem.GetProperty(propName).GetPhrase();

        Phrase.Write( str );
    }

    public void AddProperty(string name, string value)
    {
        Property newProperty = new Property();
        newProperty.name = name;
        newProperty.SetContent(value);
        AddProperty(newProperty);
    }

    public void AddProperty(Property property)
    {
        properties.Add(property);
    }

    public void RemoveProperty(string name)
    {
        Property prop = properties.Find(x => x.name == name);

        if (prop == null)
        {
            Debug.LogError("property : " + name + " hasn't been found");
        }
    }

    public bool HasProperties()
    {
        return properties.Count > 0;
    }

    public bool HasProperty(string name)
    {
        return properties.Find(x => x.name == name) != null;
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
    #endregion

    /// <summary>
    /// PROPERTY
    /// </summary>
    [System.Serializable]
    public class Property
    {
        public enum Type
        {
            boolean,
            delay,
            value
        }
        public Type type;
        public string name;
        private string content;
        public string param;

        public Item item;

        // initialisation
        public void Init(Item _item)
        {
            item = _item;

            switch (type)
            {
                case Type.boolean:

                    // si la valeur n'est pas assignée ça doit être un pourçentage, donc assigner au hasard
                    if (content.Contains("%"))
                    {
                        string percentSTR = content.Remove(content.Length - 1);

                        int percent = int.Parse(percentSTR);

                        content = Random.value * 100 < percent ? "true" : "false";
                    }

                    if (content != "true" && content != "false")
                    {
                        Debug.LogError("content for boolean invalid : " + content + " l("+content.Length+")");
                    }

                    break;
                case Type.delay:

                    // si la propriété est un delai, s'abonner au temps
                    TimeManager.GetInstance().onNextHour += HandleOnNextHour;

                    break;
                default:
                    break;
            }
        }

        // setters
        public void SetContent(string _content)
        {
            content = _content;
        }

        public void SetValue(int _value)
        {
            content = _value.ToString();
        }

        #region numeric
        public int GetValue()
        {
            int i = 0;

            if (int.TryParse(content, out i))
            {
                return i;
            }
            else
            {
                Debug.LogError("couldn't parse : " + content + " in property : " + name);
                return i;
            }
        }
        public void Add(int i)
        {
            SetValue(GetValue() + i);
        }
        public void Remove(int i)
        {
            SetValue(GetValue() - i);
        }
        #endregion

        #region property text
        public string GetContent()
        {
            return content;
        }

        /// <summary>
        /// text
        /// </summary>
        /// <returns></returns>
        public string GetPhrase()
        {
            string str = "";

            switch (type)
            {
                case Type.boolean:
                    str += Phrase.Replace("&le chien (main item)& est " + GetText());
                    break;
                case Type.delay:

                    string text;
                    Item paramItem = Item.TryGetItem(param);
                    if ( paramItem == null)
                    {
                        text = "Il reste " + GetText() + " heures avant que &le chien (main item)& devienne "+param;
                        text = Phrase.Replace(text);
                    }
                    else
                    {
                        Phrase.SetOverrideItem(paramItem);
                        text = "Il reste " + GetText() + " avant que &le chien (main item)& devienne &un chien (override item)&";
                        text = Phrase.Replace(text);
                    }

                    
                    str += text;

                    break;
                case Type.value:
                    str += Phrase.Replace("&le chien (main item)& contient " + GetText() + " eau");
                    break;
                default:
                    str = "<color=red>no type error</color>";
                    break;
            }

            return str;
        }


        public string GetDebugText()
        {
            return "property of : " + item.word.text + " / " + name + " : " + content;
        }

        public string GetText()
        {
            switch (type)
            {
                case Type.boolean:

                    string[] parts = param.Split('?');
                    return content == "true" ? parts[0] : parts[1];

                case Type.delay:

                    return content + " heures";

                case Type.value:

                    return content;

                default:
                    return "<color=red>no type error</color>";
            }
            
        }

        /// <summary>
        /// ça a pas grand chose à foutre là quand on y pense
        /// </summary>
        public void HandleOnNextHour()
        {
            // en soit à terme, il faudrait que la fonction soit dans la case elle même
            // delay/grow/10/BecomeItem(carotte)
            // pas si compliqué que ça quand on y pense

            // decrease time
            int timeLeft = GetValue();
            --timeLeft;
            SetContent(timeLeft.ToString());

            // don't do anything if the time left is above 0
            if (timeLeft > 0)
            {
                return;
            }
            TimeManager.GetInstance().onNextHour -= HandleOnNextHour;


            // ici devraient être les actions
            switch (name)
            {
                case "dry":
                    Gardening.Dry(this);
                    break;
                case "grow":
                    Gardening.Grow(this);
                    break;
                default:
                    //Debug.LogError("timer property : " + name + " is dead end");
                    break;
            }
        }
    }
    #endregion

}