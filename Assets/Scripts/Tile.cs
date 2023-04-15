using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    // location of tile in world
    public Coords coords;
    // tile type
    public Type type;

    public bool visited = false;

    public bool enclosed = false;

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

        string tileName = GetName();
        Item item = Item.GetDataItem(tileName);

        if ( item == null)
        {
            Debug.LogError("couldn't find tile item of name : " + tileName);
        }

        tileItem = Item.CreateNew(item);
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
                    Item newItem = Item.CreateNew(appearInfo.GetItem());
                    AddItem(newItem);
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
        items.Add(item);
    }
    #endregion

    #region tile description
    public string GetDescription()
    {
        // ici en fait, il faudrait aussi que les phrases d'accroches aient des paramètres dans une db
        // exemple : "vous êtes encore/DEFINED/LOC|PREP/Singular etc...
        Word word = tileItem.word;

        string str = "";

        // tile is continued ( road, forest etc... )
        if (SameAsPrevious() && tileItem.stackable)
        {
            str = Phrase.GetPhrase("tile_continue");
        }
        else if (visited == false)
        {
            str = Phrase.GetPhrase("tile_discover");
        }
        else
        {
            str = Phrase.GetPhrase("tile_goback");
        }

        return str;
    }
    #endregion

    #region items
    public bool HasItem(List<string> item_names)
    {
        return HasItem(item_names.ToArray());
    }
    public bool HasItem(string[] item_names)
    {
        foreach (var item_name in item_names)
        {
            if (HasItem(item_name))
                return true;
        }

        return false;
    }

    public bool HasItem(string item_name)
    {
        return items.Find(x => x.word.text == item_name) != null;
    }
    public void WriteItemDescription()
    {
        if (items.Count == 0)
        {
            return;
        }

        Phrase.Write("tile_item_descriptions");

    }
    public string GetItemDescriptions()
    {
        // item AND item counts
        List<ItemGroup> itemGroups = ItemGroup.GetItemGroups(items);

        // ITEM GROUP " est près de l'évier " SOCKET 
        List<ItemPhrase> phrases = new List<ItemPhrase>();

        // note : en quoi ItemGroup & ItemPhrase ne peuvent pas être les memes classes ?
        // bonne question

        string str = "";

        for (int item_group_index = 0; item_group_index < itemGroups.Count; item_group_index++)
        {
            ItemGroup itemGroup = itemGroups[item_group_index];

            // retourne la phrase de position appropriée
            Socket socket = itemGroup.item.GetSocket();

            // si la position a déjà été trouve ( pour éviter : près du mur, une armoire, près de mur, une fenêtre )
            // et donc addictioner les noms ( près du mur, une armoire ET une fenêtre )
            ItemPhrase matchingPhrase = phrases.Find(x => x.socket == socket);

            if (matchingPhrase != null && matchingPhrase.socket.relative == false)
            {
                matchingPhrase.itemGroups.Add(itemGroup);

                continue;
            }

            ItemPhrase newPhrase = new ItemPhrase();

            newPhrase.itemGroups.Add(itemGroup);
            newPhrase.socket = socket;

            str += newPhrase.GetText();
        }

        return str;
    }
    #endregion

    #region info
    public static bool SameAsPrevious()
    {

        if (GetPrevious == null)
            return false;

        return Tile.GetCurrent.type == Tile.GetPrevious.type;
    }
    #endregion

    public string GetName ()
    {
        switch (type)
        {
            // à mettre dans les objets, considerer les tiles comme des objets dès le debut comme un type
            case Type.None:
                return "";
            case Type.Plain:
                return "plain";
            case Type.Field:
                return "field";
            case Type.Clearing:
                return "clearing";
            case Type.Hill:
                return "hill";
            case Type.Mountain:
                return "mountain";
            case Type.Forest:
                return "forest";
            case Type.Woods:
                return "wood";
            case Type.Sea:
                return "sea";
            case Type.Lake:
                return "lake";
            case Type.River:
                return "river";
            case Type.Beach:
                return "beach";
            case Type.Road:
                return "road";
            case Type.TownRoad:
                return "village road";
            case Type.CoastalRoad:
                return "coastal road";
            case Type.Path:
                return "path";
            case Type.Bridge:
                return "bridge";
            case Type.TownHouse:
                return "house";
            case Type.Farm:
                return "farm";
            case Type.ForestCabin:
                return "cabin";
            case Type.CountryHouse:
                return "mansion";
            case Type.Hallway:
                return "hallway";
            case Type.Stairs:
                return "stairs";
            case Type.LivingRoom:
                return "living room";
            case Type.Kitchen:
                return "kitchen";
            case Type.DiningRoom:
                return "dining room";
            case Type.ChildBedroom:
                return "children's bed room";
            case Type.Bedroom:
                return "bedroom";
            case Type.Bathroom:
                return "bathroom";
            case Type.Toilet:
                return "toilets";
            case Type.Attic:
                return "attic";
            case Type.Basement:
                return "basement";
            case Type.Cellar:
                return "cellar";
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

    private static Tile _previous;
    public static Tile GetPrevious {
        get
        {
            return _previous;
        }
    }
    public static void SetPrevious(Tile tile)
    {
        _previous = tile;
    }

    private static Tile _current;
    public static Tile GetCurrent {
        get
        {
            return _current;
        }
    }

    public static void SetCurrent(Tile tile)
    {
        DebugManager.Instance.tile = tile;
        _current = tile;
    }

}