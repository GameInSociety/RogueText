using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public static List<Item> items = new List<Item>();

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
    public List<string> itemPositions = new List<string>();

    /// <summary>
    /// exemples :
    /// dans une forêt : "derrière l'arbre", ou "dans un buisson"
    /// dans un sac : (donc autre objet) au fond du sac, etc...
    /// </summary>
    public List<string> positionInItems = new List<string>();

    public List<Item> containedItems;

    public Word word;
    public bool stackable = false;
    public bool unique = false;

    /// <summary>
    /// container
    /// </summary>
    public bool emptied = false;

    public Item()
    {

    }
    public static Item CreateNewItem(Item copy)
    {
        Item newItem = new Item();

        newItem.index = copy.index;
        newItem.weight = copy.weight;
        newItem.value = copy.value;
        newItem.usableAnytime = copy.usableAnytime;
        newItem.appearInfos = copy.appearInfos;
        newItem.itemPositions = copy.itemPositions;
        newItem.positionInItems = copy.positionInItems;
        newItem.word = copy.word;
        newItem.stackable = copy.stackable;
        newItem.unique = copy.unique;

        return newItem;
    }


    #region remove
    public static void Remove(Item targetItem)
    {
        if (Container.opened)
        {
            Debug.Log("target item hash code : " + targetItem.GetHashCode());

            foreach (var containedItem in Container.CurrentItem.containedItems)
            {
                Debug.Log("cotnained hash code : " + containedItem.GetHashCode());
            }

            if (Container.CurrentItem.containedItems.Contains(targetItem))
            {
                Container.CurrentItem.RemoveItem(targetItem);
            }

            Container.Describe();

            return;
        }

        if (Tile.current.items.Contains(targetItem))
        {
            Debug.Log("removing item from tile");

            Tile.current.RemoveItem(targetItem);

            Tile.Describe ();
            return;
        }

        if (Inventory.Instance.items.Contains(targetItem))
        {
            Debug.Log("from inventory");

            Inventory.Instance.RemoveItem(targetItem);

            Inventory.Instance.Describe();
            return;
        }

        Debug.Log("from npthing");
    }
    #endregion

    public class AppearInfo
    {
        public int itemIndex = 0;
        public int rate = 0;
        public int amount = 0;

        public Item GetItem()
        {
            return Item.items[itemIndex];
        }

        public bool CanAppear()
        {
            

            return false;
        }
    }

    #region list
    public static string ItemListString(List<Item> _items, bool separate, bool displayWeight)
    {
        string text = "";

        int i = 0;

        foreach (var item in _items)
        {
            text += item.word.GetContent(Word.ContentType.JustWord, Word.Definition.Undefined, Word.Preposition.None, Word.Number.None);

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
    public static string ItemListString(List<ItemSocket> _itemSockets, bool separateWithLigns, bool displayWeight)
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

    #region search
    public static Item FindInWorld(string str)
    {
        Item item = null;

        // dans un container
        if (Container.opened)
        {
            Debug.Log("looking in container");

            item = Container.CurrentItem.containedItems.Find(x => x.word.text.StartsWith(str));
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

        if (item == null)
        {
            item = FindUsableAnytime(str);
        }

        // et en dernier s'il est fermé
        if (item == null)
        {
            item = FindInInventory(str);
        }

        if ( item == null)
        {

        }

        return item;
    }

    public static Item FindInTile(string str)
    {
        // is the item the exact same tile as the one we're in ?
        if (Tile.current.tileItem.word.text.StartsWith(str))
        {
            return Tile.current.tileItem;
        }

        // is the item one of the surrounding tiles ?
        foreach (var tileGroup in TileGroupDescription.tileGroups)
        {
            if (tileGroup.tile.tileItem.word.text.StartsWith(str))
            {
                return tileGroup.tile.tileItem;
            }
        }

        List<Item> items = Tile.current.items.FindAll(x => x.word.text.StartsWith(str));

        if (items.Count == 0)
        {
            items = Tile.current.items.FindAll(x => x.word.GetContent(Word.ContentType.JustWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.Plural).StartsWith(str));
        }


        /// chercher les adjectifs pour différencier les objets ( porte bleu, porte rouge )
        if (items.Count > 0)
        {

            foreach (var inputPart in DisplayInput.Instance.inputParts)
            {
                foreach (var item in items)
                {
                    string adjSTR = item.word.GetAdjective.GetContent(item.word.genre, Word.Number.Singular);

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

    public static Item FindUsableAnytime(string str)
    {
        return items.Find(x => x.word.text.StartsWith(str) && x.usableAnytime);
    }

    public static Item FindInInventory(string str)
    {
        Item item = Inventory.Instance.items.Find(x => x.word.text.StartsWith(str.ToLower()));

        if (item == null)
            return null;

        return item;
    }

    public static Item FindByName(string str)
    {
        str = str.ToLower();

        Item item = items.Find(x => x.word.text.ToLower() == str);

        if (item == null)
        {
            // find plural
            item = items.Find(x => x.word.GetPlural() == str);

            if (item != null)
            {
                //Debug.Log("found plural");
                InputInfo.GetCurrent.actionOnAll = true;
                return item;
            }
        }

        if (item == null)
        {
            
        }

        return item;
    }
    public static List<Item> FindAllByName(string str)
    {
        str = str.ToLower();

        return items.FindAll(x => x.word.text.StartsWith(str));
    }
    #endregion

    public string GetItemPosition()
    {
        // si l'objet a une position prédéfinie dans la tile ( ex : armoire => prés du mur etc... )
        if (itemPositions.Count > 0)
        {
            return itemPositions[Random.Range(0, itemPositions.Count)];
        }
        // si le lieu a des phrases attitrées ( ex : salle de bain => évier , chambre => lit etc... )
        //else if (Location.GetLocation(Tile.current.type).itemPositions.Count > 0)
        else if (Tile.current.tileItem.positionInItems.Count > 0)
        {
            return Tile.current.tileItem.positionInItems[Random.Range(0, Tile.current.tileItem.positionInItems.Count)];
        }

        // prendre une position générique ( ex : à quelques pas , non loin etc... )
        return PhraseManager.Instance.positionPhrases[Random.Range(0, PhraseManager.Instance.positionPhrases.Length)];
    }

    #region container
    public void GenerateItems()
    {
        if ( containedItems != null)
        {
            return;   
        }

        Debug.Log("first time open");
        containedItems = new List<Item>();

        foreach (var appearInfo in appearInfos)
        {
            for (int i = 0; i < appearInfo.amount; i++)
            {
                if (Random.value * 100f < appearInfo.rate)
                {
                    Item newItem = Item.CreateNewItem(appearInfo.GetItem());

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

    public string GetContainedItemsDescription()
    {
        string word_str = word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None);

        if (containedItems.Count == 0)
        {
            return "Il n'y a rien dans " + word_str;
        }

        string str = "Dans " + word_str + " : vous voyez : \n" +
            "" + Item.ItemListString(containedItems, true, true);

        return str;
    }

    public void RemoveItem (Item item)
    {
        containedItems.Remove(item);
    }
    public bool SameTypeAs( Item otherItem)
    {
        return otherItem.index == index;
    }
    public bool ExactSameAs(Item otherItem)
    {
        return otherItem == this;
    }
    #endregion
}