using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ItemManager : MonoBehaviour {

    public List<Item> dataItems = new List<Item>();

    public Item TryGetItem(string _name)
    {
        _name = _name.ToLower();

        // to find the word on ly
        //Item item = dataItems.Find(x => x.word.text.ToLower() == _name);

        // to search also in syonyms
        Item item = dataItems.Find(x =>
        x.HasWord(_name)
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
    public List<Item> FindAllByName(string str)
    {
        str = str.ToLower();

        return dataItems.FindAll(x => x.word.text.StartsWith(str));
    }
    public Item FindByName(string str)
    {
        str = str.ToLower();
        return dataItems.Find(x => x.word.text.StartsWith(str));

    }
    public Item FindInWorld(string str)
    {
        Item item = null;

        //bool foundInContainer = false;

        // dans un container
        if (Item.AnItemIsOpened)
        {
            item = Item.OpenedItem.FindItem(str);
            /*if (item != null)
            {
                foundInContainer = true;
            }*/
        }


        // chercher une premiere fois dans l'inventaire s'il est ouvert
        if (Inventory.Instance.opened)
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
            item = ItemManager.Instance.FindUsableAnytime(str);
        }

        // pour éviter que le container reste ouvert si on fait une autre action
        /*if (item != null && Item.AnItemIsOpened && !foundInContainer)
        {
            Item.OpenedItem.Close();
        }*/

        return item;
    }

    public Tile CreateTile(Coords _coords, string tileName)
    {
        Item item = CreateFromData(tileName);

        var serializedParent = JsonConvert.SerializeObject(item);
        Tile newTile = JsonConvert.DeserializeObject<Tile>(serializedParent);
        newTile.coords = _coords;

        return newTile;
    }


    private static Item FindInTile(string str)
    {
        // is the item the exact same tile as the one we're in ?
        if (Tile.GetCurrent.HasWord(str))
        {
            return Tile.GetCurrent;
        }

        //List<Item> items = Player.Instance.SurroundingTiles().FindAll();

        // is the item one of the surrounding tiles ?
        foreach (var tile in Player.Instance.SurroundingTiles())
        {
            if (tile.HasWord(str))
            {
                return tile;
            }
        }

        List<Item> items = Tile.GetCurrent.GetContainedItems.FindAll(x => x.word.Compare(str));


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


    public Item GetDataItem(string _name)
    {
        Item item = TryGetItem(_name);

        if (item == null)
        {
            Debug.LogError("couldn't find item : " + _name);
        }
        else
        {
        }

        return item;

    }

    public Item FindUsableAnytime(string str)
    {
        if ( str == "inventory")
        {
            return Inventory.Instance;
        }

        return dataItems.Find(x => x.HasWord(str) && x.usableAnytime);
    }

    public Item CreateInTile(Tile tile, string itemName)
    {
        Item newItem = CreateFromData(itemName);
        tile.AddItem(newItem);
        return newItem;
    }
    public Item CreateInItem(Item item, string itemName)
    {
        Item newItem = CreateFromData(itemName);
        item.AddItem(newItem);
        return newItem;
    }

    public List<AppearInfo> appearInfos = new List<AppearInfo>();

    /// create a new item by name
    public Item CreateFromData(string name)
    {
        Item copy = GetDataItem(name);

        // common to all
        Item newItem = new Item();

        newItem.debug_name = copy.debug_name;
        newItem.dataIndex = copy.dataIndex;
        newItem.weight = copy.weight;
        newItem.usableAnytime = copy.usableAnytime;

        // the word never changes, non ? pourquoi en copy
        newItem.words = copy.words;

        newItem.stackable = copy.stackable;

        // unique
        foreach (var prop_copy in copy.properties)
        {
            newItem.CreateProperty(prop_copy);
        }

        return newItem;
    }

    public List<Item> FindItemsWithProperty(string propName)
    {
        return dataItems.FindAll(x => x.HasProperty(propName));
    }

    private static ItemManager _instance;
    public static ItemManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ItemManager>().GetComponent<ItemManager>();
            }

            return _instance;
        }
    }
}
