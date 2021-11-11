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
    public string WriteDescription()
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

        Phrase.Write(str);

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

        // item AND item counts
        List<ItemGroup> itemGroups = ItemGroup.GetItemGroups(items);

        // ITEM GROUP " est près de l'évier " SOCKET 
        List<ItemPhrase> phrases = new List<ItemPhrase>();

        // note : en quoi ItemGroup & ItemPhrase ne peuvent pas être les memes classes ?
        // bonne question

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

            Phrase.Write(newPhrase.GetText());
        }
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