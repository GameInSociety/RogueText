using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class Tile : Item
{
    // location of tile in world
    public Coords coords;
    // tile type
    public bool visited = false;

    public bool enclosed = false;

    public static bool itemsChanged = false;

    #region tile description
    public string GetDescription()
    {
        // ici en fait, il faudrait aussi que les phrases d'accroches aient des paramètres dans une db
        // exemple : "vous êtes encore/DEFINED/LOC|PREP/Singular etc...
        string str = "";

        // tile is continued ( road, forest etc... )
        if (SameAsPrevious() && stackable)
        {
            str = TextManager.GetPhrase("tile_continue");
        }
        else if (!visited)
        {
            str = TextManager.GetPhrase("tile_discover");
        }
        else
        {
            str = TextManager.GetPhrase("tile_goback");
        }

        return str;
    }
    #endregion

    public override void GenerateItems()
    {
        base.GenerateItems();

        foreach (var item in DebugManager.Instance.itemsOnTile)
        {
            ItemManager.Instance.CreateInItem(this, item);
        }
    }

    public override string GetItemDescriptions()
    {
        return SocketManager.Instance.DescribeItems(GetContainedItems, null);
    }

    #region info
    public static bool SameAsPrevious()
    {
        if (GetPrevious == null)
            return false;

        return GetCurrent.SameTypeAs(GetPrevious);
    }
    public Player.Orientation OrientationToPlayer()
    {
        Coords dir = coords - Player.Instance.coords;

        Cardinal cardinal = (Cardinal)dir;

        Player.Orientation orientation = Player.Instance.CardinalToOrientation(cardinal);

        return orientation;
    }
    #endregion

    /// <summary>
    /// INTERIOR
    /// </summary>
    public bool HasDoor (string direction)
    {
        return GetContainedItems.Find(x =>
        x.HasWord("door") &&
        x.HasProperty("direction") &&
        x.GetProperty("direction").value == direction) != null;
    }

    /// <summary>
    /// TOOLS
    /// </summary>
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