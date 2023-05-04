using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[System.Serializable]
public class Item
{
    public string debug_name = "debug name";

    /// <summary>
    /// declaration
    /// </summary>
    public int dataIndex;
    // maybe should be a property ?
    public int weight = 0;
    public bool usableAnytime = false;

    /// <summary>
    /// exemples :
    /// dans une forêt : "derrière l'arbre", ou "dans un buisson"
    /// dans un sac : (donc autre objet) au fond du sac, etc...
    /// </summary>
    /// Je crois qu'il vaut mieux que ces trucs soient dans les sockets
    /// ( liste d'int dans le socket parce quand y'a pas de copy dans les sockets )
    ///c'est déjàle cas maisc'estpar rapport aux tiles
    // pareil
    public List<Item> GetContainedItems
    {
        get
        {
            if (mContainedItems == null)
            {
                mContainedItems = new List<Item>();
            }

            return mContainedItems;
        }
    }
    [SerializeField]
    private List<Item> mContainedItems;

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
    public static Item OpenedItem;
    public static bool AnItemIsOpened
    {
        get
        {
            return OpenedItem!= null;
        }
    }
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
        bool b = GetContainedItems != null && GetContainedItems.Count > 0;
        return b;
    }
    public virtual void GenerateItems()
    {
        if (ContainsItems())
        {
            return;
        }

        foreach (var itemInfo in AppearInfo.GetAppearInfo(dataIndex).itemInfos)
        {
            for (int i = 0; i < itemInfo.amount; i++)
            {
                if (Random.value * 100f < itemInfo.rate)
                {
                    ItemManager.Instance.CreateInItem(this, itemInfo.GetItemName());
                }
            }
        }

    }

    public void AddItem(Item item)
    {

        GetContainedItems.Add(item);

    }

    // remove item
    public void RemoveItem(Item item)
    {
        GetContainedItems.Remove(item);
    }
    // item row : wtf ? le craft system est pas ouf
    public void RemoveItem(int itemRow)
    {
        RemoveItem(GetContainedItems.Find(x => x.dataIndex == itemRow));
    }
    //

    public Item FindItem(string str)
    {
        Item item = GetContainedItems.Find(x => x.HasWord(str));

        if (item == null)
            return null;

        return item;
    }

    public Item GetItem(string itemName)
    {
        return GetContainedItems.Find(x => x.word.text == itemName);
    }

    public bool ContainsItem( Item item)
    {
        return GetContainedItems.Contains(item);
    }

    /// <summary>
    ///  WEIGHT
    /// </summary>
    public int GetContainedItemWeight()
    {
        int w = 0;
        foreach (var item in GetContainedItems)
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
        GenerateItems();

        if (ContainsItems())
        {
            OpenedItem = this;
            WriteContainedItemDescription();
            return;
        }

        Debug.Log(debug_name + " is empty");
        TextManager.WritePhrase("container_empty");
    }

    public void Close()
    {
        // toujours dans la classe inventory.cs pour l'intsant
        if (!AnItemIsOpened)
        {
            TextManager.WritePhrase("container_alreadyClosed");
            return;
        }

        TextManager.WritePhrase("container_clsose", Item.OpenedItem);

        OpenedItem = null;
    }
    ///

    /// <summary>
    /// DESCRIPTION
    /// </summary>
    public virtual void WriteContainedItemDescription()
    {
        Socket socket = new Socket();
        socket._text = "&on the dog&";

        SocketManager.Instance.DescribeItems(GetContainedItems, socket);
    }

    public bool SameTypeAs(Item otherItem)
    {
        return otherItem.dataIndex == dataIndex;
    }
    public bool ExactSameAs(Item otherItem)
    {
        return otherItem == this;
    }
    #endregion

    
    ///
    /// <summary>
    /// TOOLS
    /// </summary>
    ///


    public void PickUp()
    {
        if (Inventory.Instance.GetContainedItemWeight() + weight > Inventory.Instance.maxWeight)
        {
            TextManager.WritePhrase("inventory_TooHeavy");
            return;
        }

        Item.Remove(this);

        Inventory.Instance.AddItem(this);

        TextManager.WritePhrase("inventory_PickUp");
    }

    #region remove & destroy
    public static void Destroy(Item item){
        
        Remove(item);
    }
    public static void Remove(Item targetItem)
    {
        // first search thing in opened container
        if (AnItemIsOpened)
        {
            if (Item.OpenedItem.GetContainedItems.Contains(targetItem))
            {
                Item.OpenedItem.RemoveItem(targetItem);
            }
            return;
        }

        // then in tile
        if (Tile.GetCurrent.GetContainedItems.Contains(targetItem))
        {
            Tile.GetCurrent.RemoveItem(targetItem);

            //DisplayDescription.Instance.UpdateDescription();
            return;
        }

        // then in inventory
        if (Inventory.Instance.ContainsItem(targetItem))
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
    
    public bool IsAnItem(string item_name)
    {
        return ItemManager.Instance.TryGetItem(item_name) != null;
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
                if (combination.itemIndex == dataIndex)
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
                if (combination.itemIndex == dataIndex)
                {
                    verbList.Add(verb);
                }

            }
        }

        for (int i = 0; i < verbList.Count; i++)
        {
            str += verbList[i].GetName;
            if (!string.IsNullOrEmpty(verbList[i].GetPreposition))
            {
                str += " " + verbList[i].GetPreposition;
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
    public void WritePropertiesDescription()
    {
        TextManager.WritePhrase(GetPropertiesDescription(), this);

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

        int enabledPropCount = GetEnabledProperties().Count;

        if (enabledPropCount > 0)
        {
            str += "&the dog (override item)& is ";

            for (int i = 0; i < enabledPropCount; i++)
            {
                Property property = GetEnabledProperties()[i];

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
    // add proper property, from the data, with events and all
    public Property CreateProperty(Property property_data)
    {
        Property newProperty = new Property(property_data);

        newProperty.Init();

        if ( newProperty.events != null)
        {
            EventManager.instance.AddPropertyEvent(newProperty, this);
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
    public bool HasVisibleProperties()
    {
        return properties.FindAll(x => x.enabled && x.type != "hidden").Count > 0;
    }
    public bool HasProperty(string name)
    {
        Property property = properties.Find(x => x.name == name);

        return property != null;
    }

    public List<Property> GetEnabledProperties()
    {
        // ici "type == container" veut dire que la battery ou water des items est décrite meme si le truc est disabled,
        // à terme, mettre un parametre qui fait que la prop est décrite meme quand l'item est disabled
        // ou un autre truc mieux
        return properties.FindAll(x => x.enabled||x.type == "container");
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
    public bool HasItemWithProperty( string propertyName)
    {
        return GetContainedItems.Find(x=> x.HasProperty(propertyName)) != null;
    }
    #endregion

    public static Item GetItemOfType(string type)
    {
        Item[] items = ItemManager.Instance.dataItems.FindAll(x => x.HasProperty(type)).ToArray();

        Item item = items[Random.Range(0, items.Length)];

        return item;
    }

    public string GetRelativePosition()
    {
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
                return "front";
            case Player.Orientation.Right:
                return "right";
            case Player.Orientation.Back:
                return "behind";
            case Player.Orientation.Left:
                return "left";
            default:
                return "";

            /*case Player.Orientation.Front:
                return TextManager.GetPhrase("position_front");
            case Player.Orientation.Right:
                return TextManager.GetPhrase("position_right");
            case Player.Orientation.Back:
                return TextManager.GetPhrase("position_back");
            case Player.Orientation.Left:
                return TextManager.GetPhrase("position_left");
            default:
                return "";*/
        }

    }

}