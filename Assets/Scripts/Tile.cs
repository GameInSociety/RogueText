using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;

[System.Serializable]
public class Tile : Item
{
    // location of tile in world
    public Coords coords;

    public bool enclosed = false;

    public static bool itemsChanged = false;

    #region tile description
    // describe from the target cardinal ( example : Player Direction if move, window direction if look at window )
    // pas mal
    public void Describe( )
    {
        if (Player.Instance.coords== coords)
        {
            // don't change orientation
        }
        else
        {
            // change orientation, so the description is correct
            Coords dir = coords - Player.Instance.coords;
            Cardinal cardinal = (Cardinal) dir;
            Player.Instance.Orient(Movable.CardinalToOrientation(cardinal));
        }

        TextManager.Renew();

        DescribeSelf();

        if (!Player.Instance.CanSee())
        {
            return;
        }

        WriteSurroundingTileDescription();

        WriteContainedItemDescription();

    }

    public void WriteSurroundingTileDescription()
    {
        if (enclosed)
        {
            return;
        }

        SocketManager.Instance.DescribeItems(SurroundingTiles(), null);
    }



    public void DescribeSelf()
    {
        if (stackable)
        {
            TextManager.WritePhrase("tile_continue", (Item)this);
        }
        else if (!used)
        {
            TextManager.WritePhrase("tile_discover", (Item)this);
        }
        else
        {
            TextManager.WritePhrase("tile_goback", (Item)this);
        }


    }

    // with the reference cardinal
    // player dir if move
    // window dir if look...
    // and coming others
    public List<Item> SurroundingTiles()
    {
        List<Item> result = new List<Item>();

        List<Movable.Orientation> orientations = new List<Movable.Orientation>
        {
            Movable.Orientation.Front,
            Movable.Orientation.Right,
            Movable.Orientation.Left
        };

        foreach (var orientation in orientations)
        {
            Tile targetTile = GetTile(orientation);

            if (targetTile == null)
            {
                continue;
            }

            if (targetTile.enclosed)
            {
                continue;
            }

            if (!targetTile.HasProperty("direction"))
            {

                targetTile.CreateProperty("dir / direction / none");
            }

            Cardinal dir = Movable.OrientationToCardinal( orientation);
            targetTile.GetProperty("direction").Update(dir.ToString().ToLower());


            result.Add(targetTile);
        }

        return result;
    }

    public Tile GetTile( Movable.Orientation orientation)
    {
        Cardinal dir = Movable.OrientationToCardinal(orientation);

        Coords targetCoords = coords + (Coords)dir;

        return TileSet.current.GetTile(targetCoords);
    }
    public Tile GetAdjacentTile (Cardinal cardinal)
    {
        Coords targetCoords = coords + (Coords)cardinal;

        return TileSet.current.GetTile(targetCoords);
    }
    #endregion

    public override void TryGenerateItems()
    {
        base.TryGenerateItems();

        foreach (var item in DebugManager.Instance.itemsOnTile)
        {
            ItemManager.Instance.CreateInItem(this, item);
        }
    }

    public override void WriteContainedItemDescription()
    {
        //base.WriteContainedItemDescription();

        if (!used)
        {
            TryGenerateItems();
        }

        Socket socket = new Socket();
        socket.SetPosition("&on the dog (tile)&");

        SocketManager.Instance.DescribeItems(GetContainedItems, socket);
    }

    #region info
    public static bool SameAsPrevious()
    {
        if (GetPrevious == null)
            return false;

        return GetCurrent.SameTypeAs(GetPrevious);
    }
    public Movable.Orientation OrientationToPlayer()
    {
        Coords dir = coords - Player.Instance.coords;

        Cardinal cardinal = (Cardinal)dir;

        Movable.Orientation orientation = Movable.CardinalToOrientation(cardinal);

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