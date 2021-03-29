using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile
{
    // location of tile in world
    public Coords coords;
    // tile type
    public Type type;

    public bool visited = false;

    public static Tile current;
    public static Tile previous;
    public bool locked = false;

    public static bool itemsChanged = false;

    public List<Item> items = new List<Item>();

    public Item tileItem;

    public Tile(Coords coords)
    {
        this.coords = coords;
    }

    public void SetType(Type type)
    {
        this.type = type;

        //MapTexture.Instance.Paint(coords, type);

        string tileName = GetTileName(type);
        Item item = Item.FindByName(tileName);

        tileItem = Item.CreateNewItem(item);
    }

    #region items
    public void GenerateItems()
    {
        foreach (var appearInfo in tileItem.appearInfos)
        {
            for (int i = 0; i < appearInfo.amount; i++)
            {
                if (Random.value * 100f < appearInfo.rate)
                {
                    Item itemToAdd = Item.items[appearInfo.itemIndex];
                    AddItem(itemToAdd);
                }
            }
        }

    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);

    }
    public void AddItem(Item item)
    {
        Item newItem = Item.CreateNewItem(item);
        newItem.SetRandomAdj();
        items.Add(newItem);
    }
    #endregion

    #region tile description
    public string GetDescription()
    {
        // ici en fait, il faudrait aussi que les phrases d'accroches aient des paramètres dans une db
        // exemple : "vous êtes encore/DEFINED/LOC|PREP/Singular etc...


        Word word = tileItem.word;

        Phrase.item = tileItem;

        // tile is continued ( road, forest etc... )
        if (SameAsPrevious() && tileItem.stackable )
        {
            return Phrase.GetPhrase("tile_continue");
        }
        else if (visited == false)
        {
            return Phrase.GetPhrase("tile_discover");
        }
        else
        {
            return Phrase.GetPhrase("tile_goback");
        }

    }
    #endregion

    #region items
    public string GetItemDescriptions()
    {
        if (Container.openedItem != null)
        {
            return Container.openedItem.GetDescription();
        }
        else if (Inventory.Instance.opened)
        {
            return Inventory.Instance.GetDescription();
        }

        if (items.Count == 0)
        {
            return "Il n'y a pas grand choses à voir par ici";
        }

        // item AND item counts
        List<ItemSocket> itemSockets = ItemSocket.GetItemSockets(items);

        List<ItemPhrase> phrases = new List<ItemPhrase>();

        // dans l'évier... près du mur... etc...
        List<string> itemPositions = new List<string>();

        for (int i = 0; i < itemSockets.Count; i++)
        {
            ItemSocket itemSocket = itemSockets[i];

            // retourne la phrase de position appropriée
            string itemPosition = itemSocket.item.GetItemPosition();

            if (itemPosition.StartsWith("relative"))
            {
                itemPosition = ItemSocket.GetRelativeItemPositionPhrase(itemSocket.item.word.text);
            }

            // si la position a déjà été trouve ( pour éviter : près du mur, une armoire, près de mur, une fenêtre )
            // et donc addictioner les noms ( près du mur, une armoire ET une fenêtre )
            ItemPhrase matchingPhrase = phrases.Find(x => x.itemPosition == itemPosition);

            if (matchingPhrase != null)
            {
                matchingPhrase.itemSockets.Add(itemSocket);
                continue;
            }


            ItemPhrase newPhrase = new ItemPhrase();

            newPhrase.itemSockets.Add(itemSocket);
            newPhrase.itemPosition = itemPosition;

            phrases.Add(newPhrase);

        }

        string text = "";

        // afficher toutes les phrases
        int a = 0;
        foreach (var phrase in phrases)
        {
            text += phrase.GetText();

            if (a < itemSockets.Count - 1)
            {
                text += "\n";
            }

            ++a;
        }

        return text;
    }
    #endregion

    #region info
    public static bool SameAsPrevious()
    {

        if (previous == null)
            return false;

        return Tile.current.type == Tile.previous.type;
    }
    #endregion

    public string GetTileName ( Tile.Type type )
    {
        switch (type)
        {
            case Type.None:
                return "";
            case Type.Plain:
                return "plaine";
            case Type.Field:
                return "champ";
            case Type.Clearing:
                return "clairière";
            case Type.Hill:
                return "colline";
            case Type.Mountain:
                return "montagne";
            case Type.Forest:
                return "forêt";
            case Type.Woods:
                return "bois";
            case Type.Sea:
                return "mer";
            case Type.Lake:
                return "lac";
            case Type.River:
                return "rivière";
            case Type.Beach:
                return "plage";
            case Type.Road:
                return "route";
            case Type.TownRoad:
                return "route de ville";
            case Type.CoastalRoad:
                return "route cotière";
            case Type.Path:
                return "chemin";
            case Type.Bridge:
                return "pont";
            case Type.TownHouse:
                return "maison";
            case Type.Farm:
                return "ferme";
            case Type.ForestCabin:
                return "cabane";
            case Type.CountryHouse:
                return "villa";
            case Type.Hallway:
                return "couloir";
            case Type.Stairs:
                return "escaliers";
            case Type.LivingRoom:
                return "salon";
            case Type.Kitchen:
                return "cuisine";
            case Type.DiningRoom:
                return "salle à manger";
            case Type.ChildBedroom:
                return "chambre d'enfant";
            case Type.Bedroom:
                return "chambre à coucher";
            case Type.Bathroom:
                return "salle de bain";
            case Type.Toilet:
                return "toilettes";
            case Type.Attic:
                return "grenier";
            case Type.Basement:
                return "cave";
            case Type.Cellar:
                return "cellier";
            default:
                return "did not found name for type : " + type;
        }
    }

    public enum Type
    {

        None,

        // plains
        Plain,
        Field,
        Clearing,

        // hills
        Hill,
        Mountain,

        // forests
        Forest,
        Woods,

        // water
        Sea,
        Lake,
        River,
        Beach,

        // roads
        Road,
        TownRoad,
        CoastalRoad,
        Path,
        Bridge,

        // houses
        TownHouse,
        Farm,
        ForestCabin,
        CountryHouse,

        // Interiors
        Hallway,
        Stairs,

        LivingRoom,
        Kitchen,
        DiningRoom,
        ChildBedroom,
        Bedroom,
        Bathroom,
        Toilet,
        Attic,
        Basement,
        Cellar,

    }
}