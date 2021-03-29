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

    public List<Item> containedItems = new List<Item>();

    public Word word;

    private Adjective adjective;
    public bool stackable = true;

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

        if (copy.adjective == null)
        {
            newItem.SetRandomAdj();
        }
        else
        {
            newItem.adjective = copy.adjective;
        }

        return newItem;
    }

    public string GetWord()
    {
        string wordStr = word.GetContent(Word.ContentType.JustWord, Word.Definition.Undefined, Word.Preposition.None, Word.Number.None);

        if (!stackable)
        {
            string adjStr = GetAdjective.GetContent(word.genre, Word.Number.Singular);

            string wordGroup = wordStr + " " + adjStr;

            if (GetAdjective.beforeWord)
            {
                wordGroup = adjStr + " " + wordStr;
            }

            return wordGroup;
        }

        return wordStr;
    }


    #region remove
    public static void Remove(Item item)
    {
        if (Container.opened)
        {
            Debug.Log("from container");

            if (Container.openedItem.containedItems.Contains(item))
            {
                Container.openedItem.RemoveItem(item);
            }
            return;
        }

        foreach (var crotin in Tile.current.items)
        {
            Debug.Log( "all items in tile : " + crotin.word.text );
        }

        if (Tile.current.items.Contains(item))
        {
            Debug.Log("remobing item from time");

            Tile.current.RemoveItem(item);
            return;
        }

        if (Inventory.Instance.items.Contains(item))
        {
            Debug.Log("from inventory");

            Inventory.Instance.RemoveItem(item);
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
            text += item.GetWord();

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
    public static Item GetInWorld(string str)
    {
        Item item = null;

        if (Container.opened)
        {
            item = Container.openedItem.containedItems.Find(x => x.word.text.StartsWith(str));
        }

        // chercher une premiere fois dans l'inventaire s'il est ouvert
        if (Inventory.Instance.opened)
        {
            item = FindInInventory(str);
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
            Debug.Log("did not find singular : trying plural");
            items = Tile.current.items.FindAll(x => x.word.GetContent(Word.ContentType.JustWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.Plural).StartsWith(str));

            if (items.Count > 0)
            {
                Debug.Log("found plural word : " + items[0].word.text);
            }
        }
        else
        {
            Debug.Log("il y a des objets? ");
        }

        /// chercher les adjectifs pour différencier les objets ( porte bleu, porte rouge )
        if (items.Count > 0)
        {
            foreach (var inputPart in DisplayInput.Instance.inputParts)
            {
                foreach (var item in items)
                {
                    string adjSTR = item.GetAdjective.GetContent(item.word.genre, Word.Number.Singular);

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

    public static Item GetInWords(string[] words)
    {
        return GetInWords(words, -1);
    }

    public static Item GetInWords(string[] words, int forbiddenRow)
    {
        foreach (var part in words)
        {
            Item item = GetInWorld(part);

            if (item != null && item.index != forbiddenRow)
            {
                return item;
            }
        }

        return null;
    }

    public static Item FindByName(string str)
    {
        str = str.ToLower();

        Item item = items.Find(x => x.word.text.ToLower() == str);

        /*List<Item> possibleItems = items.FindAll(x => x.word.content.StartsWith(str));

        foreach (var i in possibleItems)
        {
            Debug.Log("possible item : " + i.word.content);
            if (i.word.content.Length > mostProbableItem.word.content.Length)
            {
                mostProbableItem = i;
            }
        }*/

        //Item item = items.Find(x => x.word.content.ToLower() == str );

        if (item == null)
        {
            // find plural
            item = items.Find(x => x.word.GetPlural() == str);

            if (item != null)
            {
                InputInfo.GetCurrent.actionOnAll = true;
                Debug.Log("PLURAL");
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

        Item item = items.Find(x => x.word.text.ToLower() == str);

        if (item == null)
        {
            // find plural
            item = items.Find(x => x.word.text.ToLower() == str);

            if (item != null)
            {
                InputInfo.GetCurrent.actionOnAll = true;
                return items;
            }
        }

        if (item == null)
        {

        }

        return items;
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

    #region adjective 
    public Adjective GetAdjective
    {
        get
        {
            if (adjective == null)
                SetRandomAdj();

            return adjective;
        }
    }
    public void SetRandomAdj()
    {
        SetAdjective(Adjective.GetRandom(Adjective.Type.Item));
    }
    public void SetAdjective(Adjective adj)
    {
        adjective = adj;
    }
    #endregion

    #region container
    public void GenerateItems()
    {
        foreach (var item in Item.items)
        {
            int targetItemIndex = InputInfo.GetCurrent.MainItem.index;
            Item.AppearInfo appearInfo = item.appearInfos.Find(x => x.itemIndex == targetItemIndex);

            if (appearInfo != null )
            {
                for (int i = 0; i < appearInfo.amount; i++)
                {
                    if (Random.value * 100f < appearInfo.rate)
                    {
                        Item newItem = Item.CreateNewItem(item);
                        items.Add(item);
                    }
                }
            }

        }
    }

    public string GetDescription()
    {
        Debug.Log("DISPLAYING CONTAINER");

        string text = "";

        if (items.Count == 0)
        {
            word.hasBeenInteractedWith = true;

            if (emptied)
            {
                text = "Il n'y a plus rien dans " + word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None);
            }
            else
            {
                text = "Il n'y a rien dans " + word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None); ;
            }

        }
        else
        {
            text = "Dans " + word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None) + ", vous voyez : ";

            text += Item.ItemListString(items, false, false);

        }

        text += "" +
            "\n" +
            "\n" +
            "Fermer " + word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None) + " ?";

        return text;
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

