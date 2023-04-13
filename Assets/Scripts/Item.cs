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
    public bool ContainsItems()
    {
        GenerateItems();

        return containedItems != null && containedItems.Count > 0;
    }
    public void GenerateItems()
    {
        if (containedItems != null)
        {
            return;
        }

        foreach (var appearInfo in appearInfos)
        {
            for (int i = 0; i < appearInfo.amount; i++)
            {
                if (Random.value * 100f < appearInfo.rate)
                {
                    if ( containedItems == null)
                    {
                        containedItems = new List<Item>();
                    }

                    Item newItem = Item.CreateNew(appearInfo.GetItem());
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
        Container.Describe(item);
    }

    public void Close()
    {
        // toujours dans la classe inventory.cs pour l'intsant
        if (!Container.opened)
        {
            Phrase.Write("&le chien (main item)& est déjà fermé");
            return;
        }

        Container.opened = false;
        Phrase.SetOverrideItem(Container.CurrentItem);
        Phrase.Write("Vous fermez &le chien (override item)&");
    }

    public void WriteContainedDescription()
    {
        if (!ContainsItems())
        {
            Phrase.Write("Il n'y a rien dans &le chien (main item)&");
            return;
        }

        string str = "Dans &le chien (main item)& : vous voyez : ";
        str += Item.ItemListString(containedItems, ListSeparator.Commas, true);

        Phrase.Write(str);
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
    [System.Serializable]
    public class AppearInfo
    {
        public int itemIndex = 0;
        public int rate = 0;
        public int amount = 1;

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
    
    /// create a new item by name
    public static Item CreateNew(string name)
    {
        Item item = GetDataItem(name);
        return CreateNew(item);
    }

    // create  new item with a copy
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
            return;
        }

        Debug.LogError("removing item : " + targetItem.word.text + " failed : not in container, tile or inventory");
    }
    #endregion

    #region search
    public static Item FindInWorld(string str)
    {
        Item item = null;

        bool foundInContainer = false;

        // dans un container
        if (Container.opened)
        {
            item = Container.CurrentItem.containedItems.Find(x => x.word.Compare(str));
            if ( item != null)
            {
                foundInContainer = true;
            }
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
    public static Item TryGetItem(string _name)
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
            text += item.word.GetContent("un chien");

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

        /*if (item.ContainsItems())
        {
            Container.Describe(item);
        }*/

        if (item.HasProperties())
        {
            foreach (var property in item.properties)
            {
                property.Write();
            }

            ++count;
        }

        str += "Vous pouvez ";

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
            str += "&le chien (main item)&";
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

    public static Item GetItemOfType(string type)
    {
        Item[] items = Item.dataItems.FindAll(x => x.HasProperty(type)).ToArray();

        Item item = items[Random.Range(0, items.Length)];

        return item;
    }

}