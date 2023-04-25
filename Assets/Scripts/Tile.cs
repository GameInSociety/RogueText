using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
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

        tileItem = ItemManager.Instance.CreateFromData(GetName());
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
                    ItemManager.Instance.CreateInTile(this, appearInfo.GetItemName());
                }
            }
        }
    }
    public void UpdateProperties()
    {
        foreach (var item in items)
        {
            item.UpdateProperties();
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
            str = PhraseKey.GetPhrase("tile_continue");
        }
        else if (!visited)
        {
            str = PhraseKey.GetPhrase("tile_discover");
        }
        else
        {
            str = PhraseKey.GetPhrase("tile_goback");
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

        PhraseKey.WritePhrase("tile_item_descriptions");

    }
    public string GetItemDescriptions()
    {
        List<Socket> sockets= new List<Socket>();

        string phrases = "";

        // get item item AND item counts
        foreach (var item in items)
        {
            // A REMETTRE
            // j'ai mis de côté quand j'ai enlevé la classe "ItemPhrase" et "ItemGroup" qui ne servaient à rien
            // mais important et à remettre
            if (item.stackable)
            {

            }

            Socket socket = Socket.GetRandomSocket(item);
            if ( sockets.Contains(socket) )
            {
                // socket already exists
                socket = sockets.Find(x => x == socket);
            }
            else
            { 
                // didn't find socket, fetching new
                sockets.Add(socket);
                socket.itemGroups.Clear();
            }

            // see if the item is already in the socket
            Socket.ItemGroup itemGroup = socket.itemGroups.Find(x=> x.item.index == item.index);

            if (itemGroup.item ==null)
            {
                // it isn't, so add it
                itemGroup = new Socket.ItemGroup();

                itemGroup.item = item;

                itemGroup.count = 1;

                socket.itemGroups.Add(itemGroup);
            }
            else
            {
                // is already is, so add to the item group count
                itemGroup.count++;
            }

        }

        // return the phrases
        int i = 0;
        foreach (var socket in sockets)
        {
            string phrase = socket.GetDescription();
            phrases += phrase;

            if ( i < sockets.Count -1) { 
                phrases += "\n";
            }

            ++i;
        }

        return phrases;
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